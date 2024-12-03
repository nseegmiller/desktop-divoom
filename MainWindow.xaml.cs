using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopDivoom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Divoom divoom = new();

        public MainWindow()
        {
            InitializeComponent();
            ipAddress.Text = "10.0.0.54";
        }

        private async void Test_Connection_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("IP Address: " + ipAddress.Text);            
            responseText.Foreground = Brushes.Black;
            responseText.Text = "Searching...";
            testConnectionButton.IsEnabled = false;
            await Divoom.TestConnection(ipAddress.Text, HandleResponse);
            testConnectionButton.IsEnabled = true;
        }

        private async void Send_Gif_Button_Click(object sender, RoutedEventArgs e)
        {
            if (sendGifURI.Text.Length > 0)
            {
                Debug.WriteLine("Sending URL at: " + sendGifURI.Text);
                responseText.Foreground = Brushes.Black;
                responseText.Text = "Sending GIF...";
                sendGifButton.IsEnabled = false;
                await Divoom.SendGif(ipAddress.Text, sendGifURI.Text, HandleResponse);
                sendGifButton.IsEnabled = true;
            }
            else
            {
                responseText.Foreground = Brushes.Red;
                responseText.Text = "Invalid URL";
            }
        }

        private void Browse_Button_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "file.gif", // Default file name
                DefaultExt = ".gif", // Default file extension
                Filter = "Gifs (.gif)|*.gif" // Filter files by extension
            };

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                sendFilePath.Text = dialog.FileName;
            }
        }

        private async void Send_File_Button_Click(object sender, RoutedEventArgs e)
        {            
            Debug.WriteLine("Sending Frames");
            responseText.Foreground = Brushes.Black;
            responseText.Text = "Sending Frames";
            sendFileButton.IsEnabled = false;
            await Divoom.SendFile(ipAddress.Text, sendFilePath.Text, HandleResponse);
            sendFileButton.IsEnabled = true;
        }

        private async void HandleResponse(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                responseText.Foreground = Brushes.Green;
                content ??= "Operation completed successfully.";
            }
            else
            {
                responseText.Foreground = Brushes.Red;
                content ??= "Error completing operation.";
            }
            responseText.Text = content;
        }
       
    }
}
