using PifeGame.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PifeGame.Application
{
    public class WebSocketUtils
    {
        public static async Task Send(SocketMessage message, WebSocket socket)
        {
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task Broadcast(SocketMessage message, ConcurrentDictionary<WebSocket, string> clients)
        {
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var tasks = new List<Task>();

            foreach (var socket in clients.Select(x => x.Key))
            {
                if (socket.State == WebSocketState.Open)
                {
                    tasks.Add(socket.SendAsync(
                        new ArraySegment<byte>(responseBytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
