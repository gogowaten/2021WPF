using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using _20210202_右クリックメニュー取得;
using System.Windows.Interop;

//エクセルのスクショ時にウィンドウ枠外のメニュー、右クリックメニューも同時に撮りたい、WPFとWinAPI - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/02/07/191939


//エクセルのリボンメニューや右クリックメニューがエクセルのウィンドウ枠にある場合も途切れずに取得するテスト
//リボンメニューを使ったアプリならエクセル以外でもキャプチャできるかも
//キャプチャはPrintScreenキー

namespace _20210206_エクセルのメニューRect取得
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
        private const int LOOP_LIMIT = 10;

        public MainWindow()
        {
            InitializeComponent();

            MyInitializeHotKey();

            //ホットキーにPrintScreenキーを登録
            ChangeHotKey(Key.PrintScreen, HOTKEY_ID1);

            //アプリ終了時にホットキーの解除
            Closing += MainWindow_Closing;
        }

        //ホットキー判定
        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != API.WM_HOTKEY) return;

            //ホットキー(今回はPrintScreen)が押されたら
            else if (msg.wParam.ToInt32() == HOTKEY_ID1)
            {
                //画面全体をキャプチャして、Rect収集して、それを使って切り抜き画像作成して表示
                MyImage.Source = CroppedBitmapFromRects(GetScreenBitmap(), GetExcelMenuRects());
            }
        }


        #region エクセルとかリボンメニューのアプリのRect取得      

        //エクセルの右クリックメニュー、リボンメニューのRect収集
        private List<Rect> GetExcelMenuRects()
        {
            IntPtr fore = API.GetForegroundWindow();
            
            var foreOwnder = GetWindowInfo(API.GetAncestor(fore, API.AncestorType.GA_ROOTOWNER));
            var popup = GetWindowInfo(API.GetWindow(foreOwnder.hWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP));

            //Foreの下層にあるウィンドウハンドルをGetWindowのNEXTで20個程度取得
            List<MyStruct> foreNexts = GetWindowInfos(GetCmdWindows(fore, API.GETWINDOW_CMD.GW_HWNDNEXT, 20));
                        
            //可視状態のものだけ残す
            var noneZero = foreNexts.Where(x => x.IsVisible == true).ToList();

            //ForeNEXTのRootOWNERとForeOWNERを比較、同じものだけ残す
            List<MyStruct> nexts = noneZero.Where(x => foreOwnder.Text == GetWindowText(API.GetAncestor(x.hWnd, API.AncestorType.GA_ROOTOWNER))).ToList();

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

        private List<MyStruct> GetWindowInfos(List<IntPtr> hWnd)
        {
            List<MyStruct> l = new();
            foreach (var item in hWnd)
            {
                l.Add(GetWindowInfo(item));
            }
            return l;
        }
        private MyStruct GetWindowInfo(IntPtr hWnd)
        {
            return new MyStruct() { 
                hWnd = hWnd, 
                Rect = GetWindowRect(hWnd),
                Text = GetWindowText(hWnd),
                IsVisible = API.IsWindowVisible(hWnd) };

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
        private void ChangeHotKey(Key Key, int hotkeyId)
        {
            ChangeHotKey(KeyInterop.VirtualKeyFromKey(Key), hotkeyId);
        }
        private void ChangeHotKey(int vKey, int hotkeyId)
        {
            //上書きはできないので、古いのを削除してから登録
            _ = API.UnregisterHotKey(MyWindowHandle, hotkeyId);

            //int mod = GetModifierKeySum();
            //int mod = 2;//ctrl
            //int mod = 1;//alt
            //int mod = 4;//shift
            //int mod = 6;//ctrl + shift
            //int mod = 0;//修飾キーなし
            int mod = 0;
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
        private struct MyStruct
        {
            public IntPtr hWnd;
            public Rect Rect;
            public bool IsVisible;
            public string Text;

            public override string ToString()
            {
                //x16は書式で、xが16進数で表示、16が表示桁数
                return $"IntPtr({hWnd.ToString("x16")}), Rect({Rect}), 可視{IsVisible}, Text({Text})";
            }
        }
    }


}

