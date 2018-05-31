using GalaSoft.MvvmLight;
using ImageCFP.Model;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using NLog;
using System;
using System.Windows.Media;
using System.Drawing;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
namespace ImageCFP.ViewModel
{

    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public MainViewModel() { }
        string CurFileName = "";
        private Bitmap curBitmap;
        int Width = 0;
        int Height = 0;
        private ObservableCollection<ImageInfo> items = new ObservableCollection<ImageInfo>();
        public ObservableCollection<ImageInfo> Items
        {
            get { return items; }
        }

#region Command
#region LoadImageCommand
        private RelayCommand _loadImageCmd;

        /// <summary>
        /// Gets the LoadImageCommand.
        /// </summary>
        public RelayCommand LoadImageCommand
        {
            get
            {
                return _loadImageCmd ?? (_loadImageCmd = new RelayCommand(
                    ExecuteLoadImageCommand,
                    CanExecuteLoadImageCommand));
            }
        }

        private void ExecuteLoadImageCommand()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //dlg.DefaultExt = ".png";
            dlg.Filter = "所有图像文件 | *.bmp; *.pcx; *.png; *.jpg; *.gif;" +
                "*.tif; *.ico; *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf|" +
                "位图( *.bmp; *.jpg; *.png;...) | *.bmp; *.pcx; *.png; *.jpg; *.gif; *.tif; *.ico|" +
                "矢量图( *.wmf; *.eps; *.emf;...) | *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf";  
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == true)
            {
                CurFileName = dlg.FileName;
                try
                {
                    curBitmap = (Bitmap)Image.FromFile(CurFileName);

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }
        }

        private bool CanExecuteLoadImageCommand()
        {
            return true;
        }
#endregion
#region SaveImageCommand
        private RelayCommand _saveImageCmd;

        /// <summary>
        /// Gets the SaveImageCommand.
        /// </summary>
        public RelayCommand SaveImageCommand
        {
            get
            {
                return _saveImageCmd ?? (_saveImageCmd = new RelayCommand(
                    ExecuteSaveImageCommand,
                    CanExecuteSaveImageCommand));
            }
        }

        private void ExecuteSaveImageCommand()
        {
           if (curBitmap == null)  
            {  
                return;  
            }  
            SaveFileDialog saveDlg = new SaveFileDialog();  
            saveDlg.Title = "保存为";  
            saveDlg.OverwritePrompt = true;  
            saveDlg.Filter =  
                "BMP文件 (*.bmp) | *.bmp|" +  
                "Gif文件 (*.gif) | *.gif|" +  
                "JPEG文件 (*.jpg) | *.jpg|" +  
                "PNG文件 (*.png) | *.png";  
            if (saveDlg.ShowDialog() == true)
            {
                string fileName = saveDlg.FileName;
                string strFilExtn = fileName.Remove(0, fileName.Length - 3);
                switch (strFilExtn)
                {
                    case "bmp":
                        curBitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case "jpg":
                        curBitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case "gif":
                        curBitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                    case "tif":
                        curBitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Tiff);
                        break;
                    case "png":
                        curBitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    default:
                        break;
                }
            }
        }

        private bool CanExecuteSaveImageCommand()
        {
            return true;
        }
#endregion
#region LoadHeightDatCommand
        private RelayCommand _loadHeightDatCmd;

        /// <summary>
        /// Gets the LoadHeightDatCommand.
        /// </summary>
        public RelayCommand LoadHeightDatCommand
        {
            get
            {
                return _loadHeightDatCmd ?? (_loadHeightDatCmd = new RelayCommand(
                    ExecuteLoadHeightDatCommand,
                    CanExecuteLoadHeightDatCommand));
            }
        }

        private void ExecuteLoadHeightDatCommand()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //dlg.DefaultExt = ".png";
            dlg.Filter = "Dat File(.dat)|*.dat";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == true)
            {
                CurFileName = dlg.FileName;
                try
                {
                    string name = System.IO.Path.GetFileName(CurFileName);
                    string filename = name.Substring(0, name.Length - 4);
                    double[] CurHeightImageData;
                    using (StreamReader reader = new StreamReader(CurFileName))
                    {
                        Width = (int)Convert.ToDouble(reader.ReadLine());
                        Height = (int)Convert.ToDouble(reader.ReadLine());
                        CurHeightImageData = new double[Width * Height];
                        for (int i = 0; i < Width * Height; i++)
                        {
                            CurHeightImageData[i] = Convert.ToDouble(reader.ReadLine());
                        }
                    }
                    ImageInfo image = new ImageInfo();
                    image.initImageInfo(filename, CurHeightImageData, Width, Height);
                    ProcessWindow win = new ProcessWindow();
                    win.DataContext = image;
                    //win.Owner = Application.Current.MainWindow;
                    win.Show();
                    //curBitmap = (Bitmap)Image.FromFile(CurFileName);

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }
        }

        private bool CanExecuteLoadHeightDatCommand()
        {
            return true;
        }
        #endregion
#endregion
    }
}