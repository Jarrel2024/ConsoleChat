using PacketTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public class UpdateChatMessagePacket : BasePacket
{
    public int From { get; set; }
    public int To { get; set; }
    public List<ChatMessagePacket> Messages { get; set; } = null!;
}
