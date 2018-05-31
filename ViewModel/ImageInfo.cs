using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using GalaSoft.MvvmLight;
using System.Windows.Markup;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Runtime.InteropServices;
using NLog;
namespace ImageCFP.ViewModel
{
    public class ShowConfigPanelMessage
    {
        public ConfigPanelType ActivePanel { get; set; }
    }
    #region enum
    public enum ConfigPanelType
    { 
        Panel_LaplasSharp,
    }
    public enum LaplasSharpModel
    {
        Temp0 = 0,//x、y方向微分处理 { 0, 1, 0, 1, -4, 1, 0, 1, 0 }
        Temp1,//扩展（包括对角线） { 1, 1, 1, 1, -8, 1, 1, 1, 1 };
        Temp2,//其他拉普拉斯变换1{ 0, -1, 0, -1, 4, -1, 0, -1, 0 };
        Temp3,//其他拉普拉斯变换2{ -1, -1, -1, -1, 8, -1, -1, -1, -1 };
        Temp4,//图像反锐化掩蔽1（将图像模糊形式从原始图像中去除）{ 0, -1, 0, -1, 5, -1, 0, -1, 0 };
        Temp5,//图像反锐化掩蔽2（将图像模糊形式从原始图像中去除）{ -1, -1, -1, -1, 9, -1, -1, -1, -1 };

    }
    public class EnumValuesExtension : MarkupExtension
    {
        private readonly Type _enumType;

        public EnumValuesExtension(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException("enumType");
            if (!enumType.IsEnum)
                throw new ArgumentException("Argument enumType must derive from type Enum.");

            _enumType = enumType;
        }

