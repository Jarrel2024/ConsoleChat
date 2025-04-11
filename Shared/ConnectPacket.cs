using PacketTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public class ConnectPacket : BasePacket
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
