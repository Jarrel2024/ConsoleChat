using PacketTcp;
using Shared;
using System.Text.Json;
using Tomlyn;

int top = 0;

Config config = new Config();

if (File.Exists("config.toml"))
{
    config = Toml.ToModel<Config>(Toml.Parse(File.ReadAllText("config.toml")));
}
else
{
    Console.Write("Remote address:");
    string? remoteAddress = Console.ReadLine();
    if (string.IsNullOrEmpty(remoteAddress))
    {
        Console.WriteLine("Invalid address");
        return;
    }
    string[] strings = remoteAddress.Split(':');
    if (strings.Length > 2)
    {
        Console.WriteLine("Invalid address");
        return;
    }
    int port = strings.Length == 2 ? int.Parse(strings[1]) : 25565;
    string address = strings[0];

    Console.Write("Username:");
    string? username = Console.ReadLine();
    if (string.IsNullOrEmpty(username))
    {
        Console.WriteLine("Invalid username");
        return;
    }
    Console.Write("Password:");
    string? password = Console.ReadLine();
    if (string.IsNullOrEmpty(password))
    {
        Console.WriteLine("Invalid password");
        return;
    }

    config.AutoLogin = true;
    config.Username = username;
    config.Password = password;
    config.Remote = address;
    config.Port = port;

    using var fs = File.Open("config.toml",FileMode.OpenOrCreate);
    fs.Seek(0, SeekOrigin.Begin);
    fs.SetLength(0);
    using var sw = new StreamWriter(fs);
    sw.Write(Toml.FromModel(config));
}

string path = $"{config.Remote}-chat.db";

Console.Clear();

SharedData data = new SharedData();

PacketClient client = new PacketClient(data.Manager);

client.PacketReceived += e =>
{
    if (e.Type != typeof(ChatMessagePacket)) return;
    var packet = e.Packet as ChatMessagePacket;
    if (packet == null) return;
    AddToChat(packet);
    AddToHistory(packet);
};
client.Connect(config.Remote, config.Port);

var res = await client.SendAsync<LoginResultPacket>(new ConnectPacket
{
    Username = config.Username,
    Password = config.Password,
});

if (!res.Success)
{
    Console.WriteLine("Invalid username or password");
    client.Stop();
    return;
}

int history = 0;
if (!File.Exists(path))
{
    File.Create(path).Close();
}
else
{
    string[] lines = File.ReadAllLines(path);
    foreach (string line in lines)
    {
        AddToChat(JsonSerializer.Deserialize<ChatMessagePacket>(line)!);
    }
    history = lines.Length;
}

RequestChatMessageLengthPacket response = await client.SendAsync<RequestChatMessageLengthPacket>(new RequestChatMessageLengthPacket());
UpdateChatMessagePacket response1 = await client.SendAsync<UpdateChatMessagePacket>(new UpdateChatMessagePacket() { From = history, To = response.Length });
foreach (var message in response1.Messages)
{
    AddToChat(message);
    AddToHistory(message);
}

while (client.IsConnected)
{
    Console.CursorLeft = 0;
    Console.CursorTop = Console.WindowTop;
    string? message = Console.ReadLine();
    Console.CursorLeft = 0;
    Console.CursorTop = Console.WindowTop;
    Console.Write(new string(' ', Console.BufferWidth));
    if (string.IsNullOrEmpty(message)) continue;
    var packet = new ChatMessagePacket { Message = message, Sender = config.Username };
    client.Send(packet);
    AddToChat(packet);
    AddToHistory(packet);
}

void AddToChat(ChatMessagePacket packet)
{
    Console.CursorTop = ++top;
    Console.CursorLeft = 0;
    Console.WriteLine($"<{packet.Sender}>: {packet.Message}");
    Console.CursorLeft = 0;
    Console.CursorTop = Console.WindowTop;
}

void AddToHistory(ChatMessagePacket packet)
{
    using var sw = new StreamWriter(path, true);
    sw.WriteLineAsync(JsonSerializer.Serialize<ChatMessagePacket>(packet, new JsonSerializerOptions { WriteIndented = false }));
}

class Config
{
    public bool AutoLogin { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Remote { get; set; } = string.Empty;
    public int Port { get; set; } = 25565;
}