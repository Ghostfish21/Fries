namespace Fries.Chat {
    public class Message {
        public readonly string senderId;
        public readonly string content;
        
        public Message(string senderId, string content) {
            this.senderId = senderId;
            this.content = content;
        }
    }
}