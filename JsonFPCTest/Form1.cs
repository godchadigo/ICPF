using PFCJsonRPC.Model;
using System.Collections.Concurrent;
using System.Diagnostics;
using TouchSocket.Rpc;

namespace JsonFPCTest
{
    public partial class Form1 : Form
    {
        private PFCJsonRPC.JsonPFC fpc = new PFCJsonRPC.JsonPFC();
        public Form1()
        {
            InitializeComponent();
            fpc.CommunicationStatusEvent += (sender, msg) => {
                this.BeginInvoke(new Action(() => {
                    richTextBox1.AppendText(msg);
                }));                
            };
            fpc.Connect();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            var result = fpc.SendObject<string>("TestJsonRpc", TouchSocket.Rpc.InvokeOption.WaitInvoke, "這是一項測試！");
            Debug.WriteLine(result);
            */
            Task.Run(async () => {
                while (true)
                {
                    try
                    {
                        string buf = string.Empty;
                        //var result = fpc.jsonRpcClient.InvokeT<OperationResult<ConcurrentDictionary<string, QJTagData>>>("GetContainer", TouchSocket.Rpc.InvokeOption.WaitInvoke , "Modbus");
                        var result = fpc.GetContainer("Modbus");

                        this.BeginInvoke(new Action(delegate {
                            richTextBox1.AppendText(fpc.isConnected + "\r\n");
                        }));
                        if (!fpc.isConnected || !result.IsOk)
                        {
                            await Task.Delay(1000);
                            continue;
                        }

                        foreach (var item in result.Data)
                        {
                            Debug.WriteLine($"UUID:{item.Value.Uuid} isOK:{item.Value.IsOk} GroupName:{item.Value.GroupName} TagName:{item.Value.TagName} Value:{item.Value.Data} MSG:{item.Value.Message}");
                            //richTextBox1.AppendText($"UUID:{item.Value.Uuid} GroupName:{item.Value.GroupName} TagName:{item.Value.TagName} Value:{item.Value.Data}" + "\r\n");
                            buf += $"UUID:{item.Value.Uuid} isOK:{item.Value.IsOk} GroupName:{item.Value.GroupName} TagName:{item.Value.TagName} Value:{item.Value.Data} MSG:{item.Value.Message}" + "\r\n";
                        }

                        this.BeginInvoke(new Action(delegate {
                            richTextBox1.AppendText(buf);
                        }));
                        await Task.Delay(200);
                    }
                    catch (Exception ex)
                    {

                    }
                    
                }
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task.Run(async () => {
                while (true)
                {
                    try
                    {
                        var result = fpc.GetMachins();
                        if (!fpc.isConnected || !result.IsOk)
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                        //var result = fpc.jsonRpcClient.InvokeT<OperationResult<List<string>>>("GetMachins", TouchSocket.Rpc.InvokeOption.WaitInvoke);
                        
                        Debug.WriteLine(result.Uuid);
                        await Task.Delay(10);
                    }
                    catch (Exception ex)
                    {

                    }
                   
                }                
            });            
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            //var result = await fpc.jsonRpcClient.InvokeTAsync<OperationResult<QJTagData>>("GetTag", TouchSocket.Rpc.InvokeOption.WaitInvoke , "Modbus" , "電壓");
            var result = fpc.GetTag("Modbus", "電壓");
            if (result.IsOk)
                Debug.WriteLine($"Uuid:{result.Uuid} TagName:{result.Data.TagName} Value:{result.Data.Data}");
            
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // 设置字符数的阈值
            int maxLength = 10000; // 一万字符

            // 获取当前 RichTextBox 控件中的字符数
            int currentLength = richTextBox1.Text.Length;

            // 如果字符数超过阈值，执行清除操作
            if (currentLength > maxLength)
            {
                richTextBox1.Clear();                                
            }
            richTextBox1.Select(richTextBox1.Text.Length,0);
            richTextBox1.ScrollToCaret();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (fpc.isConnected)
            {
                var result = fpc.GetTagList("Modbus");
                if (result.IsOk)
                {
                    foreach (var tag in result.Data)
                    {
                        richTextBox1.AppendText($"Uuid:{result.Uuid} TagName:{tag.TagName} GroupName:{tag.GroupName} \r\n");
                    }
                }
                    
            }
        }
    }
}