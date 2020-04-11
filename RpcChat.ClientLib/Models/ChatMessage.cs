using System;

namespace RpcChat.ClientLib.Models {
    public class ChatMessage {

        public ChatMessage(string text, string sender, string date) {
            Text = text;
            MessageDate = date;
            Sender = sender;
        }

        public string Text { get; private set; }

        public string MessageDate { get; private set; }

        public string Sender { get; private set; }

        public override string ToString() {
            return $"From:{Sender} at {MessageDate}:\n->{Text}\n";
        }

    }
}
