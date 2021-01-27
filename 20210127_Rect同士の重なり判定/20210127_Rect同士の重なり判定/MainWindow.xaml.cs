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

namespace _20210127_Rect同士の重なり判定
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Rect r1 = new(0, 0, 100, 100);//座標0,0   サイズ100,100

            //true    Intersects
            bool r1r2 = IsOverlapping(r1, new(10, 10, 100, 100));

            //true    FullyContains
            bool r1r3 = IsOverlapping(r1, new(10, 10, 10, 10));

            //false   Empty
            bool r1r4 = IsOverlapping(r1, new(100, 100, 100, 100));

            //true    intersects
            bool r1r5 = IsOverlapping(r1, new(99, 99, 100, 100));

            //false   Empty
            bool r1r6 = IsOverlapping(r1, new(200, 200, 100, 100));
            
            //true    FullyInside
            bool r1r7 = IsOverlapping(r1, new(-10, -10, 200, 200));
        }

        /// <summary>
        /// 2つのRectが一部でも重なっていたらtrueを返す
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        private bool IsOverlapping(Rect r1, Rect r2)
        {
            RectangleGeometry geo1 = new(r1);
            IntersectionDetail detail = geo1.FillContainsWithDetail(new RectangleGeometry(r2));
            return detail != IntersectionDetail.Empty;
            //return result != IntersectionDetail.Empty || result != IntersectionDetail.NotCalculated;
        }

    }
}
