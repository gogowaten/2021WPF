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


namespace _20210202_右クリックメニュー取得
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
        private const int LOOP_LIMIT = 5;

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
                RRR();
            }
        }

        private void RRR()
        {
            IntPtr fore = API.GetForegroundWindow();
            var infoFore = GetWindowRectAndText(fore);
            IntPtr popup = API.GetWindow(fore, API.GETWINDOW_CMD.GW_ENABLEDPOPUP);
            API.RECT popupRECT = GetWindowAPIRECT(popup);
            if (popupRECT.right != 0 && popupRECT.bottom != 0)
            {
                var infoPop = GetWindowRectAndText(popup);
                List<IntPtr> pops = GetCmdWindows(popup, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT);
                var infoPops = GetWindowRectAndTexts(pops);

                //Textを持つウィンドウ以降は除去
                var rr = DeleteWithTextWindow(pops);
                var aa = GetWindowRectAndTexts(rr);
                //ドロップシャドウウィンドウを除去
                List<Rect> result = DeleteShadowRect(aa.rs);
                //重なりのないウィンドウを除去
                result = SelectOverlappedRect(result);
                //GetForegroundwindowのRectを追加
                result.Add(GetWindowRectMitame(fore));


            }
            //GetForegroundwindowのGetWindowのENABLEDPOPUPが空だった場合
            else
            {
                API.GetCursorPos(out API.POINT cursorP);
                var hWnd = API.WindowFromPoint(cursorP);
                List<IntPtr> wList = new();
                wList.Add(fore);
                if (IsOverlapping(GetWindowRect(fore), GetWindowRect(hWnd)))
                {
                    wList.Add(hWnd);
                }              

                var info = GetWindowAPI_RECTAndTexts(wList);

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

            return MyConverterApiRectToRect(myRECT);
        }
        private Rect MyConverterApiRectToRect(API.RECT rect)
        {
            return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        /// <summary>
        /// 前後のWindowRectの重なりを判定、重なっていればリストに追加して返す。重なっていないウィンドウが出た時点で終了
        /// </summary>
        /// <param name="wList">判定したいウィンドウハンドルのリスト</param>
        /// <returns></returns>
        private List<IntPtr> OverlappedWindows(List<IntPtr> wList)
        {
            List<IntPtr> result = new();
            result.Add(wList[0]);

            for (int i = 1; i < wList.Count; i++)
            {
                if (IsOverlapping(
                    MyConverterApiRectToRect(GetWindowAPIRECT(wList[i - 1])),
                    MyConverterApiRectToRect(GetWindowAPIRECT(wList[i]))))
                {
                    //重なっていればリストに追加
                    result.Add(wList[i]);
                }
                else
                {
                    //途切れたら終了
                    return result;
                }
            }
            return result;
        }
        private List<API.RECT> SelectOverlappedRECT(List<API.RECT> rList)
        {
            List<API.RECT> result = new();
            result.Add(rList[0]);

            for (int i = 1; i < rList.Count; i++)
            {
                if (IsOverlapping(
                    MyConverterApiRectToRect(rList[i - 1]),
                    MyConverterApiRectToRect(rList[i])))
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
        private List<Rect> SelectOverlappedRect(List<Rect> rList)
        {
            List<Rect> result = new();
            result.Add(rList[0]);

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
        /// 2つのRectが一部でも重なっていたらtrueを返す
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        private bool IsOverlapping(Rect r1, Rect r2)
        {
            RectangleGeometry geo1 = new(r1);
            IntersectionDetail detail = geo1.FillContainsWithDetail(new RectangleGeometry(r2));
            return detail != IntersectionDetail.Empty;
            //return (detail != IntersectionDetail.Empty || detail != IntersectionDetail.NotCalculated, detail);
        }
        //IntersectionDetail列挙型
        //Empty             全く重なっていない
        //FullyContains     r2はr1の領域に完全に収まっている
        //FullyInside       r1はr2の領域に完全に収まっている
        //Intersects        一部が重なっている
        //NotCalculated     計算されません(よくわからん)

        /// <summary>
        /// Textがないものをリストに追加していって、Textをもつウィンドウが出た時点で終了、リストを返す
        /// </summary>
        /// <param name="wList"></param>
        /// <returns></returns>
        private List<IntPtr> DeleteWithTextWindow(List<IntPtr> wList)
        {
            List<IntPtr> result = new();
            for (int i = 0; i < wList.Count; i++)
            {
                if (GetWindowText(wList[i]) == "")
                {
                    result.Add(wList[i]);
                }
                else
                {
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// ドロップシャドウ用のウィンドウを判定して、取り除いて返す
        /// </summary>
        /// <param name="wLisnt"></param>
        /// <returns></returns>
        private List<IntPtr> DeleteShadowWindow(List<IntPtr> wLisnt)
        {
            List<IntPtr> result = new();
            result.Add(wLisnt[0]);
            _ = API.GetWindowRect(wLisnt[0], out API.RECT preRECT);
            //API.RECT preRECT = GetWindowRect(wLisnt[0]);

            for (int i = 1; i < wLisnt.Count; i++)
            {
                //前後のRectのleftとtopが同じならドロップシャドウと判定して
                //リストには加えない
                _ = API.GetWindowRect(wLisnt[i], out API.RECT imaRECT);
                if (imaRECT.left != preRECT.left || imaRECT.top != preRECT.top)
                {
                    result.Add(wLisnt[i]);
                }
                preRECT = imaRECT;
            }
            return result;
        }
        /// <summary>
        /// ドロップシャドウ用のウィンドウを判定して、取り除いて返す
        /// </summary>
        /// <param name="rList">RECTのリスト</param>
        /// <returns></returns>
        private static List<API.RECT> DeleteShadowAPI_RECT(List<API.RECT> rList)
        {
            List<API.RECT> result = new();
            result.Add(rList[0]);
            API.RECT preRECT = rList[0];
            for (int i = 1; i < rList.Count; i++)
            {
                //前後のRectのleftとtopが同じならドロップシャドウと判定して
                //リストには加えない
                API.RECT imaRECT = rList[i];
                if (imaRECT.left != preRECT.left || imaRECT.top != preRECT.top)
                {
                    result.Add(rList[i]);
                }
                preRECT = imaRECT;
            }
            return result;
        }

        private static List<Rect> DeleteShadowRect(List<Rect> rList)
        {
            List<Rect> result = new();
            result.Add(rList[0]);
            Rect preRect = rList[0];
            for (int i = 1; i < rList.Count; i++)
            {
                //前後のRectのleftとtopが同じならドロップシャドウと判定して
                //リストには加えない
                Rect imaRect = rList[i];
                if (imaRect.TopLeft != preRect.TopLeft)
                {
                    result.Add(rList[i]);
                }
                preRect = imaRect;
            }
            return result;
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


        //ウィンドウハンドルからText(タイトル名)やRECTを取得
        private (IntPtr, Rect re, string text) GetWindowRectAndText(IntPtr hWnd)
        {
            return (hWnd, GetWindowRect(hWnd), GetWindowText(hWnd));
        }
        private (List<IntPtr> ptrs, List<Rect> rs, List<string> strs)
            GetWindowRectAndTexts(List<IntPtr> pList)
        {
            List<IntPtr> ptrs = new();
            List<Rect> rs = new();
            List<string> strs = new();
            foreach (var item in pList)
            {
                ptrs.Add(item);
                rs.Add(GetWindowRect(item));
                strs.Add(GetWindowText(item));
            }
            return (ptrs, rs, strs);
        }
        //ウィンドウハンドルからText(タイトル名)やRECTを取得
        private (IntPtr, API.RECT re, string text) GetWindowAPI_RECTAndText(IntPtr hWnd)
        {
            return (hWnd, GetWindowAPIRECT(hWnd), GetWindowText(hWnd));
        }
        private (List<IntPtr> ptrs, List<API.RECT> rs, List<string> strs)
            GetWindowAPI_RECTAndTexts(List<IntPtr> pList)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> rs = new();
            List<string> strs = new();
            foreach (var item in pList)
            {
                ptrs.Add(item);
                rs.Add(GetWindowAPIRECT(item));
                strs.Add(GetWindowText(item));
            }
            return (ptrs, rs, strs);
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

        private void CheckWindow()
        {
            var foreInfo = GetWindowAPI_RECTAndText(API.GetForegroundWindow());//メモ帳のウィンドウ

            //Foregroundwindowから取得できるウィンドウ
            IntPtr foreW = API.GetForegroundWindow();
            //var ancesParent = GetWindowRectAndText(API.GetAncestor(foreW, API.AncestorType.GA_PARENT));//Textなしの全画面サイズ
            //var ancesRoot = GetWindowRectAndText(API.GetAncestor(foreW, API.AncestorType.GA_ROOT));//メモ帳のウィンドウ
            //var ancesRootOwner = GetWindowRectAndText(API.GetAncestor(foreW, API.AncestorType.GA_ROOTOWNER));//メモ帳のウィンドウ
            //var lastPop = GetWindowRectAndText(API.GetLastActivePopup(foreW));//メモ帳のウィンドウ
            //var menu = GetWindowRectAndText(API.GetMenu(foreW));//none
            //var parent = GetWindowRectAndText(API.GetParent(foreW));//none
            //var topChild = GetWindowRectAndText(API.GetTopWindow(foreW));//Textなし、メモ帳のウィンドウのクライアント

            //Foregroundwindowのcmd各種
            //var childs = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_CHILD, LOOP_LIMIT));//メモ帳のウィンドウのクライアント、その後はnone
            var popups = GetWindowAPI_RECTAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_ENABLEDPOPUP, LOOP_LIMIT));//none
                                                                                                                        //var first = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_HWNDFIRST, LOOP_LIMIT));//TextはすべてDefault IME
                                                                                                                        //var last = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_HWNDLAST, LOOP_LIMIT));//TextはすべてProgram Manager
                                                                                                                        //var nexts = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT));//全て関係ないアプリ
                                                                                                                        //var prevs = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_HWNDPREV, LOOP_LIMIT));//すべて関係ないアプリ
                                                                                                                        //var owners = GetWindowRectAndTexts(GetCmdWindows(foreW, API.GETWINDOW_CMD.GW_OWNER, LOOP_LIMIT));//none


            //カーソルの下のウィンドウハンドル
            API.GetCursorPos(out API.POINT cP);
            IntPtr hWnd = API.WindowFromPoint(cP);
            var CursorW = GetWindowAPI_RECTAndText(hWnd);//右クリックメニュー

            var AncesParent = GetWindowAPI_RECTAndText(API.GetAncestor(hWnd, API.AncestorType.GA_PARENT));//Textなしの全画面サイズ
            var AncesRoot = GetWindowAPI_RECTAndText(API.GetAncestor(hWnd, API.AncestorType.GA_ROOT));//右クリックメニュー
            var AncesRootOwner = GetWindowAPI_RECTAndText(API.GetAncestor(hWnd, API.AncestorType.GA_ROOTOWNER));//右クリックメニュー
            var LastPop = GetWindowAPI_RECTAndText(API.GetLastActivePopup(hWnd));//右クリックメニュー
            var Menu = GetWindowAPI_RECTAndText(API.GetMenu(hWnd));//none
            var Parent = GetWindowAPI_RECTAndText(API.GetParent(hWnd));//none
            var TopChild = GetWindowAPI_RECTAndText(API.GetTopWindow(hWnd));//none

            var Childs = GetWindowAPI_RECTAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_CHILD, LOOP_LIMIT));//すべてnone
            var Popups = GetWindowAPI_RECTAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_ENABLEDPOPUP, LOOP_LIMIT));//none
            var First = GetWindowAPI_RECTAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_HWNDFIRST, LOOP_LIMIT));//TextはすべてDefault IME
            var Last = GetWindowAPI_RECTAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_HWNDLAST, LOOP_LIMIT));//TextはすべてProgram Manager
            var Nexts = GetWindowAPI_RECTAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT));//右クリックメニューの影ウィンドウ、それ以降は関係ないアプリ
            var Prevs = GetWindowAPI_RECTAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_HWNDPREV, LOOP_LIMIT));//8個noneが続いたあと関係ないアプリ
            var Owners = GetWindowAPI_RECTAndTexts(GetCmdWindows(hWnd, API.GETWINDOW_CMD.GW_OWNER, LOOP_LIMIT));//none

        }
    }
}
