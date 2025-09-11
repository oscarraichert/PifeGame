namespace PifeGame.Domain
{
    public class SocketMessage
    {
        public MessageType MessageType { get; set; }
        public string? Payload { get; set; }
    }
}
