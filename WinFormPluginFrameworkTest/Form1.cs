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

        

        //***** 事件偵測 *****//
        public void onDeviceConnect(string deviceName)
        {
            //Console.WriteLine($"偵測到來自主機端發出的 {deviceName} 設備上線!");
            richTextBox1.AppendText($"偵測到來自主機端發出的 {deviceName} 設備上線!");
        }
        public void onDeviceDisconnect(string deviceName)
        {
            //Console.WriteLine($"偵測到來自主機端發出的 {deviceName} 設備斷線!");
            richTextBox1.AppendText($"偵測到來自主機端發出的 {deviceName} 設備斷線!");
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
                    var result = pfc.Send("Test");
                    Debug.WriteLine(result.Message);
                    await Task.Delay(10);
                }                
            });
            
        }
    }
}