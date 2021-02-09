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

using System.Windows.Interop;

//アプリのスクショでウィンドウ枠外のメニューもキャプチャしたい - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/02/09/213524


namespace _20210208_メニューキャプチャ普通のアプリとエクセル
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //ホットキー
        private const int HOTKEY_ID1 = 0x0001;//ID
        private IntPtr MyWindowHandle;//アプリのハンドル

        //ウィンドウ探査loopの回数上限値
        private const int LOOP_LIMIT = 20;

        //
        private BitmapSource MyBitmapSource;

        public MainWindow()
        {
            InitializeComponent();

            MyInitializeHotKey();

            //ホットキーに修飾キーとPrintScreenキーを登録
            //int mod = GetModifierKeySum();
            //int mod = 2;//ctrl
            //int mod = 1;//alt
            //int mod = 4;//shift
            //int mod = 6;//ctrl + shift
            //int mod = 0;//修飾キーなし
            int mod = 0;
            ChangeHotKey(mod, Key.PrintScreen, HOTKEY_ID1);

            //アプリ終了時にホットキーの解除
            Closing += MainWindow_Closing;
        }

        //ホットキー判定
        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != API.WM_HOTKEY) return;

            //ホットキーが押されたら
            else if (msg.wParam.ToInt32() == HOTKEY_ID1)
            {
                //画面全体をキャプチャして、Rect収集して、それを使って切り抜き画像作成
                MyBitmapSource = CroppedBitmapFromRects(GetScreenBitmap(), RR());
                //画像表示
                MyImage.Source = MyBitmapSource;
              

            }
        }

        private List<Rect> RR()
        {
            List<Rect> R = new();

            var fore = GetWindowInfo(API.GetForegroundWindow());

            //エクセル系アプリ
            if (fore.Text == "")
            {   
                MyWidndowInfo rootOwner = GetWindowInfo(
                    API.GetAncestor(fore.hWnd, API.AncestorType.GA_ROOTOWNER));
                MyWidndowInfo parent = GetWindowInfo(
                    API.GetParent(fore.hWnd));
                MyWidndowInfo popup = GetWindowInfo(
                    API.GetWindow(rootOwner.hWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP));

                //Foreの下層にあるウィンドウハンドルをGetWindowのNEXTで収集
                List<MyWidndowInfo> next = GetWindowInfos(
                    GetCmdWindows(fore.hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT));

                //可視状態のものだけ残す
                next = next.Where(x => x.IsVisible == true).ToList();
                
                //RootOwnerがForeのRootOwnerと同じものだけ残す
                next = next.Where(x => rootOwner.Text == GetWindowText(API.GetAncestor(x.hWnd, API.AncestorType.GA_ROOTOWNER))).ToList();

                //見た目通りのRectを取得
                R = next.Select(x => GetWindowRectMitame(x.hWnd)).ToList();

                //ForeNEXTを上から順番にRectを見て、width = 0が見つかったらそれ以降は除外
                R = SelectNoneZeroRects(R);

                //popupウィンドウのRectを追加
                if (popup.Rect.Width != 0)
                {
                    R.Add(popup.Rect);
                }
                                
                //ParentのTextが""ならParentは無いので、代わりにRootOwnerのRectを追加
                if (parent.Text == "")
                {
                    R.Add(GetWindowRectMitame(rootOwner.hWnd));
                }
                //ParentのTextがあればダイアログボックスウィンドウが最前面なので、そのRectを追加
                else
                {
                    R.Add(GetWindowRectMitame(parent.hWnd));
                }
            }

            //普通のアプリ
            else
            {
                API.GetCursorPos(out API.POINT cp);
                MyWidndowInfo cursor = GetWindowInfo(API.WindowFromPoint(cp));
                
                List<MyWidndowInfo> prev = GetWindowInfos(
                    GetCmdWindows(cursor.hWnd, API.GETWINDOW_CMD.GW_HWNDPREV, LOOP_LIMIT));

                List<MyWidndowInfo> next = GetWindowInfos(
                    GetCmdWindows(cursor.hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT));

                R = SelectRects(prev).Union(SelectRects(next)).ToList();

                //重なり判定はForegroundのRectと、それ以外のRectを結合したRectで判定する
                //Rectの結合はGeometryGroupを使う
                GeometryGroup gg = new();
                for (int i = 0; i < R.Count; i++)
                {
                    gg.Children.Add(new RectangleGeometry(R[i]));
                }

                //重なり判定
                //重なりがなければメニューウィンドウは開かれていないと判定して
                //収集したRect全破棄
                if (IsOverlapping(gg, new RectangleGeometry(fore.Rect)) == false)
                {
                    R = new();
                }
                //ForeのRectを追加
                R.Add(GetWindowRectMitame(fore.hWnd));

                //PopupのRectを追加
                MyWidndowInfo popup = GetWindowInfo(
                    API.GetWindow(fore.hWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP));
                
                if (popup.Rect.Width != 0) R.Add(popup.Rect);

                //ForegroundのウィンドウRectだけでいい
            }
            return R;
        }
        private void RRR()
        {
            var fore = GetWindowInfo(API.GetForegroundWindow());
            var rootOwner = GetWindowInfo(API.GetAncestor(fore.hWnd, API.AncestorType.GA_ROOTOWNER));
            //var popup = GetWindowInfos(GetCmdWindows(fore.hWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP, LOOP_LIMIT));
            //var next = GetWindowInfos(GetCmdWindows(fore.hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT));
            //var prev = GetWindowInfos(GetCmdWindows(fore.hWnd, API.GETWINDOW_CMD.GW_HWNDPREV, LOOP_LIMIT));
            //var child = GetWindowInfos(GetCmdWindows(fore.hWnd, API.GETWINDOW_CMD.GW_CHILD, LOOP_LIMIT));

            API.GetCursorPos(out API.POINT cp);
            var cursor = GetWindowInfo(API.WindowFromPoint(cp));
            var cursorrootOwner = GetWindowInfo(API.GetAncestor(cursor.hWnd, API.AncestorType.GA_ROOTOWNER));
            var cursorpopup = GetWindowInfos(GetCmdWindows(cursor.hWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP, LOOP_LIMIT));
            var cursornext = GetWindowInfos(GetCmdWindows(cursor.hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT));
            var cursorprev = GetWindowInfos(GetCmdWindows(cursor.hWnd, API.GETWINDOW_CMD.GW_HWNDPREV, LOOP_LIMIT));
            var cursorchild = GetWindowInfos(GetCmdWindows(cursor.hWnd, API.GETWINDOW_CMD.GW_CHILD, LOOP_LIMIT));

        }
        #region エクセルとかリボンメニューのアプリのRect取得      

        //エクセルの右クリックメニュー、リボンメニューのRect収集
        private List<Rect> GetExcelMenuRects()
        {
            IntPtr fore = API.GetForegroundWindow();

            var foreOwnder = GetWindowInfo(API.GetAncestor(fore, API.AncestorType.GA_ROOTOWNER));
            var popup = GetWindowInfo(API.GetWindow(foreOwnder.hWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP));

            //Foreの下層にあるウィンドウハンドルをGetWindowのNEXTで20個程度取得
            List<MyWidndowInfo> foreNexts = GetWindowInfos(GetCmdWindows(fore, API.GETWINDOW_CMD.GW_HWNDNEXT, 20));

            //可視状態のものだけ残す
            var noneZero = foreNexts.Where(x => x.IsVisible == true).ToList();

            //ForeNEXTのRootOWNERとForeOWNERを比較、同じものだけ残す
            List<MyWidndowInfo> nexts = noneZero.Where(x => foreOwnder.Text == GetWindowText(API.GetAncestor(x.hWnd, API.AncestorType.GA_ROOTOWNER))).ToList();

            //見た目通りのRectを取得
            List<Rect> nextRect = nexts.Select(x => GetWindowRectMitame(x.hWnd)).ToList();

            //ForeNEXTを上から順番にRectを見て、0が見つかったらそれ以降は除外
            List<Rect> nextRect2 = SelectNoneZeroRects(nextRect);

            //popupウィンドウのRectを追加
            if (popup.Rect.Width != 0)
            {
                nextRect2.Add(popup.Rect);
            }

            //最後にRootOWNERの見た目通りのRectを追加
            nextRect2.Add(GetWindowRectMitame(foreOwnder.hWnd));
            return nextRect2;

        }


        //RectのListを順番にwidthが0を探して、見つかったらそれ以降のRectは除外して返す
        private List<Rect> SelectNoneZeroRects(List<Rect> rl)
        {
            List<Rect> r = new();
            for (int i = 0; i < rl.Count; i++)
            {
                if (rl[i].Width == 0)
                {
                    return r;
                }
                else
                {
                    r.Add(rl[i]);
                }
            }
            return r;
        }


        #endregion エクセルとかリボンメニューのアプリのRect取得

        #region 通常アプリ系のRect取得
        private List<Rect> SelectRects(List<MyWidndowInfo> pList)
        {
            //可視状態のものだけ残す
            pList = pList.Where(x => x.IsVisible == true).ToList();
            //Textを持つウィンドウ以降を除去
            pList = DeleteWithTextWindow(pList);
            //残ったウィンドウの見た目通りのRect取得
            List<Rect> rs = pList.Select(x => GetWindowRectMitame(x.hWnd)).ToList();
            if (rs.Count == 0) return rs;
            //ドロップシャドウウィンドウのRectを除去
            rs = DeleteShadowRect(rs);
            //サイズが0のRectを除去
            rs = rs.Where(x => x.Size.Width != 0 && x.Size.Height != 0).ToList();
            //前後のRectが重なっているところまで選択して、以降は除外
            return SelectOverlappedRect(rs);
        }

        //WPFのRectの重なり判定、RectangleGeometryにしてからFillContainsWithDetailメソッドでできた
        //https://gogowaten.hatenablog.com/entry/2021/01/28/124714
        /// <summary>
        /// 前後のRectの重なりを判定、重なっていればリストに追加して返す。重なっていないRectが出た時点で終了
        /// </summary>
        /// <param name="rList"></param>
        /// <returns></returns>
        private List<Rect> SelectOverlappedRect(List<Rect> rList)
        {
            List<Rect> result = new();
            if (rList.Count == 0) return result;

            result.Add(rList[0]);

            //順番に判定
            for (int i = 1; i < rList.Count; i++)
            {
                if (IsOverlapping(rList[i - 1], rList[i]))
                {
                    //重なっていればリストに追加
                    result.Add(rList[i]);
                }
                else
                {
                    //途切れたら終了
                    return result;
                }
            }
            return result;
        }
        /// <summary>
        /// 2つのGeometryが一部でも重なっていたらTrueを返す
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        private bool IsOverlapping(Geometry g1, Geometry g2)
        {

            IntersectionDetail detail = g1.FillContainsWithDetail(g2);
            return detail != IntersectionDetail.Empty;
            //return (detail != IntersectionDetail.Empty || detail != IntersectionDetail.NotCalculated, detail);
        }
        /// <summary>
        /// 2つのRectが一部でも重なっていたらtrueを返す
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>        
        private bool IsOverlapping(Rect r1, Rect r2)
        {
            return IsOverlapping(new RectangleGeometry(r1), new RectangleGeometry(r2));
        }
        //IntersectionDetail列挙型
        //Empty             全く重なっていない
        //FullyContains     r2はr1の領域に完全に収まっている
        //FullyInside       r1はr2の領域に完全に収まっている
        //Intersects        一部が重なっている
        //NotCalculated     計算されません(よくわからん)


        /// <summary>
        /// ドロップシャドウ用のウィンドウを判定して、取り除いて返す。前後のRectのtopleftが同じなら後のRectはドロップシャドウと判定する
        /// </summary>
        /// <param name="rList"></param>
        /// <returns></returns>       
        private List<Rect> DeleteShadowRect(List<Rect> rList)
        {
            List<Rect> result = new();
            result.Add(rList[0]);
            Rect preRect = rList[0];//前Rect
            for (int i = 0; i < rList.Count; i++)
            {
                //リストに加えて
                Rect imaRect = rList[i];//後Rect
                result.Add(imaRect);

                //前後の座標が同じ場合は
                if (imaRect.TopLeft == preRect.TopLeft)
                {
                    //サイズが大きい方を削除
                    if (imaRect.Size.Width < preRect.Size.Width)
                    {
                        result.Remove(rList[i - 1]);
                    }
                    else
                    {
                        result.Remove(rList[i]);
                    }
                }
                preRect = imaRect;//前Rectに後Rectを入れて次へ
            }
            return result;
        }

        /// <summary>
        /// Textがないものをリストに追加していって、Textをもつウィンドウが出た時点で終了、リストを返す
        /// </summary>
        /// <param name="wList"></param>
        /// <returns></returns>
        private List<MyWidndowInfo> DeleteWithTextWindow(List<MyWidndowInfo> wList)
        {
            for (int i = 0; i < wList.Count; i++)
            {
                if (wList[i].Text != "")
                {
                    wList.RemoveRange(i, wList.Count - i);
                    return wList;
                }
            }

            return wList;
        }

        #endregion 通常アプリ系のRect取得


        #region Rect取得

        //ウィンドウの見た目通りのRect取得はDwmGetWindowAttribute
        //https://gogowaten.hatenablog.com/entry/2020/11/17/004505
        //見た目通りのRect取得
        private Rect GetWindowRectMitame(IntPtr hWnd)
        {
            //見た目通りのWindowRectを取得
            _ = API.DwmGetWindowAttribute(
                hWnd,
                API.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                out API.RECT myRECT,
                System.Runtime.InteropServices.Marshal.SizeOf(typeof(API.RECT)));

            return MyConverterApiRectToRect(myRECT);
        }
        private Rect MyConverterApiRectToRect(API.RECT rect)
        {
            return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }



        //指定したAPI.GETWINDOW_CMDを収集、自分自身も含む
        private List<IntPtr> GetCmdWindows
            (IntPtr hWnd, API.GETWINDOW_CMD cmd, int loopCount)
        {
            List<IntPtr> v = new();
            v.Add(hWnd);//自分自身

            IntPtr temp = API.GetWindow(hWnd, cmd);
            for (int i = 0; i < loopCount; i++)
            {
                v.Add(temp);
                temp = API.GetWindow(temp, cmd);
            }
            return v;
        }

        private List<MyWidndowInfo> GetWindowInfos(List<IntPtr> hWnd)
        {
            List<MyWidndowInfo> l = new();
            foreach (var item in hWnd)
            {
                l.Add(GetWindowInfo(item));
            }
            return l;
        }
        private MyWidndowInfo GetWindowInfo(IntPtr hWnd)
        {
            return new MyWidndowInfo()
            {
                hWnd = hWnd,
                Rect = GetWindowRect(hWnd),
                Text = GetWindowText(hWnd),
                IsVisible = API.IsWindowVisible(hWnd)
            };

        }

        //ウィンドウハンドルからRect取得
        private Rect GetWindowRect(IntPtr hWnd)
        {
            _ = API.GetWindowRect(hWnd, out API.RECT re);
            return MyConverterApiRectToRect(re);
        }
        //ウィンドウハンドルからRECT取得
        private static API.RECT GetWindowAPIRECT(IntPtr hWnd)
        {
            _ = API.GetWindowRect(hWnd, out API.RECT re);
            return re;
        }

        //ウィンドウハンドルからText取得
        private static string GetWindowText(IntPtr hWnd)
        {
            StringBuilder text = new StringBuilder(65535);
            _ = API.GetWindowText(hWnd, text, 65535);
            return text.ToString();
        }

        #endregion Rect取得


        #region 画像切り抜き
        //WPF、画像から複数箇所を矩形(Rect)に切り抜いて、それぞれ位置を合わせて1枚の画像にしてファイルに保存する - 午後わてんのブログ
        //https://gogowaten.hatenablog.com/entry/2021/01/24/233657

        /// <summary>
        /// 複数Rect範囲を組み合わせた形にbitmapを切り抜く
        /// </summary>
        /// <param name="source">元の画像</param>
        /// <param name="rectList">Rectのコレクション</param>
        /// <returns></returns>
        private BitmapSource CroppedBitmapFromRects(BitmapSource source, List<Rect> rectList)
        {
            var dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                //それぞれのRect範囲で切り抜いた画像を描画していく
                foreach (var rect in rectList)
                {
                    dc.DrawImage(new CroppedBitmap(source, RectToIntRectWith切り捨て(rect)), rect);
                }
            }

            //描画位置調整
            dv.Offset = new Vector(-dv.ContentBounds.X, -dv.ContentBounds.Y);

            //bitmap作成、縦横サイズは切り抜き後の画像全体がピッタリ収まるサイズにする
            //PixelFormatsはPbgra32で決め打ち、これ以外だとエラーになるかも、
            //画像を読み込んだbitmapImageのPixelFormats.Bgr32では、なぜかエラーになった
            var bmp = new RenderTargetBitmap(
                (int)Math.Ceiling(dv.ContentBounds.Width),
                (int)Math.Ceiling(dv.ContentBounds.Height),
                96, 96, PixelFormats.Pbgra32);

            bmp.Render(dv);
            return bmp;
        }

        //RectからInt32Rect作成、小数点以下切り捨て編
        private Int32Rect RectToIntRectWith切り捨て(Rect re)
        {
            return new Int32Rect((int)re.X, (int)re.Y, (int)re.Width, (int)re.Height);
        }



        //bitmapをpng画像ファイルで保存、アプリの実行ファイルと同じフォルダ、ファイル名は年月日_時分秒
        private void SaveImage(BitmapSource source)
        {
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(source));
            string path = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            path += ".png";
            using (var pp = new System.IO.FileStream(
                path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                encoder.Save(pp);
            }
        }
        #endregion 画像切り抜き



        //ウィンドウDCからのキャプチャではアルファ値が変なので、画面全体をキャプチャして切り抜き
        //https://gogowaten.hatenablog.com/entry/2020/11/16/005641
        //仮想画面全体の画像取得
        private BitmapSource GetScreenBitmap()
        {
            var screenDC = API.GetDC(IntPtr.Zero);//仮想画面全体のDC、コピー元
            var memDC = API.CreateCompatibleDC(screenDC);//コピー先DC作成
            int width = (int)SystemParameters.VirtualScreenWidth;
            int height = (int)SystemParameters.VirtualScreenHeight;
            var hBmp = API.CreateCompatibleBitmap(screenDC, width, height);//コピー先のbitmapオブジェクト作成
            API.SelectObject(memDC, hBmp);//コピー先DCにbitmapオブジェクトを指定

            //コピー元からコピー先へビットブロック転送
            //通常のコピーなのでSRCCOPYを指定
            API.BitBlt(memDC, 0, 0, width, height, screenDC, 0, 0, API.SRCCOPY);
            //bitmapオブジェクトからbitmapSource作成
            BitmapSource source =
                Imaging.CreateBitmapSourceFromHBitmap(
                    hBmp,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

            //後片付け
            API.DeleteObject(hBmp);
            _ = API.ReleaseDC(IntPtr.Zero, screenDC);
            _ = API.ReleaseDC(IntPtr.Zero, memDC);

            //画像
            return source;
        }


        #region ホットキー関連
        //アプリのウィンドウが非アクティブ状態でも任意のキーの入力を感知、WPFでグローバルホットキーの登録
        //https://gogowaten.hatenablog.com/entry/2020/12/11/132125
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //ホットキーの登録解除
            _ = API.UnregisterHotKey(MyWindowHandle, HOTKEY_ID1);
            ComponentDispatcher.ThreadPreprocessMessage -= ComponentDispatcher_ThreadPreprocessMessage;
        }

        private void MyInitializeHotKey()
        {
            MyWindowHandle = new WindowInteropHelper(this).Handle;
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }
        private void ChangeHotKey(int mod, Key Key, int hotkeyId)
        {
            ChangeHotKey(mod, KeyInterop.VirtualKeyFromKey(Key), hotkeyId);
        }
        private void ChangeHotKey(int mod, int vKey, int hotkeyId)
        {
            //上書きはできないので、古いのを削除してから登録
            _ = API.UnregisterHotKey(MyWindowHandle, hotkeyId);

            if (API.RegisterHotKey(MyWindowHandle, hotkeyId, mod, vKey) == 0)
            {
                MessageBox.Show("登録に失敗");
            }
            else
            {
                //MessageBox.Show("登録完了");
            }
        }
        #endregion ホットキー関連









        //ウィンドウハンドルからウィンドウの情報用
        //ウィンドウのハンドル、Rect、Text、IsVisible
        private struct MyWidndowInfo
        {
            public IntPtr hWnd;
            public Rect Rect;
            public bool IsVisible;
            public string Text;

            public override string ToString()
            {
                string visible = IsVisible == true ? "可視" : "不可視";
                //x16は書式で、xが16進数で表示、16が表示桁数
                return $"IntPtr({hWnd.ToString("x16")}), Rect({Rect}), {visible}, Text({Text})";
            }
        }

        #region ボタンクリックイベントでの動作
        //画像保存
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(MyBitmapSource);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //クリップボードのpng形式画像を読み込むことができないアプリ用
            //ただし透明部分は真っ黒になる
            Clipboard.SetImage(MyBitmapSource);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            //png形式にして画像をクリップボードにコピー
            //クリップボードのpng形式画像を読み込めるアプリ用
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(MyBitmapSource));
            using var ms = new System.IO.MemoryStream();
            enc.Save(ms);
            Clipboard.SetData("PNG", ms);
        }


        #endregion ボタンクリックイベントでの動作


    }


}

