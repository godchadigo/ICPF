using PFC;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
            Task.Run(async () =>
            {
                while (true)
                {

                    await Task.Delay(1000);
                }                
            });
            var test = new ReadDataModel()
            {
                DeviceName = "MBUS_1",
                Address = "0",
                ReadLength = 100,
                DatasType = DataType.Int16,
            };

            var result = pfc.GetData(test);
            Debug.WriteLine(result.Message);

        }
    }
}