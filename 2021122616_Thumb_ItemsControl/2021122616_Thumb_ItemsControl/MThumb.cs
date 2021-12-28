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

namespace _2021122616_Thumb_ItemsControl
{
    class MThumb : Thumb
    {
        private readonly string ROOT_PANEL_NAME = "rootPanel";
        private ItemsControl MyItemsControl;
        private readonly string MY_ITEMS_CONTROL_NAME = "MyItemsControl";
        private Canvas MyCanvas;
        private readonly string MY_CANVAS_NAME = "MyCanvas";
        private Grid MyGrid;
        public MThumb()
        {
            //Thumb
            //  ┗
            //ItemsPanel
            //FrameworkElementFactory canvas = new(typeof(Canvas), MY_CANVAS_NAME);
            //ItemsPanelTemplate itemsPanelTemplate = new();
            //itemsPanelTemplate.VisualTree = canvas;
            //MyItemsControl.ItemsPanel = itemsPanelTemplate;

            //FrameworkElementFactory itemsControl = new(typeof(ItemsControl), MY_ITEMS_CONTROL_NAME);
            FrameworkElementFactory grid = new(typeof(Grid), ROOT_PANEL_NAME);
            //grid.AppendChild(itemsControl);
            FrameworkElementFactory aic = new(typeof(AItemsControl), "aic");
            grid.AppendChild(aic);






            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = grid;
            this.Template = template;
            ApplyTemplate();
            MyGrid = this.Template.FindName(ROOT_PANEL_NAME, this) as Grid;

            //Thumb
            //  Template
            //      Grid
            //        ItemsControl
            //          ItemsControl.ItemsPanelProperty
            //              ItemsPanelTemplate
            //                  Canvas
            //          ItemsControl.ItemTemplateProperty
            //              DataTemplate
            //          ItemsControl.ItemContainerStyleProperty
            //              Style
            //                  Setter Canvas.Left Binding Path = Left

            //BindingOperations.SetBinding(this, Canvas.LeftProperty, new Binding("Left"));
            //BindingOperations.SetBinding(this, Canvas.LeftProperty, new Binding("[1].Left"));
            BindingOperations.SetBinding(this, Canvas.LeftProperty, new Binding("[0].Left"));
            
            DragDelta += MThumb_DragDelta;
        }

        private void MThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            

        }
    }
    public class AItemsControl : ItemsControl
    {
        private readonly string ROOT_PANEL_NAME = "rootPanel";
        
        public AItemsControl()
        {   
            FrameworkElementFactory fCanvas = new(typeof(Canvas));
            ItemsPanelTemplate itemsPanelTemplate = new();
            itemsPanelTemplate.VisualTree = fCanvas;
            this.ItemsPanel = itemsPanelTemplate;


            FrameworkElementFactory fTextBlock = new(typeof(TextBlock), "text");
            fTextBlock.SetBinding(TextBlock.TextProperty, new Binding("Left"));
            DataTemplate dataTemplate = new();
            dataTemplate.VisualTree = fTextBlock;
            this.ItemTemplate = dataTemplate;
            this.ApplyTemplate();


            //Setter setter1 = new(Canvas.LeftProperty, new Binding("Left"));
            //Style style = new();
            //style.Setters.Add(setter1);
            //this.ItemContainerStyle = style;


            this.SetBinding(ItemsControl.ItemsSourceProperty, new Binding());

            //Style style = new();
            //style.Setters.Add(new Setter(TextBlock.TextProperty, = new SetterBaseCollection();
            //this.ItemContainerStyle=
        }
        public void AddElement(UIElement element)
        {

        }
    }
}
