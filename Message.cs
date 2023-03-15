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

        private IMqttClient mqttClient;
        ConcurrentQueue<MqttApplicationMessage> messageQueue = new ConcurrentQueue<MqttApplicationMessage>();
        MqttClientOptions optionsBuilder;

        string subcribeTopic = "ttn/g2d/";

        private string serverAddress;
        private string userName;
        private string password;
        private int serverPort;

        public Message(string serverAddress, int serverPort, string userName, string password, string clientId)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;

            this.userName = userName;
            this.password = password;
            this.subcribeTopic += $"{clientId}/#";

            new Task(async () => await InitMqtt()).Start();
        }

        private async Task InitMqtt()
        {
            optionsBuilder = new MqttClientOptionsBuilder()
                    .WithClientId("display-" + DateTime.Now.Ticks)
                    .WithTcpServer(serverAddress, serverPort)
                    .WithCredentials(userName, password)
                    .WithCleanSession()
                    .Build();

            mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(optionsBuilder, CancellationToken.None);
            await mqttClient.SubscribeAsync(subcribeTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

            mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedHandler;
            mqttClient.DisconnectedAsync += MqttDisconnectedEvent;
        }

        private async Task MqttDisconnectedEvent(MqttClientDisconnectedEventArgs e)
        {
            Log.Fatal($"MQTT disconnected: {JsonConvert.SerializeObject(e)}");

            await Task.Delay(5000);

            await mqttClient.ConnectAsync(optionsBuilder, CancellationToken.None);
            await mqttClient.SubscribeAsync(subcribeTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
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
