using System.Windows;
using ImageCFP.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using NLog.Config;
using NLog.Targets;
using NLog;
using System;
using System.Windows.Media;
namespace ImageCFP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            InitLogger();
            
        }
        public void InitLogger()
        {
            // Step 1. Create configuration object 
            LoggingConfiguration logConfig = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var target = new WpfRichTextBoxTarget();
            logConfig.AddTarget("RichTextBox", target);
            target.FormName = "ConverterWindow";
            target.ControlName = "loggingBox";
            FileTarget fileTarget = new FileTarget();
            logConfig.AddTarget("logFile", fileTarget);

            // Step 3. Set target properties
            target.Layout = "${longdate:useUTC=true}|${level:uppercase=true}|${logger}::${message}";
            target.AutoScroll = true;
            target.MaxLines = 100000;
            target.UseDefaultRowColoringRules = true;
            fileTarget.FileName = "C:\\temp\\NanoScopy.log.txt";
            fileTarget.Layout = "${longdate:useUTC=true}|${level:uppercase=true}|${logger}::${message}";
            fileTarget.ArchiveAboveSize = 10000;

            // Step 4. Define rules
            LoggingRule ruleRichTextBox = new LoggingRule("*", LogLevel.Trace, target);
            logConfig.LoggingRules.Add(ruleRichTextBox);
            LoggingRule ruleFile = new LoggingRule("*", LogLevel.Info, fileTarget);
            logConfig.LoggingRules.Add(ruleFile);

            // Step 5. Activate the configuration
            LogManager.Configuration = logConfig;
        }
        private void EnglishButtonClick(object sender, System.EventArgs e)
        {
            //LanguageHelper lh = new LanguageHelper();
            string languageDictPath = "pack://application:,,,/ImageCFP;component/Skins/EN-US.xaml";
            //lh.LoadLanguageFile(s);
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()//此处【0】是因为资源中语言放在第1个。
            {
                Source = new Uri(languageDictPath, UriKind.RelativeOrAbsolute)
            };
            SolidColorBrush brush1 = new SolidColorBrush(Colors.White);
            SolidColorBrush brush2 = new SolidColorBrush(Colors.Black);
            ENLable.Foreground = brush2;
            CHLable.Foreground = brush1;
        }
        private void ChineseButtonClick(object sender, System.EventArgs e)
        {
            string languageDictPath = "pack://application:,,,/ImageCFP;component/Skins/ZH-CN.xaml";
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()//此处【1】是因为资源中语言放在第1个。
            {
                Source = new Uri(languageDictPath, UriKind.RelativeOrAbsolute)
            };
            SolidColorBrush brush1 = new SolidColorBrush(Colors.White);
            SolidColorBrush brush2 = new SolidColorBrush(Colors.Black);
            ENLable.Foreground = brush1;
            CHLable.Foreground = brush2;
        }
    }
}