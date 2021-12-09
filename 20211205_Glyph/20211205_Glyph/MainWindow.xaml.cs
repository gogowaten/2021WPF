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

            Test2();


        }
        private void Test2()
        {
            //Glyph系クラス
            GlyphRun glyphRun;
            GlyphRunDrawing glyphRunDrawing;
            Glyphs glyphs;
            GlyphTypeface glyphTypeface;

            glyphRun. = new();
            //GlyphRun クラス (System.Windows.Media) | Microsoft Docs
            //https://docs.microsoft.com/ja-jp/dotnet/api/system.windows.media.glyphrun?view=net-6.0
            //  同じ描画スタイルが設定され、サイズ、フォント、およびフォントの書体が同じである一連のグリフを表します。
            //GlyphRunDrawingと組み合わせて使う

            //プロパティ
            //AdvanceWidths
            //グリフ インデックスに対応するアドバンス幅を表す Double 値の一覧を取得または設定します。

            //BaselineOrigin    Point
            //GlyphRun のベースライン原点を取得または設定します。

            //BidiLevel     Int32
            //GlyphRun の双方向の入れ子レベルを取得または設定します。

            //CaretStops    bool
            //GlyphRun を表す Unicode で UTF16 コード ポイント毎にキャレット ストップがあるかどうかを決定する Boolean 値の一覧を取得または設定します。

            //Characters    List<Char>
            //GlyphRun の Unicode を表す UTF16 コード ポイントの一覧を取得または設定します。

            //ClusterMap    List<Uint16>
            //GlyphRun の文字をグリフ インデックスにマップする UInt16 値の一覧を取得または設定します。

            //DeviceFontName    string
            //GlyphRun が最適化される対象の、デバイス固有のフォントを取得または設定します。

            //FontRenderingEmSize   double
            //GlyphRun のレンダリングに使用する全角サイズを取得または設定します。

            //GlyphIndices      list<Uint16>
            //描画物理フォントのグリフ インデックスを表す UInt16 値の配列を取得または設定します。

            //GlyphOffsets      List<point>
            //GlyphRun のグリフのオフセットを表す Point 値の配列を取得または設定します。

            //GlyphTypeface     GlyphTypeface
            //GlyphTypeface の GlyphRun を取得または設定します。

            //IsHitTestable     bool
            //GlyphRun 内に有効なキャレット文字ヒットがあるかどうかを示す値を取得します。

            //IsSideways        bool
            //グリフを回転するかどうかを示す値を取得または設定します。

            //Language          XmlLanguage
            //XmlLanguage の GlyphRun を取得または設定します。

            //PixelsPerDip      Single
            //テキストを表示する PixelsPerDip を取得または設定します。


            //メソッド
            //BuildGeometry
            //  GlyphRunのジオメトリを取得します。
            //ComputeAlignmentBox   Rect
            //  GlyphRunの配置ボックスを取得します。
            //ComputeInkBoundingBox Rect
            //  GlyphRunのインク境界ボックスを取得します。
            //GetCaretCharacterHitFromDistance(Double, Boolean) CharacterHitグリフラン内でヒットした文字に関する情報を表します。
            //  GlyphRunのキャレットの文字ヒットを表すCharacterHit値を取得します。
            //GetDistanceFromCaretCharacterHit(CharacterHit) double
            //  GlyphRunの前縁から、指定された文字ヒットを含むキャレットストップの前縁または後縁までのオフセットを取得します。
            //GetNextCaretCharacterHit(CharacterHit) CharacterHit
            //  GlyphRunで論理方向にヒットした次の有効なキャレット文字を取得します。
            //GetPreviousCaretCharacterHit(CharacterHit) CharacterHit
            //  GlyphRunで論理方向にヒットした前の有効なキャレット文字を取得します。

        }

        private void Test1()
        {

            GlyphRun glyphRun = new(96);
            GlyphRunDrawing glyphRunDrawing = new(Brushes.Red, glyphRun);
            GlyphTypeface glyphTypeface = new(new Uri(@"C:\Windows\Fonts\meiryo.ttc"), StyleSimulations.BoldSimulation);
            Glyphs glyphs = new();
            glyphs.FontUri = new Uri(@"C:\Windows\Fonts\meiryo.ttc");
            string indices = glyphs.Indices;

            InputScope scope = glyphs.InputScope;
            //GlyphRun grun = glyphs.ToGlyphRun();

            var neko = glyphs.DeviceFontName;






            //GlyphRun gr = new(new GlyphTypeface(uris[53]),
            //    0,//bidiLevel
            //      false,//isSideways
            //      50,//renderingEmSize
            //    96,//pixelsPerDip
            //    new ushort[] { 42, 43 },//glyphindices
            //    new Point(0, 30),//baselineOrigin
            //    new double[] { 8, 70 },//advanceWidths
            //    new Point[] { new Point(00, -30), new Point(0, -30) }, //glyphOffsets,
            //    null, null, null, null, null);
            //GlyphRunDrawing glyphRunDrawing = new(Brushes.Red, gr);
            //DrawingVisual dv = new();
            //using (var dc = dv.RenderOpen())
            //{
            //    dc.DrawGlyphRun(Brushes.Green, gr);
            //}
            //RenderTargetBitmap renderTargetBitmap = new(300, 100, 96, 96, PixelFormats.Pbgra32);
            //renderTargetBitmap.Render(dv);



            //GlyphTypeface glyphTypeface = new(new Uri(@"C:\Windows\Fonts\meiryo.ttc"));
            //string str = "ゆっくりしていってね！！！";
            //List<ushort> gIndex = new();
            //List<double> gWidth = new();
            //List<Point> gPoints = new();
            //double totalWidth = 0;
            //for (int i = 0; i < str.Length; i++)
            //{
            //    ushort index = glyphTypeface.CharacterToGlyphMap[str[i]];
            //    gIndex.Add(index);
            //    double width = glyphTypeface.AdvanceWidths[index];
            //    gWidth.Add(width);

            //    gPoints.Add(new(totalWidth, -30));
            //    totalWidth += width;
            //}
            //GlyphRun glyphRun = new(glyphTypeface, 0, false, 30, 96,
            //    gIndex, new Point(0, 50), gWidth,
            //    gPoints, null, null, null, null, null);
            //dv = new();
            //using (var dc = dv.RenderOpen())
            //{
            //    dc.DrawGlyphRun(Brushes.MediumBlue, glyphRun);
            //}
            //RenderTargetBitmap renderTargetBitmap2 = new(300, 100, 96, 96, PixelFormats.Pbgra32);
            //renderTargetBitmap2.Render(dv);


        }
    }
}
