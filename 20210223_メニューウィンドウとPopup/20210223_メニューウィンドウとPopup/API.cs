using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;


namespace _20210223_メニューウィンドウとPopup
{
    static class API
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

        //DWM（Desktop Window Manager）
        //見た目通りのRectを取得できる、引数のdwAttributeにDWMWA_EXTENDED_FRAME_BOUNDSを渡す
        //引数のcbAttributeにはRECTのサイズ、Marshal.SizeOf(typeof(RECT))これを渡す
        //戻り値が0なら成功、0以外ならエラー値
        [DllImport("dwmapi.dll")]
        internal static extern long DwmGetWindowAttribute(IntPtr hWnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT rect, int cbAttribute);

        //ウィンドウ属性
        //列挙値の開始は0だとずれていたので1からにした
        internal enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,//見た目通りのウィンドウのRect
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        };


        //
        [DllImport("user32.dll")]
        internal static extern IntPtr GetActiveWindow();

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

        //ウィンドウのクライアント領域のRect取得
        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        //クライアント領域の座標を画面全体での座標に変換
        [DllImport("user32.dll")]
        internal static extern bool ClientToScreen(IntPtr hWnd, out POINT lpPoint);


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

        /// <summary>
        /// 指定したWindowのメニューのハンドルを返す
        /// </summary>
        /// <param name="hWnd">Windowのハンドル</param>
        /// <returns>Windowがメニューを持たない場合はnullを返す</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetMenu(IntPtr hWnd);

        /// <summary>
        /// キーボードフォーカスを持つWindowのハンドルを返す
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        internal static extern IntPtr GetMenuBarInfo(IntPtr hWnd, MenuObjectId idObject, long idItem, MENUBARINFO pmbi);

        public struct MENUBARINFO
        {
            public long cbSize;
            public RECT rcBar;
            public IntPtr hMenu;
            public bool fBarFocused;
            public bool fFocused;
        }
        public enum MenuObjectId : long
        {
            OBJID_CLIENT = 0xFFFFFFFC,
            OBJID_MENU = 0xFFFFFFFD,
            OBJID_SYSMENU = 0xFFFFFFFF,
        }

        [DllImport("user32.dll")]
        internal static extern IntPtr GetMenuItemRect(IntPtr hWnd, IntPtr hMenu, uint uItem, out RECT rect);


        //指定座標にあるウィンドウのハンドル取得
        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(POINT pOINT);

        //祖先ウィンドウを取得
        [DllImport("user32.dll")]
        internal static extern IntPtr GetAncestor(IntPtr hWnd, AncestorType type);

        public enum AncestorType
        {
            GA_PARENT = 1,
            GA_ROOT = 2,//Parentを辿ってルートを取得
            GA_ROOTOWNER = 3,//GetParentを使ってルートを取得

        }

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDesktopWindow();


        [DllImport("user32.dll")]
        internal static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        internal static extern IntPtr GetSubMenu(IntPtr hWnd, int nPos);

        [DllImport("user32.dll")]
        internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);




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

        //マウスカーソル座標
        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out POINT lpPoint);


        //Bitmap描画関連
        //DC取得
        //nullを渡すと画面全体のDCを取得、ウィンドウハンドルを渡すとそのウィンドウのクライアント領域DC
        //失敗した場合の戻り値はnull
        //使い終わったらReleaseDC
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        //渡したDCに互換性のあるDC作成
        //失敗した場合の戻り値はnull
        //使い終わったらDeleteDC
        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        //指定されたDCに関連付けられているデバイスと互換性のあるビットマップを作成
        //使い終わったらDeleteObject
        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        //DCにオブジェクトを指定する、オブジェクトの種類はbitmap、brush、font、pen、Regionなど
        [DllImport("gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        //画像転送
        [DllImport("gdi32.dll")]
        internal static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);
        internal const int SRCCOPY = 0x00cc0020;
        internal const int SRCINVERT = 0x00660046;

        ////
        //[DllImport("user32.dll")]
        //private static extern bool PrintWindow(IntPtr hWnd, IntPtr hDC, uint nFlags);
        //private const uint nFrags_PW_CLIENTONLY = 0x00000001;

        //[DllImport("user32.dll")]
        //private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr ho);


    }
}