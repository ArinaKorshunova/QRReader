using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace QRReader.TwineData
{
    // A delegate type for hooking up change notifications.
    public delegate void TwainEventHandler(WpfTwain sender);
    public delegate void TwainTransferReadyHandler(WpfTwain sender, List<ImageSource> imageSources);

    public class WpfTwain : DependencyObject
    {
        // events 
        public event TwainEventHandler TwainCloseRequest;
        public event TwainEventHandler TwainCloseOk;
        public event TwainEventHandler TwainDeviceEvent;
        public event TwainTransferReadyHandler TwainTransferReady;
        public bool IsScanning { get { return TwainMessageProcessing; } }

        private bool TwainMessageProcessing = false;
        private Twain tw = null;
        private int picnumber = 0; // global picture counter

        private System.IntPtr _handle = IntPtr.Zero;

        public Window TheMainWindow { get { return App.Current.MainWindow; } }

        public uint WM_App_Acquire = Win32.RegisterWindowMessage("IBN_WfpTwain_Acquire");
        //If you do not want so register a message, a simple const will do (just make sure it is unique)
        //public const uint WM_App_Aquire =  0x8123; // WM_App + 0x123 - should be uniqe within the application

        public System.IntPtr WindowHandle
        {
            get
            {
                if (_handle == IntPtr.Zero)
                    _handle = (new WindowInteropHelper(TheMainWindow)).Handle;
                return _handle;
            }
        }

        public WpfTwain()
        {
            // hook to events of the main window
            if (WindowHandle != IntPtr.Zero)
            {
                // main windows is initialized and we can hook events and start woking with it
                HostWindow_Loaded(this, null);
            }
            else
            {
                // hook events etc later, when the main window is loaded.
                TheMainWindow.Loaded += HostWindow_Loaded;
            }
            TheMainWindow.Closing += HostWindow_Closing;
        }

        ~WpfTwain()
        {
            // by now the interface should already be closed. we call terminate just in case.
            TerminateTw();
        }

        /// <summary>
        /// Open the Twain source selection dialog
        /// </summary>
        /// <returns></returns>
        public bool Select()
        {
            if (tw != null)
            {
                tw.Select();
                return true;
            }
            else
                return false;
        }

        static UInt32 wParam_buffer;

        /// <summary>
        /// Activate Twain aquire
        /// 
        /// notes:
        /// Activation is done using post message to reduce friction between WPF and Windows events.
        /// </summary>
        /// <param name="showUI"></param>
        /// <returns></returns>
        public bool Acquire(bool showUI)
        {
            if (tw != null)
            {
                TwainMessageProcessing = true;
                bool posted = Win32.PostMessage(WindowHandle, WM_App_Acquire, (IntPtr)(showUI ? 1 : 0), IntPtr.Zero);
                return true;
            }
            else
                return false;
        }

        private void HostWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddMessageHook();
            tw = new Twain();
            tw.Init(WindowHandle);
        }

        private void HostWindow_Closing(object sender, EventArgs e)
        {
            RemoveMessageHook();
        }


        private void AddMessageHook()
        {
            HwndSource src = HwndSource.FromHwnd(WindowHandle);
            src.AddHook(new HwndSourceHook(this.WndProc));
        }

        private void RemoveMessageHook()
        {
            HwndSource src = HwndSource.FromHwnd(WindowHandle);
            src.AddHook(new HwndSourceHook(this.WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            System.Windows.Forms.Message m = new System.Windows.Forms.Message();
            m.HWnd = hwnd;
            m.Msg = msg;
            m.WParam = wParam;
            m.LParam = lParam;

            if (handled)
                return IntPtr.Zero;

            if (msg == WM_App_Acquire)
            {
                tw.Acquire(wParam != IntPtr.Zero);
            }

            //registered with "DSMAPPMESSAGE32"
            if (/*TwainMessageProcessing &&*/ tw != null)
            {
                PreFilterMessage(m, ref handled);
            }
            return IntPtr.Zero;
        }

        private void TerminateTw()
        {
            if (tw != null)
            {
                tw.Finish();
                tw = null;
            }
            TwainMessageProcessing = false;
        }

        // Twain event callbacks
        protected void OnTwainCloseRequest()
        {
            if (TwainCloseRequest != null)
                TwainCloseRequest(this);
            tw.CloseSrc();
        }

        protected void OnTwainCloseOk()
        {
            if (TwainCloseOk != null)
                TwainCloseOk(this);

            //EndingScan();
            tw.CloseSrc();
        }

        protected void OnTwainDeviceEvent()
        {
            if (TwainDeviceEvent != null)
                TwainDeviceEvent(this);
        }

        protected void OnTwainTransferReady()
        {
            if (TwainTransferReady == null)
                return; // not likely..

            List<ImageSource> imageSources = new List<ImageSource>();
            ArrayList pics = tw.TransferPictures();
            tw.CloseSrc();
            EndingScan();
            picnumber++;
            for (int i = 0; i < pics.Count; i++)
            {
                IntPtr imgHandle = (IntPtr)pics[i];
                if (i == 0)
                { // first image only
                    imageSources.Add(DibToBitmap.FormHDib(imgHandle));
                    //Refresh(image1); // force a redraw
                }
                Win32.GlobalFree(imgHandle);
            }
            // for some reason the main window does not refresh properly - resizing of the window solves the proble.
            // happens only with the Canon LIDE Scanner
            // Suspected: some messages where eaten by Twain
            TwainTransferReady(this, imageSources);
        }

        private void EndingScan()
        {
            // stop sending messged to the Twain processon
            if (TwainMessageProcessing)
            {
                TwainMessageProcessing = false;
            }

            //// Enable scan buttons
            //ScanButton.IsEnabled = ScanUIButton.IsEnabled = true;//this.Enabled = true;
        }



        protected void PreFilterMessage(System.Windows.Forms.Message m, ref bool handled)
        {
            TwainCommand cmd = tw.PassMessage(ref m);
            if (cmd == TwainCommand.Not || cmd == TwainCommand.Null)
                return; // do not change handled

            switch (cmd)
            {
                case TwainCommand.CloseRequest:
                    {
                        OnTwainCloseRequest();
                        break;
                    }
                case TwainCommand.CloseOk:
                    {
                        OnTwainCloseOk();
                        break;
                    }
                case TwainCommand.DeviceEvent:
                    {
                        OnTwainDeviceEvent();
                        break;
                    }
                case TwainCommand.TransferReady:
                    {
                        OnTwainTransferReady();
                        break;
                    }
            }

            handled = true;
        }
    }
}
