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

namespace _20210117_Getメニューウィンドウ
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

            this.Top = 0; this.Left = 0;
            MyInitializeHotKey();
            //ホットキーにPrintScreenキーを登録
            ChangeHotKey(Key.PrintScreen, HOTKEY_ID1);

            Closing += MainWindow_Closing;
        }

        #region ホットキー関連
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
        # endregion ホットキー関連

        //ホットキー判定
        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != API.WM_HOTKEY) return;
            else if (msg.wParam.ToInt32() == HOTKEY_ID1)
            {
                //var aa = GetWindowRectAndText(API.GetWindow(API.GetForegroundWindow(), API.GETWINDOW_CMD.GW_CHILD));
                //var bb = GetWindowRectAndText(API.GetWindow(API.GetForegroundWindow(), API.GETWINDOW_CMD.GW_HWNDPREV));
                //var cc = GetWindowRectAndText(API.GetWindow(API.GetForegroundWindow(), API.GETWINDOW_CMD.GW_HWNDNEXT));
                //var dd = GetWindowRectAndText(API.GetWindow(API.GetForegroundWindow(), API.GETWINDOW_CMD.GW_HWNDFIRST));
                //var ee = GetWindowRectAndText(API.GetWindow(API.GetForegroundWindow(), API.GETWINDOW_CMD.GW_HWNDLAST));
                //var ff = GetWindowRectAndText(API.GetWindow(API.GetForegroundWindow(), API.GETWINDOW_CMD.GW_OWNER));
                //var desktop = GetWindowRectAndText(API.GetDesktopWindow());
                //var desktopPop = GetWindowRectAndText(API.GetWindow(desktop.Item1, API.GETWINDOW_CMD.GW_ENABLEDPOPUP));
                //var desktopChi = GetWindowRectAndText(API.GetWindow(desktop.Item1, API.GETWINDOW_CMD.GW_CHILD));
                //var desktopFir = GetWindowRectAndText(API.GetWindow(desktop.Item1, API.GETWINDOW_CMD.GW_HWNDFIRST));
                //var desktopLas = GetWindowRectAndText(API.GetWindow(desktop.Item1, API.GETWINDOW_CMD.GW_HWNDLAST));
                //var desktopNex = GetWindowRectAndText(API.GetWindow(desktop.Item1, API.GETWINDOW_CMD.GW_HWNDNEXT));
                //var desktopPre = GetWindowRectAndText(API.GetWindow(desktop.Item1, API.GETWINDOW_CMD.GW_HWNDPREV));
                //var desktopOwn = GetWindowRectAndText(API.GetWindow(desktop.Item1, API.GETWINDOW_CMD.GW_OWNER));
                //var shell = GetWindowRectAndText(API.GetShellWindow());
                //var topw = GetWindowRectAndText(API.GetTopWindow(IntPtr.Zero));
                //var topf = GetWindowRectAndText(API.GetTopWindow(API.GetForegroundWindow()));





                //WindowInfos(API.GetForegroundWindow());

                //マウスカーソル下のWindow
                //_ = API.GetCursorPos(out API.POINT cursorPoint);
                //WindowInfos(API.WindowFromPoint(cursorPoint));

                //ForeのMenu
                //WindowInfos(API.GetMenu(API.GetForegroundWindow()));

                //GetWindowでForeのENABLEDPOPUP
                //WindowInfos(API.GetWindow(API.GetForegroundWindow(), API.GETWINDOW_CMD.GW_ENABLEDPOPUP));

                //var reList = GetRects();
                var reList2 = GetRects2();
                var bmp = CroppedBitmapFromRects(ScreenCapture(), reList2);
            }

        }

        //見た目通りのRect取得
        private Rect GetWindowRectMitame(IntPtr hWnd)
        {
            //見た目通りのWindowRectを取得
            API.RECT myRECT;
            API.DwmGetWindowAttribute(
                hWnd,
                API.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                out myRECT, System.Runtime.InteropServices.Marshal.SizeOf(typeof(API.RECT)));

            return MyConvertApiRectToRect(myRECT);
        }

        //private BitmapSource CroppedRectsBitmap(BitmapSource source, List<Rect> rList)
        //{
        //    DrawingVisual dv = new();
        //    foreach (var item in rList)
        //    {
        //        using (var dc = dv.RenderOpen())
        //        {
        //            dc.DrawImage(new CroppedBitmap(source, MyConvertRect(item)), item);
        //        }
        //    }
        //    dv.Offset = new Vector(-dv.ContentBounds.X, -dv.ContentBounds.Y);

        //    var bmp = new RenderTargetBitmap(
        //        (int)dv.ContentBounds.Width, (int)dv.ContentBounds.Height,
        //        96, 96, PixelFormats.Pbgra32);
        //    bmp.Render(dv);
        //    return bmp;
        //}
        //private Int32Rect MyConvertRect(Rect rect)
        //{
        //    return new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        //}
        //private Int32Rect MyConvertRect(Int32Rect rect)
        //{
        //    return new Int32Rect(rect.X, rect.Y, rect.Width, rect.Height);
        //}

        private List<Rect> GetRects2()
        {
            //カーソル下のウィンドウを基準にRootOwnerWindowまでのRectを収集
            _ = API.GetCursorPos(out API.POINT myPoint);
            IntPtr w = API.WindowFromPoint(myPoint);
            var neko = GetWindowRectAndText(w);
            var row = API.GetAncestor(w, API.AncestorType.GA_ROOTOWNER);
            List<Rect> reList = new();
            var temp = GetWindowsToWithTextNexts2(w, 20);
            Rect preR;
            for (int i = 0; i < temp.ptrs.Count; i++)
            {
                Rect imaR = GetWindowRectMitame(temp.ptrs[i]);
                //Rect imaR = MyConvertApiRectToRect(temp.res[i]);
                if (preR.TopLeft != imaR.TopLeft)
                {
                    reList.Add(imaR);                    
                    preR = imaR;
                }
            }
            //最前面のを付け足す
            reList.Add(GetWindowRectMitame(API.GetForegroundWindow()));
            return reList;
        }

        private List<Rect> GetRects()
        {
            //カーソル下のウィンドウを基準にRootOwnerWindowまでのRectを収集
            _ = API.GetCursorPos(out API.POINT myPoint);
            IntPtr w = API.WindowFromPoint(myPoint);
            var neko = GetWindowRectAndText(w);
            var row = API.GetAncestor(w, API.AncestorType.GA_ROOTOWNER);
            List<Rect> reList = new();
            //var temp = GetWindowsOwner(w, 20);
            //var temp = GetWindowsParent(w, 20);
            //var temp = GetWindowsToWithTextOwner(w, 20);
            //var temp = GetWindowsToWithTextNexts(w, 20);
            //var temp = GetWindowsToWithTextAncestor(w, 20, API.AncestorType.GA_ROOT);
            var temp = GetWindowsToWithTextAncestor(w, 20, API.AncestorType.GA_PARENT);
            foreach (var item in temp.res)
            {
                reList.Add(MyConvertApiRectToRect(item));
            }
            List<string> strList = new();
            for (int i = 0; i < temp.ptrs.Count; i++)
            {
                strList.Add(GetWindowText(API.GetAncestor(temp.ptrs[i], API.AncestorType.GA_ROOTOWNER)));
            }

            return reList;
        }
        private Rect MyConvertApiRectToRect(API.RECT rect)
        {
            return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

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


        private void WindowInfos(IntPtr hWnd)
        {
            var wnd = GetWindowRectAndText(hWnd);
            var parentWindows = GetWindowsParent(hWnd, LOOP_LIMIT);
            var nextWindows = GetWindowsCMD(hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT);
            var ownerWindows = GetWindowsCMD(hWnd, API.GETWINDOW_CMD.GW_OWNER, LOOP_LIMIT);
            var ancestorParent = GetWindowRectAndText(API.GetAncestor(hWnd, API.AncestorType.GA_PARENT));
            var ancestorRoot = GetWindowRectAndText(API.GetAncestor(hWnd, API.AncestorType.GA_ROOT));
            var ancestorRootOwner = GetWindowRectAndText(API.GetAncestor(hWnd, API.AncestorType.GA_ROOTOWNER));

            var submenu0 = GetWindowRectAndText(API.GetSubMenu(hWnd, 0));
            var submenu1 = GetWindowRectAndText(API.GetSubMenu(hWnd, 1));
            var submenu2 = GetWindowRectAndText(API.GetSubMenu(hWnd, -1));

        }

        //
        /// <summary>
        /// GetWindowでCMDを指定して指定回数まで遡って、すべて取得
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="cmd">API.GETWINDOW_CMDのどれかを指定</param>
        /// <param name="loopCount">取得ウィンドウ数上限</param>
        /// <returns></returns>
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetWindowsCMD(IntPtr hWnd, API.GETWINDOW_CMD cmd, int loopCount)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;

            var temp = API.GetWindow(hWnd, cmd);

            //cmdが一致するウィンドウが無くなるまで辿る
            while (temp != IntPtr.Zero && count < loopCount)
            {
                ptrs.Add(temp);
                res.Add(GetWindowRect(temp));
                strs.Add(GetWindowText(temp));

                temp = API.GetWindow(temp, cmd);
                count++;
            }
            return (ptrs, res, strs);
        }


        //AncestorWindowを指定回数まで遡って、すべて取得
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetWindowsToWithTextAncestor(IntPtr hWnd, int loopCount, API.AncestorType type)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;

            IntPtr temp = hWnd;
            string text;

            //text付きNextが見つかるまで辿る
            do
            {
                ptrs.Add(temp);
                res.Add(GetWindowRect(temp));
                text = GetWindowText(temp);
                strs.Add(text);

                temp = API.GetAncestor(temp, type);
                count++;
            } while (text == "" && temp != IntPtr.Zero && count < loopCount);

            return (ptrs, res, strs);
        }

        //NextWindowを指定回数まで遡って、すべて取得
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetWindowsToWithTextNexts2(IntPtr hWnd, int loopCount)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;

            IntPtr temp = hWnd;
            string text;

            //text付きNextが見つかるまで辿る
            do
            {
                text = GetWindowText(temp);
                if (text != "") break;
                ptrs.Add(temp);
                res.Add(GetWindowRect(temp));
                strs.Add(text);

                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_HWNDNEXT);
                count++;
            } while (text == "" && temp != IntPtr.Zero && count < loopCount);

            return (ptrs, res, strs);
        }

        //NextWindowを指定回数まで遡って、すべて取得
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetWindowsToWithTextNexts(IntPtr hWnd, int loopCount)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;

            IntPtr temp = hWnd;
            string text;

            //text付きNextが見つかるまで辿る
            do
            {
                ptrs.Add(temp);
                res.Add(GetWindowRect(temp));
                text = GetWindowText(temp);
                strs.Add(text);

                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_HWNDNEXT);
                count++;
            } while (text == "" && temp != IntPtr.Zero && count < loopCount);

            return (ptrs, res, strs);
        }

        //NextWindowをすべて取得
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetWindowsNext(IntPtr hWnd, int loopCount)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;

            var temp = API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT);

            //Nextが無くなるまで辿る
            while (temp != IntPtr.Zero && count < loopCount)
            {
                ptrs.Add(temp);
                res.Add(GetWindowRect(temp));
                strs.Add(GetWindowText(temp));

                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_HWNDNEXT);
                count++;
            }
            return (ptrs, res, strs);
        }



        //すべてのParentWindowを指定回数まで遡って取得
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetWindowsParent(IntPtr hWnd, int loopCount)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;
            IntPtr temp = hWnd;

            //Parentが無くなるまで辿る
            do
            {
                ptrs.Add(temp);
                res.Add(GetWindowRect(temp));
                strs.Add(GetWindowText(temp));

                temp = API.GetParent(temp);
                count++;
            } while (temp != IntPtr.Zero && count < loopCount);

            return (ptrs, res, strs);
        }

        //すべてのOwnerWindowを指定回数まで遡って取得
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetWindowsOwner(IntPtr hWnd, int loopCount)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;
            IntPtr temp = hWnd;

            //Ownerが無くなるまで辿る
            do
            {
                ptrs.Add(temp);
                res.Add(GetWindowRect(temp));
                strs.Add(GetWindowText(temp));

                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_OWNER);
                count++;
            } while (temp != IntPtr.Zero && count < loopCount);

            return (ptrs, res, strs);
        }

        //OwnerWindowを指定回数まで遡って、すべて取得
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetWindowsToWithTextOwner(IntPtr hWnd, int loopCount)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;

            var temp = API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_OWNER);

            //Ownerが無くなるまで辿る
            while (temp != IntPtr.Zero && count < loopCount)
            {
                ptrs.Add(temp);
                res.Add(GetWindowRect(temp));
                strs.Add(GetWindowText(temp));

                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_OWNER);
                count++;
            }
            return (ptrs, res, strs);
        }

        private IntPtr GetWindowUnderCursor()
        {
            _ = API.GetCursorPos(out API.POINT cursorP);
            return API.WindowFromPoint(cursorP);
        }



        /// <summary>
        /// WindowのRECTとテキスト(タイトル)取得
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns></returns>
        private (IntPtr, API.RECT re, string text) GetWindowRectAndText(IntPtr hWnd)
        {
            return (hWnd, GetWindowRect(hWnd), GetWindowText(hWnd));
        }
        private API.RECT GetWindowRect(IntPtr hWnd)
        {
            _ = API.GetWindowRect(hWnd, out API.RECT re);
            return re;
        }
        private string GetWindowText(IntPtr hWnd)
        {
            var text = new StringBuilder(65535);
            _ = API.GetWindowText(hWnd, text, 65535);
            return text.ToString();
        }


        private BitmapSource ScreenCapture()
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


    }
}
