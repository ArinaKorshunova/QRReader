   M S B A M L   `   `   `  ���� C  ?QRReader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:QRReader.ViewModelQRReader.ViewModel  -clr-namespace:QRReader.ViewQRReader.View      QRReader.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     6	viewmodel clr-namespace:QRReader.ViewModel     -localclr-namespace:QRReader.View       � Title$4  .Распознавание штрих-кода��$	��350��$	��525�� � WindowState$ 	Maximized=� � 
ResizeMode$ 	CanResize=� � DataContext -  'QRReader.ViewModel.MainWindowsViewModel  .���  � RowDefinitions �� $��50 ��� $��* ��� $��60 � � ColumnDefinitions �� $	��16* ��� $	��20* �$	��100���� $��5 ��� $	��20* �$	��500��.+��� $��Stretch=�$��Stretch=�$��2��$��3�� �� ResizeBehavior$ PreviousAndNext=�$
��Auto�����#423535$
��Auto����  �� Orientation$ 
Horizontal=�$��Stretch=�$��Stretch=�.+��� 
����� �$��10q�$��Загрузить��$
��Left=�$��Stretch=�$��5q����� *LoadFileCommand+{��� *ScannEnable+�� 
����� �$��10q�$��Сканировать��$
��Left=�$��Stretch=�$��5q����� *ScannCommand+{��� *ScannEnable+�� 
����� �$��10q�$/��)Сохранить изображение��$
��Left=�$��Stretch=�$��5q����� *SaveScanCommand+�� $��1��
���Gc��$��Right=�$��10q�$��5q�$��Распознать��{��� *ReaderIsEnable+���� *DecodeQRCodeCommand+�� $��1��$��2��$��2��$��2q����#423535$��10q�._��� $��12q�$��Uniform=�$	��Top=�$
��Left=����� *	LoadImage+�� $��3��$��3��$��2q����#423535$��10q�._�� ����  �� Color$ White��	 �� Opacity$		 0.1Q� �� $	��10* ��� $��50 �.+�#   QRReader.View.PaymentDataView   �� *PaymentDataVM+�� $��3��$��15��$��Right=�
�������$��10q�$��5q�$��Сохранить��