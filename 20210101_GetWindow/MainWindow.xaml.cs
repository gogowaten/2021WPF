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
        private Dictionary<API.GETWINDOW_CMD, API.RECT> GetCMDRects(IntPtr hWnd)
        {
            Dictionary<API.GETWINDOW_CMD, API.RECT> result = new();
            var cmd = Enum.GetValues(typeof(API.GETWINDOW_CMD)).Cast<API.GETWINDOW_CMD>();

            foreach (var item in cmd)
            {
                _ = API.GetWindowRect(API.GetWindow(hWnd, item), out API.RECT re);
                result.Add(item, re);
            }

            return result;
        }
        private IntPtr GetChildWindow(IntPtr hWnd)
        {
            return API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_CHILD);
        }


        //子ウィンドウすべてのRECTを返す
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
                //MessageBox.Show("ホットキーーーーーーーーーーーー");
                IntPtr foreWnd = API.GetForegroundWindow();
                var parentWnd = API.GetParent(foreWnd);

                var foreRects = GetCMDRects(foreWnd);
                var parentRects = GetCMDRects(parentWnd);

                _ = API.GetWindowRect(foreWnd, out API.RECT fRe);
                _ = API.GetWindowRect(parentWnd, out API.RECT pRe);

                var childRects = GetChildWindowRect(foreWnd);
                var ccree = GetNextWindowTextAndRects(GetChildWindow(foreWnd));
                var childWnd = GetChildWindow(parentWnd);
                var pc = GetNextWindowTextAndRects(childWnd);
                var preRects = GetPrevWindowTextAndRects(parentWnd);
                var owner = API.GetWindow(foreWnd, API.GETWINDOW_CMD.GW_OWNER);

                API.GetWindowRect(foreWnd, out API.RECT foreRect);
                API.GetWindowRect(parentWnd, out API.RECT PareRect);
                API.GetWindowRect(childWnd, out API.RECT ChildRect);
                API.GetWindowRect(owner, out API.RECT OwnerRect);
                API.GetWindowRect(API.GetWindow(foreWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP), out API.RECT popupRect);
                API.GetWindowRect(API.GetWindow(parentWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP), out API.RECT popupRectParent);
                API.GetWindowRect(API.GetWindow(childWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP), out API.RECT popupRectChild);
                API.GetWindowRect(API.GetWindow(owner, API.GETWINDOW_CMD.GW_ENABLEDPOPUP), out API.RECT popupRectOwner);

                //var textFore = new StringBuilder(65535);
                //API.GetWindowText(foreWnd, textFore, 65535);

                //var stb = new StringBuilder(65535);
                //int wndText = API.GetWindowText(parentWnd, stb, 65535);

            }
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
            //指定されたウィンドウが親ウィンドウである場合、取得されたハンドルは、Zオーダーの最上位にある子ウィンドウを識別します。それ以外の場合、取得されたハンドルはNULLです。この関数は、指定されたウィンドウの子ウィンドウのみを調べます。子孫ウィンドウは調べません。
            GW_ENABLEDPOPUP = 6,
            //取得されたハンドルは、指定されたウィンドウが所有する有効なポップアップウィンドウを識別します（検索では、GW_HWNDNEXTを使用して最初に見つかったそのようなウィンドウが使用されます）。それ以外の場合、有効なポップアップウィンドウがない場合、取得されるハンドルは指定されたウィンドウのハンドルです。
            GW_HWNDFIRST = 0,
            //取得されたハンドルは、Zオーダーで最も高い同じタイプのウィンドウを識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_HWNDLAST = 1,
            //取得されたハンドルは、Zオーダーで最も低い同じタイプのウィンドウを識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_HWNDNEXT = 2,
            //取得されたハンドルは、指定されたウィンドウの下のウィンドウをZオーダーで識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_HWNDPREV = 3,
            //取得されたハンドルは、指定されたウィンドウの上のウィンドウをZオーダーで識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_OWNER = 4,
            //取得されたハンドルは、指定されたウィンドウの所有者ウィンドウを識別します（存在する場合）。詳細については、「所有するWindows」を参照してください。
        }



        //グローバルホットキー登録用
        internal const int WM_HOTKEY = 0x0312;
        [DllImport("user32.dll")]
        internal static extern int RegisterHotKey(IntPtr hWnd, int id, int modkyey, int vKey);
        [DllImport("user32.dll")]
        internal static extern int UnregisterHotKey(IntPtr hWnd, int id);

    }
}
