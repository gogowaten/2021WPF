﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Windows.Interop;

namespace _20210127_メニューウィンドウハンドル取得
{
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

            //var r1 = new Rect(0, 0, 100, 100);
            //var r2 = new Rect(50, 50, 100, 100);
            //var r3 = new Rect(200, 200, 100, 100);
            //var neko = IsOverlappingRects(r1, r2);

            //List<Rect> rList = new() { r1, r2, r3 };
            //List<Rect> overLapping = OverlappedRects(rList);
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
        #endregion ホットキー関連

        //ホットキー判定
        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != API.WM_HOTKEY) return;

            //ホットキー(今回はPrintScreen)が押されたら
            else if (msg.wParam.ToInt32() == HOTKEY_ID1)
            {
                //マウスカーソル下にあるWindowを基準にRectを収集
                List<Rect> reList = GetWidndowRectsFromCursorPoint();

                //最前面Windowとの重なりを判定
                Rect foreWin = GetWindowRectMitame(API.GetForegroundWindow());
                if (IsOverlappingRects(new RectangleGeometry(foreWin), MakeGeometryGroup(reList)))
                {
                    //一部でも重なっていたら
                    //最前面Windowの見た目通りのRectを付け足す
                    reList.Add(foreWin);
                }
                else
                {
                    //全く重なっていない場合
                    //最前面Windowの見た目通りのRectだけ
                    reList = new() { foreWin };
                }

                //収集したRectに従って画像を切り抜いて1枚の画像にする
                var bmp = CroppedBitmapFromRects(ScreenCapture(), reList);
                //確認のためにアプリに表示
                MyImage.Source = bmp;
                //確認のために画像をクリップボードにコピー
                Clipboard.SetImage(bmp);

            }

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
        private Rect MyConvertApiRectToRect(API.RECT rect)
        {
            return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        //マウスカーソル下にあるWindowを基準にして、そこから下に階層を辿って
        private List<Rect> GetWidndowRectsFromCursorPoint()
        {
            //マウスカーソル下にあるウィンドウハンドル取得
            _ = API.GetCursorPos(out API.POINT myPoint);
            IntPtr hWnd = API.WindowFromPoint(myPoint);

            //Textの無いNextウィンドウを収集            
            (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) = GetNextWindowsWithNoneText(hWnd, LOOP_LIMIT);
            //List<(IntPtr ptrs, API.RECT res, string strs)> neko = new();
            //List<(IntPtr ptrs, API.RECT res, string strs)> inu = new();
            //List<(IntPtr ptrs, API.RECT res, string strs)> uma = new();
            //foreach (var item in ptrs)
            //{
            //    neko.Add(GetWindowRectAndText(API.GetAncestor(item, API.AncestorType.GA_ROOTOWNER)));
            //    inu.Add(GetWindowRectAndText(API.GetAncestor(item, API.AncestorType.GA_ROOT)));
            //    uma.Add(GetWindowRectAndText(API.GetAncestor(item, API.AncestorType.GA_PARENT)));
            //}

            //Rectを収集
            //左上座標が同じWindowはドロップシャドウ用のWindowなのでリストから除外
            List<Rect> reList = DeleteSameLocateRect(res);

            //重なりがあるRectだけのリストにする
            return OverlappedRects(reList);
        }
        //同じ座標のRectを除外する
        private List<Rect> DeleteSameLocateRect(List<API.RECT> rects)
        {
            List<Rect> rList = new();
            for (int i = 0; i < rects.Count; i++)
            {
                rList.Add(MyConvertApiRectToRect(rects[i]));//そのままのRect、RECTからRectに変換)
            }
            return DeleteSameLocateRect(rList);
        }
        private List<Rect> DeleteSameLocateRect(List<Rect> rects)
        {
            List<Rect> reList = new();
            Rect preR;
            for (int i = 0; i < rects.Count; i++)
            {
                //Rect imaR = GetWindowRectMitame(ptrs[i]);//見た目通りのRect(そのままのRectと変化なかった)
                Rect imaR = rects[i];
                if (preR.TopLeft != imaR.TopLeft)
                {
                    reList.Add(imaR);
                    preR = imaR;
                }
            }
            return reList;
        }


        private bool IsOverlappingRects(Rect r1, Rect r2)
        {
            return IsOverlappingRects(new RectangleGeometry(r1), new RectangleGeometry(r2));
        }
        private bool IsOverlappingRects(Geometry r1, Geometry r2)
        {
            return !(r1.FillContainsWithDetail(r2) == IntersectionDetail.Empty);
        }

        /// <summary>
        /// 重なっているRectを返す、リストの先頭から重ねて行って、重ならないRectが出てきた時点で返す
        /// </summary>
        /// <param name="rList"></param>
        /// <returns></returns>
        private List<Rect> OverlappedRects(List<Rect> rList)
        {
            if (rList.Count == 0) return rList;

            List<Rect> result = new();
            result.Add(rList[0]);

            //最初のRectからGeometryを作成してGeometryGropuに追加
            GeometryGroup gg = new();
            gg.Children.Add(new RectangleGeometry(rList[0]));
            //GeometryGroupとRectの重なりを判定
            for (int i = 1; i < rList.Count; i++)
            {
                RectangleGeometry rg = new(rList[i]);
                //グループとRectの重なり判定
                if (IsOverlappingRects(gg, rg))
                {
                    result.Add(rList[i]);//リストに追加
                    gg.Children.Add(rg);//グループに追加
                }
                else
                {
                    //重なりが途切れたら終了
                    return result;
                }
            }
            return result;
        }

        //RectのリストからGeometryGroup作成
        private GeometryGroup MakeGeometryGroup(List<Rect> rects)
        {
            GeometryGroup gg = new();
            foreach (var item in rects)
            {
                gg.Children.Add(new RectangleGeometry(item));
            }
            return gg;
        }


        //メニューウィンドウの収集、メニューウィンドウにはTextがないので、これを利用している
        /// <summary>
        /// NextWindowを指定回数まで遡って、TextありのWindowが出るまですべて取得。Nextってのは基準になるウィンドウの下、Z軸(Zオーダー)で見たときの下にあるWindow。
        /// </summary>
        /// <param name="hWnd">基準になるウィンドウハンドル</param>
        /// <param name="loopCount">Nextを辿る回数の上限値</param>
        /// <returns>ウィンドウハンドル、RECT、Text</returns>
        private (List<IntPtr> ptrs, List<API.RECT> res, List<string> strs) GetNextWindowsWithNoneText(IntPtr hWnd, int loopCount)
        {
            List<IntPtr> ptrs = new();
            List<API.RECT> res = new();
            List<string> strs = new();
            int count = 0;

            IntPtr temp = hWnd;

            //NextWindowの収集
            do
            {
                if (temp == IntPtr.Zero) break;//Nextがなければ完了
                string text = GetWindowText(temp);
                //TextがあるWindowなら完了
                if (text != "") break;
                //Rectの幅や高さが0なら完了
                API.RECT r = GetWindowRect(temp);
                if (r.bottom - r.top == 0 || r.right - r.left == 0) break;

                //リストに追加
                ptrs.Add(temp);
                res.Add(r);
                strs.Add(text);

                //下の階層(next)のWindow取得
                temp = API.GetWindow(temp, API.GETWINDOW_CMD.GW_HWNDNEXT);
                count++;
            } while (count < loopCount);

            return (ptrs, res, strs);
        }




        //ウィンドウハンドルからText(タイトル名)やRECTを取得
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


        //デスクトップ全体のキャプチャ画像取得
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
