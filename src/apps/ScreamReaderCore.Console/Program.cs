// See https://aka.ms/new-console-template for more information
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
using var provider = new NetworkProvider();
using var network = provider.Open(SocketType.Udp, 4010).Value;

var thread = new Thread(ReceiveAMessage);
thread.Start();

await Task.Delay(TimeSpan.FromSeconds(5));

cts.Cancel();

await Task.Delay(TimeSpan.FromSeconds(1));

Console.WriteLine("Exiting");

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

void ReceiveAMessage()
{
    
    var buffer = network.ReceiveAsync(cts.Token).GetAwaiter().GetResult();

    if (buffer.IsSuccess)
    {
        Console.WriteLine("Received packet of length " + buffer.Value.Length);
    }
    else
    {
        Console.WriteLine("Did not receive packet" + buffer.Error);
    }
}