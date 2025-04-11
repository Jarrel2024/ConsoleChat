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
            var aes = options.UseAESCrypto();
            aes.GenerateKeysFromPassword("123456", [1,2,3,4]);
            options.MapPackets();
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
        });
    }
}