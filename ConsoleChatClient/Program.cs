using ConsoleChatClient;
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
ClientRenderer renderer = new ClientRenderer();

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
    // 将光标移动到输入行
    Console.SetCursorPosition(0, Console.WindowHeight - 1);
    Console.Write("输入: ");
    string? input = Console.ReadLine()?.Trim();
    
    if (string.IsNullOrEmpty(input))
    {
        renderer.Render();
        continue; // 忽略空输入
    }
    
    if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
    {
        break; // 输入 /exit 退出程序
    }

    // 添加用户输入的消息
    renderer.AddMessage(new Message(input,config.Username));
    client.Send(new ChatMessagePacket { Message = input,Sender=config.Username});
}

void AddToChat(ChatMessagePacket packet)
{
    renderer.AddMessage(new Message(packet.Message,packet.Sender??""));
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