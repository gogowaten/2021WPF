using System.Windows;
using System.Windows.Media;

//IntersectionDetail 列挙型(System.Windows.Media) | Microsoft Docs
//https://docs.microsoft.com/ja-jp/dotnet/api/system.windows.media.intersectiondetail

//WPFのRectの重なり判定、RectangleGeometryにしてからFillContainsWithDetailメソッドでできた - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/01/28/124714

namespace _20210127_Rect同士の重なり判定
{
    /// <summary>
    /// RectからRectangleGeometryを作って、FillContainsWithDetailメソッドを使って判定できる
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Rect r1 = new(0, 0, 100, 100);//座標0,0   サイズ100,100

            //true    Intersects
            var r1r2 = IsOverlapping(r1, new(10, 10, 100, 100));

            //true    FullyContains
            var r1r3 = IsOverlapping(r1, new(10, 10, 50, 50));

            //false   Empty
            var r1r4 = IsOverlapping(r1, new(100, 100, 100, 100));

            //true    intersects
            var r1r5 = IsOverlapping(r1, new(99, 99, 100, 100));

            //false   Empty
            var r1r6 = IsOverlapping(r1, new(110, 50, 100, 100));

            //true    FullyInside
            var r1r7 = IsOverlapping(r1, new(-10, -10, 200, 200));
        }

        /// <summary>
        /// 2つのRectが一部でも重なっていたらtrueを返す
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        private (bool, IntersectionDetail) IsOverlapping(Rect r1, Rect r2)
        {
            RectangleGeometry geo1 = new(r1);
            IntersectionDetail detail = geo1.FillContainsWithDetail(new RectangleGeometry(r2));
            return (detail != IntersectionDetail.Empty, detail);
            //return (detail != IntersectionDetail.Empty || detail != IntersectionDetail.NotCalculated, detail);
        }

        //IntersectionDetail列挙型
        //Empty             全く重なっていない
        //FullyContains     r2はr1の領域に完全に収まっている
        //FullyInside       r1はr2の領域に完全に収まっている
        //Intersects        一部が重なっている
        //NotCalculated     計算されません(よくわからん)

    }
}
