using Grpc.Core;
using RpcChat.Server.Models;
using System.Threading.Tasks;

namespace RpcChat.Server.Rpc {

    /// <summary>
    /// This class implements (overrides) base class stub generated 
    /// from the 'proto' files where the RPC contract is defined
    /// </summary>
    class ChatService : ChatContract.ChatContractBase {

        /// <summary>
        /// Returns a stream of messages from the sever
        /// </summary>
        public override async Task GetMessages(
            GetMessagesRequest request,
            IServerStreamWriter<GetMessagesResponse> responseStream,
            ServerCallContext context) 
        {
            System.Console.WriteLine(
                $"Call to {nameof(GetMessages)}() from {context.Host}");

            // retrieve all messages from server
            var repository = new Repositories.MessageRepository().GetAllMessages();
            
            // send each message through the output stream
            foreach (var msg in repository) {
                await responseStream.WriteAsync(
                    // here we are mapping our local data 'model' to the protobuffer contract 'model'
                    new GetMessagesResponse() {
                        From = msg.Sender,
                        Text = msg.Text,
                        Date = msg.ArrivalDate.ToString("dd/MM/yyyy HH:mm:ss")
                    });
            }
        }

        /// <summary>
        /// Post a message to the server
        /// </summary>
        public override Task<SendMessageResponse> SendMessage(
            SendMessageRequest request, 
            ServerCallContext context) 
        {

            System.Console.WriteLine(
                $"{context.Host} sent message Name={request.From}; Text={request.Text}");

            // first we map the received protobuffer 'model' to the local data 'model'
            var msg = new ChatMessage(request.Text, request.From);

            // now push the new message into the repository
            var repo = new Repositories.MessageRepository();
            repo.PostMessage(msg);

            // finally we return the expected response to the client
            return Task.FromResult(new SendMessageResponse());
        }

    }
}
