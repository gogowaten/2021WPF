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


namespace _20210212_WinAPI_GetWindowInfo
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

                //MyText1();
                //MyTitleBarInfoForeWindow();
                MyTitleBarInfoFromCursorUnderWindow();

            }
        }

        private void MyTitleBarInfoFromCursorUnderWindow()
        {
            API.GetCursorPos(out API.POINT cp);
            MyWidndowInfo cursor = GetWindowInfo(API.WindowFromPoint(cp));            
            var ti = new API.TITLEBARINFO();
            ti.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(ti);
            API.GetTitleBarInfo(cursor.hWnd, ref ti);

            var fore = GetWindowInfo(API.GetForegroundWindow());
            var tti = new API.TITLEBARINFO();
            tti.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(tti);
            API.GetTitleBarInfo(fore.hWnd, ref tti);

        }
    
        //未使用
        private void MyText1()
        {
            //API.GetCursorPos(out API.POINT cp);
            //MyWidndowInfo cursor = GetWindowInfo(API.WindowFromPoint(cp));


            API.WINDOWINFO info = new();
            //API.GetWindowInfo(cursor.hWnd, ref info);
            API.GetWindowInfo(GetWindowInfo(API.GetForegroundWindow()).hWnd, ref info);

            //Foregroundに最大化ボタンがあるかどうか、ないウィンドウでもあり判定になる
            //Pixtack紫陽花のヴァージョンウィンドウには最大化ボタンがないけど、あり判定になる
            var neko = info.dwStyle & 0x00010000;
            bool inu = neko == (uint)API.WINDOW_STYLE.WS_MAXIMIZEBOX;
            bool uma = (info.dwStyle & (uint)API.WINDOW_STYLE.WS_MAXIMIZEBOX) != 0;
            MessageBox.Show($"{uma}");


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