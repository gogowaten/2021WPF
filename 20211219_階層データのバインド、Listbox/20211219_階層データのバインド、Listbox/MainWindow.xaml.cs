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

namespace _20211219_階層データのバインド_Listbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var Teams = Enumerable.Range(0, 5).Select(a => "Team" + a);
            Data MyData = new Data()
            {
                Name = "2021",
                Children = new()
                {
                    new Data()
                    {
                        Name = "12",
                        Children = new()
                        {
                            new Data() { Name = "1" },
                            new Data() { Name = "2" },
                        }
                    },
                    new Data()
                    {
                        Name = "11",
                        Children = new()
                        {
                            new Data() { Name = "1" },
                            new Data() { Name = "2" },
                        }
                    }
                }
            };
            DataContext = MyData;
        }
    }

    public class Data
    {
        public ObservableCollection<Data> Children { get; set; } = new();
        public string Name { get; set; }
    }
}
