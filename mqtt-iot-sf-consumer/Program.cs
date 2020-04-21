using M2Mqtt;
using M2Mqtt.Messages;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace mqtt_iot_sf_consumer
{
    class Program
    {
        private static ManualResetEvent manualResetEvent;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine();

            string iotEndpoint = "a3211ymll254ks-ats.iot.us-east-2.amazonaws.com";
            int brokerPort = 8883;

            Console.WriteLine("AWS IoT dotnet  message consumer starting ...");

            var certFile = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Amazon_Root_CA_1.crt"));
            var certBase64 = Convert.ToBase64String(certFile);


            var pfxFile = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificate.cert.pfx"));

            var base64 = Convert.ToBase64String(pfxFile);


            var caCert = X509Certificate.CreateFromCertFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Amazon_Root_CA_1.crt"));
            var clientCert = new X509Certificate2(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificate.cert.pfx"), "password");

            var client = new MqttClient(iotEndpoint, brokerPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);

            client.MqttMsgSubscribed += IoTClient_MqttMsgSubscribed;
            client.MqttMsgPublishReceived += IoTClient_MqttMsgPublishReceived;

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            Console.WriteLine($"Connected to AWS IoT with client ID: {clientId}");

            string topic = "sf/topic/+";//subscribe
            Console.WriteLine($"Subscribing to topic: {topic}");
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

            //Keep the main thread alive for the event receivers to get invoked
            KeepConsoleAppRunning(() =>
            {
                client.Disconnect();
                Console.WriteLine("Disconnecting client...");
            });
        }

        private static void IoTClient_MqttMsgPublishReceived(object sender, M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            Console.WriteLine("Message received: " + Encoding.UTF8.GetString(e.Message));
        }

        private static void IoTClient_MqttMsgSubscribed(object sender, M2Mqtt.Messages.MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine($"Successfully subscribed to the AWS IoT topic.");
        }

        private static void KeepConsoleAppRunning(Action onShutdown)
        {
            manualResetEvent = new ManualResetEvent(false);
            Console.WriteLine("Press CTRL + C to exit");

            Console.CancelKeyPress += (sender, e) =>
            {
                onShutdown();
                e.Cancel = true;
                manualResetEvent.Set();
            };

            manualResetEvent.WaitOne();
        }
    }
}
