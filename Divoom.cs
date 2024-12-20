﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
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
        const string SEND_FILE_COMMAND = "Draw/SendHttpGif";
        const string RESET_PIC_ID = "Draw/ResetHttpGifId";
        const string GET_GIF_ID = "Draw/GetHttpGifId";

        private static StringContent MakePayload(Object content)
        {
            StringContent payload = new(
                JsonSerializer.Serialize(content),
                Encoding.UTF8,
                "application/json");
            return payload;
        }

        private static string[] ConvertImageToBase64(Bitmap gif)
        {
            try
            { 
                // Get raw RGB bytes
                byte[][] rgbData = GetRgbBytes(gif);

                string[] strings = new string[rgbData.Length];
                // Convert to Base64
                for (int i = 0; i < rgbData.Length; i++)
                {
                    strings[i] = Convert.ToBase64String(rgbData[i]);
                }
                return strings;
            }
            catch (Exception ex)
            {
                return ["Exception: " + ex.Message];
            }
        }

        static byte[][] GetRgbBytes(Bitmap gif)
        {
            FrameDimension dimension = new FrameDimension(gif.FrameDimensionsList[0]);
            // Number of frames
            int frameCount = gif.GetFrameCount(dimension);
            int sizeOfRgbData = gif.Width * gif.Height * 3;
            byte[][] rgbData = new byte[frameCount][];

            for (int curFrame = 0; curFrame < frameCount; ++curFrame)
            {
                // Operate 1 frame at a time, initially each frame as we go
                gif.SelectActiveFrame(dimension, curFrame);
                rgbData[curFrame] = new byte[sizeOfRgbData];

                // Lock the bitmap data for direct access
                BitmapData bitmapData = gif.LockBits(
                    new Rectangle(0, 0, gif.Width, gif.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                // Calculate the size of the RGB data
                int stride = Math.Abs(bitmapData.Stride);
                int bgraDataSize = stride * gif.Height;

                // Copy the RGB data into a byte array
                byte[] bgraData = new byte[bgraDataSize];
                System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, bgraData, 0, bgraDataSize);

                // Unlock the bitmap data
                gif.UnlockBits(bitmapData);

                // Rearrange BGR to RGB
                int rgbPixel = 0;
                for (int i = 0; i < bgraData.Length; i += 4)
                {
                    rgbData[curFrame][rgbPixel] = bgraData[i + 2];
                    rgbData[curFrame][rgbPixel + 1] = bgraData[i + 1];
                    rgbData[curFrame][rgbPixel + 2] = bgraData[i];
                    rgbPixel += 3;
                }
            }
            return rgbData;
        }

        private static async Task SendPayload(string ipAddress, StringContent payload, Action<HttpResponseMessage>? callback, string? successMessage)
        {
            string path = HTTP + ipAddress + POST;
            try
            {
                HttpResponseMessage response = await client.PostAsync(path, payload);
                string content = await response.Content.ReadAsStringAsync();
                if (callback is not null && successMessage is not null)
                {
                    response.Content = CreateResponseContent(successMessage);
                    callback(response);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                HttpResponseMessage response = new(HttpStatusCode.InternalServerError)
                {
                    Content = CreateResponseContent(ex.InnerException is null ? "Unknown error" : ex.InnerException.ToString())
                };
                if (callback is not null)
                {
                    callback(response);
                }
            }
        }

        public static Task TestConnection(string ipAddress, Action<HttpResponseMessage> callback)
        {
            StringContent payload = MakePayload(new
            {
                Command = TEST_COMMAND
            });
            _ = SendPayload(ipAddress, payload, callback, "Device found!");
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
            _ = SendPayload(ipAddress, payload, callback, "Remote GIF send successfully");
            return Task.CompletedTask;
        }

        public static async Task<Task> ResetPicId(string ipAddress, Action<HttpResponseMessage> callback)
        {
            StringContent payload = MakePayload(new
            {
                Command = RESET_PIC_ID
            });
            await SendPayload(ipAddress, payload, callback, "GIF ID reset");
            return Task.CompletedTask;
        }

        private static int GetFrameTime(Bitmap gif, int frame)
        {
            if (gif is null) return 0;

            FrameDimension dimension = new(gif.FrameDimensionsList[0]);
            gif.SelectActiveFrame(dimension, frame);
            PropertyItem? frameProperty = gif.GetPropertyItem(20736);
            if (frameProperty is null) return 0;
            byte[]? delayPropertyBytes = frameProperty.Value;
            if (delayPropertyBytes is null) return 0;
            
            int frameDelay = BitConverter.ToInt32(delayPropertyBytes, frame * 4) * 10;
            return frameDelay;
        }

        private static StringContent CreateResponseContent(string error)
        {
            return new StringContent(
                error, // The error message content
                Encoding.UTF8, // Encoding for the message
                "application/json" // Media type for the response (can be application/json, text/plain, etc.)
            );
        }

        private static bool IsValidImageSize(Bitmap gif)
        {
            return (gif.Width == 64 && gif.Height == 64) ||
                    (gif.Width == 32 && gif.Height == 32) ||
                     (gif.Width == 16 && gif.Height == 16);
        }

        public static async Task<Task> SendFile(string ipAddress, string filePath, Action<HttpResponseMessage> callback)
        {
            await ResetPicId(ipAddress, callback);
            using (Bitmap gif = new(filePath))
            {
                if (!IsValidImageSize(gif))
                {
                    HttpResponseMessage response = new(HttpStatusCode.NotAcceptable)
                    {
                        Content = CreateResponseContent($"Image must be 16x16, 32x32, or 64x64. This is: {gif.Width}x{gif.Height}")
                    };
                    callback(response);
                    return Task.CompletedTask;
                }
                string[] base64Frames = ConvertImageToBase64(gif);
                for (int i = 0; i < base64Frames.Length; i++)
                {
                    int frameTime = GetFrameTime(gif, i);
                    StringContent payload = MakePayload(new
                    {
                        Command = SEND_FILE_COMMAND,
                        PicNum = base64Frames.Length,
                        PicWidth = gif.Width,
                        PicOffset = i,
                        PicID = 1,
                        PicSpeed = frameTime,
                        PicData = base64Frames[i]
                    });
                    _ = SendPayload(ipAddress, payload, callback, "Local image sent successfully");
                }
            }
            return Task.CompletedTask;
        }
    }
}
