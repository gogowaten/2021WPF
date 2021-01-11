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

using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Interop;

namespace _20210101_GetWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //ホットキー
        private const int HOTKEY_ID1 = 0x0001;//ID
        private IntPtr MyWindowHandle;//アプリのハンドル


        public MainWindow()
        {
            InitializeComponent();

            int a = 2;
            int b = 4;
            int c = a + b;
            var d = c & a;
            var e = c & b;
            var f = Convert.ToString(c, 2);
            uint g = 0x8000_0000;
            var h = Convert.ToString(g, 2);
            var i = g >> 8;


            MyInitializeHotKey();
            //ホットキー登録
            ChangeHotKey(Key.PrintScreen, HOTKEY_ID1);

            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //ホットキーの登録解除
            _ = API.UnregisterHotKey(MyWindowHandle, HOTKEY_ID1);
            ComponentDispatcher.ThreadPreprocessMessage -= ComponentDispatcher_ThreadPreprocessMessage;
        }

        private IntPtr GetParentWindowFromForegroundWindow()
        {
            //最前面ウィンドウを起点にWindowTextがあるもの(GetWindowTextの戻り値が0以外)をGetParentで10回まで辿る            
            //見つからなかった場合は最前面ウィンドウのハンドルにする
            IntPtr hForeWnd = API.GetForegroundWindow();
            var wndText = new StringBuilder(65535);
            int count = 0;
            IntPtr hWnd = hForeWnd;
            while (API.GetWindowText(hWnd, wndText, 65535) == 0)
            {
                hWnd = API.GetParent(hWnd);
                count++;
                if (count > 10)
                {
                    hWnd = hForeWnd;
                    break;
                }
            }
            return hWnd;
        }

        //最前面ウィンドウから親ウィンドウを辿って、すべてのウィンドウのRECTを返す
        private List<API.RECT> GetWindowRects()
        {
            IntPtr hForeWnd = API.GetForegroundWindow();
            var wndText = new StringBuilder(65535);
            int count = 0;
            IntPtr hWnd = hForeWnd;
            List<API.RECT> reList = new();
            API.GetWindowRect(hWnd, out API.RECT re);
            reList.Add(re);

            while (API.GetWindowText(hWnd, wndText, 65535) == 0)
            {
                hWnd = API.GetParent(hWnd);
                API.GetWindowRect(hWnd, out re);
                reList.Add(re);
                count++;
                if (count > 10)
                {
                    hWnd = hForeWnd;
                    break;
                }
            }
            return reList;
        }

        //対象ウィンドウのCmd全部のRECTを取得
        private (List<string>, Dictionary<API.GETWINDOW_CMD, API.RECT>) GetCMDRects(IntPtr hWnd)
        {
            Dictionary<API.GETWINDOW_CMD, API.RECT> result = new();
            List<string> strList = new();
            var text = new StringBuilder(65535);
            var cmd = Enum.GetValues(typeof(API.GETWINDOW_CMD)).Cast<API.GETWINDOW_CMD>();
            IntPtr wnd = IntPtr.Zero;
            foreach (var item in cmd)
            {
                wnd = API.GetWindow(hWnd, item);
                _ = API.GetWindowRect(wnd, out API.RECT re);
                result.Add(item, re);
                API.GetWindowText(wnd, text, 65535);
                strList.Add(text.ToString());
            }

            return (strList, result);
        }
        //private IntPtr GetChildWindow(IntPtr hWnd)
        //{
        //    return API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_CHILD);
        //}


        //
        private List<API.RECT> GetChildWindowRect(IntPtr hWndParent)
        {
            List<API.RECT> reList = new();
            var childe = API.GetWindow(hWndParent, API.GETWINDOW_CMD.GW_CHILD);
            IntPtr temp = API.GetWindow(childe, API.GETWINDOW_CMD.GW_HWNDNEXT);
            while (temp != IntPtr.Zero || reList.Count < 20)
            {
                API.GetWindowRect(temp, out API.RECT re);
                reList.Add(re);

                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_HWNDNEXT);
            }
            return reList;
        }
        private List<API.RECT> GetWindowRects(IntPtr hWnd, API.GETWINDOW_CMD cmd)
        {
            List<API.RECT> reList = new();
            IntPtr temp = API.GetWindow(hWnd, cmd);
            while (temp != IntPtr.Zero || reList.Count < 20)
            {
                API.GetWindowRect(temp, out API.RECT re);
                reList.Add(re);
                temp = API.GetWindow(temp, cmd);
            }
            return reList;
        }
        private (List<string>, List<API.RECT>) GetNextWindowTextAndRects(IntPtr hWnd)
        {
            List<string> strList = new();
            List<API.RECT> reList = new();

            IntPtr temp = API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT);
            StringBuilder text = new StringBuilder(65535);

            while (temp != IntPtr.Zero || strList.Count < 20)
            {
                API.GetWindowRect(temp, out API.RECT re);
                API.GetWindowText(temp, text, 65535);
                strList.Add(text.ToString());
                reList.Add(re);
                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_HWNDNEXT);
            }
            return (strList, reList);
        }
        private (List<string>, List<API.RECT>) GetPrevWindowTextAndRects(IntPtr hWnd)
        {
            List<string> strList = new();
            List<API.RECT> reList = new();
            API.GETWINDOW_CMD cmd = API.GETWINDOW_CMD.GW_HWNDPREV;

            IntPtr temp = API.GetWindow(hWnd, cmd);
            StringBuilder text = new StringBuilder(65535);

            while (temp != IntPtr.Zero || strList.Count < 20)
            {
                API.GetWindowRect(temp, out API.RECT re);
                API.GetWindowText(temp, text, 65535);
                strList.Add(text.ToString());
                reList.Add(re);
                temp = API.GetWindow(temp, cmd);
            }
            return (strList, reList);
        }

        //ホットキー動作
        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != API.WM_HOTKEY) return;
            else if (msg.wParam.ToInt32() == HOTKEY_ID1)
            {
                IntPtr fHnd = API.GetForegroundWindow();
                var Fore = GetWindowRectAndText(fHnd);
                var ForeのChilds = GetChilds(fHnd);
                var ForeのOwner = GetOwner(fHnd);
                var ForeのOwnerすべて = GetOwners(fHnd, 20);

                var lp = GetWindowRectAndText(API.GetLastActivePopup(ForeのOwner.ptr));
                var tw = GetWindowRectAndText(API.GetTopWindow(ForeのOwner.ptr));
                var ttw = GetWindowRectAndText(API.GetTopWindow(IntPtr.Zero));

            }
        }

        //タイトル付きのOwnerすべてを取得
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetOwners(IntPtr ptr, int loopCount)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;

            var temp = API.GetWindow(ptr, API.GETWINDOW_CMD.GW_OWNER);

            //Ownerが無くなるまで辿る
            while (temp != IntPtr.Zero && count < loopCount)
            {
                string str = GetWindowText(temp);
                //タイトル付きならリストに追加
                if (str != "")
                {
                    ptrs.Add(temp);
                    res.Add(GetWindowRect(temp));
                    strs.Add(str);
                }

                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_OWNER);
                count++;
            }
            return (ptrs, res, strs);
        }

        //タイトル付きのOwner取得
        private (IntPtr ptr, API.RECT re, string str) GetOwner(IntPtr hWnd)
        {
            int count = 0;
            IntPtr ptr = hWnd;
            string str = GetWindowText(hWnd);
            while (str == "" && count < 20)
            {
                ptr = API.GetWindow(ptr, API.GETWINDOW_CMD.GW_OWNER);
                str = GetWindowText(ptr);
                count++;
            }
            _ = API.GetWindowRect(ptr, out API.RECT re);
            return (ptr, re, str);
        }

        //RECTとText取得
        private (API.RECT rect, string text) GetWindowRectAndText(IntPtr hWnd)
        {
            return (GetWindowRect(hWnd), GetWindowText(hWnd));
        }

        //すべてのChild
        private (List<IntPtr> ptrs, List<API.RECT> rects, List<string> text) GetChilds(IntPtr hWnd)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> rects = new();
            List<string> text = new();
            IntPtr temp = API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_CHILD);
            while (temp != IntPtr.Zero)
            {
                ptrs.Add(temp);
                rects.Add(GetWindowRect(temp));
                text.Add(GetWindowText(temp));
                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_CHILD);
            }
            return (ptrs, rects, text);
        }
        private API.RECT GetWindowRect(IntPtr hWnd)
        {
            API.GetWindowRect(hWnd, out API.RECT re);
            return re;
        }
        private string GetWindowText(IntPtr hWnd)
        {
            var sb = new StringBuilder(65535);
            _ = API.GetWindowText(hWnd, sb, 65535);
            return sb.ToString();
        }
        private void Test1()
        {
            //MessageBox.Show("ホットキーーーーーーーーーーーー");
            IntPtr foreWnd = API.GetForegroundWindow();
            var parentWnd = API.GetParent(foreWnd);
            var owner = API.GetWindow(foreWnd, API.GETWINDOW_CMD.GW_OWNER);

            var tstr = GetOwnerWidndowWithText();
            var tstr2 = GetOwnerWidndowWithText(foreWnd);
            var foreのOwnerのChild = GetWindowEx(tstr2.Item1, API.GETWINDOW_CMD.GW_CHILD);
            var ForeのOwnerのPopup = GetWindowEx(tstr2.Item1, API.GETWINDOW_CMD.GW_ENABLEDPOPUP);
            var ForeのOwner = GetWindowEx(foreWnd, API.GETWINDOW_CMD.GW_OWNER);
            var ForeのOwnerのOwner = GetWindowEx(ForeのOwner.Item1, API.GETWINDOW_CMD.GW_OWNER);
            var ForeのOwnerのPopupのOwner = GetWindowEx(ForeのOwnerのPopup.Item1, API.GETWINDOW_CMD.GW_OWNER);
            var ForeのPopup = GetWindowEx(foreWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP);

            var activePop = API.GetLastActivePopup(foreWnd);//常にForegroundWindowになる
            var foreInfo = GetWindowInfo(foreWnd);
            var ispop = IsPopupWindow(foreWnd);

            var npw = GetNextPopUpWindows(foreWnd);
            var npwo = GetNextPopUpWindows2(foreWnd);
            var ppwo = GetPrevPopUpWindows2(foreWnd);


            string title1 = GetWindowTitle(foreWnd);
            var foreCMDRects = GetCMDRects(foreWnd);
            var foreNextRects = GetNextWindowTextAndRects(foreWnd);
            var forePrevRects = GetPrevWindowTextAndRects(foreWnd);
            var wInfo = new API.WINDOWINFO();
            wInfo.cbSize = Marshal.SizeOf(wInfo);
            API.GetWindowInfo(foreWnd, ref wInfo);
            var f = Convert.ToString(wInfo.dwStyle, 2);
            var g = Convert.ToString(wInfo.dwStyle, 16);
            var h = wInfo.dwStyle & 0x8000_0000;
            var i = h == 0x8000_0000;
            var j = Convert.ToString(h, 16);
            (string, IntPtr) pp = GetParentWithTitle(foreWnd);
            (string, IntPtr ioo) oo = GetOwnerWithTitle(foreWnd);
            //var gp = GetPopupWindows(oo.ioo);
            var gpt = GetPopupToTaitleWindow(API.GetWindow(foreWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP));
            var gptp = GetPopupToParentTaitleWindow(API.GetWindow(foreWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP));
            string title2 = GetWindowTitle(foreWnd);

            var parentCMDRects = GetCMDRects(parentWnd);
            var parentNextRects = GetNextWindowTextAndRects(parentWnd);
            var parentPrevRects = GetPrevWindowTextAndRects(parentWnd);

            var ownCMDRects = GetCMDRects(owner);
            var ownNextRects = GetNextWindowTextAndRects(owner);
            var ownPrevRects = GetPrevWindowTextAndRects(owner);

            _ = API.GetWindowRect(foreWnd, out API.RECT fRe);
            _ = API.GetWindowRect(parentWnd, out API.RECT pRe);
            _ = API.GetWindowRect(owner, out API.RECT oRe);

            var foreChildWnd = API.GetWindow(foreWnd, API.GETWINDOW_CMD.GW_CHILD);
            var foreChildCMDRects = GetCMDRects(foreChildWnd);
            var foreChildsNextRect = GetNextWindowTextAndRects(foreChildWnd);
            var foreChildsPrevRect = GetPrevWindowTextAndRects(foreChildWnd);

            var parentChild = API.GetWindow(parentWnd, API.GETWINDOW_CMD.GW_CHILD);
            var parentChildCMDRects = GetCMDRects(parentChild);
            var parentChildsNextRect = GetNextWindowTextAndRects(parentChild);
            var parentChildsPrevRect = GetPrevWindowTextAndRects(parentChild);

            var ownChild = API.GetWindow(owner, API.GETWINDOW_CMD.GW_CHILD);
            var ownChildCMDRects = GetCMDRects(ownChild);
            var ownChildsNextRect = GetNextWindowTextAndRects(ownChild);
            var ownPrevChildsNextRect = GetPrevWindowTextAndRects(ownChild);

            API.GetWindowRect(foreWnd, out API.RECT foreRect);
            API.GetWindowRect(parentWnd, out API.RECT PareRect);
            API.GetWindowRect(foreChildWnd, out API.RECT ChildRect);
            API.GetWindowRect(owner, out API.RECT OwnerRect);
            API.GetWindowRect(API.GetWindow(foreWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP), out API.RECT popupRect);
            API.GetWindowRect(API.GetWindow(parentWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP), out API.RECT popupRectParent);
            API.GetWindowRect(API.GetWindow(foreChildWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP), out API.RECT popupRectChild);
            API.GetWindowRect(API.GetWindow(owner, API.GETWINDOW_CMD.GW_ENABLEDPOPUP), out API.RECT popupRectOwner);


            //var textFore = new StringBuilder(65535);
            //API.GetWindowText(foreWnd, textFore, 65535);

            //var stb = new StringBuilder(65535);
            //int wndText = API.GetWindowText(parentWnd, stb, 65535);

        }

        private (List<IntPtr>, List<API.RECT>) GetPrevPopUpWindows2(IntPtr hWnd)
        {
            int count = 0;
            List<IntPtr> wnd = new();
            List<API.RECT> reList = new();
            (string, IntPtr) owner = GetOwnerWithTitle(hWnd);

            IntPtr temp = API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_HWNDPREV);
            while (temp != IntPtr.Zero || count < 20)
            {
                if (GetOwnerWithTitle(temp).Item1 == owner.Item1 && IsPopupWindow(temp))
                {
                    API.GetWindowRect(temp, out API.RECT re);
                    wnd.Add(temp);
                    reList.Add(re);
                }
                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_HWNDPREV);
                count++;
            }
            return (wnd, reList);
        }
        private (List<IntPtr>, List<API.RECT>) GetNextPopUpWindows2(IntPtr hWnd)
        {
            int count = 0;
            List<IntPtr> wnd = new();
            List<API.RECT> reList = new();
            (string, IntPtr) owner = GetOwnerWithTitle(hWnd);

            IntPtr temp = API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT);
            while (temp != IntPtr.Zero || count < 20)
            {
                if (GetOwnerWithTitle(temp).Item1 == owner.Item1 && IsPopupWindow(temp))
                {
                    API.GetWindowRect(temp, out API.RECT re);
                    wnd.Add(temp);
                    reList.Add(re);
                }
                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_HWNDNEXT);
                count++;
            }
            return (wnd, reList);
        }
        //同じOwnerWindowならtrueを返す
        private bool IsSameOwner(IntPtr owner, IntPtr hWnd)
        {
            (string title, IntPtr owner) ot = GetOwnerWithTitle(hWnd);
            return ot.owner == owner;
        }

        private (List<IntPtr>, List<API.RECT>) GetNextPopUpWindows(IntPtr hWnd)
        {
            List<IntPtr> wnd = new();
            List<API.RECT> reList = new();

            IntPtr temp = API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT);
            while (temp != IntPtr.Zero || wnd.Count < 20)
            {
                if (IsPopupWindow(temp))
                {
                    API.GetWindowRect(temp, out API.RECT re);
                    wnd.Add(temp);
                    reList.Add(re);
                }
                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_HWNDNEXT);
            }
            return (wnd, reList);
        }

        //WindowのStyleがPopupならtrueを返す
        private bool IsPopupWindow(IntPtr hWnd)
        {
            API.WINDOWINFO wi = GetWindowInfo(hWnd);
            uint pop = (uint)API.WINDOW_STYLE.WS_POPUP;
            var style = wi.dwStyle & pop;
            return style == pop;
        }

        private API.WINDOWINFO GetWindowInfo(IntPtr hWnd)
        {
            API.WINDOWINFO wi = new();
            wi.cbSize = Marshal.SizeOf(wi);
            API.GetWindowInfo(hWnd, ref wi);
            return wi;
        }


        /// <summary>
        /// WindowとCmdを渡して、Cmdに当てはまるWindowと、そのRECTとTextを返す
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="cmd">API.GETWINDOW_CMD</param>
        /// <returns></returns>
        private (IntPtr, API.RECT, string) GetWindowEx(IntPtr hWnd, API.GETWINDOW_CMD cmd)
        {
            var str = new StringBuilder(65535);
            IntPtr hh = API.GetWindow(hWnd, cmd);
            _ = API.GetWindowText(hh, str, 65535);
            API.GetWindowRect(hh, out API.RECT re);
            return (hh, re, str.ToString());
        }



        /// <summary>
        /// ForegroundWindowからWindowTextがあるOwnerWindowを辿って返す
        /// </summary>
        /// <returns></returns>
        private (IntPtr, string) GetOwnerWidndowWithText()
        {
            return GetOwnerWidndowWithText(API.GetForegroundWindow());
        }
        /// <summary>
        /// 渡したWindowからWindowTextがあるOwnerWindowを辿って返す
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>       
        private (IntPtr, string) GetOwnerWidndowWithText(IntPtr hWnd)
        {
            var str = new StringBuilder(65535);
            int count = 0;
            IntPtr h = hWnd;
            _ = API.GetWindowText(h, str, 65535);
            while (str.ToString() == "" && count < 20)
            {
                h = API.GetWindow(h, API.GETWINDOW_CMD.GW_OWNER);
                API.GetWindowText(h, str, 65535);
                count++;
            }
            if (str.ToString() == "") { return (IntPtr.Zero, str.ToString()); }
            else return (h, str.ToString());
        }
        private string GetWindowTitle(IntPtr hWnd)
        {
            var str = new StringBuilder(65535);
            API.GetWindowText(hWnd, str, 65535);
            return str.ToString();
        }
        //ポップアップからタイトル付きのウィンドウまでOwnerで辿る
        //ポップアップのハンドルを渡す
        private (string, List<API.RECT>) GetPopupToTaitleWindow(IntPtr hWnd)
        {
            int count = 0;
            StringBuilder sb = new StringBuilder(65535);
            string title = "";
            List<API.RECT> reList = new();

            API.GetWindowRect(hWnd, out API.RECT re);
            reList.Add(re);

            IntPtr owner = hWnd;
            while (owner != IntPtr.Zero && title == "" && count < 20)
            {
                owner = API.GetWindow(owner, API.GETWINDOW_CMD.GW_OWNER);
                API.GetWindowText(owner, sb, 65535);
                title = sb.ToString();
                API.GetWindowRect(owner, out re);
                reList.Add(re);
                count++;
            }
            return (title, reList);
        }
        //ポップアップからタイトル付きのウィンドウまでParentで辿る
        //ポップアップのハンドルを渡す
        private (string, List<API.RECT>) GetPopupToParentTaitleWindow(IntPtr hWnd)
        {
            int count = 0;
            StringBuilder sb = new StringBuilder(65535);
            string title = "";
            List<API.RECT> reList = new();

            API.GetWindowRect(hWnd, out API.RECT re);
            reList.Add(re);

            IntPtr parent = hWnd;
            while (parent != IntPtr.Zero && title == "" && count < 20)
            {
                parent = API.GetParent(parent);
                API.GetWindowText(parent, sb, 65535);
                title = sb.ToString();
                API.GetWindowRect(parent, out re);
                reList.Add(re);
                count++;
            }
            return (title, reList);
        }

        //ポップアップを辿る
        //→意味なかった、最上位のポップアップを取得するから辿る意味なかった
        private (List<IntPtr>, List<API.RECT>) GetPopupWindows(IntPtr hWnd)
        {
            int count = 0;
            var pop = API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP);

            List<IntPtr> ps = new();
            List<API.RECT> rList = new();
            while (pop != IntPtr.Zero && count < 20)
            {
                ps.Add(pop);
                API.GetWindowRect(pop, out API.RECT re);
                rList.Add(re);
                pop = API.GetWindow(pop, API.GETWINDOW_CMD.GW_ENABLEDPOPUP);
                count++;
            }

            return (ps, rList);
        }
        //タイトル付きのウィンドウをParentで辿る
        private (string, IntPtr) GetParentWithTitle(IntPtr hWnd)
        {
            int count = 0;
            var str = new StringBuilder(65535);
            IntPtr hParent = hWnd;
            _ = API.GetWindowText(hParent, str, 65535);
            while (string.IsNullOrEmpty(str.ToString()) && count < 20)
            {
                hParent = API.GetParent(hParent);
                _ = API.GetWindowText(hParent, str, 65535);
                count++;
            }
            return (str.ToString(), hParent);
        }
        //タイトル付きのウィンドウをOwnerで辿る
        private (string, IntPtr) GetOwnerWithTitle(IntPtr hWnd)
        {
            int count = 0;
            var str = new StringBuilder(65535);
            IntPtr hTemp = hWnd;
            _ = API.GetWindowText(hTemp, str, 65535);
            while (string.IsNullOrEmpty(str.ToString()) && count < 20)
            {
                hTemp = API.GetWindow(hTemp, API.GETWINDOW_CMD.GW_OWNER);
                _ = API.GetWindowText(hTemp, str, 65535);
                count++;
            }
            return (str.ToString(), hTemp);
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

                //MyGroupBoxHotKey.Header = "無効なホットキー";
            }
            else
            {
                //MessageBox.Show("登録完了");

                //MyGroupBoxHotKey.Header = "ホットキー";
            }
        }

        //private int GetModifierKeySum()
        //{
        //    int mod = 0;
        //    if (MyAppConfig.HotkeyAlt) mod += (int)ModifierKeys.Alt;
        //    if (MyAppConfig.HotkeyCtrl) mod += (int)ModifierKeys.Control;
        //    if (MyAppConfig.HotkeyShift) mod += (int)ModifierKeys.Shift;
        //    if (MyAppConfig.HotkeyWin) mod += (int)ModifierKeys.Windows;
        //    return mod;
        //}

    }

    public static class API
    {
        //Rect取得用
        public struct RECT
        {
            //型はlongじゃなくてintが正解！！！！！！！！！！！！！！
            //longだとおかしな値になる
            public int left;
            public int top;
            public int right;
            public int bottom;
            public override string ToString()
            {
                return $"横:{right - left:0000}, 縦:{bottom - top:0000}  ({left}, {top}, {right}, {bottom})";
            }
        }
        //座標取得用
        public struct POINT
        {
            public int X;
            public int Y;
        }
        //ウィンドウ情報用
        public struct WINDOWINFO
        {
            public int cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public short wCreatorVersion;
        }
        public enum WINDOW_STYLE : uint
        {
            WS_BORDER = 0x00800000,
            WS_CAPTION = 0x00C00000,
            WS_CHILD = 0x40000000,
            WS_CHILDWINDOW = 0x40000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_DISABLED = 0x08000000,
            WS_DLGFRAME = 0x00400000,
            WS_GROUP = 0x00020000,
            WS_HSCROLL = 0x00100000,
            WS_ICONIC = 0x20000000,
            WS_MAXIMIZE = 0x01000000,
            WS_MAXIMIZEBOX = 0x00010000,
            WS_MINIMIZE = 0x20000000,
            WS_MINIMIZEBOX = 0x00020000,
            WS_OVERLAPPED = 0x00000000,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            //The window is an overlapped window.Same as the WS_TILEDWINDOW style.
            WS_POPUP = 0x80000000,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_SIZEBOX = 0x00040000,
            WS_SYSMENU = 0x00080000,
            WS_TABSTOP = 0x00010000,
            WS_THICKFRAME = 0x00040000,
            WS_TILED = 0x00000000,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,
            //(WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX)
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x00200000,
        }

        //ウィンドウのRect取得
        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        //手前にあるウィンドウのハンドル取得
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        //ウィンドウ名取得
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hWin, StringBuilder lpString, int nMaxCount);

        //パレントウィンドウ取得
        [DllImport("user32.dll")]
        internal static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindow(IntPtr hWnd, GETWINDOW_CMD uCmd);//本当のuCmdはuint型
        public enum GETWINDOW_CMD
        {
            GW_CHILD = 5,
            //指定されたウィンドウが親ウィンドウである場合、取得されたハンドルは、Zオーダーの最上位にある子ウィンドウを識別します。
            //それ以外の場合、取得されたハンドルはNULLです。この関数は、指定されたウィンドウの子ウィンドウのみを調べます。子孫ウィンドウは調べません。
            GW_ENABLEDPOPUP = 6,
            //取得されたハンドルは、指定されたウィンドウが所有する有効なポップアップウィンドウを識別します
            //（検索では、GW_HWNDNEXTを使用して最初に見つかったそのようなウィンドウが使用されます）。
            //それ以外の場合、有効なポップアップウィンドウがない場合、取得されるハンドルは指定されたウィンドウのハンドルです。
            GW_HWNDFIRST = 0,
            //取得されたハンドルは、Zオーダーで最も高い同じタイプのウィンドウを識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。
            //指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。
            //指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_HWNDLAST = 1,
            //取得されたハンドルは、Zオーダーで最も低い同じタイプのウィンドウを識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_HWNDNEXT = 2,
            //取得されたハンドルは、指定されたウィンドウの下のウィンドウをZオーダーで識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。
            //指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。
            //指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_HWNDPREV = 3,
            //取得されたハンドルは、指定されたウィンドウの上のウィンドウをZオーダーで識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。
            //指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。
            //指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_OWNER = 4,
            //取得されたハンドルは、指定されたウィンドウの所有者ウィンドウを識別します（存在する場合）。詳細については、「所有するWindows」を参照してください。
        }

        [DllImport("user32.dll")]
        internal static extern int GetWindowInfo(IntPtr hWnd, ref WINDOWINFO info);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        /// <summary>
        /// 指定したWindowの一番上のChildWindowを返す
        /// </summary>
        /// <param name="hWnd">IntPtr.Zeroを指定すると一番上のWindowを返す</param>
        /// <returns>ChildWindowを持たない場合はnullを返す</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetTopWindow(IntPtr hWnd);





        //public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam, List<IntPtr> intPtrs);
        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //internal static extern bool EnumChildWindows(IntPtr hWnd, EnumWindowsDelegate enumWindows, IntPtr lparam);


        //internal static List<IntPtr> GetChildWindows(IntPtr hWnd)
        //{
        //    List<IntPtr> childList = new();
        //    EnumChildWindows(hWnd, new EnumWindowsDelegate(EnumWindowCallBack), IntPtr.Zero);
        //    return childList;
        //}
        //private static bool EnumWindowCallBack(IntPtr hWnd, IntPtr lparam, List<IntPtr> childList)
        //{
        //    childList.Add(hWnd);
        //    return true;
        //}














        //グローバルホットキー登録用
        internal const int WM_HOTKEY = 0x0312;
        [DllImport("user32.dll")]
        internal static extern int RegisterHotKey(IntPtr hWnd, int id, int modkyey, int vKey);
        [DllImport("user32.dll")]
        internal static extern int UnregisterHotKey(IntPtr hWnd, int id);

    }
}
