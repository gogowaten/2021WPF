using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

//参照したところ
//プログラミングな日々: WPF用縦書きテキストブロック Tategaki ver .2.1.0
//https://days-of-programming.blogspot.com/2015/01/wpf-tategaki-ver210.html

namespace _20211203_フォントファイル一覧取得
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var fontFileUris = GetFontUris();
            var fontFileList = fontFileUris.ToArray();
        }

        /// <summary>
        /// インストールされている全てのフォントファイルのUriリスト作成
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Uri> GetFontUris()
        {
            //systemのフォントフォルダから一覧
            IEnumerable<Uri> result = MakeList(
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts));

            //ユーザーのフォントフォルダが存在する場合は、
            //それも取得してConcatで付け加える
            string userPath = System.IO.Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Windows\Fonts");
            if (System.IO.Directory.Exists(userPath))
            {
                result = result.Concat(MakeList(userPath));
            }

            //指定フォルダのフォントファイルすべてをUriにして取得
            static IEnumerable<Uri> MakeList(string folderPath)
            {
                IEnumerable<Uri> result =
                    System.IO.Directory.GetFiles(folderPath, "*.ttf").Concat
                    (System.IO.Directory.GetFiles(folderPath, "*.ttc")).Concat
                    (System.IO.Directory.GetFiles(folderPath, "*.otf")).Select
                    (x => new Uri(x));
                return result;
            }
            return result;
        }

    }
}
