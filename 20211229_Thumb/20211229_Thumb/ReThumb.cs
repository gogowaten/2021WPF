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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace _20211229_Thumb
{
    public class ReThumb : Thumb, System.ComponentModel.INotifyPropertyChanged
    {
        private Canvas RootCanvas;
        public bool IsRoot;
        public bool IsGroup;
        public ReThumb ParentReThumb;
        public ObservableCollection<ReThumb> Children = new();
        private double left;
        private double top;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public double Left { get => left; set { left = value; OnPropertyChanged(); } }
        public double Top { get => top; set { top = value; OnPropertyChanged(); } }

        public ReThumb()
        {
            ControlTemplate template = new();
            FrameworkElementFactory fCanvas = new(typeof(Canvas), "rootcanvas");
            template.VisualTree = fCanvas;
            this.Template = template;
            ApplyTemplate();
            RootCanvas = template.FindName("rootcanvas", this) as Canvas;

            Children.CollectionChanged += Children_CollectionChanged;
            DragDelta += ReThumb_DragDelta;
            this.SetBinding(Canvas.LeftProperty, new Binding("Left"));
            this.SetBinding(Canvas.TopProperty, new Binding("Top"));

        }

        private void ReThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Left += e.HorizontalChange;
            Top += e.VerticalChange;
        }

        public ReThumb(UIElement element) : this()
        {
            AddElement(element);
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newitem = e.NewItems;
            var nitem = newitem[0];
            var olditem = e.OldItems;
            var oitem = olditem[0];
            if (Children.Count < 2 && IsGroup) { IsGroup = false; }
            else { IsGroup = true; }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {

            }
        }

        public void AddElement(UIElement element)
        {
            RootCanvas.Children.Add(element);
        }
        public void MakeGroup(List<ReThumb> reThumbs)
        {
            Children.Union(reThumbs);
        }
    }
}
