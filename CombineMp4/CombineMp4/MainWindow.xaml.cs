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


//連番mp4ファイルを結合する
//条件
//  Eドライブ直下にffmpegがある
//  mp4はすべて同じ設定でエンコードされている

//Eドライブ直下にあるffmpegを使って
//ファイルをドロップされたらリストボックスに表示
//リストのテキストファイルを作成保存
//テキストファイルの作成場所はアプリの実行ファイルと同じ場所
//ffmpegを起動してコマンドを投げて実行

namespace CombineMp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string MyListFilePath;
        private string MyLastPath;

        public MainWindow()
        {
            InitializeComponent();

            MyListFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "myList.txt");
        }

        private void MyButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (MyListBox.ItemsSource == null) return;

            SaveListFile();

            ProcessStartInfo pInfo = new(@"E:\ffmpeg.exe");
            pInfo.CreateNoWindow = false;
            pInfo.UseShellExecute = true;
            pInfo.Arguments = $"-f concat -safe 0 -i {MyListFilePath} -c copy {MyLastPath}";

            Process proc = Process.Start(pInfo);
        }

        //リストボックスの一覧からリストファイル作成保存
        private void SaveListFile()
        {

            using (System.IO.StreamWriter sw = new(MyListFilePath, false))//BOMなし            
            {
                foreach (object item in MyListBox.Items)
                {
                    string str = AddString(item.ToString());
                    sw.WriteLine(str);
                }
            }

            //ffmpegの書式に変換
            static string AddString(string str)
            {
                return "file " + "'" + str + "'";
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false)
            {
                return;
            }

            string[] datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> paths = datas.ToList();
            //拡張子がmp4のファイルリスト作成
            paths = paths.Where(x => x.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)).ToList();
            if (paths.Count == 0)
            {
                MyListBox.ItemsSource = null;
                MyLastPath = "";
                return;
            }

            paths.Sort();
            MyListBox.ItemsSource = paths;

            MyLastPath = System.IO.Path.GetDirectoryName(paths[0]);
            MyLastPath = System.IO.Path.Combine(MyLastPath, "combine.mp4");

        }


    }
}
