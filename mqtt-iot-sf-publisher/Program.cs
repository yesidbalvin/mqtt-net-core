using M2Mqtt;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace mqtt_iot_sf
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine();

            string iotEndpoint = "a3211ymll254ks-ats.iot.us-east-2.amazonaws.com";
            Console.WriteLine("AWS IoT DotNet core message publisher starting");
            int brokerPort = 8883;

            string message = "Test message";
            string topic = "sf/topic/1";//publish

            var caCert = X509Certificate.CreateFromCertFile(Path.Join(AppContext.BaseDirectory, "Amazon_Root_CA_1.crt"));
            var clientCert = new X509Certificate2(Path.Join(AppContext.BaseDirectory, "certificate.cert.pfx"), "password");

            var client = new MqttClient(iotEndpoint, brokerPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            Console.WriteLine($"Connected to AWS IoT with client id: {clientId}.");

            int i = 0;
            while (true)
            {
                client.Publish(topic, Encoding.UTF8.GetBytes($"{message} {i}"));
                Console.WriteLine($"Published: {message} {i}");
                i++;
                Thread.Sleep(5000);
            }
        }
    }
}
