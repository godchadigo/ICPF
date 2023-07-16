using ICPFCore;


namespace PluginB
{
    public class PluginB : PluginBase 
    {
        public override string PluginName { get; set; } = "PluginB";        

        public override void onLoading()
        {
            base.onLoading();
        }
        public override void onCloseing()
        {                        
            base.onCloseing();
        }
        public override void CommandTrig(string args)
        {
            base.CommandTrig(args);
        }

    }
}