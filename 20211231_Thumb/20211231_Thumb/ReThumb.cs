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
using System.Collections.Specialized;

namespace _20211231_Thumb
{
    public class ReThumb : Thumb, System.ComponentModel.INotifyPropertyChanged
    {
        private Canvas RootCanvas;
        //public bool IsRoot;
        public bool IsGroup;
        public ReThumb ParentReThumb;
        public ReThumb RootReThumb;//動かすThumb
                                   //public ObservableCollection<ReThumb> ChildrenOld { get; set; } = new();
                                   //        読み取り専用に公開するパターン - Qiita
                                   //https://qiita.com/Azleep/items/299fafdf51f260bbecb2

        protected ObservableCollection<ReThumb> children = new();
        public ReadOnlyObservableCollection<ReThumb> Children { get; private set; }
        //public ReadOnlyObservableCollection<ReThumb> Children1 => new(Children);

        private double left;
        private double top;
        private string idName;
        private readonly ObservableCollection<ReThumb> readInt;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public double Left { get => left; set { left = value; OnPropertyChanged(); } }
        public double Top { get => top; set { top = value; OnPropertyChanged(); } }
        public string IdName { get => idName; set { idName = value; OnPropertyChanged(); } }

        protected ReThumb()
        {
            Children = new(children);


            ControlTemplate template = new();
            FrameworkElementFactory fCanvas = new(typeof(Canvas), "rootcanvas");
            template.VisualTree = fCanvas;
            this.Template = template;
            ApplyTemplate();
            RootCanvas = template.FindName("rootcanvas", this) as Canvas;

            children.CollectionChanged += Children_CollectionChanged;
            //ChildrenOld.CollectionChanged += Children_CollectionChanged;
            DragDelta += ReThumb_DragDelta;

            this.SetBinding(Canvas.LeftProperty, MakeBind(nameof(Left)));
            this.SetBinding(Canvas.TopProperty, MakeBind(nameof(Top)));

            this.Focusable = true;
            //IsRoot = true;
            RootReThumb = this;

        }

        public ReThumb(UIElement element, string name = "", double x = 0, double y = 0) : this()
        {
            AddElement(element);
            IdName = string.IsNullOrEmpty(name) ? DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") : name;
            Left = x;
            Top = y;
        }
        public ReThumb(UIElement element, double x = 0, double y = 0) : this(element, null, x, y) { }


        //複数ThumbからグループThumb作成
        public ReThumb(List<ReThumb> reThumbs, string name = "") : this()
        {
            IdName = string.IsNullOrEmpty(name) ? DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") : name;
            double left = reThumbs.Min(a => a.Left);
            double top = reThumbs.Min(a => a.Top);
            foreach (ReThumb item in reThumbs)
            {
                item.Left -= left;
                item.Top -= top;
                children.Add(item);
            }
            Left = left;
            Top = top;
        }


        //グループ解除
        public ICollection<ReThumb> UnGroup()
        {
            List<ReThumb> list = new();
            foreach (ReThumb item in children.ToList())
            //foreach (ReThumb item in ChildrenOld.ToList())
            {
                children.Remove(item);
                //ChildrenOld.Remove(item);
                list.Add(item);
            }
            return list;
        }


        protected virtual void ReThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Left += e.HorizontalChange;
            Top += e.VerticalChange;
        }

        protected virtual void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems;
            var oldItem = e.OldItems;
            if (children.Count < 2 && IsGroup) { IsGroup = false; }
            //if (ChildrenOld.Count < 2 && IsGroup) { IsGroup = false; }
            else { IsGroup = true; }

            //追加された場合
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    ReThumb re = item as ReThumb;
                    RootCanvas.Children.Add(re);
                    //
                    //re.IsRoot = false;
                    re.ParentReThumb = this;
                    //通常のグループ化の場合
                    if (typeof(Layer) != this.GetType())
                    {
                        ReplaceRootReThumb(re, this.RootReThumb);
                        //DragDeltaを外す
                        re.DragDelta -= re.ReThumb_DragDelta;
                    }

                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    ReThumb re = item as ReThumb;
                    RootCanvas.Children.Remove(re);
                    //
                    if (this.ParentReThumb != null)
                    {
                        this.ParentReThumb.children.Add(re);
                        //this.ParentReThumb.ChildrenOld.Add(re);
                        //re.IsRoot = false;
                        re.ParentReThumb = this.ParentReThumb;
                    }
                    else
                    {
                        //re.IsRoot = true;
                    }
                    re.DragDelta += re.ReThumb_DragDelta;

                }
            }
        }

        private void ReplaceRootReThumb(ReThumb current, ReThumb root)
        {
            current.RootReThumb = root;
            if (current.children.Count < 1) { return; }
            foreach (var item in current.children)
            {
                item.RootReThumb = root;
                ReplaceRootReThumb(item, root);
            }

        }
        private Binding MakeBind(string path)
        {
            Binding b = new();
            b.Source = this;
            b.Mode = BindingMode.TwoWay;
            b.Path = new PropertyPath(path);
            return b;
        }
        private void AddElement(UIElement element)
        {
            RootCanvas.Children.Add(element);
        }

        //グループに要素(Thumb)を追加
        public virtual void AddChildren(ReThumb thumb)
        {
            if (Left > thumb.left)
            {
                Left = thumb.left;
                thumb.Left = 0;
            }
            if (Top > thumb.top)
            {
                Top = thumb.top;
                thumb.Top = 0;
            }
            children.Add(thumb);
        }


        public override string ToString()
        {
            //return base.ToString();
            return $"{IdName}, x={Left}, y={Top}";
        }
    }

    public class Layer : ReThumb
    {
        //public readonly bool IsRoot;
        public Layer()
        {
            DragDelta -= ReThumb_DragDelta;
        }

        public override void AddChildren(ReThumb thumb)
        {
            //base.AddChildren(thumb);
            children.Add(thumb);
        }
        //protected override void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{

        //}

        //private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{

        //}
    }
}
