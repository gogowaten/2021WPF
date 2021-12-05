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
//SystemFontFamiliesを表示する part1 : Win32 & wpf メモ
//http://blog.livedoor.jp/oans/archives/54753149.html

namespace _20211205_Glyph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //GlyphRun glyphRun = new(96);
            //GlyphRunDrawing glyphRunDrawing = new(Brushes.Red, glyphRun);
            //GlyphTypeface glyphTypeface = new(new Uri(@"C:\Windows\Fonts\meiryo.ttc"), StyleSimulations.BoldSimulation);
            //Glyphs glyphs = new();
            //glyphs.FontUri = new Uri(@"C:\Windows\Fonts\meiryo.ttc");
            //string indices = glyphs.Indices;

            //InputScope scope = glyphs.InputScope;
            ////GlyphRun grun = glyphs.ToGlyphRun();

            //var neko = glyphs.DeviceFontName;
            Typeface typeface = new(@"C:\Windows\Fonts\meiryo.ttc");
            LanguageSpecificStringDictionary face = typeface.FaceNames;
            FontFamily ff = typeface.FontFamily;
            var fUri = ff.Source;

            //
            //foreach (var item in Fonts.SystemFontFamilies)
            //{
            //    var neko = item.Source;
            //    var uma = item.FamilyNames;
            //    var map = item.FamilyMaps;
            //    var iFace = item.FamilyTypefaces;
            //    var faces = item.GetTypefaces();
            //}
            //ICollection<FontFamily> systemFonts = Fonts.SystemFontFamilies;
            //ICollection<Typeface>[] typefaces = systemFonts.Select(a => a.GetTypefaces()).ToArray();
            //IEnumerable<FontFamily[]> sysff = typefaces.Select(a => a.Select(a => a.FontFamily).ToArray());
            //IEnumerable<FontFamily>[] fff = systemFonts.Select(a => a.GetTypefaces().Select(a => a.FontFamily)).ToArray();
            //IEnumerable<string>[] sysffs = typefaces.Select(a => a.Select(a => a.FontFamily).Select(a => a.Source)).ToArray();
            //var inu = systemFonts.Select(a => a.GetTypefaces().Select(a => a.FontFamily).Select(a => a.Source).ToList());


            List<Uri> uris = new();
            GlyphTypeface glyphTypeface1 = null;
            Dictionary<string, Uri> myDict = new();
            foreach (var aaa in Fonts.SystemFontFamilies.Select(a => a.GetTypefaces()))
            {
                _ = aaa.FirstOrDefault(a => a.TryGetGlyphTypeface(out glyphTypeface1));
                //_ = aaa.Select(a => a.TryGetGlyphTypeface(out glyphTypeface1));//空っぽ
                if (glyphTypeface1 != null)
                {
                    myDict.TryAdd(System.IO.Path.GetFileNameWithoutExtension(glyphTypeface1.FontUri.AbsolutePath), glyphTypeface1.FontUri);
                    uris.Add(glyphTypeface1.FontUri);
                    glyphTypeface1 = null;
                }
            }
            var groupe = uris.GroupBy(a => System.IO.Path.GetFileNameWithoutExtension(a.AbsolutePath));
            //var fdict = uris.ToDictionary(a => System.IO.Path.GetFileNameWithoutExtension(a.AbsolutePath));
            //SortedDictionary<string, Uri> sfdict = new(fdict);
            SortedDictionary<string, Uri> fontDict = new();
            foreach (var item in uris)
            {
                fontDict.Add(System.IO.Path.GetFileNameWithoutExtension(item.AbsolutePath), item);
            }
            


            GlyphRun gr = new(new GlyphTypeface(uris[53]),
                0,//bidiLevel
                  false,//isSideways
                  50,//renderingEmSize
                96,//pixelsPerDip
                new ushort[] { 42, 43 },//glyphindices
                new Point(0, 30),//baselineOrigin
                new double[] { 8, 70 },//advanceWidths
                new Point[] { new Point(00, -30), new Point(0, -30) }, //glyphOffsets,
                null, null, null, null, null);
            GlyphRunDrawing glyphRunDrawing = new(Brushes.Red, gr);
            DrawingVisual dv = new();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawGlyphRun(Brushes.Green, gr);
            }
            RenderTargetBitmap renderTargetBitmap = new(300, 100, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(dv);



            GlyphTypeface glyphTypeface = new(new Uri(@"C:\Windows\Fonts\meiryo.ttc"));
            string str = "ゆっくりしていってね！！！";
            List<ushort> gIndex = new();
            List<double> gWidth = new();
            List<Point> gPoints = new();
            double totalWidth = 0;
            for (int i = 0; i < str.Length; i++)
            {
                ushort index = glyphTypeface.CharacterToGlyphMap[str[i]];
                gIndex.Add(index);
                double width = glyphTypeface.AdvanceWidths[index];
                gWidth.Add(width);

                gPoints.Add(new(totalWidth, -30));
                totalWidth += width;
            }
            GlyphRun glyphRun = new(glyphTypeface, 0, false, 30, 96,
                gIndex, new Point(0, 50), gWidth,
                gPoints, null, null, null, null, null);
            dv = new();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawGlyphRun(Brushes.MediumBlue, glyphRun);
            }
            RenderTargetBitmap renderTargetBitmap2 = new(300, 100, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap2.Render(dv);



        }


    }
}
