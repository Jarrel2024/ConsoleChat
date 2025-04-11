using PacketTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public class UpdateChatMessagePacket : BasePacket
{
    public required int From { get; set; }
    public required int To { get; set; }
    public List<ChatMessagePacket> Messages { get; set; } = null!;
}
