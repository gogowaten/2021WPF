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
using System.ComponentModel;

namespace _2021122416
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:_2021122416"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:_2021122416;assembly=_2021122416"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class CustomControl1 : Thumb, System.ComponentModel.INotifyPropertyChanged
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }
        private readonly string ROOT_PANEL_NAME = "RootPanel";
        private Canvas MyRootCanvas;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public double Left { get => left; set { left = value; OnPropertyChanged(); } }
        public double Top { get => top; set { top = value; OnPropertyChanged(); } }
        private double left;
        private double top;


        public CustomControl1()
        {
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), ROOT_PANEL_NAME);
            this.Template = template;
            ApplyTemplate();
            MyRootCanvas = this.Template.FindName(ROOT_PANEL_NAME, this) as Canvas;
            MyRootCanvas.Background = Brushes.Transparent;
            Focusable = true;

            DragDelta += CustomControl1_DragDelta;
            SetBinding(Canvas.LeftProperty, MakeBinding(nameof(Left)));
            SetBinding(Canvas.TopProperty, MakeBinding(nameof(Top)));
        }
        public CustomControl1(double left, double top, UIElement element) : this()
        {
            Left = left; Top = top;
            MyRootCanvas.Children.Add(element);

        }
    

        private void CustomControl1_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Left += e.HorizontalChange;
            Top += e.VerticalChange;
        }
        private Binding MakeBinding(string pName)
        {
            Binding b = new();
            b.Source = this;
            b.Path = new PropertyPath(pName);
            b.Mode = BindingMode.TwoWay;
            return b;
        }
        public void AddElement(UIElement element)
        {
            MyRootCanvas.Children.Add(element);
        }
    }
}
