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


namespace _20210917_Trimtest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const string APP_NAME = "Pixtrim3rd";
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
            //タイトルバーにアプリ名とバージョン表示
            var cl = Environment.GetCommandLineArgs();
            this.Title = APP_NAME + " ver " + System.Diagnostics.FileVersionInfo.GetVersionInfo(cl[0]).FileVersion;

            MyListBox.SelectionChanged += MyListBox_SelectionChanged;


            InitializeAppData();
        }

        private void MyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            MyImage.Source = MakeBitmapSourceBgra32FromFile((string)e.AddedItems[0]);
        }

        private void InitializeAppData()
        {

            //設定ファイルが存在すれば読み込んで適用、なければ初期化して適用
            string configPath = AppDir + "\\" + APP_DATA_FILE_NAME;
            if (System.IO.File.Exists(configPath))
            {
                MyAppData = LoadAppData(configPath);
            }

            else
            {
                MyAppData = new AppData();
            }
            this.DataContext = MyAppData;
        }

        #region アプリの設定ファイルの読み書き        
        //アプリの設定保存
        private bool SaveAppData(string path)
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
                SaveAppData(dialog.FileName);
            }
        }


        //アプリの設定読み込み
        private void LoadAppDataGetPath()
        {
            Microsoft.Win32.OpenFileDialog dialog = new();
            dialog.Filter = "(xml)|*.xml";
            if (dialog.ShowDialog() == true)
            {
                AppData config = LoadAppData(dialog.FileName);
                if (config == null) return;
                MyAppData = config;
                this.DataContext = MyAppData;
            }
        }
        private AppData LoadAppData(string path)
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

        #region 画像保存

        private void SaveAllImage()
        {
            //MyProgressBar.Value = 0;
            if (MyListBox.ItemsSource == null) return;

            BitmapSource source;
            Int32Rect rect = MakeTrimRect();
            List<string> list = MyListBox.ItemsSource.Cast<string>().ToList();
            double count = 0;

            foreach (string sourcePath in list)
            {
                source = MakeBitmapSourceBgra32FromFile(sourcePath);
                if (source == null) continue;
                if (ValidationTrim(source, rect) == false) continue;

                string folderPath = System.IO.Path.GetDirectoryName(sourcePath);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(sourcePath);

                fileName += "_trim.png";
                string savePath = System.IO.Path.Combine(folderPath, fileName);

                SaveTrimImage(source, savePath, rect);

                count++;


            }
        }
        private Int32Rect MakeTrimRect()
        {
            return new Int32Rect(MyAppData.TrimLeft, MyAppData.TrimTop, MyAppData.TrimWidth, MyAppData.TrimHeight);
        }
        private void SaveTrimImage(BitmapSource source, string path, Int32Rect rect)
        {
            BitmapSource trimImage = new CroppedBitmap(source, rect);
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(trimImage));
            using (System.IO.FileStream fs = new(path, System.IO.FileMode.Create))
            {
                encoder.Save(fs);
            }
        }
        private bool ValidationTrim(BitmapSource source, Int32Rect rect)
        {
            bool result = true;
            if (rect.X < 0 || rect.Y < 0) result = false;
            if (rect.X + rect.Width > source.Width || rect.Y + rect.Height > source.Height)
            {
                result = false;
            }
            return result;
        }

        #endregion 画像保存

        #region 画像読み込み

        /// <summary>
        /// 画像ファイルパスからPixelFormats.Bgra32のBitmapSource作成
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private BitmapSource MakeBitmapSourceBgra32FromFile(string filePath, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            try
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    source = BitmapFrame.Create(stream);
                    if (source.Format != PixelFormats.Bgra32)
                    {
                        source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
                    }
                    int w = source.PixelWidth;
                    int h = source.PixelHeight;
                    int stride = (w * source.Format.BitsPerPixel + 7) / 8;
                    byte[] pixels = new byte[h * stride];
                    source.CopyPixels(pixels, stride, 0);
                    source = BitmapSource.Create(w, h, dpiX, dpiY, source.Format, source.Palette, pixels, stride);
                };
            }
            catch (Exception)
            { }
            return source;
        }

        #endregion 画像読み込み


        //アプリ終了時
        private void Window_Closed(object sender, EventArgs e)
        {
            //設定保存
            _ = SaveAppData(System.IO.Path.Combine(AppDir, APP_DATA_FILE_NAME));

        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;

            string[] datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> paths = datas.ToList();
            paths.Sort();

            MyListBox.ItemsSource = paths;
            MyListBox.SelectedIndex = 0;
        }

        private void MyButtonSave_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.IsIndeterminate = true;
            SaveAllImage();
            MyProgressBar.IsIndeterminate = false;
        }

        private void MyButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var i = MyImage.Source;
        }

        private void MyButtonSimm_Click(object sender, RoutedEventArgs e)
        {
            byte[] piexls = ToGray8Array((BitmapSource)MyImage.Source);

        }
        private byte[] ToGray8Array(BitmapSource source)
        {
            var gray = new FormatConvertedBitmap(source, PixelFormats.Gray8, null, 0);
            var rect = MakeTrimRect();
            var trim = new CroppedBitmap(gray, rect);
            int stride = rect.Width;
            byte[] piexls = new byte[rect.Y * stride];
            trim.CopyPixels(piexls, stride, 0);
            return piexls;
        }
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

