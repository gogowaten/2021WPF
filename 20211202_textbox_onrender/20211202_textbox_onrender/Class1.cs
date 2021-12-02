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
//WPFのTextBoxの表示文字を装飾するサンプル – 山本隆の開発日誌
//https://www.gesource.jp/weblog/?p=8258

namespace _20211202_textbox_onrender
{
    class Class1 : TextBox
    {
        private Pen myPen = new(Brushes.MediumBlue, 1);

        public Class1()
        {
            Foreground = Brushes.Transparent;
            Background = Brushes.Transparent;
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            InvalidateVisual();
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            FormattedText formattedText = new(
                Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    FontFamily,
                    FontStyle,
                    FontWeight,
                    FontStretch),
                FontSize,
                Brushes.Red,
                96
                );
            Geometry geo = formattedText.BuildGeometry(new Point());
            drawingContext.DrawGeometry(Brushes.MediumAquamarine, null, geo);
            //drawingContext.DrawGeometry(Brushes.MediumAquamarine, new Pen(Brushes.Black, 1), geo);

            geo.GetRenderBounds(MyPen);
            var pgeo = geo.GetWidenedPathGeometry(MyPen);
            var rbound = geo.GetRenderBounds(MyPen);
            var flat = geo.GetFlattenedPathGeometry();
            var outline = geo.GetOutlinedPathGeometry();
            var ppgeo = pgeo.GetWidenedPathGeometry(MyPen);

            BitmapSource flatBitmap = RenderText(flat);
            BitmapSource outlineBitmap = RenderText(outline);
            BitmapSource geoBitmap = RenderText(geo);
            BitmapSource pgeoBitmap = RenderText(pgeo);
            BitmapSource ppgeoBitmap = RenderText(outline);


        }
        private BitmapSource RenderText(Geometry geo)
        {
            DrawingVisual dv = new();
            //GuidelineSet guidelineSet = new(new double[] { 0.1, 0.2 }, new double[] { 0.2, 0.2 });
            using (DrawingContext dc = dv.RenderOpen())
            {
                //dc.PushGuidelineSet(guidelineSet);
                dc.DrawGeometry(Brushes.Red, null, geo);
                //dc.DrawGeometry(Brushes.Red, MyPen, geo);
            }

            RenderTargetBitmap render = new(200, 100, 96, 96, PixelFormats.Pbgra32);
            for (int i = 0; i < 10; i++)
            {
                dv.Offset = new Vector(i, i);
                render.Render(dv);

            }
            return render;
        }


        public Pen MyPen
        {
            get { return myPen; }
            set
            {
                myPen = value;
            }
        }
    }
}
