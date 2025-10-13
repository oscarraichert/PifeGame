using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PifeGame.Domain
{
    public enum MessageType
    {
        InvalidMessageType = 0,
        NewRoom = 1,
        JoinRoom = 2,
        ChatMessage = 3,
        LeaveRoom = 4,
    }
}
