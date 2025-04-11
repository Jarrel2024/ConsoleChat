using ConsoleChatServer;
using PacketTcp;
using PacketTcp.Events;
using Shared;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

SharedData data = new SharedData();
ChatDbContext db = new ChatDbContext();

Dictionary<Guid, User> Users = [];

PacketServer server = new PacketServer(25565,data.Manager);
server.ClientConnected += c =>
{
    Console.WriteLine($"Client {c.Id} connected");
};
server.ClientDisconnected += c =>
{
    Console.WriteLine($"Client {c.Id} disconnected");
};

server.PacketReceived += e =>
{
    if (e.Packet is not ConnectPacket packet) return;
    User? user = db.Users.FirstOrDefault(x => x.Username == packet.Username && x.PasswordHash.SequenceEqual(SHA256.HashData(Encoding.UTF8.GetBytes(packet.Password))));
    e.SendCallback(new LoginResultPacket { Success = user != null});
    if (user == null) return;
    lock (Users)
    {
        Users[e.Client.Id] = user;
    }
};

server.PacketReceived += e =>
{
    if (!isAuthed(e)) return;
    if (e.Packet is not UpdateChatMessagePacket packet) return;
    IQueryable<ChatMessagePacket> chatMessages = db.ChatMessages.Skip(packet.From).Take(packet.To-packet.From).Select(x=>x.ToPacket());
    e.SendCallback(new UpdateChatMessagePacket
    {
        From = packet.From,
        To = packet.To,
        Messages = chatMessages.ToList()
    });
};
server.PacketReceived += e =>
{
    if (!isAuthed(e)) return;
    if (e.Type != typeof(ChatMessagePacket)) return;
    db.ChatMessages.Add(new ChatMessage
    {
        Message = ((ChatMessagePacket)e.Packet).Message,
        Sender = ((ChatMessagePacket)e.Packet).Sender ?? "",
    });
    db.SaveChangesAsync();
    foreach (var client in server.Clients)
    {
        if (client.Socket == e.Client.Socket) continue;
        var packet = e.Packet as ChatMessagePacket;
        if (packet == null) return;
        server.Send(client.Id, packet);
    }
    Console.WriteLine($"Client {e.Client.Id} sent: {((ChatMessagePacket)e.Packet).Message}");
};

server.PacketReceived += e =>
{
    if (!isAuthed(e)) return;
    if (e.Packet is not RequestChatMessageLengthPacket packet) return;
    e.SendCallback(new RequestChatMessageLengthPacket { Length = db.ChatMessages.Count()});
};

server.ClientDisconnected += c =>
{
    lock (Users)
    {
        Users.Remove(c.Id);
    }
};

server.Start();

while (server.IsListening) 
{ 
    string? str = Console.ReadLine();
    if (str == null) continue;
    if (str == "exit") break;
    string[] strings = str.Split(' ');
    if (strings.Length == 2)
    {
        db.Users.Add(new User
        {
            Username = strings[0],
            PasswordHash = SHA256.HashData(Encoding.UTF8.GetBytes(strings[1])),
        });
        db.SaveChanges();
    }
};

bool isAuthed(PacketEvent packet)
{
    lock (Users)
    {
        return Users.ContainsKey(packet.Client.Id);
    }
}