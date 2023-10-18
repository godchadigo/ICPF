using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace JsonRPCTest
{
    public partial class Form1 : Form
    {
        private WebSocketJsonRpcClient jsonRpcClient = new WebSocketJsonRpcClient();
        public Form1()
        {
            InitializeComponent();
      
        }
     

        private void button1_Click(object sender, EventArgs e)
        {
            string result = jsonRpcClient.InvokeT<string>("TestJsonRpc", InvokeOption.WaitInvoke, "RRQM");
            Console.WriteLine(result);
        }
    }
}
