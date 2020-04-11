using System;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace RpcChat.Client {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        ClientLib.Rpc.ChatClient client;

        public MainWindow() {
            InitializeComponent();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e) {

            // already connected
            if (client != null) return; 

            // parse and validate host/port inputs
            var host = TextBox_IP.Text;
            int port;
            if (!int.TryParse(TextBox_Port.Text, out port)) {
                MessageBox.Show("Invalid port number");
                return;
            }

            client = new ClientLib.Rpc.ChatClient(host, port);
            client.MessagesReceived += OnMessageReceived;

            SetOnline();

        }

        private void SetOnline() {
            TextBox_Status.Text = "ONLINE";
            TextBox_Status.Background = new SolidColorBrush(Color.FromRgb(0x45, 0xB0, 0x9C));
        }

        private void SetOffline() {
            TextBox_Status.Text = "OFFLINE";
            TextBox_Status.Background = new SolidColorBrush(Color.FromRgb(0xB0, 0x45, 0x67));
        }

        private void OnMessageReceived(object sender, ClientLib.Rpc.MessagesReceivedEventArgs args) {
            try {
                Dispatcher.Invoke(new Action(() => {    // dispatch the call to the UI thread
                    StringBuilder sb = new StringBuilder();
                    foreach (var msg in args.Messages) {
                        sb.AppendLine(msg.ToString());
                    }
                    TextBox_Chat.Text = sb.ToString();
                }));
            } catch (System.Threading.Tasks.TaskCanceledException) {
                // dispatcher shut down... ignore
            }            
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e) {
            
            // check if its connected
            if (client == null) return;

            // wait client to shutdown
            client.Shutdown();

            client = null;

            SetOffline();
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e) {
            
            if (client == null) return; // check if its connected

            try {
                // send the message to the server
                client.SendMessage(TextBox_Username.Text, TextBox_Input.Text);

                // clear text box after sending
                TextBox_Input.Text = string.Empty;

                // update client immediately
                client.Refresh();

            } catch (Exception) {
                MessageBox.Show("Error while sending the message to the server.");
            }
        }
    }
}
