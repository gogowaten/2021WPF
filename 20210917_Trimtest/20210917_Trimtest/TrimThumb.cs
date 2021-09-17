
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls.Primitives;//Thumb
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Windows;
using System.Windows.Controls;//ControlTemplate
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;

using System.Globalization;

namespace _20210917_Trimtest
{
    public class TrimThumb : Thumb
    {
        private double PaddingSize = 20;//Thumb上でマウスカーソルの形を矢印にする範囲
        private double StartedWidth;//サイズ変更開始時の横幅記録用
        private double StartedHeight;//縦幅用

        private const string TEMPLATE_NAME = "myBase";
        private Rectangle MyPanel;
        public TrimThumb()
        {
            this.DragDelta += TrimThumb_DragDelta;
            this.MouseMove += TrimThumb_MouseMove;
            this.DragStarted += TrimThumb_DragStarted;

            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Rectangle), TEMPLATE_NAME);
            this.Template = template;
            _ = this.ApplyTemplate();
            MyPanel = (Rectangle)template.FindName(TEMPLATE_NAME, this);
            MyPanel.Fill = Brushes.Cyan;
            MyPanel.Opacity = 0.5;
            this.Focusable = true;//フォーカス可能


        }

        //ドラッグ開始時、サイズを記録する
        private void TrimThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            StartedWidth = Width;
            StartedHeight = Height;
        }

        //カーソル移動時、位置に依って形状を変化
        private void TrimThumb_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(this);

            //右下の場合
            if (p.X > Width - PaddingSize && p.Y > Height - PaddingSize)
            {
                Cursor = Cursors.SizeNWSE;
            }
            //右側の場合
            else if (p.X > Width - PaddingSize)
            {
                Cursor = Cursors.SizeWE;
            }
            //下側
            else if (p.Y > Height - PaddingSize)
            {
                Cursor = Cursors.SizeNS;
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        }



        //ドラッグ移動時、サイズ変更
        private void TrimThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //横幅変更、サイズ = サイズ変更開始時のサイズ＋ドラッグ移動量
            void ChangeWidthSize()
            {
                //変更後の幅が小さくなりすぎないように調整
                double afterW = StartedWidth + e.HorizontalChange;
                if (afterW > PaddingSize)
                {
                    Width = afterW;
                }
            }
            //縦幅変更
            void ChangeHeightSize()
            {
                double afterH = StartedHeight + e.VerticalChange;
                if (afterH > PaddingSize)
                {
                    Height = afterH;
                }
            }

            //カーソルの形で判定
            if (Cursor == Cursors.SizeWE)//左右
            {
                ChangeWidthSize();
            }
            else if (Cursor == Cursors.SizeNS)//上下
            {
                ChangeHeightSize();
            }
            else if (Cursor == Cursors.SizeNWSE)//斜め
            {
                ChangeHeightSize();
                ChangeWidthSize();
            }
            else if (Cursor == Cursors.Arrow)//通常の矢印
            {
                //ドラッグ移動
                Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
                Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
            }
        }
    }
}