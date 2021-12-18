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


namespace _20211218_シリアル化
{
    [DataContract]
    [KnownType(typeof(Data)), KnownType(typeof(Brush))]
    public class Data
    {
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


        //ブラシ系はそのままではシリアライズできないので要素を分解して保持することになる
        //public SolidColorBrush SolidColorBrush { get; set; }//シリアル化できない
        //public Brush Brush { get; set; }//シリアル化できない
        //オブジェクト系もできない
        //public Button Button { get; set; }
    }
}
