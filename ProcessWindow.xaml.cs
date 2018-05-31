using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using ImageCFP.ViewModel;
namespace ImageCFP
{
    /// <summary>
    /// Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : Window
    {
        public ProcessWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<ShowConfigPanelMessage>(this, (action) => ShowConfigPanel(action));
            LaplasSharpSetting.Hide();
        }
        public void ShowConfigPanel(ShowConfigPanelMessage action)
        {
            switch (action.ActivePanel)
            {
                case ConfigPanelType.Panel_LaplasSharp:
                    LaplasSharpSetting.Show();
                    LaplasSharpSetting.Float();
                    break;

            }
        }
        #region --------------------TITLE BAR--------------------
        //标题栏的关闭按钮事件处理代码
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();//Close方法关闭主窗体
        }
        //标题栏的最大化按钮事件处理代码
        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {   //首先判断当前WindowState的状态是否是最大化
            if (this.WindowState == WindowState.Maximized)
                //如果为最大化，则设置为标准样式
                this.WindowState = WindowState.Normal;
            else //否则设置为最大化样式
                this.WindowState = WindowState.Maximized;
        }
        //将窗口最小化事件处理代码
        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;//指定最小化
        }
        #endregion
        private void OnMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            this.DragMove();
        }
    }
}
