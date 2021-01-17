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
        private const int LOOP_LIMIT = 20;

        public MainWindow()
        {
            InitializeComponent();
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
                _ = API.GetCursorPos(out API.POINT cursorPoint);
                //WindowInfos(API.GetForegroundWindow());
                WindowInfos(API.WindowFromPoint(cursorPoint));

            }

        }

        private void WindowInfos(IntPtr hWnd)
        {
            var info = GetWindowRectAndText(hWnd);
            var toWithTextParentWindow = GetWindowsToWithTextParent(hWnd, LOOP_LIMIT);
            var toWithTextOwnerWindow = GetWindowsToWithTextOwner(hWnd, LOOP_LIMIT);
            var ancestorParent = GetWindowRectAndText(API.GetAncestor(hWnd, API.AncestorType.GA_PARENT));
            var ancestorRoot = GetWindowRectAndText(API.GetAncestor(hWnd, API.AncestorType.GA_ROOT));
            var ancestorRootOwner = GetWindowRectAndText(API.GetAncestor(hWnd, API.AncestorType.GA_ROOTOWNER));

        }

        //タイトル付きParentWindowまですべてのWindowを取得
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetWindowsToWithTextParent(IntPtr hWnd, int loopCount)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;

            var temp = API.GetParent(hWnd);

            //Ownerが無くなるまで辿る
            while (temp != IntPtr.Zero && count < loopCount)
            {
                ptrs.Add(temp);
                res.Add(GetWindowRect(temp));
                strs.Add(GetWindowText(temp));

                temp = API.GetParent(temp);
                count++;
            }
            return (ptrs, res, strs);
        }
        
        //タイトル付きParentWindowまですべてのWindowを取得
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

                temp = API.GetWindow(hWnd, API.GETWINDOW_CMD.GW_OWNER);
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




    }
}
