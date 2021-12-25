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
using System.Collections.ObjectModel;

//【WPF】ItemsControlの基本的な使い方 - Qiita
//https://qiita.com/ebipilaf/items/c3e9e501eb0560a12ce8
//ここのXAMLをC#で書いてみた

namespace _2021122416
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private ExThumb currentExThumb;
        ////private ExThumb MyExThumb1;
        ////private ExThumb MyExThumb2;
        ////private ExThumb MyGroupExThumb1;
        //private int CountForName;
        //private ObservableCollection<ExThumb> MyLayers = new();
        //private ExThumb MyCurrentLayer;
        //private ExThumb MyMainPanel;
        public ObservableCollection<Store> Mall { get; set; } = new();
        //public ExThumb CurrentExThumb
        //{
        //    get => currentExThumb; set
        //    {
        //        currentExThumb = value;
        //        DataContext = value;
        //    }
        //}

        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 5; i++)
            {
                Mall.Add(new Store() { FavoriteCount = i, Name = $"name{i}", Prefecture = $"pre{i}" });
            }
            DataContext = this;

            ControlTemplate template = MyListBox.Template;
            TemplateContent itemPanelTemp = MyListBox.ItemsPanel.Template;
            DataTemplate itemTemp = MyListBox.ItemTemplate;
            ItemsPanelTemplate ip = MyListBox.ItemsPanel;
            TemplateContent iptemp = ip.Template;
            
            //var panel = ip.FindName("PanelTemplate", MyListBox.Template);

        }
    
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            Mall.Add(new() { Prefecture = "add", FavoriteCount = 00, Name = "ADD" });
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            Mall.Remove(Mall[0]);
        }
    }

    public class Store
    {
        public string Name { get; set; }
        public string Prefecture { get; set; }
        public int FavoriteCount { get; set; }
    }
}
