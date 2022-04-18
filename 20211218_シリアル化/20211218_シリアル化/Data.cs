using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace _20211218_シリアル化
{
    [DataContract]
    [KnownType(typeof(Data)),
        KnownType(typeof(Brush)),
        KnownType(typeof(LineSegment)),
        KnownType(typeof(RectangleGeometry)),
        KnownType(typeof(MatrixTransform)),
        KnownType(typeof(SolidColorBrush)),
        KnownType(typeof(Polygon)),        
        KnownType(typeof(BezierSegment)),
        KnownType(typeof(PathFigure))
        ]
    public class Data : INotifyPropertyChanged
    {
        private int myNotifyInt;

        [DataMember]
        public int Top { get; private set; }
        [DataMember]
        public int Left { get; set; }

        [DataMember]
        public List<int> MyListInt { get; set; } = new();

        [DataMember]
        public List<Data> Children { get; set; } = new();
        [DataMember]
        public Color MyColor { get; set; }//問題ない

        //問題ない、ObservableCollection
        [DataMember]
        public ObservableCollection<int> Observable { get; set; } = new();
        [DataMember]
        public ObservableCollection<Data> ObservableData { get; set; } = new();

        //問題ない、Dictionary
        [DataMember]
        public Dictionary<string, int> MyDictionary { get; set; } = new();


        //問題ない、INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        [DataMember]
        public int MyNotifyInt
        {
            get => myNotifyInt; set
            {
                if (MyNotifyInt == value) { return; }
                myNotifyInt = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public LineSegment LineSegment { get; set; }//問題ない
        [DataMember]
        public BezierSegment BezierSegment { get; set; }//問題ない
        [DataMember]
        public ArcSegment ArcSegment { get; set; }//問題ない
        [DataMember]
        public PolyBezierSegment PolyBezierSegment { get; set; }//問題ない
        [DataMember]
        public PolyLineSegment PolyLineSegment { get; set; }
        [DataMember]
        public PolyQuadraticBezierSegment PolyQuadraticBezierSegment { get; set; }
        [DataMember]
        public QuadraticBezierSegment QuadraticBezierSegment { get; set; }
        [DataMember]
        public PathSegmentCollection pathSegments { get; set; } = new();
        [DataMember]
        public RectangleGeometry RectangleGeometry { get; set; }//Geometryはシリアル化できないと思ったけど、シリアル化するクラスの頭に属性を付けたらできた、属性は2つ必要でKnownType(typeof(RectangleGeometry))とKnownType(typeof(MatrixTransform))

        //ブラシ系はそのままではシリアライズできないのでクラスの頭に属性をつける
        //その場合はBrushじゃなくてSolidColorBrushと細かく指定する必要がある
        //public SolidColorBrush SolidColorBrush { get; set; }
        [DataMember]
        public SolidColorBrush SolidColorBrush { get; set; }//シリアル化できない→属性付与でできるようになる

        [DataMember]
        public PathFigure PathFigure { get; set; }//PathFigureも属性付与でできるようになる
        

        //public Polygon Polygon { get; set; }//エラーになる、含まれるCursorがシリアル化できない
        //public PathSegment PathSegment { get; set; }//エラーになる




        //DependencyPropertyはシリアル化できない
        //[DataMember]
        //public int MyDependencyInt
        //{
        //    get { return (int)GetValue(MyDependencyIntProperty); }
        //    set { SetValue(MyDependencyIntProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for MyDependencyInt.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty MyDependencyIntProperty =
        //    DependencyProperty.Register("MyDependencyInt", typeof(int), typeof(Data), new PropertyMetadata(0));



        //Buttonできない？KnownType(typeof(Button))とKnownType(typeof(Cursor))を
        //付けてもCorsorがなにかしないとできないって表示される
        //[DataMember]
        //public Button Button { get; set; }
    }



}
