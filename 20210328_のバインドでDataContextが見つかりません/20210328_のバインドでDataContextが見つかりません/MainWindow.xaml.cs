using System.Windows;

namespace _20210328_のバインドでDataContextが見つかりません
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MyData data = new()
            {
                TextAAA = "新・AAAAA",
                TextBBB = "神・BBBBB",
                TextCCC = "真・CCCCC"
            };
            this.DataContext = data;
        }
    }

    //public class MyData
    //{
    //    public string TextAAA { get; set; }
    //    public string TextBBB { get; set; }
    //    public string TextCCC { get; set; }
    //}
    public class MyData
    {
        public string TextAAA { get; set; }
        public string TextBBB { get; set; } = "仮・BBBBB";
        public string TextCCC { get; set; }
        public MyData()
        {
            TextCCC = "仮・CCCCC";
        }
    }

}
