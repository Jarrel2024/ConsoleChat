using PacketTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public class RequestChatMessageLengthPacket : BasePacket
{
    public int Length { get; set; } = 0;
}
