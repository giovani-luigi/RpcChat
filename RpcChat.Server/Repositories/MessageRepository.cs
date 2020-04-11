using RpcChat.Server.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RpcChat.Server.Repositories {
    public class MessageRepository {

        private static List<ChatMessage> messages = new List<ChatMessage>();

        public MessageRepository() {
        }

        public void PostMessage(ChatMessage msg) {
            messages.Add(msg);
        }

        public IEnumerable<ChatMessage> GetAllMessages() {
            return messages;
        }

    }
}
