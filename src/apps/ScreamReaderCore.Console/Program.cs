// See https://aka.ms/new-console-template for more information

using NAudio.CoreAudioApi;
using NAudio.Wave;
using ScreamReaderCore.Lib;
using ScreamReaderCore.Networking;

Console.WriteLine("Hello, World!");

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
    cts.Cancel();
    Console.WriteLine("Exiting...");
};



// new UdpWaveStreamPlayer().Start(cts.Token);
// using var provider = new NetworkProvider();
// using var network = provider.Open(SocketType.Udp, 4010).Value;
//
// var thread = new Thread(ReceiveAMessage);
// thread.Start();
//
// await Task.Delay(TimeSpan.FromSeconds(5));
//
// cts.Cancel();
//
// await Task.Delay(TimeSpan.FromSeconds(1));
//
// Console.WriteLine("Exiting");

// for (var i = 0; i < 10; i++)
// {
//     using var network = new NetworkProvider(4010);
//     var tasks = new[] { network.ReceiveAsync(cts.Token), network.ReceiveAsync(cts.Token) };
//     await Task.WhenAll(tasks);
//     
//     var buffer = tasks[0].Result;
//     
//     if(buffer.Length != 1157)
//     {
//         Console.WriteLine($"Received packet of length {buffer.Length}");
//     }
// }

var deviceEnumerator = new MMDeviceEnumerator();
var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

foreach (var device in devices)
{
    if (device.FriendlyName == "Headphones (WH-1000XM6)")
    {
        var properties = device.Properties;
        for (var i = 0; i < properties.Count; i++)
        {
            var key = properties.Get(i);
            var value = properties.GetValue(i);
            Console.WriteLine($"{key.propertyId}: {value.Value.ToString()}");
        }
    }
    
    Console.WriteLine($"Found device: {device.FriendlyName}, ID: {device.ID}");
}

Console.WriteLine("Playing audio...");

// await Task.Delay(-1, cts.Token);
//
// void ReceiveLoop()
// {
//     var token = cts.Token;
//     while (token.IsCancellationRequested == false)
//     {
//         var receiveTask = network.ReceiveAsync(token);
//         receiveTask.Wait(token);
//         var buffer = receiveTask.Result;
//         if (buffer.Value.Length != 1157)
//         {
//             Console.WriteLine($"Received packet of length {buffer.Length}");
//         }
//     }
// }

// void ReceiveAMessage()
// {
//     
//     var buffer = network.ReceiveAsync(cts.Token).GetAwaiter().GetResult();
//
//     if (buffer.IsSuccess)
//     {
//         Console.WriteLine("Received packet of length " + buffer.Value.Length);
//     }
//     else
//     {
//         Console.WriteLine("Did not receive packet" + buffer.Error);
//     }
// }