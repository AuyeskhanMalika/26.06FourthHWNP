using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Drawing;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Forms;

namespace _26._05
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread serverThread;
        private Socket serverSocket;
        public MainWindow()
        {
            InitializeComponent();
        }


        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() => {
                richTextBox.AppendText(message + "\n");
                richTextBox.ScrollToEnd();
            });
        }
        private void Start()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var ipAdress = "0.0.0.0";
            var port = 12345;
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAdress), port);
            serverSocket.Bind(endPoint);
            LogMessage("Server is running " + endPoint.ToString());

            serverThread = new Thread(ReceiveProcess);
            serverThread.Start();
        }
        private async void ReceiveProcess()
        {
            byte[] buffer = new byte[64 * 1024];
            EndPoint endPoint = new IPEndPoint(0, 0);
            while (true)
            {
                int size = serverSocket.ReceiveFrom(buffer, ref endPoint);
                var message = Encoding.UTF8.GetString(buffer, 0, size);
                LogMessage($"Received a message from the client. Message:{ message} Size:{size}");
                if (message.Contains("/sendScreenShot"))
                {
                    var screenshotBytes = await TakeScreenShot();
                    serverSocket.SendTo(screenshotBytes, endPoint);
                    LogMessage("Screenshot sent");
                    continue;
                }
            }
        }
        private Task<byte[]> TakeScreenShot()
        {
            Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, bitmap.RawFormat);
                    return Task.FromResult(memoryStream.ToArray());
                }
            }
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }
        }
    }
}
