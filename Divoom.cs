using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DesktopDivoom
{
    public class Divoom
    {
        static HttpClient client = new();
        const string HTTP = "http://";
        const string POST = "/post";
        const string TEST_PAYLOAD = "Device/GetWeatherInfo";
        const string PLAY_GIF_PAYLOAD = "Device/PlayTFGIF";

        public static void Initialize()
        {
            //client.BaseAddress = new Uri("http://" + ipAddress + ":80");
            if (client == null)
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        private static async Task SendPayload(string ipAddress, Payload payload, Action<HttpResponseMessage> callback)
        {
            Initialize();
            string path = HTTP + ipAddress + POST;
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(path, payload);
                callback(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                callback(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });
            }
        }

        public static Task TestConnection(string ipAddress, Action<HttpResponseMessage> callback)
        {
            Payload testPayload = new()
            {
                Command = TEST_PAYLOAD
            };
            _ = SendPayload(ipAddress, testPayload, callback);
            return Task.CompletedTask;
        }

        public static Task SendGif(string ipAddress, string fileName, Action<HttpResponseMessage> callback)
        {
            Payload testPayload = new()
            {
                Command = PLAY_GIF_PAYLOAD,
                FileType = 2,
                FileName = fileName,
            };
            _ = SendPayload(ipAddress, testPayload, callback);
            return Task.CompletedTask;
        }
    }
}
