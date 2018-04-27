#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using static uPLibrary.Networking.M2Mqtt.MqttClient;
using Newtonsoft.Json.Linq;

namespace MqttClientForLight
{
    public class Program
    {
        public static MqttClient mqTTclient;
        public const string URLMqTT = "";
        public const string URLEllie = "";

        public const string URLEllieExample = "http://localhost:3000";
        public const string URLExample = "broker.hivemq.com";

        public static HttpClient ellieClient = new HttpClient();
        public static Thread mqttThread;

        private static string[] lastData = new string[4] {"0", "0", "0", "0"};

        public static void Main(string[] args)
        {
            try
            {
                ellieClient.BaseAddress = new Uri(URLEllie);
                ellieClient.DefaultRequestHeaders.Accept.Clear();
                ellieClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                mqttThread = new Thread(new ThreadStart(CreateMqttThread));
                mqttThread.Start();
            }
        }

        public static void CreateMqttThread()
        {
            mqTTclient = new MqttClient(URLMqTT);
            mqTTclient.MqttMsgPublishReceived += msgPublishedReceived;
            mqTTclient.MqttMsgSubscribed += msgSubscribed;
            mqTTclient.ConnectionClosed += conCLosed;
            mqTTclient.MqttMsgPublished += publishSent;

            var clientID = Guid.NewGuid().ToString();
            mqTTclient.Connect(clientID);
		
	    // Insert the topic here
            mqTTclient.Subscribe(new string[] { "" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

            //while (true)
            //{
            //    var msg = Console.ReadLine();
            //    mqTTclient.Publish("public/i339762_group1allie", Encoding.UTF8.GetBytes(msg));
            //}
        }

        public static void publishSent(object sender, MqttMsgPublishedEventArgs e)
        {
            Console.WriteLine(e.IsPublished);
        }
        public static void msgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine("Subscribed for id: " + e.MessageId);
        }
        public static void conCLosed(object sender, EventArgs e)
        {
            if (!mqTTclient.IsConnected)
            {
                Console.WriteLine("Lost connection" + 
                    Environment.NewLine +
                    "Trying to reestablish connection");
                try
                {
                    mqttThread.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public static async void msgPublishedReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var data = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
            Console.WriteLine("data: " + data);
            try
            {
                await EllieClientPost(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task<Uri> EllieClientPost(string data)
        {

            ComareData(data);
            var dataSplit = data.Split(';');

            string dataString = "";
            dataString = buildCorString(lastData[0]);
            dataString += buildRGBString(lastData[0], lastData[1], lastData[2], lastData[3]);

            var parameters = new Dictionary<string, string> { { "data", dataString } };


            //HttpResponseMessage response = await ellieClient.PostAsync("/api/setSingleRow", new FormUrlEncodedContent (parameters));
            HttpResponseMessage response = await ellieClient.PostAsync("/api/setRGB", new FormUrlEncodedContent(parameters));
            response.EnsureSuccessStatusCode();
            return null; 
        }

        //returns "[[" [[x, y, [[ x, y, x, y x, y
        public static string buildCorString(string y)
        {
            string returnString = "[[";
            for (int i = 0; i < 21; i++)
            {
                for (int j = 0; j < int.Parse(y); j++)
                {
                    returnString += i;
                    returnString += ",";
                    returnString += j;
                    if (i < 20 || j < int.Parse(y) - 1)
                    {
                        returnString += ",";
                    }
                }
            }
            returnString += "],[";
            return returnString;
        }

        public static string buildRGBString(string y, string R, string G, string B)
        {
            string returnString = "";
            for (int i = 0; i < 21; i++)
            {
                for (int j = 0; j < int.Parse(y); j++)
                {
                    returnString += R;
                    returnString += ",";
                    returnString += G;
                    returnString += ",";
                    returnString += B;
                    if (i < 20 || j < int.Parse(y) - 1)
                    {
                        returnString += ",";
                    }
                }
            }
            returnString += "]]";
            return returnString;
        }


        public static EllieDto DataMapper(string data)
        {
            var ellie = new EllieDto();
            var dataArr = data.Split(';');
            ellie.data = string.Format("[{0},[{1},{2},{3}]]", 1, 10, 10, 10);
            
            return ellie;
        }

        public static bool CompareData(string data)
        {
            bool returnVar = false;
            string[] dataArr = data.Split(';');
            var count = 0;
            foreach (string ledValue in dataArr)
            {
                int tmp1 = int.Parse(ledValue);
                int tmp2 = int.Parse(lastData[count]);

                //if (tmp1 > tmp2 + 3 || tmp1 < tmp2 - 3)
                //{
                    lastData[count] = ledValue;
                    returnVar = true;
                //}
                count++;
            }
            return returnVar;
        }

    }
    
    public class EllieDto
    {
        public string data { get; set; }
    }
}
