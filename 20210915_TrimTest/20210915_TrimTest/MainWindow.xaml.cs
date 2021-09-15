using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Runtime.Serialization;//AppDataで使用
using System.Xml;//シリアライズで使用


namespace _20210915_TrimTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppData MyAppData;//Bindingするアプリのデータ
        private const string APP_DATA_FILE_NAME = "myData.xml";
        private string AppDir;//アプリの実行ファイルのある場所


        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
        }
        private void MyInitialize()
        {
            AppDir = Environment.CurrentDirectory;

        }

        private void InitializeAppData()
        {
            //設定ファイルが存在すれば読み込んで適用、なければ初期化して適用
            string configPath = AppDir + "\\" + APP_DATA_FILE_NAME;
            if (System.IO.File.Exists(configPath))
            {
                MyAppData = LoadConfig(configPath);
            }

            else
            {
                MyAppData = new AppData();
            }
            this.DataContext = MyAppData;
        }

        #region アプリの設定ファイルの読み書き        
        //アプリの設定保存
        private bool SaveConfig(string path)
        {
            var serializer = new DataContractSerializer(typeof(AppData));
            XmlWriterSettings settings = new();
            settings.Encoding = new UTF8Encoding();
            try
            {
                using (var xw = XmlWriter.Create(path, settings))
                {
                    serializer.WriteObject(xw, MyAppData);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"アプリの設定保存できなかった\n{ex.Message}",
                    $"{System.Reflection.Assembly.GetExecutingAssembly()}");
                return false;
            }
        }
        private void SaveAppDataGetPath()
        {
            Microsoft.Win32.SaveFileDialog dialog = new();
            dialog.Filter = "(xml)|*.xml";
            if (dialog.ShowDialog() == true)
            {
                SaveConfig(dialog.FileName);
            }
        }


        //アプリの設定読み込み
        private void LoadAppDataGetPath()
        {
            Microsoft.Win32.OpenFileDialog dialog = new();
            dialog.Filter = "(xml)|*.xml";
            if (dialog.ShowDialog() == true)
            {
                AppData config = LoadConfig(dialog.FileName);
                if (config == null) return;
                MyAppData = config;
                this.DataContext = MyAppData;
            }
        }
        private AppData LoadConfig(string path)
        {
            var serealizer = new DataContractSerializer(typeof(AppData));
            try
            {
                using XmlReader xr = XmlReader.Create(path);
                return (AppData)serealizer.ReadObject(xr);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"読み込みできなかった\n{ex.Message}",
                    $"{System.Reflection.Assembly.GetExecutingAssembly().GetName()}");
                return null;
            }
        }
        #endregion アプリの設定ファイルの読み書き
    }


    [DataContract]
    public class AppData
    {
        //切り抜き範囲
        [DataMember] public int TrimTop { get; set; } = 0;
        [DataMember] public int TrimLeft { get; set; } = 0;
        [DataMember] public int TrimWidth { get; set; } = 100;
        [DataMember] public int TrimHeight { get; set; } = 100;

        //アプリのウィンドウ
        [DataMember] public double AppTop { get; set; } = 0;
        [DataMember] public double AppLeft { get; set; } = 0;
        [DataMember] public double AppWidth { get; set; } = 614;
        [DataMember] public double AppHeight { get; set; } = 520;

        //jpeg品質
        [DataMember] public int JpegQuality { get; set; } = 90;



    }
}
