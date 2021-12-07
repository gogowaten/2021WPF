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
using System.Globalization;

//SystemFontFamiliesを表示する part1 : Win32 & wpf メモ
//http://blog.livedoor.jp/oans/archives/54753149.html

namespace _20211206_FontList作成
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<FontData> FontDatas = new();
        public MainWindow()
        {
            InitializeComponent();

         

            Uri uri = new("file:///C:/WINDOWS/FONTS/MEIRYO.TTC#2");
            //Uri uri = new(@"C:/WINDOWS/FONTS/MEIRYO.TTC#2");//これは後で使うときにエラーになる
            //Uri uri = new(@"file:///C:/WINDOWS/FONTS/MEIRYO.TTC#2");
            //Uri uri = new(@"C:\Windows\Fonts\meiryo.ttc");
            Glyphs glyphs = new();
            glyphs.FontUri = uri;
            GlyphTypeface glyphTypeface = new(uri);
            var fns = glyphTypeface.FamilyNames;
            var facens = glyphTypeface.FaceNames;
            MyTextBlock.FontFamily = new(uri, "Meiryo Ui");

            //以下は同じ効果
            //this.FontFamily = new(new Uri(@"C:/Windows/Fonts/Meiryo.ttc"), "Meiryo UI");            
            //this.FontFamily = new("Meiryo UI");

            ////インストールされていないフォントは使えないみたい
            ////以下はエラーにはならないけど反映されず初期値のFontFamilyが使われる
            //uri = new(@"I:\4D080K4\hdd0\Program Files\Pinnacle\Pinnacle Express\Fonts\Pretext_.ttf");
            //MyTextBlock.FontFamily = new(uri, "PRETEXT");



            //今のPCで使っている言語(日本語)取得
            var language =
                System.Windows.Markup.XmlLanguage.GetLanguage(
                CultureInfo.CurrentCulture.IetfLanguageTag);
            var culture = CultureInfo.CurrentCulture;

            ////Fonts.SystemFontFamiliesの
            //foreach (var item in Fonts.SystemFontFamilies)
            //{
            //    var neko= item.FamilyTypefaces;
            //    var inu = item.GetTypefaces();

            //    ICollection<Typeface> types = item.GetTypefaces();
            //    foreach (var type in types)
            //    {
            //        type.TryGetGlyphTypeface(out GlyphTypeface gType);
            //        var uri = gType.FontUri;
            //        FontDatas.Add(new()
            //        {
            //            Uri = uri,
            //            LocalPath = uri.LocalPath,
            //            FamilyName = gType.FamilyNames[culture],
            //            TypeName = gType.FaceNames[culture],
            //            FontWeight = type.Weight,
            //            FontStyle = type.Style
            //        });

            //    }

            //}


            //#region インストールされているフォントのDictionary作成、Key：日本語名、Value：FontFamiry

            ////フォント全部
            //ICollection<FontFamily> sysFonts = Fonts.SystemFontFamilies;

            ////test、FontFamiryのリスト作成
            //var lang4 = sysFonts
            //    .Select(a => a.FamilyNames).ToArray();
            ////test、フォント名で日本語名があればそれを取得、なければnull
            ////FirstOrDefaultを使って日本語名取得、なければnullを返す
            //var lang5 = sysFonts
            //    .Select(a => a.FamilyNames.FirstOrDefault(b => b.Key == language).Value).ToArray();
            ////test、フォント名で日本語名があればそれを取得、なければ初期名取得
            //var lang6 = sysFonts
            //    .Select(a => a.FamilyNames.FirstOrDefault(b => b.Key == language).Value ?? a.Source).ToArray();
            ////Dictionary作成、Keyは日本語名、なければ初期名、ValueはFontFamiry
            //var lang7 = sysFonts
            //    .ToDictionary(a => a.FamilyNames.FirstOrDefault(b => b.Key == language).Value ?? a.Source);
            ////名前でソートしたいのでDortedDictionaryを使って作成してるけど、名前に重複があるとエラーになるから対策したほうがいい
            //SortedDictionary<string, FontFamily> keyValuePairs7 = new(lang7);
            ////↑と同じ結果
            //var lang8 = sysFonts
            //    .ToDictionary(a => a.FamilyNames.FirstOrDefault(b => b.Key == language).Value ?? a.Source, v => v);
            //SortedDictionary<string, FontFamily> keyValuePairs8 = new(lang8);

            //#endregion インストールされているフォントのDictionary作成、Key：日本語名、Value：FontFamiry


            //var tFaces1 = keyValuePairs7
            //    .Select(a => a.Value.GetTypefaces()).ToArray();
            //var tFaces2 = keyValuePairs7
            //    .Select(a => a.Value.GetTypefaces()
            //    .Select(a => a.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface2))).ToArray();
            //GlyphTypeface gtf = null;
            //Dictionary<string, Uri> myDict2 = new();
            //Dictionary<string, Typeface> myDict3 = new();
            //var cul = CultureInfo.CurrentCulture;
            //foreach (var item in keyValuePairs7.Select(a => a.Value.GetTypefaces()))
            //{
            //    item.FirstOrDefault(a => a.TryGetGlyphTypeface(out gtf));
            //    if (gtf != null)
            //    {
            //        string key = System.IO.Path.GetFileNameWithoutExtension(gtf.FontUri.LocalPath);
            //        var str = gtf.FaceNames.FirstOrDefault(a => a.Key == cul).Value;
            //        var sst = gtf.FaceNames.Select(a => a).FirstOrDefault(a => a.Key.Name == cul.Name);
            //        foreach (var names in gtf.FaceNames)
            //        {
            //            if (names.Key.Name == cul.Name)
            //            {
            //                var nekko = names.Value;
            //            }
            //        }
            //        if (myDict2.TryAdd(
            //            System.IO.Path.GetFileNameWithoutExtension(gtf.FontUri.LocalPath),
            //            gtf.FontUri) == false)
            //        {
            //            myDict3.Add(
            //                item.FirstOrDefault(a => a.FaceNames != null).FontFamily.Source,
            //                item.FirstOrDefault(a => a.FontFamily != null));
            //        };
            //        gtf = null;
            //    }
            //    else
            //    {
            //        myDict3.Add(
            //            item.FirstOrDefault(a => a.FaceNames != null).FontFamily.Source,
            //            item.FirstOrDefault(a => a.FontFamily != null));
            //    }
            //}

            //List<Uri> uris = new();
            //GlyphTypeface glyphTypeface1 = null;
            //Dictionary<string, Uri> myDict = new();
            //List<FontFamily> ffList = new();
            //foreach (var aaa in Fonts.SystemFontFamilies.Select(a => a.GetTypefaces()))
            //{
            //    _ = aaa.FirstOrDefault(a => a.TryGetGlyphTypeface(out glyphTypeface1));
            //    //_ = aaa.Select(a => a.TryGetGlyphTypeface(out glyphTypeface1));//空っぽ
            //    if (glyphTypeface1 != null)
            //    {
            //        myDict.TryAdd(System.IO.Path.GetFileNameWithoutExtension(glyphTypeface1.FontUri.LocalPath), glyphTypeface1.FontUri);
            //        uris.Add(glyphTypeface1.FontUri);
            //        var neko = aaa.Select(a => a.FontFamily).ToList();
            //        var inu = aaa.Select(a => a.FontFamily.FamilyNames.FirstOrDefault(b => b.Key == language).Value);
            //        var inu2 = aaa.Select(a => a.FontFamily.FamilyNames.FirstOrDefault(b => b.Key == language).Value).ToArray();
            //        var inu3 = aaa.Select(a => a.FontFamily.FamilyNames.FirstOrDefault(b => b.Key == language).Value).ToArray().FirstOrDefault(a => a != null);
            //        var inu4 = aaa.FirstOrDefault(a => a.FontFamily.FamilyNames != null);
            //        var inu5 = aaa.FirstOrDefault(a => a.FontFamily != null);
            //        glyphTypeface1 = null;
            //    }
            //}
            //var group = uris.GroupBy(a => System.IO.Path.GetFileNameWithoutExtension(a.LocalPath)).ToArray();
            ////
            //Dictionary<string, Uri> groupFirst = uris
            //    .GroupBy(a => System.IO.Path.GetFileNameWithoutExtension(a.LocalPath))
            //    .ToDictionary(a => a.Key, a => a.First());
            //SortedDictionary<string, Uri> fontDict = new(groupFirst);
            ////
            //Dictionary<string, Uri> groupFirst2 = uris
            //    .GroupBy(a => System.IO.Path.GetFileNameWithoutExtension(a.LocalPath))
            //    .ToDictionary(a => a.Key, a => a.First());



            //var f17 = uris[17];
            //var f18 = uris[18];
            //GlyphTypeface glyphTypeface = new(uris[17]);
            //string str = "aゆっくり";
            //double emSize = 50;
            //List<ushort> indices = new();
            //List<double> widths = new();
            //List<Point> points = new();
            //List<double> heights = new();
            //double totalH = 0;
            //double totalW = 0;
            //for (int i = 0; i < str.Length; i++)
            //{
            //    char key = str[i];
            //    //文字のインデックスが存在した場合のみ処理
            //    if (glyphTypeface.CharacterToGlyphMap.ContainsKey(key))
            //    {
            //        ushort index = glyphTypeface.CharacterToGlyphMap[key];
            //        indices.Add(index);
            //        double w = glyphTypeface.AdvanceWidths[index] * emSize;
            //        widths.Add(w);
            //        points.Add(new(30, -emSize));
            //        totalW += w;
            //        double h = glyphTypeface.AdvanceHeights[index];
            //        heights.Add(h);

            //    };

            //}
            //GlyphRun glyphRun = new(glyphTypeface,
            //    0,
            //    false,
            //    50,
            //    96,
            //    indices,
            //    new Point(),
            //    widths,
            //    points,
            //    null, null, null, null, language);

            //DrawingVisual dv = new();
            //using (var dc = dv.RenderOpen())
            //{
            //    dc.DrawGlyphRun(Brushes.MediumAquamarine, glyphRun);
            //}
            //RenderTargetBitmap bitmap = new(500, 200, 96, 96, PixelFormats.Pbgra32);
            //bitmap.Render(dv);
            //GlyphRunDrawing glyphRunDrawing = new(Brushes.MediumAquamarine, glyphRun);
            //DrawingBrush brush = new(glyphRunDrawing);
            ////MyGrid.Background = brush;

            //Glyphs glyphs = new();
            //glyphs.FontRenderingEmSize = 50;
            //glyphs.FontUri = uris[18];
            //glyphs.Fill = Brushes.MediumBlue;
            ////glyphs.Indices = "！！";
            //glyphs.OriginX = 50;
            //glyphs.OriginY = 50;
            //glyphs.StyleSimulations = StyleSimulations.BoldItalicSimulation;
            //glyphs.UnicodeString = "abc";
            //MyGrid.Children.Add(glyphs);
        }
    }

    public class FontData
    {
        public string FamilyName { get; set; }
        public Uri Uri { get; set; }
        public string TypeName { get; set; }
        public string LocalPath { get; set; }

        public StyleSimulations StyleSimulations { get; set; }
        public FontWeight FontWeight { get; set; }
        public FontStyle FontStyle { get; set; }

        public override string ToString()
        {
            string str = FamilyName + "_" + FontWeight + "_" + FontStyle;
            return str;
            //return base.ToString();
        }
    }
}
