using Grpc.Core;
using RpcChat.ClientLib.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RpcChat.ClientLib.Rpc {

    public class MessagesReceivedEventArgs : EventArgs {
        public IEnumerable<ChatMessage> Messages;
    }

    public class ChatClient {

        private const int POLL_INTERVAL = 1000;

        public delegate void MessagesReceivedHandler(object sender, MessagesReceivedEventArgs args);

        public event MessagesReceivedHandler MessagesReceived;

        private int updating = 0;

        // list where we store locally the messages
        IEnumerable<ChatMessage> messages = new List<ChatMessage>();

        ChatContract.ChatContractClient client; // the RPC client
        Channel channel;    // channel for client-server communication
        Timer timer; // timer to poll server for new messages     

        public ChatClient(string host, int port) {

            // creates a channel where we will communicate with the host
            channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);

            // creates a client based in the chat contract defined by the probuffer
            client = new ChatContract.ChatContractClient(channel);

            // create a timer to poll the server for new messages
            timer = new Timer(UpdateLocalMessages, null, POLL_INTERVAL, -1);
        }

        private async void UpdateLocalMessages(object state) {

            if (Interlocked.CompareExchange(ref updating, 1, 0) == 0) {
                try {

                    // stop timer to avoid reentrance while we await messages
                    timer.Change(-1, -1);

                    // wait all messages to arrive
                    messages = await GetMessagesAsync();

                    // after receiving all messages, raise the event to notify all listeners
                    MessagesReceived?.Invoke(this, new MessagesReceivedEventArgs() { Messages = messages });

                    // restart timer for next iteration
                    timer.Change(POLL_INTERVAL, -1);

                } catch (RpcException) {
                    // error contacting the server. as timer was not re-enabled we just ignore and return
                } finally {
                    Interlocked.Exchange(ref updating, 0); // reset back to not entered
                }
            }
        }

        /// <summary>
        /// Retrieve all messages from the server using a RPC call
        /// </summary>
        private async Task<IEnumerable<ChatMessage>> GetMessagesAsync() {

            List<ChatMessage> messages = new List<ChatMessage>();

            // call the remote method passing the request object
            using (var stream = client.GetMessages(new GetMessagesRequest())) {
                // read the returned stream, one by one
                while (await stream.ResponseStream.MoveNext()) {
                    // convert protobuffer model to local data model
                    var current = stream.ResponseStream.Current;
                    messages.Add(new ChatMessage(current.Text, current.From, current.Date));
                }
            }
            return messages;
        }

        public void Refresh() {
            UpdateLocalMessages(null);
        }

        public void SendMessage(string sender, string message) {
            // invoke the method as if it was local, passing a contract defined message as parameter
            client.SendMessage(new SendMessageRequest() { From = sender, Text = message });
        }

        public void Shutdown() {
            timer.Change(-1, -1); // stop polling timer
            channel.ShutdownAsync().Wait(); // shutdown communication channel
        }
    }
}
