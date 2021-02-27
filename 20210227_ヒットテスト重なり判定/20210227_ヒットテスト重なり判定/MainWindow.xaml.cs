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
//WPF でVisualTree のヒット テストを実行する - やる気駆動型エンジニアの備忘録
//https://iyemon018.hatenablog.com/entry/2017/06/22/190133

//これじゃない気がする
//Geometryでの重なり判定より難しい、わからん
namespace _20210227_ヒットテスト重なり判定
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.PreviewMouseDown += MainWindow_PreviewMouseDown;
        }

        
        private readonly List<DependencyObject> _hitResults = new List<DependencyObject>();

        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TouchElementViewListBox.ItemsSource = null;
            _hitResults.Clear();

            Point position = e.GetPosition(this);
            VisualTreeHelper.HitTest(this
                                     , null
                                     , new HitTestResultCallback(OnHitTestResultCallback)
                                     , new PointHitTestParameters(position));

            TouchElementViewListBox.ItemsSource = _hitResults.OfType<FrameworkElement>()
                                                             .Select(x => $"{x.Name}:{x}")
                                                             .ToList();

            HitTestResult ht = VisualTreeHelper.HitTest(this, position);
            DependencyObject o = ht.VisualHit;
            PointHitTestParameters pp = new(position);
            HitTestResultCallback hit = new(MyTest);
            hitObject.Clear();
            VisualTreeHelper.HitTest(this, null, new HitTestResultCallback(MyTest), new PointHitTestParameters(position));

        }

        private HitTestResultBehavior OnHitTestResultCallback(HitTestResult result)
        {
            _hitResults.Add(result.VisualHit);
            return HitTestResultBehavior.Continue;
        }

        private List<DependencyObject> hitObject = new();
        private HitTestResultBehavior MyTest(HitTestResult result)
        {
            hitObject.Add(result.VisualHit);
            return HitTestResultBehavior.Continue;
        }
    }
}
