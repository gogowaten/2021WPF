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
    [KnownType(typeof(Data)), KnownType(typeof(Brush))]
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
        
        //public PathSegment PathSegment { get; set; }//エラーになる


        //public PathFigure PathFigure { get; set; }//PathFigureもシリアル化できない？




        //public Geometry Geometry { get; set; }//Geometryはシリアル化できない


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


        //ブラシ系はそのままではシリアライズできないので要素を分解して保持することになる
        //public SolidColorBrush SolidColorBrush { get; set; }//シリアル化できない
        //public Brush Brush { get; set; }//シリアル化できない
        //オブジェクト系もできない
        //public Button Button { get; set; }
    }


    
}
