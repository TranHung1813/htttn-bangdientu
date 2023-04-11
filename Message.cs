using MQTTnet.Client;
using MQTTnet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Threading;
using Newtonsoft.Json;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace Display
{
    public class Message
    {
        public MqttApplicationMessage GetMessage
        {
            get
            {
                if(messageQueue.TryDequeue(out MqttApplicationMessage message))
                    return message;

                return null;
            }
        }
        public async void SendMessage(string payload)
        {
            if(GroupsList.Count > 0)
            {
                try
                {
                    foreach (var group in GroupsList)
                    {
                        string GroupPublishTopic = publishTopic_Msg + group.Id + "/" + ClientId;
                        await PublishMessageAsync(GroupPublishTopic, payload, false);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "PublishMessage");
                }
            }
        }
        private IMqttClient mqttClient;
        ConcurrentQueue<MqttApplicationMessage> messageQueue = new ConcurrentQueue<MqttApplicationMessage>();
        MqttClientOptions optionsBuilder;

        public string subcribeTopic_Default = "ttn/g2d/";
        public string subcribeTopic_Groups = "rx/";
        public string subcribeTopic_Msg = "tx/";
        public string publishTopic_Msg = "tx/";

        private string serverAddress;
        private string userName;
        private string password;
        private int serverPort;

        private string ClientId = "";

        public static List<Group> GroupsList = new List<Group>();

        public Message(string serverAddress, int serverPort, string userName, string password, string clientId)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;

            this.userName = userName;
            this.password = password;
            ClientId = clientId;
            this.subcribeTopic_Default += $"{clientId}";
            this.subcribeTopic_Groups += $"{clientId}";

            new Task(async () => await InitMqtt()).Start();
        }

        private async Task InitMqtt()
        {
            try
            {
                optionsBuilder = new MqttClientOptionsBuilder()
                        .WithClientId("display-" + DateTime.Now.Ticks)
                        .WithTcpServer(serverAddress, serverPort)
                        .WithCredentials(userName, password)
                        .WithCleanSession()
                        .Build();

                mqttClient = new MqttFactory().CreateMqttClient();
                await mqttClient.ConnectAsync(optionsBuilder, CancellationToken.None);
                await mqttClient.SubscribeAsync(subcribeTopic_Default, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
                await mqttClient.SubscribeAsync(subcribeTopic_Groups, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

                if (GroupsList.Count > 0)
                {
                    try
                    {
                        foreach (var group in GroupsList)
                        {
                            string GroupTopic = subcribeTopic_Msg + group.Id;
                            await mqttClient.SubscribeAsync(GroupTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Subcribe2Groups");
                    }
                }

                mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedHandler;
                mqttClient.DisconnectedAsync += MqttDisconnectedEvent;
            }
            catch
            {
                Log.Error("InitMqtt_Fail");
                await Task.Delay(5000);

                await InitMqtt();

            }
        }
        public static void Load_Groups_Info(List<Group> list)
        {
            GroupsList = list;
        }

        public async void Subcribe2Groups(List<Group> groups)
        {
            try
            {
                foreach (var group in groups)
                {
                    string GroupTopic = subcribeTopic_Msg + group.Id;
                    await mqttClient.SubscribeAsync(GroupTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Subcribe2Groups");
            }
            mqttClient.ApplicationMessageReceivedAsync -= ApplicationMessageReceivedHandler;
            mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedHandler;
        }

        private async Task MqttDisconnectedEvent(MqttClientDisconnectedEventArgs e)
        {
            Log.Fatal($"MQTT disconnected: {JsonConvert.SerializeObject(e)}");

            await Task.Delay(5000);

            await mqttClient.ConnectAsync(optionsBuilder, CancellationToken.None);
            await mqttClient.SubscribeAsync(subcribeTopic_Default, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            await mqttClient.SubscribeAsync(subcribeTopic_Groups, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

            if (GroupsList.Count > 0)
            {
                try
                {
                    foreach (var group in GroupsList)
                    {
                        string GroupTopic = subcribeTopic_Msg + group.Id;
                        await mqttClient.SubscribeAsync(GroupTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Subcribe2Groups");
                }
            }

            mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedHandler;
            mqttClient.DisconnectedAsync += MqttDisconnectedEvent;
        }

        public async Task ApplicationMessageReceivedHandler(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            try
            {
                messageQueue.Enqueue(eventArgs.ApplicationMessage);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MQTT AddNewMessage");
            }
        }

        private async Task PublishMessageAsync(string topic, string payload, bool Retain = false)
        {
            try
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(payload)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                        .WithRetainFlag(Retain)
                        .Build();
                var result = await mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None);

            }
            catch (Exception ex)
            {
                Log.Error(ex, " PublishMessage");
            }

        }
    }
}
