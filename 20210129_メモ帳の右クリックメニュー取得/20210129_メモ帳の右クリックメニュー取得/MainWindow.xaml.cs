using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

using System.Windows.Interop;

//WPF、他のアプリの右クリックメニューのウィンドウハンドル取得方法がわからん - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/02/02/135943

//最前面（アクティブ）アプリの右クリックメニューの、ウィンドウハンドルを取得したい
//唯一できた方法はカーソル下のウィンドウハンドルを取得する方法
//でもこれじゃ右クリックメニューって判定しているわけじゃないから、これじゃない感じ

namespace _20210129_最前面アプリの右クリックメニュー取得
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
        private const int LOOP_LIMIT = 3;

        public MainWindow()
        {
            InitializeComponent();

            MyInitializeHotKey();

            //ホットキーにPrintScreenキーを登録
            ChangeHotKey(Key.PrintScreen, HOTKEY_ID1);

            //アプリ終了時にホットキーの解除
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
            int mod = 0;//修飾キーなし
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


        //ホットキー判定
        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != API.WM_HOTKEY) return;

            //ホットキー(今回はPrintScreen)が押されたら
            else if (msg.wParam.ToInt32() == HOTKEY_ID1)
            {
                var foreInfo = GetWindowRectAndText(API.GetForegroundWindow());//メモ帳のウィンドウ
                var focusInfo = GetWindowRectAndText(API.GetFocus());//なし
                var shellInfo = GetWindowRectAndText(API.GetShellWindow());//program manager
                var activeInfo = GetWindowRectAndText(API.GetActiveWindow());

                //Foregroundwindowから取得できるウィンドウ
                IntPtr foreW = API.GetForegroundWindow();
                var ancesParent = GetWindowRectAndText(API.GetAncestor(foreW, API.AncestorType.GA_PARENT));//Textなしの全画面サイズ
                var ancesRoot = GetWindowRectAndText(API.GetAncestor(foreW, API.AncestorType.GA_ROOT));//メモ帳のウィンドウ
                var ancesRootOwner = GetWindowRectAndText(API.GetAncestor(foreW, API.AncestorType.GA_ROOTOWNER));//メモ帳のウィンドウ
                var lastPop = GetWindowRectAndText(API.GetLastActivePopup(foreW));//メモ帳のウィンドウ
                var menu = GetWindowRectAndText(API.GetMenu(foreW));//none
                var parent = GetWindowRectAndText(API.GetParent(foreW));//none
                var topChild = GetWindowRectAndText(API.GetTopWindow(foreW));//Textなし、メモ帳のウィンドウのクライアント

                //Foregroundwindowのcmd各種
                var childs = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_CHILD, LOOP_LIMIT));//メモ帳のウィンドウのクライアント、その後はnone
                var popups = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_ENABLEDPOPUP, LOOP_LIMIT));//none
                var first = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_HWNDFIRST, LOOP_LIMIT));//TextはすべてDefault IME
                var last = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_HWNDLAST, LOOP_LIMIT));//TextはすべてProgram Manager
                var nexts = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT));//全て関係ないアプリ
                var prevs = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_HWNDPREV, LOOP_LIMIT));//すべて関係ないアプリ
                var owners = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_OWNER, LOOP_LIMIT));//none


                //カーソルの下のウィンドウハンドル
                API.GetCursorPos(out API.POINT cP);
                IntPtr hWnd = API.WindowFromPoint(cP);
                var CursorW = GetWindowRectAndText(hWnd);//右クリックメニュー

                var AncesParent = GetWindowRectAndText(API.GetAncestor(hWnd, API.AncestorType.GA_PARENT));//Textなしの全画面サイズ
                var AncesRoot = GetWindowRectAndText(API.GetAncestor(hWnd, API.AncestorType.GA_ROOT));//右クリックメニュー
                var AncesRootOwner = GetWindowRectAndText(API.GetAncestor(hWnd, API.AncestorType.GA_ROOTOWNER));//右クリックメニュー
                var LastPop = GetWindowRectAndText(API.GetLastActivePopup(hWnd));//右クリックメニュー
                var Menu = GetWindowRectAndText(API.GetMenu(hWnd));//none
                var Parent = GetWindowRectAndText(API.GetParent(hWnd));//none
                var TopChild = GetWindowRectAndText(API.GetTopWindow(hWnd));//none

                var Childs = GetWindowRectAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_CHILD, LOOP_LIMIT));//すべてnone
                var Popups = GetWindowRectAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP, LOOP_LIMIT));//none
                var First = GetWindowRectAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_HWNDFIRST, LOOP_LIMIT));//TextはすべてDefault IME
                var Last = GetWindowRectAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_HWNDLAST, LOOP_LIMIT));//TextはすべてProgram Manager
                var Nexts = GetWindowRectAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT));//右クリックメニューの影ウィンドウ、それ以降は関係ないアプリ
                var Prevs = GetWindowRectAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_HWNDPREV, LOOP_LIMIT));//8個noneが続いたあと関係ないアプリ
                var Owners = GetWindowRectAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_OWNER, LOOP_LIMIT));//none

            }
        }
     
        //指定したAPI.GETWINDOW_CMDを収集
        private List<IntPtr> GetCmdWindows
            (IntPtr hWnd, API.GETWINDOW_CMD cmd, int loopCount)
        {
            List<IntPtr> v = new();
            IntPtr temp = API.GetWindow(hWnd, cmd);
            for (int i = 0; i < loopCount; i++)
            {
                v.Add(temp);
                temp = API.GetWindow(temp, cmd);                
            }
            return v;
        }


        //ウィンドウハンドルからText(タイトル名)やRECTを取得
        private (IntPtr, API.RECT re, string text) GetWindowRectAndText(IntPtr hWnd)
        {
            return (hWnd, GetWindowRect(hWnd), GetWindowText(hWnd));
        }
        private (List<IntPtr> ptrs, List<API.RECT> rs, List<string> strs)
            GetWindowRectAndTexts(List<IntPtr> pList)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> rs = new();
            List<string> strs = new();
            foreach (var item in pList)
            {
                ptrs.Add(item);
                rs.Add(GetWindowRect(item));
                strs.Add(GetWindowText(item));
            }
            return (ptrs, rs, strs);
        }

        //ウィンドウハンドルからRECT取得
        private API.RECT GetWindowRect(IntPtr hWnd)
        {
            _ = API.GetWindowRect(hWnd, out API.RECT re);
            return re;
        }

        //ウィンドウハンドルからText取得
        private string GetWindowText(IntPtr hWnd)
        {
            var text = new StringBuilder(65535);
            _ = API.GetWindowText(hWnd, text, 65535);
            return text.ToString();
        }

    }
}
