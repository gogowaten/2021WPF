﻿using System;
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
        public ReThumb RootReThumb;//動かすThumb
        public ObservableCollection<ReThumb> Children { get; private set; } = new();
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

            this.SetBinding(Canvas.LeftProperty, MakeBind(nameof(Left)));
            this.SetBinding(Canvas.TopProperty, MakeBind(nameof(Top)));

            this.Focusable = true;
            IsRoot = true;
            RootReThumb = this;

        }
        public ReThumb(double x = 0, double y = 0) : this()
        {
            Left = x;
            Top = y;
        }

        public ReThumb(UIElement element, double x = 0, double y = 0) : this(x, y)
        {
            AddElement(element);
        }

        //グループ解除
        public ICollection<ReThumb> UnGroup()
        {
            List<ReThumb> list = new();
            foreach (ReThumb item in Children.ToList())
            {
                Children.Remove(item);
                list.Add(item);
            }
            return list;
        }
        //複数ThumbからグループThumb作成
        public ReThumb(IEnumerable<ReThumb> reThumbs) : this()
        {
            double left = reThumbs.Min(a => a.Left);
            double top = reThumbs.Min(a => a.Top);
            foreach (ReThumb item in reThumbs)
            {
                item.Left -= left;
                item.Top -= top;
                Children.Add(item);
            }
            Left = left;
            Top = top;
            //this.RootReThumb = this;
        }


        private void ReThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Left += e.HorizontalChange;
            Top += e.VerticalChange;
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var items = e.NewItems;
            var olditem = e.OldItems;
            if (Children.Count < 2 && IsGroup) { IsGroup = false; }
            else { IsGroup = true; }

            //追加された場合
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    ReThumb re = item as ReThumb;
                    RootCanvas.Children.Add(re);
                    //DragDeltaを外す
                    re.DragDelta -= re.ReThumb_DragDelta;
                    //
                    re.IsRoot = false;
                    re.ParentReThumb = this;
                    ReplaceRootReThumb(re, this.RootReThumb);
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
                        this.ParentReThumb.Children.Add(re);
                        re.IsRoot = false;
                        re.ParentReThumb = this.ParentReThumb;
                    }
                    else
                    {
                        re.IsRoot = true;
                    }
                    re.DragDelta += re.ReThumb_DragDelta;

                }
            }
        }
        private void ReplaceRootReThumb(ReThumb current, ReThumb root)
        {
            current.RootReThumb = root;
            if (current.Children.Count < 1) { return; }
            foreach (var item in current.Children)
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
        public void AddElement(UIElement element)
        {
            RootCanvas.Children.Add(element);
        }


        public override string ToString()
        {
            //return base.ToString();
            return $"x={Left}, y={Top}";
        }
    }
}
