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

namespace _20211212_1文字画像
{
    /// <summary>
    /// 文字の装飾
    /// FormattedTextを使う、太字と斜体の指定ができる
    /// さらにTextDecorationを使うと打ち消し線、アンダーライン、オーバーライン、ベースラインの表示ができる
    /// 画像として得るにはDrawText、またはGeometryに変換してDrawGeometry
    /// DrawTextでは文字の色とDecorationの色を別に指定できるけど、Geometryではできなさそう
    /// 逆にGeometryでは縁取りができるけど、DrawTextではできない
    /// 
    /// 範囲指定でアンダーラインなどできる？→できた、けどGeometryでは文字色と同じになるはず
    /// 縁取りの2重3重にできる？→できた
    /// 太字以上に太くできる？→できた、GeometryのGetWidenedPathGeometry()
    /// ドロップシャドウできる？できた、
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Test1("（ゆっくり）abcdefghijkl");
            Test1("a");

        }

        private void Test1(string str)
        {
            FormattedText fmText = new(
                str,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    new FontFamily("Meiryo UI"),
                    FontStyle,
                    FontWeight,
                    FontStretch),
                    100,
                Brushes.MediumAquamarine,
                96);

            TextDecoration textDecoration = new(TextDecorationLocation.Strikethrough, new Pen(Brushes.Black, 4), 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);
            TextDecorationCollection textDecorations = new(new List<TextDecoration>() { textDecoration });
            fmText.SetTextDecorations(textDecorations);

            DrawingVisual dv = new();
            int w = (int)fmText.WidthIncludingTrailingWhitespace;
            int h = (int)fmText.Height;
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Green, null, new Rect(0, 0, w, h));
                dc.DrawText(fmText, new Point(0, 0));
                Point offsetP = new(0, -((fmText.Height - fmText.Extent) / 2.0));
                //dc.DrawText(fmText, offsetP);
            }

            RenderTargetBitmap bitmap = new((int)w, (int)h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);

            Geometry geo = fmText.BuildGeometry(new(0, 0));
            dv = new();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Green, null, new Rect(0, 0, w, h));
                dc.DrawGeometry(Brushes.MediumBlue, null, geo);
            }
            bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);

            //多色縁取りその1
            Pen myPen = new(Brushes.Transparent, 2);
            myPen.LineJoin = PenLineJoin.Round;
            PathGeometry wideGeo1 = geo.GetWidenedPathGeometry(myPen);
            PathGeometry wideGeo2 = wideGeo1.GetWidenedPathGeometry(myPen);
            dv = new();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Gray, null, new Rect(0, 0, w, h));
                dc.DrawGeometry(Brushes.Black, myPen, wideGeo2);
                dc.DrawGeometry(Brushes.LightGray, null, wideGeo1);
                dc.DrawGeometry(Brushes.Red, null, geo);
            }
            bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);


            //多色縁取りその2
            myPen = new(Brushes.Transparent, 10);
            myPen.LineJoin = PenLineJoin.Round;
            wideGeo1 = geo.GetWidenedPathGeometry(myPen);
            dv = new();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Gray, null, new Rect(0, 0, w, h));
                myPen = new(new LinearGradientBrush(Colors.WhiteSmoke, Colors.LightGray, new(0, 0), new(0, 1)), 4);
                dc.DrawGeometry(Brushes.Yellow, myPen, wideGeo1.GetOutlinedPathGeometry(0.5, ToleranceType.Relative));
                //dc.DrawGeometry(Brushes.LightGray, null, outLine1);
                dc.DrawGeometry(Brushes.Red, null, geo);
            }
            bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);

            //太字以上に太く
            myPen = new(Brushes.Transparent, 10);
            myPen.LineJoin = PenLineJoin.Round;
            wideGeo1 = geo.GetWidenedPathGeometry(myPen);
            dv = new();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.DarkRed, null, new Rect(0, 0, w, h));
                dc.DrawGeometry(Brushes.Salmon, null, wideGeo1);
            }
            bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);

            //ドロップシャドウ方法1
            dv = new();
            System.Windows.Media.Effects.DropShadowEffect dropShadowEffect = new();
            dropShadowEffect.Color = Colors.Cyan;
            dv.Effect = dropShadowEffect;
            using (var dc = dv.RenderOpen())
            {
                //ドロップシャドウ付きの文字の描画
                dc.DrawGeometry(Brushes.Salmon, null, geo);
            }
            VisualBrush visualBrush = new(dv) { Stretch = Stretch.None };
            //別のDrawingVisualを用意
            DrawingVisual dv2 = new();
            using (var dc = dv2.RenderOpen())
            {
                //背景のRectangleとVisualBrushを順に描画
                dc.DrawRectangle(Brushes.DarkRed, null, new Rect(0, 0, w, h));
                dc.DrawRectangle(visualBrush, null, new Rect(0, 0, w, h));
            }
            bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv2);

            //ドロップシャドウ方法2
            dv = new();
            System.Windows.Media.Effects.DropShadowEffect dropShadowEffect2 = new();
            dropShadowEffect2.Color = Colors.Cyan;
            dv.Effect = dropShadowEffect2;
            using (var dc = dv.RenderOpen())
            {
                //ドロップシャドウ付きの文字の描画
                dc.DrawGeometry(Brushes.Salmon, null, geo);
            }
            //画像化
            bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);

            using (var dc = dv.RenderOpen())
            {
                //背景塗りして、その上に画像化した文字を描画
                dc.DrawRectangle(Brushes.DarkRed, null, new Rect(0, 0, w, h));
                dc.DrawImage(bitmap, new Rect(0, 0, w, h));
            }
            bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv2);


            //範囲指定装飾1、FormattedText、装飾と文字色は別に指定できる
            textDecoration = new(
                TextDecorationLocation.Underline,
                new Pen(Brushes.Red, 20),
                0,
                TextDecorationUnit.FontRecommended,
                TextDecorationUnit.FontRecommended);
            textDecorations = new();
            textDecorations.Add(textDecoration);
            fmText.SetTextDecorations(null);
            fmText.SetTextDecorations(textDecorations, 3, 5);//範囲指定
            dv = new();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Green, null, new Rect(0, 0, w, h));
                dc.DrawText(fmText, new Point(0, 0));
                Point offsetP = new(0, -((fmText.Height - fmText.Extent) / 2.0));
                //dc.DrawText(fmText, offsetP);
            }
            bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);


            //範囲指定装飾2、Geometry、装飾は文字色と同じになる
            textDecoration = new(
                TextDecorationLocation.Underline,
                new Pen(Brushes.Red, 20),
                0,
                TextDecorationUnit.FontRecommended,
                TextDecorationUnit.FontRecommended);
            textDecorations = new();
            textDecorations.Add(textDecoration);
            fmText.SetTextDecorations(null);
            fmText.SetTextDecorations(textDecorations, 3, 5);//範囲指定
            geo = fmText.BuildGeometry(new Point());
            dv = new();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Green, null, new Rect(0, 0, w, h));
                dc.DrawGeometry(Brushes.MediumAquamarine, null, geo);
            }
            bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);


            //多重縁取り
            geo = fmText.BuildGeometry(new Point());
            //myPen = new Pen(Brushes.Transparent, 2);
            //wideGeo1 = geo.GetWidenedPathGeometry(myPen);
            //wideGeo2 = wideGeo1.GetWidenedPathGeometry(myPen);
            //int wideCount = wideGeo1.Figures.Count;
            //PathGeometry outline1 = geo.GetOutlinedPathGeometry();
            //PathGeometry outline2 = outline1.GetOutlinedPathGeometry();
            //int outCount = outline1.Figures.Count;
            //PathGeometry flatGeo = geo.GetFlattenedPathGeometry();
            //PathGeometry flatGeo2 = flatGeo.GetFlattenedPathGeometry();
            //int flatCount = flatGeo.Figures.Count;
            //TranslateTransform tt = new(20, 20);
            var geo2 = fmText.BuildGeometry(new Point(20,20));
            using (var dc = dv.RenderOpen())
            {
                dc.DrawGeometry(null, new Pen(Brushes.Black, 8), geo2);
                dc.DrawGeometry(null, new Pen(Brushes.DarkGoldenrod, 6), geo);
                dc.DrawGeometry(null, new Pen(Brushes.Gold, 4), geo);
                dc.DrawGeometry(null, new Pen(Brushes.White, 2), geo);
                dc.DrawGeometry(Brushes.Red, null, geo);
            }
            bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);

        }

        private BitmapSource GeometryToBitmap(Geometry geometry, Brush brush, Pen pen, int width, int height)
        {
            DrawingVisual drawing = new();
            using (var dc = drawing.RenderOpen())
            {
                dc.DrawGeometry(brush, pen, geometry);
            }
            RenderTargetBitmap bitmap = new(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawing);
            return bitmap;
        }
    }
}
