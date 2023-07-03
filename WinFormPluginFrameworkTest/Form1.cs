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

        private void LSManager_TEvent(object? sender, EventArgs e)
        {

        }

        private void Program_ProgramCreated(object? sender, EventArgs e)
        {

        }



        //***** �ƥ󰻴� *****//
        public void onDeviceConnect(string deviceName)
        {
            //Console.WriteLine($"������ӦۥD���ݵo�X�� {deviceName} �]�ƤW�u!");
            richTextBox1.AppendText($"������ӦۥD���ݵo�X�� {deviceName} �]�ƤW�u!");
        }
        public void onDeviceDisconnect(string deviceName)
        {
            //Console.WriteLine($"������ӦۥD���ݵo�X�� {deviceName} �]���_�u!");
            richTextBox1.AppendText($"������ӦۥD���ݵo�X�� {deviceName} �]���_�u!");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var test = new ReadDataModel()
            {
                DeviceName = "MBUS_1",
                Address = "0",
                ReadLength = 100,
                DatasType = DataType.Int16,
            };

            Task.Run(async () =>
            {
                while (true)
                {
                    this.BeginInvoke(new Action(delegate
                    {
                        var result = pfc.GetData(test);
                        richTextBox1.AppendText(result.Message.ToString() + "\r\n");
                        Debug.WriteLine(result.Message);
                    }));

                    await Task.Delay(50);
                }
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }
        private const int MaxLength = 100000;
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
    }
}