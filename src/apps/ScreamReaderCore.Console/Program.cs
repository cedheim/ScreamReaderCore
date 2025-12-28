// See https://aka.ms/new-console-template for more information
using ScreamReaderCore.Lib;

Console.WriteLine("Hello, World!");

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
    cts.Cancel();
    Console.WriteLine("Exiting...");
};

new UdpWaveStreamPlayer().Start(cts.Token);

Console.WriteLine("Playing audio...");

await Task.Delay(-1, cts.Token);
