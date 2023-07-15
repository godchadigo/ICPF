using ICPFCore;

namespace Plugin.Mqtt
{
    public class MqttCore: PluginBase
    {
        public override string PluginName { get; set; } = "MqttCore";
        public override void onLoading()
        {
            
            
            Dictionary<object, object> mqttConfig = new Dictionary<object, object>
            {
                { "IP", "127.0.0.1" },
                { "Port" , 502 },
                { "Path" , "12/456/機台1" }
            };
            
            var result = Core.CreateConfig(this, "Config" );
            if (!result.isExist)
            {
                //result.config.Write("Pushlish", mqttConfig);
                //result.config.Save();
            }
            else
            {

            }
            
            /*
            config.Write("Test3", "你好");
            config.Write("Test2", 10.0f);
            config.Write("Test1", 123);
            config.Save();
            var res = config.ReadBool("Test3");
            Console.WriteLine("Test3: " + res);
            */
            /*
            config.Write("StudentModel", new Student() { Id = 1, Name = "Chadigo", Description = "這是一位良好學生。" });
            config.Save();

            var studentModel = config.Read("StudentModel");

            if (studentModel.value is Student st1)
            {
                Console.WriteLine("學生的名子:" + st1.Name);
            }
            var rRes = config.Remove("StudentModel");
            config.Save();
            Console.WriteLine("Result : " + rRes);

            var studentModel2 = config.Read("StudentModel");

            Console.WriteLine("Read2 : " + studentModel2.isOk);
            if (studentModel2.value is Student st2)
            {
                Console.WriteLine("學生的名子:" + st2.Name);
            }
            */
            base.onLoading();
        }
        public override void onCloseing()
        {
            base.onCloseing();
        }
    }
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}