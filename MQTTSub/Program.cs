using System.Security.Authentication;
using System.Text;
using System.Web;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.WebSocket4Net;
using MQTTnet.Formatter;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

//Broker IPs here
string[] brokerIPs = { "125.234.135.55", "125.234.135.55", "125.234.135.55" };
string sysCtrl = "";
string eventCnt = "";
string eventComSewCnt = "";
string eventMchntime = "";
string eventTeaching = "";
string eventNfcUid = "";
string eventHandling = "";
string temp = "";
bool isDisconnected = false;

var mqttFactory = new MqttFactory();
var mqttClient = mqttFactory.CreateMqttClient();

async Task Handle_Received_Application_Message(string broker)
{
    if (isDisconnected)
    {
        isDisconnected = false;
        mqttClient = mqttFactory.CreateMqttClient();
    }

    var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(broker).Build();

    mqttClient.ApplicationMessageReceivedAsync += e =>
    {
        Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
        switch (e.ApplicationMessage.Topic)
        {
            case "/SysCtrl":
                sysCtrl = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                break;
            case "/Event/Cnt":
                eventCnt = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                break;
            case "/Event/ComSew/Cnt":
                eventComSewCnt = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                break;
            case "/Event/Mchn_time":
                eventMchntime = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                break;
            case "/Event/Teaching":
                eventTeaching = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                break;
            case "/Event/Handling":
                eventHandling = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                break;
        }
        Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

        return Task.CompletedTask;
    };

    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

    var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
        .WithTopicFilter(
        f =>
        {
            f.WithTopic("/SysCtrl");
        })
    .WithTopicFilter(
        f =>
        {
            f.WithTopic("/Event/Cnt");
        })
    .WithTopicFilter(
        f =>
        {
            f.WithTopic("/Event/ComSew/Cnt");
        })
    .WithTopicFilter(
        f =>
        {
            f.WithTopic("/Event/Mchn_time");
        })
    .WithTopicFilter(
        f =>
        {
            f.WithTopic("/Event/Teaching");
        })
    .WithTopicFilter(
        f =>
        {
            f.WithTopic("/Event/Nfc/Uid");
        })
    .WithTopicFilter(
        f =>
        {
            f.WithTopic("/Event/Handling");
        })
    .Build();

    await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

    Console.WriteLine("MQTT client subscribed to topic.");

    Console.WriteLine("Press enter to exit.");
    Console.ReadLine();
    //}
}
//Handle_Received_Application_Message("125.234.135.55");
async Task Disconnect_Clean(string broker)
{
    isDisconnected = true;
    mqttClient.Dispose();
    Console.WriteLine("Disconnected to broker " + broker);
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/subscribe/{brokerIP}/{topic}", (string brokerIP, string topic) =>
{
    topic = HttpUtility.UrlDecode(topic);

    Console.WriteLine("Request for subscribing broker = " + brokerIP + " & topic = " + topic);

    Handle_Received_Application_Message(brokerIP);

    return Results.Ok();

});

app.MapGet("/disconnect/{brokerIP}", (string brokerIP) =>
{
    Disconnect_Clean(brokerIP);

    Console.WriteLine("Request for disconnecting broker = " + brokerIP);

    return Results.Ok();
});

app.MapGet("/{brokerIP}/{topic}/messages", (string brokerIP, string topic) =>
{
    topic = HttpUtility.UrlDecode(topic);

    Console.WriteLine("Request for get messages from broker = " + brokerIP + " & topic = " + topic);

    switch (topic)
    {
        case "/SysCtrl":
            temp = sysCtrl;
            break;
        case "/Event/Cnt":
            temp = eventCnt;
            break;
        case "/Event/ComSew/Cnt":
            temp = eventComSewCnt;
            break;
        case "/Event/Teaching":
            temp = eventTeaching;
            break;
        case "/Event/Mchn_time":
            temp = eventMchntime;
            break;
        case "/Event/Handling":
            temp = eventHandling;
            break;
    }
    return Results.Text(temp);
});

app.MapGet("/test", () => "Done");

app.Run();



