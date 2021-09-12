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

using System.Diagnostics;

namespace _20210912_別のアプリを起動
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string MyListFileName = "myList.txt";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            //Process p = Process.Start("notepad.exe", @"D:\動画D\新しいテキスト ドキュメント.txt");


            //ProcessStartInfo pInfo = new("notepad.exe");
            //pInfo.Arguments = @"D:\動画D\新しいテキスト ドキュメント.txt";
            //Process.Start(pInfo);

            //string arg = @"-i ""D:\動画D\Bks029.avi"" -vcodec cpoy -acodec copy E:\myff.mp4";
            //string inF = @"D:\動画D\Bks029.avi";
            //string outF = @"E:/myff.mp4";

            //ProcessStartInfo pInfo = new(@"E:\ffmpeg.exe");
            //pInfo.Arguments = @"";
            //pInfo.CreateNoWindow = false;
            //pInfo.UseShellExecute = false;
            //pInfo.Arguments = $"-i {inF} -vcodec copy -acodec copy {outF}";
            ////pInfo.RedirectStandardOutput = true;
            //Process proc = Process.Start(pInfo);


            MyExe();



        }
        private void MyExe()
        {
            string listPath = @"F:\PS2\bkzumi\";
            listPath += MyListFileName;

            ProcessStartInfo pInfo = new(@"E:\ffmpeg.exe");

            pInfo.CreateNoWindow = false;
            pInfo.UseShellExecute = true;
            string outF = @"E:/myffvs.mp4";
            pInfo.Arguments = $"-f concat -safe 0 -i {listPath} -c copy {outF}";           
            //実行
            var proc = Process.Start(pInfo);
            //pInfo.RedirectStandardOutput = true;
            //pInfo.WindowStyle = ProcessWindowStyle.Normal;
            //proc.ErrorDataReceived += Proc_ErrorDataReceived;
            //proc.Disposed += Proc_Disposed;
            //proc.Exited += Proc_Exited;
        }

        private void Proc_Exited(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Proc_Disposed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            string[] datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> paths = datas.ToList();
            //拡張子がmp4のファイルリスト作成
            paths = paths.Where(x => x.EndsWith(".mp4")).ToList();
            paths.Sort();
            MyListBox.ItemsSource = paths;

            string listPath = @"F:\PS2\bkzumi\";
            listPath += MyListFileName;
            using (System.IO.StreamWriter sw = new(listPath, false))//BOMなし
            //using (System.IO.StreamWriter sw = new(listPath, false, System.Text.Encoding.UTF8))//BOMありtxtはffmpgeでエラーになる
            {
                foreach (var item in MyListBox.Items)
                {
                    string str = AddString(item.ToString());
                    sw.WriteLine(str);
                }
            }

        }

        private string AddString(string str)
        {
            return "file " + "'" + str + "'";
        }
    }
}