        public override object ProvideValue(System.IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_enumType);
        }
    }
    #endregion
    public class ImageInfo : ViewModelBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public int Height = 0;
        public int Width = 0;
        public ImageInfo() { }
        public void initImageInfo(string name, byte[] ImageData, int width, int height,bool iscolorImage = false)
        {
            
            IsColorImage = iscolorImage;
            ImageName = name;
            IsHeightMap = false;
            Width = width;
            Height = height;
            if (iscolorImage)
            {
                CurrentImage = new byte[3 * width * height];
                LastImage = new byte[3* width * height];
                for (int i = 0; i < 3 * width * height; i++)
                {
                    CurrentImage[i] = ImageData[i];
                    LastImage[i] = ImageData[i];
                }
            }
            else
            {
                CurrentImage = new byte[width * height];
                LastImage = new byte[width * height];
                for (int i = 0; i < width * height; i++)
                {
                    CurrentImage[i] = ImageData[i];
                    LastImage[i] = ImageData[i];
                }
            }
            ImgSource = IImageDataToImageSource();
        }
        public void initImageInfo(string name, double[] heightData, int width, int height)
        {
            ImageName = name;
            IsHeightMap = true;
            Width = width;
            Height = height;
            CurrentHeightImage = new double[width * height];
            LastHeightImage = new double[width *height];
            for (int i = 0; i < width * height; i++)
            {
                CurrentHeightImage[i] = heightData[i];
                LastHeightImage[i] = heightData[i];
                if (CurrentHeightImage[i] >= BadPixel || double.IsNaN(CurrentHeightImage[i]))
                    HasBadPixel = true;
            }
            ImgSource = IImageDataToImageSource();
        }
        public byte[] LastImage;
        public byte[] CurrentImage;
        public double[] LastHeightImage;
        public double[] CurrentHeightImage;
        public bool IsColorImage = false;//原图是否为彩色图像
        public bool IsColorCoding = false;//是否伪彩色处理
        public bool IsHeightMap = false;
        public bool HasBadPixel = false;
        private double BadPixel = int.MaxValue;
        ImageProcessing imgprocess = new ImageProcessing();
        public string imageName = "";
        public string ImageName
        {
            get { return imageName; }
            set
            {
                if (value != imageName)
                {
                    imageName = value;
                    RaisePropertyChanged("ImageName");
                }
            }
        }
        private ImageSource imgSource;
        public ImageSource ImgSource
        {
            get { return imgSource; }
            set
            {
                if (value != imgSource)
                {
                    imgSource = value;
                    RaisePropertyChanged("ImgSource");
                }
            }
        }
        #region MVVM
        /// <summary>
        /// The <see cref="IsPreview" /> property's name.
        /// </summary>
        public const string IsPreviewPropertyName = "IsPreview";

        private bool _isPreview = false;

        /// <summary>
        /// Sets and gets the IsPreview property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPreview
        {
            get
            {
                return _isPreview;
            }

            set
            {
                if (_isPreview == value)
                {
                    return;
                }

                RaisePropertyChanging(IsPreviewPropertyName);
                _isPreview = value;
                RaisePropertyChanged(IsPreviewPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="LaplasSharp" /> property's name.
        /// </summary>
        public const string LaplasSharpPropertyName = "LaplasSharp";

        private LaplasSharpModel _laplasSharp = LaplasSharpModel.Temp0;

        /// <summary>
        /// Sets and gets the LaplasSharp property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public LaplasSharpModel LaplasSharp
        {
            get
            {
                return _laplasSharp;
            }

            set
            {
                if (_laplasSharp == value)
                {
                    return;
                }

                RaisePropertyChanging(LaplasSharpPropertyName);
                _laplasSharp = value;
                RaisePropertyChanged(LaplasSharpPropertyName);
            }
        }
        #endregion
        #region command
        private RelayCommand<ConfigPanelType> _showConfigCmd;

        /// <summary>
        /// Gets the ShowConfigCmd.
        /// </summary>
        public RelayCommand<ConfigPanelType> ShowConfigCmd
        {
            get
            {
                return _showConfigCmd
                    ?? (_showConfigCmd = new RelayCommand<ConfigPanelType>(
                                          p =>
                                          {
                                              ShowConfigPanelMessage msg = new ShowConfigPanelMessage();
                                              msg.ActivePanel = p;
                                              Messenger.Default.Send<ShowConfigPanelMessage>(msg);
                                          }));
            }
        }
        private RelayCommand _cancelCmd;

        /// <summary>
        /// Gets the CancelCommand.
        /// </summary>
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCmd ?? (_cancelCmd = new RelayCommand(
                    ExecuteCancelCommand,
                    CanExecuteCancelCommand));
            }
        }

        private void ExecuteCancelCommand()
        {

            try
            {
                if (IsHeightMap)
                {
                    CurrentHeightImage = LastHeightImage;
                }
                else
                {
                    CurrentImage = LastImage;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

        }

        private bool CanExecuteCancelCommand()
        {
            return Width > 0 && Height > 0;
        }
        private RelayCommand _laplasSharpCmd;

        /// <summary>
        /// Gets the LaplasSharpCommand.
        /// </summary>
        public RelayCommand LaplasSharpCommand
        {
            get
            {
                return _laplasSharpCmd ?? (_laplasSharpCmd = new RelayCommand(
                    ExecuteLaplasSharpCommand,
                    CanExecuteLaplasSharpCommand));
            }
        }

        private void ExecuteLaplasSharpCommand()
        {

            try
            {
                if (IsHeightMap)
                {
                    LastHeightImage = CurrentHeightImage;
                    CurrentHeightImage = imgprocess.LaplasSharp(Width, Height, CurrentHeightImage, (int)LaplasSharp, HasBadPixel);
                }
                else
                {
                    LastImage = CurrentImage;
                    CurrentImage = imgprocess.LaplasSharp(Width, Height, CurrentImage, (int)LaplasSharp);
                }
                ImgSource = IImageDataToImageSource();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

        }

        private bool CanExecuteLaplasSharpCommand()
        {
            return Width > 0 && Height > 0;
        }
        #endregion
        #region ImageDataToImageSource
        public BitmapSource IImageDataToImageSource()
        {
            if (Width > 0 && Height > 0)
            {
                double dpiX = 96d;
                double dpiY = 96d;
                if (IsHeightMap)
                {
                    if (IsColorCoding)
                    {
                        PixelFormat pixelFormat = PixelFormats.Bgr24;
                        byte[] colorvalues = GrayToDistriColor(CurrentHeightImage);
                        BitmapSource bitmap = BitmapSource.Create(Width, Height, dpiX, dpiY, pixelFormat, null, colorvalues, 3 * Width);
                        return bitmap;
                    }
                    else
                    {
                        byte[] buffer = new byte[Width * Height];
                        double MaxValue = double.MinValue;
                        double MinValue = double.MaxValue;
                        if (HasBadPixel)
                        {
                            for (int i = 0; i < Width * Height; i++)
                            {
                                if (CurrentHeightImage[i] < BadPixel && !double.IsNaN(CurrentHeightImage[i]))
                                {
                                    MaxValue = CurrentHeightImage[i] < MaxValue ? MaxValue : CurrentHeightImage[i];
                                    MinValue = CurrentHeightImage[i] > MinValue ? MinValue : CurrentHeightImage[i];
                                }
                            }
                            for (int i = 0; i < Width * Height; i++)
                            {
                                if (CurrentHeightImage[i] < BadPixel && !double.IsNaN(CurrentHeightImage[i]))
                                    buffer[i] = (byte)((254) * (CurrentHeightImage[i] - MinValue) / (MaxValue - MinValue));
                                else
                                    buffer[i] = 255;
                            }

                        }
                        else
                        {
                            for (int i = 0; i < Width * Height; i++)
                            {
                                    MaxValue = CurrentHeightImage[i] < MaxValue ? MaxValue : CurrentHeightImage[i];
                                    MinValue = CurrentHeightImage[i] > MinValue ? MinValue : CurrentHeightImage[i];
                            }
                            for (int i = 0; i < Width * Height; i++)
                            {
                                buffer[i] = (byte)(255 * (CurrentHeightImage[i] - MinValue) / (MaxValue - MinValue));
                            }
                        }
                        List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
                        for (int i = 1; i <= 255; i++)
                            colors.Add(Color.FromRgb((byte)i, (byte)i, (byte)i));
                        if (HasBadPixel)
                            colors.Add(System.Windows.Media.Colors.Red);
                        else
                            colors.Add(System.Windows.Media.Colors.White);
                        BitmapPalette Gray256Red = new BitmapPalette(colors);
                        PixelFormat pixelFormat = PixelFormats.Indexed8;
                        BitmapSource bitmap = BitmapSource.Create(Width, Height, dpiX, dpiY, pixelFormat, Gray256Red, buffer, Width);
                        return bitmap;
                    }
                }
                else
                {
                    //if (ByteSize == 2)
                    //{
                    //    PixelFormat pixelFormat = PixelFormats.Gray16;
                    //    IntPtr dataPtr = IntPtr.Zero;
                    //    GCHandle _hObject = GCHandle.Alloc(CurrentImage, GCHandleType.Pinned);
                    //    dataPtr = _hObject.AddrOfPinnedObject();

                    //    BitmapSource bitmap = BitmapSource.Create(Width, Height, dpiX, dpiY, pixelFormat, null, dataPtr, ByteSize, 2 * Width);
                    //    if (_hObject.IsAllocated)
                    //        _hObject.Free();
                    //    return bitmap;
                    //}
                    //else
                    //{
                        if (IsColorCoding)
                        {
                            PixelFormat pixelFormat = PixelFormats.Bgr24;
                            byte[] colorvalues = GrayToDistriColor(CurrentImage);
                            //IntPtr dataPtr = IntPtr.Zero;
                            //GCHandle _hObject = GCHandle.Alloc(colorvalues, GCHandleType.Pinned);
                            //dataPtr = _hObject.AddrOfPinnedObject();
                            //BitmapSource bitmap = BitmapSource.Create(Width, Height, dpiX, dpiY, pixelFormat, null, dataPtr, 3, 3 * Width);
                            //_hObject.Free();
                            BitmapSource bitmap = BitmapSource.Create(Width, Height, dpiX, dpiY, pixelFormat, null, colorvalues, 3 * Width);
                            return bitmap;
                        }
                        else
                        {
                            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
                            for (int i = 1; i <= 255; i++)
                                colors.Add(Color.FromRgb((byte)i, (byte)i, (byte)i));
                            if (HasBadPixel)
                                colors.Add(System.Windows.Media.Colors.Red);
                            else
                                colors.Add(System.Windows.Media.Colors.White);
                            BitmapPalette Gray256Red = new BitmapPalette(colors);
                            PixelFormat pixelFormat = PixelFormats.Indexed8;
                            //IntPtr dataPtr = IntPtr.Zero;
                            //GCHandle _hObject = GCHandle.Alloc(CurrentImage, GCHandleType.Pinned);
                            //dataPtr = _hObject.AddrOfPinnedObject();
                            //BitmapSource bitmap = BitmapSource.Create(Width, Height, dpiX, dpiY, pixelFormat, Gray256Red, dataPtr, 1, Width);
                            //_hObject.Free();
                            BitmapSource bitmap = BitmapSource.Create(Width, Height, dpiX, dpiY, pixelFormat, Gray256Red, CurrentImage,Width);
                            return bitmap;
                        }
                    //}
                }
            }
            else
                return null;
        }
        public byte[] GrayToDistriColor(double[] heightmap)
        {
            byte[] rgbValues = new byte[heightmap.Length * 3];
            double ZUpper = heightmap.Max();
            double ZLower = heightmap.Min();
            double Num2 = (ZUpper + ZLower) / 2;
            for (int i = 0; i < heightmap.Length; i++)
            {
                int j = i * 3;
                if (double.IsNaN(heightmap[i]))
                {
                    rgbValues[j + 2] = 0;
                    rgbValues[j + 1] = 0;
                    rgbValues[j] = 0; 
                }
                else
                {
                    if (heightmap[i] < ZLower)
                    {
                        rgbValues[j + 2] = 0;
                        rgbValues[j + 1] = 0;
                        rgbValues[j] = 255;
                    }
                    else if (heightmap[i] >= ZLower && heightmap[i] < Num2)
                    {
                        rgbValues[j + 2] = (byte)(2 * (heightmap[i] - ZLower) / (ZUpper - ZLower) * 127);
                        rgbValues[j + 1] = (byte)(2 * (heightmap[i] - ZLower) / (ZUpper - ZLower) * 255);
                        rgbValues[j] = (byte)(255 - 2 * (heightmap[i] - ZLower) / (ZUpper - ZLower) * 127);
                    }
                    else if (heightmap[i] >= Num2 && heightmap[i] < ZUpper)
                    {
                        rgbValues[j + 2] = (byte)(127 + 2 * (heightmap[i] - Num2) / (ZUpper - ZLower) * 127);
                        rgbValues[j + 1] = (byte)(255 - 2 * (heightmap[i] - Num2) / (ZUpper - ZLower) * 255);
                        rgbValues[j] = (byte)(127 - 2 * (heightmap[i] - Num2) / (ZUpper - ZLower) * 127);
                    }
                    else
                    {
                        rgbValues[j + 2] = 255;
                        rgbValues[j + 1] = 0;
                        rgbValues[j] = 0;
                    }
                }
            }
            return rgbValues;
        }
        public byte[] GrayToDistriColor(byte[] heightMap)
        {
            byte[] rgbValues = new byte[heightMap.Length * 3];
            double[] heightmap = new double[heightMap.Length];
            for (int i = 0; i < heightMap.Length; i++)
                heightmap[i] = (double)heightMap[i];
            double ZUpper = heightmap.Max();
            double ZLower = heightmap.Min();
            double Num2 = (ZUpper + ZLower) / 2;
            for (int i = 0; i < heightmap.Length; i++)
            {
                int j = i * 3;
                if (heightmap[i] < ZLower)
                {
                    rgbValues[j + 2] = 0;
                    rgbValues[j + 1] = 0;
                    rgbValues[j] = 255;
                }
                else if (heightmap[i] >= ZLower && heightmap[i] < Num2)
                {
                    rgbValues[j + 2] = (byte)(2 * (heightmap[i] - ZLower) / (ZUpper - ZLower) * 127);
                    rgbValues[j + 1] = (byte)(2 * (heightmap[i] - ZLower) / (ZUpper - ZLower) * 255);
                    rgbValues[j] = (byte)(255 - 2 * (heightmap[i] - ZLower) / (ZUpper - ZLower) * 127);
                }
                else if (heightmap[i] >= Num2 && heightmap[i] < ZUpper)
                {
                    rgbValues[j + 2] = (byte)(127 + 2 * (heightmap[i] - Num2) / (ZUpper - ZLower) * 127);
                    rgbValues[j + 1] = (byte)(255 - 2 * (heightmap[i] - Num2) / (ZUpper - ZLower) * 255);
                    rgbValues[j] = (byte)(127 - 2 * (heightmap[i] - Num2) / (ZUpper - ZLower) * 127);
                }
                else
                {
                    rgbValues[j + 2] = 255;
                    rgbValues[j + 1] = 0;
                    rgbValues[j] = 0;
                }
            }
            return rgbValues;
        }
        #endregion
    }
}

