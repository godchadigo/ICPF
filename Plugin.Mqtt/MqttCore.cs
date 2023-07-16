using ICPF.Config;
using ICPFCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Text;

namespace Plugin.Mqtt
{
    public class MqttCore: PluginBase
    {
        public override string PluginName { get; set; } = "MqttCore";
        private static MqttServer mqttServer;
        private static Config brokerConfig;
        private bool showDebug = false;
        private CancellationTokenSource cts;
        public override void onLoading()
        {
                                    
            var result = Core.CreateConfig(this, "BrokerConfig" );
            
            if (!result.isExist)
            {
                result.config.Write("MqttConfig", new MqttConfigModel() { 
                    Name = "MySimpleMqttBroker" ,
                    IP = "localhost",
                    Port = 1883,
                    UseAuthrization = true,
                    Users = new List<User>() { new User() { Account = "chadigo" , Password = "123456" }  },
                });
                result.config.Write("PublishTagTopic", new List<PublishTagModel>() { new PublishTagModel() { Topic="123" , DeviceName="MBUS_2", TagName= "1F溫度表_溫度" , Qos = MqttQualityOfServiceLevel.AtMostOnce } } );
                result.config.Write("PublishAddressTopic", new List<PublishAddressModel>() { 
                    new PublishAddressModel() { Topic="456" , DeviceName="MBUS_2", Address="10" , ReadLength=10, DatasType = DataType.UInt32 , Qos = MqttQualityOfServiceLevel.AtMostOnce }
                });
                result.config.Save();                
            }
            else
            {

            }
            brokerConfig = result.config;

            base.onLoading();
            
            var publishConfig = result.config.Read<MqttConfigModel>("MqttConfig");
            var topicList = result.config.Read<List<PublishTagModel>>("PublishTopic");
            //Console.WriteLine(publishConfig.ToString());
            if (publishConfig.isOk)
            {                                

                if (publishConfig.value is MqttConfigModel publishModel)
                {
                    var port = publishModel.Port;
                    Task.Run(async () =>
                    {
                        var mqttFactory = new MqttFactory();
                        // The port can be changed using the following API (not used in this example).
                        var mqttServerOptions = new MqttServerOptionsBuilder()
                            .WithDefaultEndpoint()
                            .WithDefaultEndpointPort(port)                            
                            .Build();
                        mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
                        Console.WriteLine("啟動Mqtt Broker...");
                        mqttServer.InterceptingPublishAsync += Server_InterceptingPublishAsync;
                        mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;
                        mqttServer.ValidatingConnectionAsync += MqttServer_ValidatingConnectionAsync;
                        await mqttServer.StartAsync();
                        Console.WriteLine("Mqtt Broker啟動完成!");

                        Console.WriteLine("啟動定時發報任務!");
                        await StartTask();                        
                    });
                }                
            }

        }
        public override async void onCloseing()
        {
            base.onCloseing();
            cts.Cancel();
            await mqttServer.StopAsync();
        }
        public override void CommandTrig(string args)
        {
            base.CommandTrig(args);
            if (args == "mqttshow") showDebug = !showDebug;            
        }

        async Task StartTask()
        {
            cts = new CancellationTokenSource();
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var topicTagList = brokerConfig.Read<List<PublishTagModel>>("PublishTagTopic");
                    if (topicTagList.isOk)
                    {
                        foreach (var topic in topicTagList.value)
                        {
                            var topicName = topic.Topic;
                            var deviceName = topic.DeviceName;
                            var tagName = topic.TagName;

                            var tagResult = await GetTag(deviceName, tagName);

                            if (tagResult.IsOk)
                            {
                                string data = string.Empty;
                                if (tagResult.Data.Length > 1)
                                {
                                    data = Newtonsoft.Json.JsonConvert.SerializeObject(tagResult.Data);
                                }
                                else
                                {
                                    data = tagResult.Data[0]?.ToString();
                                }

                                var message = new MqttApplicationMessageBuilder().WithTopic(topicName).WithPayload(data).Build();
                                await mqttServer.InjectApplicationMessage(
                                  new InjectedMqttApplicationMessage(message)
                                  {
                                      SenderClientId = "BrokerTagPublisher"
                                  });
                            }
                        }
                    }
                    var topicAddressList = brokerConfig.Read<List<PublishAddressModel>>("PublishAddressTopic");
                    if (topicAddressList.isOk)
                    {
                        foreach (var topic in topicAddressList.value)
                        {
                            var topicName = topic.Topic;
                            var deviceName = topic.DeviceName;
                            var address = topic.Address;
                            var len = topic.ReadLength;
                            var dt = topic.DatasType;

                            var tagResult = await GetData(new ReadDataModel()
                            {
                                DeviceName = deviceName,
                                Address = address,
                                ReadLength = len,
                                DatasType = dt
                            });

                            if (tagResult.IsOk)
                            {
                                string data = string.Empty;
                                if (tagResult.Data.Length > 1)
                                {
                                    data = Newtonsoft.Json.JsonConvert.SerializeObject(tagResult.Data);
                                }
                                else
                                {
                                    data = tagResult.Data[0]?.ToString();
                                }

                                var message = new MqttApplicationMessageBuilder().WithTopic(topicName).WithPayload(data).Build();
                                await mqttServer.InjectApplicationMessage(
                                  new InjectedMqttApplicationMessage(message)
                                  {
                                      SenderClientId = "BrokerAddressPublisher"
                                  });
                            }
                        }
                    }
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {

            }finally {
                cts.Dispose();
                cts = null;
            }
              
        }
        Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
        {
            var mqttConfig = brokerConfig.Read<MqttConfigModel>("MqttConfig");
            if (mqttConfig.isOk)
            {
                var useAuthrization = mqttConfig.value.UseAuthrization;
                var userList = mqttConfig.value.Users;

                if (!useAuthrization)
                {
                    arg.ReasonCode = MqttConnectReasonCode.Success;
                    return Task.CompletedTask;
                }

                if (arg.ClientId.Length < 10)
                {
                    arg.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                    return Task.CompletedTask;
                }

                foreach (var user in userList)
                {
                    if (arg.UserName == user.Account && arg.Password == user.Password)
                    {
                        arg.ReasonCode = MqttConnectReasonCode.Success;
                        return Task.CompletedTask;
                    }
                }

                arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                return Task.CompletedTask;
            }
            else
            {
                arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                return Task.CompletedTask;
            }
        }
        Task MqttServer_ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
        Task Server_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
        {
            // Convert Payload to string
            var payload = arg.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(arg.ApplicationMessage?.Payload);

            if (showDebug)
            {
                Console.WriteLine(
               " TimeStamp: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}, QoS = {4}, Retain-Flag = {5}",

               DateTime.Now,
               arg.ClientId,
               arg.ApplicationMessage?.Topic,
               payload,
               arg.ApplicationMessage?.QualityOfServiceLevel,
               arg.ApplicationMessage?.Retain);
            }           
            return Task.CompletedTask;
        }
        
    }
    public class MqttConfigModel
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public bool UseAuthrization { get; set; }
        public List<User> Users {get;set;}              
    }
    public class User
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }
    public class PublishTagModel
    {
        public string Topic { get; set; }
        public string DeviceName { get; set; }
        public string TagName { get; set; }
        public MqttQualityOfServiceLevel Qos {  get; set; }
    }
    public class PublishAddressModel
    {
        public string Topic { get; set; }
        public string DeviceName { get; set; }
        public string Address { get; set; }
        public ushort ReadLength { get; set; }
        public DataType DatasType { get; set; }
        public MqttQualityOfServiceLevel Qos { get; set; }
    }
}