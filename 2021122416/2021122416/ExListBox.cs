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
using System.Windows.Controls.Primitives;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.ComponentModel;

//【WPF】ItemsControlの基本的な使い方 - Qiita
//https://qiita.com/ebipilaf/items/c3e9e501eb0560a12ce8


namespace _2021122416
{
    class ExListBox : ListBox
    {
        public ExListBox()
        {
            Binding binding = new();
            binding.Path = new PropertyPath("Mall");
            BindingOperations.SetBinding(this, ItemsSourceProperty, binding);

            //ListBox.Template = ControlTemplate
            //全体の書式、枠の太さと色、背景色の指定
            FrameworkElementFactory factoryBorder = new FrameworkElementFactory(typeof(Border));
            factoryBorder.SetValue(Border.BackgroundProperty, Brushes.MediumOrchid);
            factoryBorder.SetValue(Border.BorderThicknessProperty, new Thickness(5));
            factoryBorder.SetValue(Border.BorderBrushProperty, Brushes.MediumAquamarine);

            ControlTemplate template = new(typeof(ListBox));
            template.VisualTree = factoryBorder;

            FrameworkElementFactory factoryItemsPresenter = new(typeof(ItemsPresenter));
            factoryItemsPresenter.SetValue(MarginProperty, new Thickness(5));
            factoryBorder.AppendChild(factoryItemsPresenter);


            //ItemsPanal
            //要素が横に並ぶようにしたWrapPanelを指定してみた
            FrameworkElementFactory panelFactory = new(typeof(WrapPanel));
            panelFactory.SetValue(WrapPanel.OrientationProperty, Orientation.Horizontal);
            ItemsPanelTemplate itemsPanelTemplate = new();
            itemsPanelTemplate.VisualTree = panelFactory;
            this.ItemsPanel = itemsPanelTemplate;


            //ItemTemplate = DataTemplate
            MultiBinding multiBinding = new();
            multiBinding.StringFormat = "{0} {1}";
            multiBinding.Bindings.Add(new Binding("Prefecture"));
            multiBinding.Bindings.Add(new Binding("Name"));
            FrameworkElementFactory textBlock1Factory = new(typeof(TextBlock));
            textBlock1Factory.SetBinding(TextBlock.TextProperty, multiBinding);

            FrameworkElementFactory textBlock2Factory = new(typeof(TextBlock));
            Binding binding1 = new("FavoriteCount");
            binding1.StringFormat = "お気に入り : {0}";
            textBlock2Factory.SetBinding(TextBlock.TextProperty, binding1);

            FrameworkElementFactory stackPanelFactory = new(typeof(StackPanel));
            stackPanelFactory.AppendChild(textBlock1Factory);
            stackPanelFactory.AppendChild(textBlock2Factory);


            DataTemplate dataTemplate = new();
            dataTemplate.VisualTree = stackPanelFactory;
            this.ItemTemplate = dataTemplate;



            this.Template = template;
        }
    }
}
