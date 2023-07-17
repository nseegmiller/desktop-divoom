using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        }
        private async Task TestConnection()
        {
            testResponse.Foreground = Brushes.Black;
            testResponse.Text = "Searching...";
            testConnectionButton.IsEnabled = false;
            await Divoom.TestConnection(ipAddress.Text, HandleTestResponse);
            testConnectionButton.IsEnabled = true;
        }

        private void Test_Connection_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("IP Address: " + ipAddress.Text);
            _ = TestConnection();
        }

        private void sendGifButton_Click(object sender, RoutedEventArgs e)
        {
            if (sendGifURI.Text.Length > 0)
            {
                Debug.WriteLine("Sending URL at: " + sendGifURI.Text);
                Divoom.SendGif(ipAddress.Text, sendGifURI.Text, HandleGifSendResponse);
            }
            else
            {
                sendGifResponse.Foreground = Brushes.Red;
                sendGifResponse.Text = "Invalid URL";
            }
        }

        private void HandleGifSendResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                sendGifResponse.Foreground = Brushes.Green;
                sendGifResponse.Text = "GIF sent to device.";
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                sendGifResponse.Foreground = Brushes.Red;
                sendGifResponse.Text = "Error sending GIF.";
            }
        }

        private void HandleTestResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Debug.WriteLine("Device Found!");
                testResponse.Foreground = Brushes.Green;
                testResponse.Text = "Device Found!";
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                testResponse.Foreground = Brushes.Red;
                testResponse.Text = "Invalid IP Address.";
            }
            else
            {
                Debug.WriteLine("Device Not Found.");
                testResponse.Foreground = Brushes.Red;
                testResponse.Text = "Device Not Found.";                
            }       
        }

        private void IP_Address_Changed(object sender, TextChangedEventArgs e)
        {

        }

        
    }
}
