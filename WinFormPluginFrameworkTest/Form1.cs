using PFC;
using System.Data;
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
            pfc.CommunicationStatusEvent += Pfc_CommunicationErrorEvent;
            pfc.Connect("127.0.0.1:5000");
        }

        private void Pfc_CommunicationErrorEvent(object? sender, string e)
        {
            this.BeginInvoke(new Action(() =>
            {
                richTextBox1.AppendText(e + "\r\n");
            }));
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
            var mbus = new ReadDataModel()
            {
                DeviceName = "MBUS_1",
                Address = "0",
                ReadLength = 50,
                DatasType = DataType.Int32,
            };
            var vigor = new ReadDataModel()
            {
                DeviceName = "VSM_1",
                Address = "D0",
                ReadLength = 1024,
                DatasType = DataType.Int16,
            };
            Task.Run(async () =>
            {
                while (true)
                {
                    /*
                    var result = pfc.GetData(test);
                    this.BeginInvoke(new Action(() => {
                        richTextBox1.AppendText(result.Data.ToString() + "\r\n");
                        Debug.WriteLine(result.Message);
                    }));
                    */
                    var mcResult = await pfc.GetData(vigor);
                    this.BeginInvoke(new Action(() =>
                    {
                        if (mcResult.IsOk)
                        {
                            richTextBox1.AppendText(mcResult.DeviceName + " # " + mcResult.Message + " | " + mcResult.Data.ToString() + "\r\n");
                            Debug.WriteLine(mcResult.Message);
                        }
                    }));
                    await Task.Delay(1);
                }
            });
        }
        private Int16 count = 0;
        /// <summary>
        /// 寫入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                var test = new WriteDataModel()
                {
                    DeviceName = "Keyence8500_1",
                    Address = "DM0",
                    Datas = new object[] { count, count, count, count, count, count, count, count, },
                    DatasType = DataType.Int32,
                };
                var mbus = new WriteDataModel()
                {
                    DeviceName = "MBUS_1",
                    Address = "0",
                    Datas = new object[] { count, count, count, count, count, count, count, count, },
                    DatasType = DataType.Int32,
                };
                var vigor = new WriteDataModel()
                {
                    DeviceName = "VSM_1",
                    Address = "D0",
                    Datas = new object[] { 0, count, count, count, count, count, count, count, },
                    DatasType = DataType.Int16,
                };
                count++;
                var result = await pfc.SetData(vigor);

                this.BeginInvoke(new Action(delegate {
                    richTextBox1.AppendText(result.DeviceName + " # " + result.Message + " | " + mbus.ToString() + "\r\n");
                }));
                
            });
            
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

        private async void button4_Click(object sender, EventArgs e)
        {
            //var result = await pfc.GetTag("統亞1F-1", "電流");
            var result = await pfc.GetTagGroup("統亞1F-1", "基礎數據");
            
            if (result.IsOk)
            {
                foreach (var item in result.Data)
                {
                    //Console.WriteLine(result.Message);
                    this.BeginInvoke(new Action(delegate {
                        richTextBox1.AppendText(item.DeviceName + " # " + item.Message + " | Data:" + item.Data + "TagName:" + item.TagName + "GroupName:" + item.GroupName + "\r\n");
                    }));
                }
            }         
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            await Task.Run(async() =>
            {
                var result = await pfc.GetContainer("統亞1F-1");
                if (!result.IsOk) return;
                this.BeginInvoke(new Action(delegate {
                    foreach (var tag in result.Data)
                    {
                        if (tag.Value.IsOk)
                        {                            
                            richTextBox1.AppendText(" # " + result.Message + " | " + tag.Value.GroupName + " " + tag.Value.TagName + " " + tag.Value.Data + "\r\n");
                        }
                        else
                        {
                            richTextBox1.AppendText(" # " + result.Message + " | " + tag.Value.DeviceName + tag.Value.Message + "\r\n");
                        }

                    }

                }));
            });
            
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    var result = await pfc.GetTag("統亞1F-1", "電壓");
                    if (!result.IsOk) return;
                    this.BeginInvoke(new Action(delegate {
                        richTextBox1.AppendText(" # " + result.Data.Message + " | DeviceName" + result.Data.DeviceName + " TagName:" + result.Data.TagName + " Data:" + result.Data.Data + "\r\n");
                    }));
                    await Task.Delay(10);
                }
                
            });
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                var result = await pfc.GetMachins();
                if (!result.IsOk) return;
                this.BeginInvoke(new Action(delegate {
                    foreach (var deviceName in result.Data)
                    {
                        richTextBox1.AppendText(" # " + result.Message + " | DeviceName" + deviceName + "\r\n");
                    }                    
                }));
            });
        }
    }
}