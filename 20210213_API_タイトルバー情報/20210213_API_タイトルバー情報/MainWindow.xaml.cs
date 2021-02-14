using System;
using System.Text;
using System.Windows;
using System.Windows.Input;

using System.Windows.Interop;

//GetTitleBarInfo function(winuser.h) -Win32 apps | Microsoft Docs
//https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-gettitlebarinfo
//TITLEBARINFO(winuser.h) - Win32 apps | Microsoft Docs
//https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-titlebarinfo
//        GetTitleBarInfo
//https://forums.codeguru.com/showthread.php?443988-GetTitleBarInfo


//ウィンドウのタイトルバー情報を取得するWinAPIのGetTitleBarInfoを使ってみた、WPF、C# - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/02/14/162607


namespace _20210213_API_タイトルバー情報
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

                MyTitleBarInfoFromCursorUnderWindow();

            }
        }

        private void MyTitleBarInfoFromCursorUnderWindow()
        {
            //マウスカーソル下のウィンドウのタイトルバー情報
            API.GetCursorPos(out API.POINT cp);
            MyWidndowInfo cursor = GetWindowInfo(API.WindowFromPoint(cp));
            var tiカーソル下 = new API.TITLEBARINFO();
            tiカーソル下.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(tiカーソル下);
            API.GetTitleBarInfo(cursor.hWnd, ref tiカーソル下);

            //最前面ウィンドウのタイトルバー情報
            var fore = GetWindowInfo(API.GetForegroundWindow());
            var ti最前面 = new API.TITLEBARINFO();
            ti最前面.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(ti最前面);
            API.GetTitleBarInfo(fore.hWnd, ref ti最前面);

        }


        #region Rect取得


        private Rect MyConverterApiRectToRect(API.RECT rect)
        {
            return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
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


    }


}