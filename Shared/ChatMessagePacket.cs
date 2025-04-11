using PacketTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public class ChatMessagePacket : BasePacket
{
    public required string Message { get; set; }
    public string? Sender { get; set; }
}
