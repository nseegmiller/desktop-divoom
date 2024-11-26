using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DesktopDivoom
{
    public class Divoom
    {
        static HttpClient client = new();
        const string HTTP = "http://";
        const string POST = "/post";
        const string TEST_COMMAND = "Device/GetWeatherInfo";
        const string PLAY_GIF_COMMAND = "Device/PlayTFGif";

        private static StringContent MakePayload(Object content)
        {
            StringContent payload = new(
                JsonSerializer.Serialize(content),
                Encoding.UTF8,
                "application/json");
            return payload;
        }

        private static async Task SendPayload(string ipAddress, StringContent payload, Action<HttpResponseMessage> callback)
        {
            string path = HTTP + ipAddress + POST;
            try
            {
                HttpResponseMessage response = await client.PostAsync(path, payload);

                var jsonResponse = await response.Content.ReadAsStringAsync();
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
            StringContent payload = MakePayload(new
            {
                Command = TEST_COMMAND
            });
            _ = SendPayload(ipAddress, payload, callback);
            return Task.CompletedTask;
        }

        public static Task SendGif(string ipAddress, string fileName, Action<HttpResponseMessage> callback)
        {
            StringContent payload = MakePayload(new
            {
                Command = PLAY_GIF_COMMAND,
                FileType = 2,
                FileName = fileName
            });
            _ = SendPayload(ipAddress, payload, callback);
            return Task.CompletedTask;
        }
    }
}
