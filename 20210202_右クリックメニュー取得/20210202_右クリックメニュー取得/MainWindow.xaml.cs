using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Windows.Interop;

//スクショテストアプリ
//ctrl + shift + PrintScreen でキャプチャ
//キャプチャされるのは最前面アプリのウィンドウとそのメニューウィンドウ
//saveボタンで実行ファイルと同じフォルダに画像ファイルとして保存
//右クリックメニューの上にマウスカーソルを置かないと枠外のメニューはキャプチャされない

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
                //Rect収集
                List<Rect> rectList = MakeForeWinndwWithMenuWindowRectList();
                //全画面画像取得
                var bmp = GetScreenBitmap();
                //収集したRectを使って切り抜き画像作成して表示
                MyImage.Source = CroppedBitmapFromRects(bmp, rectList);
            }
        }

        #region Rect取得

        /// <summary>
        /// 最前面ウィンドウと、そのメニューや右クリックメニューウィンドウ群のRectリストを作成
        /// </summary>
        /// <returns></returns>
        private List<Rect> MakeForeWinndwWithMenuWindowRectList()
        {
            List<Rect> result = new();
            //Foregroundのハンドル取得
            IntPtr fore = API.GetForegroundWindow();
            var infoFore = GetWindowRectAndText(fore);
            //ForegroundのPopupハンドルとRect取得
            IntPtr popup = API.GetWindow(fore, API.GETWINDOW_CMD.GW_ENABLEDPOPUP);
            Rect popupRect = GetWindowRect(popup);
            var infoPop = GetWindowRectAndText(popup);//確認用

            //Popupが存在する(Rectが0じゃない)場合
            if (popupRect != new Rect(0, 0, 0, 0))
            {
                //PopupのNEXT(下にあるウィンドウハンドル)を収集
                List<IntPtr> pops = GetCmdWindows(popup, API.GETWINDOW_CMD.GW_HWNDNEXT, LOOP_LIMIT);
                var infoPops = GetWindowRectAndTexts(pops);//確認用
                //Textを持つウィンドウ以降を除去
                List<IntPtr> noneText = DeleteWithTextWindow(pops);

                //残ったウィンドウのRect取得
                List<Rect> rs = noneText.Select(x => GetWindowRect(x)).ToList();
                //ドロップシャドウウィンドウのRectを除去
                result = DeleteShadowRect(rs);
                //前後のRectが重なっているところまで選択して、以降は除外
                result = SelectOverlappedRect(result);
                //GetForegroundwindowの見た目通りのRectを追加
                result.Add(GetWindowRectMitame(fore));
            }
            //Popupが存在しない(Rectが0)場合
            else
            {
                //GetForegroundwindowの見た目通りのRectを追加
                Rect foreRect = GetWindowRectMitame(fore);
                result.Add(foreRect);

                //マウスカーソル下のウィンドウの見た目通りのRect取得
                API.GetCursorPos(out API.POINT cursorP);
                Rect cursorRect = GetWindowRectMitame(API.WindowFromPoint(cursorP));
                //2つのRectが重なっていた場合は、カーソル下のウィンドウRectを追加
                if (IsOverlapping(foreRect, cursorRect))
                {
                    result.Add(cursorRect);
                }
            }
            return result;
        }


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

        //WPFのRectの重なり判定、RectangleGeometryにしてからFillContainsWithDetailメソッドでできた
        //https://gogowaten.hatenablog.com/entry/2021/01/28/124714
        /// <summary>
        /// 前後のRectの重なりを判定、重なっていればリストに追加して返す。重なっていないRectが出た時点で終了
        /// </summary>
        /// <param name="rList"></param>
        /// <returns></returns>
        private List<Rect> SelectOverlappedRect(List<Rect> rList)
        {
            List<Rect> result = new();
            result.Add(rList[0]);

            //順番に判定
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
        /// ドロップシャドウ用のウィンドウを判定して、取り除いて返す。前後のRectのtopleftが同じなら後のRectはドロップシャドウと判定する
        /// </summary>
        /// <param name="rList"></param>
        /// <returns></returns>
        private static List<Rect> DeleteShadowRect(List<Rect> rList)
        {
            List<Rect> result = new();
            result.Add(rList[0]);
            Rect preRect = rList[0];//前Rect
            for (int i = 1; i < rList.Count; i++)
            {
                //前後のRectのleftとtopが同じならドロップシャドウと判定して
                //リストには加えない
                Rect imaRect = rList[i];//後Rect
                if (imaRect.TopLeft != preRect.TopLeft)
                {
                    result.Add(rList[i]);
                }
                preRect = imaRect;//前Rectに後Rectを入れて次へ
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
            int mod = 6;
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



        #region 未使用
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


        #endregion 未使用

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveImage((BitmapSource)MyImage.Source);
        }
    }
}
