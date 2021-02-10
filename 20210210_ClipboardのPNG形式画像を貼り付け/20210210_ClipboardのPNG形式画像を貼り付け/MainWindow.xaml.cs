using System.Windows;
using System.Windows.Media.Imaging;

//アルファ値を失わずに画像のコピペできた、.NET WPFのClipboard - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/02/10/134406


namespace _20210210_ClipboardのPNG形式画像を貼り付け
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = GetImageFromClipboardWithPNG();
            if (source == null)
            {
                MessageBox.Show("クリップボードに画像はなかった");
            }
            else
            {
                MyImage.Source = source;
            }

        }

        /// <summary>
        /// クリップボードからBitmapSourceを取り出して返す、PNG(アルファ値保持)形式に対応
        /// </summary>
        /// <returns></returns>
        private BitmapSource GetImageFromClipboardWithPNG()
        {
            BitmapSource source = null;
            //クリップボードにPNG形式のデータがあったら、それを使ってBitmapFrame作成して返す
            //なければ普通にClipboardのGetImage、それでもなければnullを返す
            using var ms = (System.IO.MemoryStream)Clipboard.GetData("PNG");
            if (ms != null)
            {
                //source = BitmapFrame.Create(ms);//これだと取得できない
                source = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);                
            }
            else if (Clipboard.ContainsImage())
            {
                source = Clipboard.GetImage();
            }
            return source;
        }
    }
}
