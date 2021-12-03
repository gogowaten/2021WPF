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

namespace _20211203_Glyph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SortedDictionary<string, string> MyFonts = new();

            var dirF = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            //フォントフォルダのフォントファイルリストUri取得
            //string[] uris = System.IO.Directory.GetFiles(dirF, "*.ttf");
            IEnumerable<Uri> uris =
                System.IO.Directory.GetFiles(dirF, "*.ttf").Concat
                (System.IO.Directory.GetFiles(dirF, "*.ttc")).Concat
                (System.IO.Directory.GetFiles(dirF, "*.otf")).Select
                (x => new Uri(x));
            //IEnumerable<string> ffs =
            //    System.IO.Directory.GetFiles(dirF, "*.ttf").Concat
            //    (System.IO.Directory.GetFiles(dirF, "*.ttc")).Concat
            //    (System.IO.Directory.GetFiles(dirF, "*.otf"));


            //フォントフォルダのファイルリスト取得、これだと余計なファイルも入る
            var ffs2 = System.IO.Directory.GetFiles(dirF, "*", System.IO.SearchOption.AllDirectories);

            //ユーザーのフォントフォルダのファイルリスト取得
            var userDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Windows\Fonts");
            string[] userFonts;
            //List<string> userFonts = new();
            if (System.IO.Directory.Exists(userDir))
            {
                userFonts = System.IO.Directory.GetFiles(userDir, "*");
            }

            var ggg = GetFontUris();




            ICollection<FontFamily> fs = Fonts.SystemFontFamilies;



            foreach (var item in fs)
            {
                var ftf = item.FamilyTypefaces;
                var type = item.GetType();
                var tfs = item.GetTypefaces();
                var fm = item.FamilyMaps;
                var bu = item.BaseUri;
                LanguageSpecificStringDictionary fn = item.FamilyNames;
                var so = item.Source;
                foreach (var name in fn)
                {
                    string fName = name.Value;
                    string dd = System.IO.Path.Combine(dirF, fName);
                    dd += ".otf";
                    var ex = System.IO.File.Exists(dd);
                }
            }

            string str = "ゆっくりしていってね！！！";
            Glyphs glyphs = new()
            {
                FontUri = new Uri("Meiryo UI"),
                FontRenderingEmSize = 50,
                UnicodeString = str,
                Fill = Brushes.MediumAquamarine
            };
            MyGrid.Children.Add(glyphs);

        }

        /// <summary>
        /// フォントファイルのUriリスト作成
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Uri> GetFontUris()
        {
            //systemのフォントフォルダpath
            string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            //ユーザーのフォントフォルダpath
            string userPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Windows\Fonts");

            IEnumerable<Uri> uris = MakeList(systemPath).Concat(MakeList(userPath));

            //指定フォルダのフォントファイルリスト作成、対象はttf,ttc,otf
            IEnumerable<Uri> MakeList(string folderPath)
            {
                IEnumerable<Uri> temp =
                    System.IO.Directory.GetFiles(folderPath, "*.ttf").Concat
                    (System.IO.Directory.GetFiles(folderPath, "*.ttc")).Concat
                    (System.IO.Directory.GetFiles(folderPath, "*.otf")).Select
                    (x => new Uri(x));
                return temp;
            }
            return uris;
        }
    }
}
