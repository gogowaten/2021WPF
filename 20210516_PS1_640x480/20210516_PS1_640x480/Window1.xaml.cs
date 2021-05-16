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
using System.Windows.Shapes;

namespace _20210516_PS1_640x480
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class Window1 : Window
    {
        MainWindow MyMain;
        public Window1(MainWindow main)
        {
            InitializeComponent();
            MyMain = main;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            string[] datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            string paths = datas[0];

            if (System.IO.File.GetAttributes(paths).HasFlag(System.IO.FileAttributes.Directory))
            {
                MyMain.MyTextBlockDir.Text = paths;
            }
            else
            {
                MyMain.MyTextBlockDir.Text = System.IO.Path.GetDirectoryName(paths);
            }
            this.Close();
        }
    }
}
