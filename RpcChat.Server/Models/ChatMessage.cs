using System;

namespace RpcChat.Server.Models {
    public class ChatMessage {

        public ChatMessage(string text, string sender) {
            Text = text;
            ArrivalDate = DateTime.Now;
            Sender = sender;
        }

        public string Text { get; private set; }

        public DateTime ArrivalDate { get; private set; }

        public string Sender { get; private set; }

    }
}
