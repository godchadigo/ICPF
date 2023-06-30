using ConsolePluginTest;


namespace PluginA
{
    public class PluginA : MemoryShareManager, IPlugin
    {
        public void onLoading()
        {            
            Thread t = new Thread (async () => { 
                while (true)
                {
                    Console.WriteLine("PluginA : " + instance.Data);
                    await Task.Delay(1000);
                }
            });    
            t.Start ();
        }
        public void onCloseing()
        {
            Console.WriteLine("PluginA Closeing...");
        }
    }
}