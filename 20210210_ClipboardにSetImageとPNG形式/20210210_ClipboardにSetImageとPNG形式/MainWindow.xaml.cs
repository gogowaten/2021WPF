using System.Windows;
using System.Windows.Media.Imaging;

namespace _20210210_ClipboardにSetImageとPNG形式
{
    public partial class MainWindow : Window
    {
        BitmapSource MyBitmapSource;

        public MainWindow()
        {
            InitializeComponent();

            //アプリに埋め込んだ画像ファイルを取り出す
            string name = "_20210210_ClipboardにSetImageとPNG形式.不透明と半透明.png";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream != null) MyBitmapSource = BitmapFrame.Create(stream);

            MyImage.Source = MyBitmapSource;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClipboadSetImageWithPng(MyBitmapSource);
        }

        //        クリップボードに複数の形式のデータをコピーする - .NET Tips(VB.NET, C#...)
        //https://dobon.net/vb/dotnet/system/clipboardmultidata.html

        /// <summary>
        /// BitmapSourceをPNG形式に変換したものと、そのままの形式の両方をクリップボードにコピーする
        /// </summary>
        /// <param name="source"></param>
        private void ClipboadSetImageWithPng(BitmapSource source)
        {
            //DataObjectに入れたいデータを入れて、それをクリップボードにセットする
            DataObject data = new();

            //BitmapSource形式そのままでセット
            data.SetData(typeof(BitmapSource), source);

            //PNG形式にエンコードしたものをMemoryStreamして、それをセット
            //画像をPNGにエンコード
            PngBitmapEncoder pngEnc = new();
            pngEnc.Frames.Add(BitmapFrame.Create(source));
            //エンコードした画像をMemoryStreamにSava
            using var ms = new System.IO.MemoryStream();
            pngEnc.Save(ms);
            data.SetData("PNG", ms);

            //クリップボードにセット
            Clipboard.SetDataObject(data, true);

        }

    }
}
