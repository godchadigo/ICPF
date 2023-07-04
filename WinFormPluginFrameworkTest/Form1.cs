using PFC;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinFormPluginFrameworkTest
{
    public partial class Form1 : Form
    {
        private PFC.PFC pfc;
        public Form1()
        {
            InitializeComponent();
            pfc = new PFC.PFC();
            pfc.Connect();
        }

        /// <summary>
        /// 讀取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            var test = new ReadDataModel()
            {
                DeviceName = "Keyence8500_1",
                Address = "DM0",
                ReadLength = 100,
                DatasType = DataType.Int32,
            };
            var mc = new ReadDataModel()
            {
                DeviceName = "MC_1",
                Address = "D0",
                ReadLength = 100,
                DatasType = DataType.Int32,
            };
            Task.Run(async () => {
                while (true)
                {
                    /*
                    var result = pfc.GetData(test);
                    this.BeginInvoke(new Action(() => {
                        richTextBox1.AppendText(result.Data.ToString() + "\r\n");
                        Debug.WriteLine(result.Message);
                    }));
                    */
                    var mcResult = pfc.GetData(mc);
                    this.BeginInvoke(new Action(() => {
                        if (mcResult.IsOk)
                        {
                            richTextBox1.AppendText(mcResult.Data.ToString() + "\r\n");
                            Debug.WriteLine(mcResult.Message);
                        }                        
                    }));
                    await Task.Delay(10);
                }
            });                       
        }
        private int count = 9999999;
        /// <summary>
        /// 寫入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            var test = new WriteDataModel()
            {
                DeviceName = "Keyence8500_1",
                Address = "DM0",
                Datas = new object[] { count++ , count , count, count, count, count, count, count, },
                DatasType = DataType.Int32,
            };
            var result = pfc.SetData(test);
            richTextBox1.AppendText(result.Message.ToString() + "\r\n");
        }
        /// <summary>
        /// 清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        #region Event
        private const int MaxLength = 20000;
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

            if (richTextBox1.TextLength > MaxLength)
            {

                richTextBox1.Clear();
            }
            else
            {
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.ScrollToCaret();
            }
        }
        #endregion

    }
}