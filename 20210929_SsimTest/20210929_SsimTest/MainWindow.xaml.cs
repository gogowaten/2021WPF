using System;
using System.Linq;
using System.Windows;

//画像比較に使われているSSIMの計算式をエクセルとC#で試してみた、平均、分散、共分散 - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/09/30/135653


namespace _20210929_SsimTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double C1 = 0.01 * 255 * (0.01 * 255);//(0.01*255)^2=6.5025
        private const double C2 = 0.03 * 255 * (0.03 * 255);//(0.03*255)^2=58.5225
        private const double C3 = C2 / 2.0;//58.5225/2=29.26125
        public MainWindow()
        {
            InitializeComponent();
        }


        #region 基本計算
        /// <summary>
        /// 共分散
        /// </summary>
        /// <param name="vs1"></param>
        /// <param name="ave1"></param>
        /// <param name="vs2"></param>
        /// <param name="ave2"></param>
        /// <returns></returns>
        private double Covariance(byte[] vs1, double ave1, byte[] vs2, double ave2)
        {
            if (vs1.Length != vs2.Length)
            {
                return double.NaN;
            }
            double total = 0;
            for (int i = 0; i < vs1.Length; i++)
            {
                total += (vs1[i] - ave1) * (vs2[i] - ave2);
            }
            return total / vs2.Length;
        }
        private double Covariance(byte[] vs1, byte[] vs2)
        {
            if (vs1.Length != vs2.Length)
            {
                return double.NaN;
            }

            double ave1 = Average(vs1);
            double ave2 = Average(vs2);
            double total = 0;
            for (int i = 0; i < vs1.Length; i++)
            {
                total += (vs1[i] - ave1) * (vs2[i] - ave2);
            }
            return total / vs2.Length;
        }

        /// <summary>
        /// 分散
        /// </summary>
        /// <param name="vs"></param>
        /// <param name="average"></param>
        /// <returns></returns>
        private double Variance(byte[] vs, double average)
        {
            double total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                double temp = vs[i] - average;
                total += temp * temp;
            }
            return total / vs.Length;
        }

        /// <summary>
        /// 分散、求め方その2
        /// </summary>
        /// <param name="vs"></param>
        /// <param name="average"></param>
        /// <returns></returns>
        private double Variance分散2(byte[] vs, double average)
        {
            double total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return (total / vs.Length) - (average * average);
        }

        /// <summary>
        /// 平均
        /// </summary>
        /// <param name="vs"></param>
        /// <returns></returns>
        private double Average(byte[] vs)
        {
            ulong total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total / (double)vs.Length;
        }
        #endregion 基本計算

        private double SSIM(byte[] vs1, byte[] vs2)
        {
            double ave1 = Average(vs1);//平均
            double ave2 = Average(vs2);
            double covar = Covariance(vs1, ave1, vs2, ave2);//共分散
            double vari1 = Variance(vs1, ave1);//分散
            double vari2 = Variance(vs2, ave2);
            double bunsi = (2 * ave1 * ave2 + C1) * (2 * covar + C2);//分子
            double bunbo = (ave1 * ave1 + ave2 * ave2 + C1) * (vari1 + vari2 + C2);//分母
            double ssim = bunsi / bunbo;
            return ssim;
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            double result = double.NaN;
            try
            {
                byte[] vs1 = MyTextBox1.Text.Split(' ').Select(x => byte.Parse(x)).ToArray();
                byte[] vs2 = MyTextBox2.Text.Split(' ').Select(x => byte.Parse(x)).ToArray();
                result = SSIM(vs1, vs2);
                MyTextBlock.Text = $"SSIM = {result}";
            }
            catch (Exception)
            {
                MyTextBlock.Text = $"SSIM = {result}";
            }
            
        }
    }
}
