using PacketTcp.Managers;

namespace Shared;

public class SharedData
{
    public PacketManager Manager { get; private set; }
    public SharedData()
    {
        Manager = new PacketManager(options =>
        {
            options.SyncClientId = true;
        });
        Manager.MapPackets();
    }
}