using PluginFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.B
{
    public class MyBase 
    {
        public string PluginName => "MyBase";
        public virtual void onLoading() { }
    }
}
