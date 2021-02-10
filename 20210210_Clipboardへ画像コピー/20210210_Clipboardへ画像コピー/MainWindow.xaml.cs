using System;
using System.Windows;
using System.Windows.Media.Imaging;

//アルファ値を失わずに画像のコピペできた、.NET WPFのClipboard - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/02/10/134406


namespace _20210210_Clipboardへ画像コピー
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource MyBitmapSource;
        public MainWindow()
        {
            InitializeComponent();

            //アプリに埋め込んだ画像ファイルを取り出す
            string name = "_20210210_Clipboardへ画像コピー.不透明と半透明.png";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream != null) MyBitmapSource = BitmapFrame.Create(stream);
            
            MyImage.Source = MyBitmapSource;
        }        

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //普通にセット
            Clipboard.SetImage(MyBitmapSource);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //PNG形式でセット
            ClipboadToPngImage(MyBitmapSource);
        }
        private void ClipboadToPngImage(BitmapSource source)
        {
            //画像をPNGにエンコード
            PngBitmapEncoder pngEnc = new();
            pngEnc.Frames.Add(BitmapFrame.Create(source));
            //エンコードした画像をMemoryStreamにSava
            using var ms = new System.IO.MemoryStream();
            pngEnc.Save(ms);
            //MemoryStreamをクリップボードにコピー
            Clipboard.SetData("PNG", ms);
        }
    }
}
