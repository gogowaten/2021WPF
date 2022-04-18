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
using System.Xml;
using System.Windows.Markup;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace _20211218_シリアル化
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Data MyData;
        Data MyDataWithChilden;
        public MainWindow()
        {
            InitializeComponent();

            //TextBlock textBlock = new() { Text = "test" };
            //string inu = Test1(textBlock);

            MyData = new();
            MyData.Left = 20;
            MyData.MyListInt.Add(3);
            MyData.MyListInt.Add(200);
            MyData.Observable = new ObservableCollection<int>() { 2, 5, 8 };
            Dictionary<string, int> dict = new();
            dict.Add("test1", 20);
            dict.Add("test2", 50);
            MyData.MyDictionary = dict;
            MyData.MyDictionary.Add("test3", 99);

            //MyData.MyDependencyInt = 50;
            MyData.MyNotifyInt = 50;

            MyDataWithChilden = new();
            MyDataWithChilden.Children.Add(MyData);

            MyData.LineSegment = new LineSegment();
            MyData.BezierSegment = new BezierSegment();
            MyData.ArcSegment = new ArcSegment();
            MyData.PolyBezierSegment = new();
            MyData.QuadraticBezierSegment = new();
            MyData.PolyQuadraticBezierSegment = new(new List<Point>() { new Point(44, 44), new Point(55, 55) }, true);
            MyData.PolyLineSegment = new();
            MyData.pathSegments.Add(new LineSegment());
            MyData.RectangleGeometry = new(new Rect(0, 0, 2, 2));
            MyData.SolidColorBrush = Brushes.MediumAquamarine;
            MyData.PathFigure = new();
            MyData.ObservableData.Add(new Data() { Left = 111 });


            //MyData.Button = new();

            string filePath = @"E:\MyData.xml";
            Test3(MyData);
            //Test4(MyData, filePath);
            //string neko2 = Test2(MyData);

            //Test3(MyDataWithChilden);
            //string neko = Test1(MyData);
            DataContext = MyData;

        }

        /// <summary>
        /// XamlDesignerSerializationManagerを使ってXAMLをシリアライズする
        /// XAMLならほとんどシリアライズできるけど、XAMLだけ
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string Test1(object obj)
        {
            XmlWriterSettings settings = new();
            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            StringBuilder sb = new();
            XmlWriter writer;
            using (writer = XmlWriter.Create(sb, settings))
            {
                try
                {
                    XamlDesignerSerializationManager manager = new(writer)
                    {
                        XamlWriterMode = XamlWriterMode.Expression
                    };
                    XamlWriter.Save(obj, writer);
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show(ex.Message);
                }
            }
            return sb.ToString();
        }

        private string Test2(object obj)
        {

            DataContractSerializer serializer = new(typeof(Data));
            XmlWriterSettings settings = new();
            settings.Encoding = new UTF8Encoding(false);
            //settings.Indent = true;
            //settings.NewLineOnAttributes = false;
            //settings.ConformanceLevel = ConformanceLevel.Fragment;

            StringBuilder sb = new();
            XmlWriter writer;
            using (writer = XmlWriter.Create(sb, settings))
            {
                try
                {
                    serializer.WriteObject(writer, obj);
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show(ex.Message);
                }
            }
            var neko = writer;
            return sb.ToString();
        }

        private void Test3(object obj)
        {

            string fileName = @"E:\MyData.xml";

            XmlWriterSettings settings = new();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = true;//インデント、必須じゃないけどテキストエディタで開いたときに見やすくなる
            settings.NewLineOnAttributes = false;//改行、必須じゃないけどテキストエディタで開いたときに見やすくなる
            settings.ConformanceLevel = ConformanceLevel.Fragment;//いらないかな

            XmlWriter writer;
            DataContractSerializer serializer = new(typeof(Data));
            using (writer = XmlWriter.Create(fileName, settings))
            {
                try
                {
                    serializer.WriteObject(writer, obj);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //XmlSerializerはDictionaryのシリアル化ができない
        private void Test4(object obj, string filePath)
        {
            XmlWriterSettings settings = new();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = true;//インデント、必須じゃないけどテキストエディタで開いたときに見やすくなる
            settings.NewLineOnAttributes = false;//改行、必須じゃないけどテキストエディタで開いたときに見やすくなる
            settings.ConformanceLevel = ConformanceLevel.Fragment;//いらないかな
            try
            {
                XmlSerializer xmlSerializer = new(typeof(Data));//Dictionaryがシリアル化できない
                using XmlWriter xmlWriter = XmlWriter.Create(filePath, settings);
                xmlSerializer.Serialize(xmlWriter, obj);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

    }
}
