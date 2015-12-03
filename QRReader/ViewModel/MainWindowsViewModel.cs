using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QRReader.Controls;
using QRReader.Model;
using QRReader.TwineData;
using ZXing;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace QRReader.ViewModel
{
    class MainWindowsViewModel : Window
    {
        #region Переменные

            protected WpfTwain TwainInterface = null;

        #endregion

        #region Свойства

            public string FilePath { get; set; }
            public PaymentDataModel PaymentData { get; set; }

        #endregion

        #region Свойства зависимости

            public string Text
            {
                get { return (string)GetValue(TextProperty); }
                set { SetValue(TextProperty, value); }
            }

            public static readonly DependencyProperty TextProperty =
                DependencyProperty.Register("Text", typeof(string), typeof(MainWindowsViewModel), new UIPropertyMetadata(null));

            public DelegateCommand LoadFileCommand
            {
                get { return (DelegateCommand)GetValue(LoadFileCommandProperty); }
                set { SetValue(LoadFileCommandProperty, value); }
            }

            public static readonly DependencyProperty LoadFileCommandProperty =
                DependencyProperty.Register("LoadFileCommand", typeof(DelegateCommand), typeof(MainWindowsViewModel), new UIPropertyMetadata(null));

            public DelegateCommand DecodeQRCodeCommand
            {
                get { return (DelegateCommand)GetValue(DecodeQRCodeCommandProperty); }
                set { SetValue(DecodeQRCodeCommandProperty, value); }
            }

            public static readonly DependencyProperty DecodeQRCodeCommandProperty =
                DependencyProperty.Register("DecodeQRCodeCommand", typeof(DelegateCommand), typeof(MainWindowsViewModel), new UIPropertyMetadata(null));


            public DelegateCommand ScannCommand
            {
                get { return (DelegateCommand)GetValue(ScannCommandProperty); }
                set { SetValue(ScannCommandProperty, value); }
            }

            public static readonly DependencyProperty ScannCommandProperty =
                DependencyProperty.Register("ScannCommand", typeof(DelegateCommand), typeof(MainWindowsViewModel), new UIPropertyMetadata(null));

            public bool ScannEnable
            {
                get { return (bool)GetValue(ScannEnableProperty); }
                set { SetValue(ScannEnableProperty, value); }
            }

            public static readonly DependencyProperty ScannEnableProperty =
                DependencyProperty.Register("ScannEnable", typeof(bool), typeof(MainWindowsViewModel), new UIPropertyMetadata(null));

            public ImageSource LoadImage
            {
                get { return (ImageSource)GetValue(LoadImageProperty); }
                set { SetValue(LoadImageProperty, value); }
            }

            public static readonly DependencyProperty LoadImageProperty =
                DependencyProperty.Register("LoadImage", typeof(ImageSource), typeof(MainWindowsViewModel), new PropertyMetadata(null));

            public DelegateCommand SaveScanCommand
            {
                get { return (DelegateCommand)GetValue(SaveScanCommandProperty); }
                set { SetValue(SaveScanCommandProperty, value); }
            }

            public static readonly DependencyProperty SaveScanCommandProperty =
                DependencyProperty.Register("SaveScanCommand", typeof(DelegateCommand), typeof(MainWindowsViewModel), new UIPropertyMetadata(null));

            public bool ReaderIsEnable
            {
                get { return (bool)GetValue(ReaderIsEnableProperty); }
                set { SetValue(ReaderIsEnableProperty, value); }
            }

            public static readonly DependencyProperty ReaderIsEnableProperty =
                DependencyProperty.Register("ReaderIsEnable", typeof(bool), typeof(MainWindowsViewModel), new UIPropertyMetadata(null));

        #endregion

        #region Конструкторы

            public MainWindowsViewModel()
            {
                LoadFileCommand = new DelegateCommand(LoadFile);
                ScannCommand = new DelegateCommand(Scann);
                DecodeQRCodeCommand = new DelegateCommand(DecodeQRCode);
                SaveScanCommand = new DelegateCommand(SaveScann);
                //LoadImage = new BitmapImage();
                ScannEnable = true;
                TwainInterface = new WpfTwain();
                TwainInterface.TwainTransferReady += new TwainTransferReadyHandler(TwainWin_TwainTransferReady);
                TwainInterface.TwainCloseRequest += new TwainEventHandler(TwainUIClose);
                PaymentData = new PaymentDataModel();
                ReaderIsEnable = false;
            }

        #endregion

        #region   Twain event handlers

            private void TwainWin_TwainTransferReady(WpfTwain sender, List<ImageSource> imageSources)
            {
                ScannEnable = !TwainInterface.IsScanning;
                foreach (ImageSource ims in imageSources)
                {

                    LoadImage = ims;
                }
                ReaderIsEnable = true;
            }

            private void TwainUIClose(WpfTwain sender)
            {
                // Сканирование закончено
                ScannEnable = true;
                ReaderIsEnable = true;
            }

        #endregion

        #region Методы

            public void LoadFile()
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG;)|*.BMP;*.JPG;*.GIF;*.PNG;|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    FilePath = openFileDialog.InitialDirectory + openFileDialog.FileName;
                    LoadImage = new BitmapImage(new Uri(FilePath));
                    ReaderIsEnable = true;
                }
            }

            public void Scann()
            {
                ScannEnable = false;
                ReaderIsEnable = false;
                TwainInterface.Acquire(false);
            }

            public void SaveScann()
            {
                if (LoadImage != null)
                {
                    string savePath = string.Empty;
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = DateTime.Now.ToString("dd.MM.yyyy hh-mm") + ".jpg";
                    saveFileDialog.Filter =
                        "Image Files(*.BMP;*.JPG;*.GIF;*.PNG;)|*.BMP;*.JPG;*.GIF;*.PNG;|All files (*.*)|*.*";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        savePath = saveFileDialog.InitialDirectory + saveFileDialog.FileName;
                        using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                        {
                            try
                            {
                                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                BitmapFrame bf = BitmapFrame.Create((BitmapSource) LoadImage);
                                encoder.Frames.Add(bf);
                                encoder.Save(fs);
                                MessageBox.Show("Успешно сохранено!");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Возникла ошибка: " + ex.Message);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Данные отсутствуют");
                }
            }

            private ImageCodecInfo GetEncoder(ImageFormat format)
            {

                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

                foreach (ImageCodecInfo codec in codecs)
                {
                    if (codec.FormatID == format.Guid)
                    {
                        return codec;
                    }
                }
                return null;
            }

            public void DecodeQRCode()
            {
                ReaderIsEnable = false;
                Bitmap b = null;
                BitmapImage bitmapImage = new BitmapImage();
                PaymentData = new PaymentDataModel();
                if (LoadImage != null)
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    byte[] bytes = null;
                    var bitmapSource = LoadImage as BitmapSource;

                    if (bitmapSource != null)
                    {
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                        using (var stream = new MemoryStream())
                        {
                            encoder.Save(stream);
                            bytes = stream.ToArray();
                        }
                    }
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        ReaderIsEnable = true;
                    }

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        BitmapEncoder enc = new BmpBitmapEncoder();
                        enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                        enc.Save(outStream);
                        Bitmap bitmap = new Bitmap(outStream);

                        b = new Bitmap(bitmap);
                    }
                }

                IBarcodeReader reader = new BarcodeReader();
                var decode = reader.Decode(b);
                if (decode == null)
                {
                    MessageBox.Show("Штрих-код повержден или отсутствует!", "Ошибка расшифровки штрих-кода");
                }
                else
                {
                    Text = decode.Text; 
                }
                ReaderIsEnable = true;
            }

            private byte[] GetRGBValues(Bitmap bmp)
            {
                // Lock the bitmap's bits. 
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = bmpData.Stride * bmp.Height;
                byte[] rgbValues = new byte[bytes];
                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                bmp.UnlockBits(bmpData);

                return rgbValues;
            }

        #endregion
    }
}
