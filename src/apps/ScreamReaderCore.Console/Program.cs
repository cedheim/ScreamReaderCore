// See https://aka.ms/new-console-template for more information

using System.Net;
using ScreamReaderCore.Audio;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Lib;
using ScreamReaderCore.Networking;

var network = new NetworkProvider();
using var deviceEnumerator = new AudioDeviceEnumerator();
var audio = new AudioProvider(deviceEnumerator);

var settings = new PcmReceiverSettings(4010, new IPAddress([239, 255, 77, 77]), false);
using var output = new ScreamOutput(audio);
using var receiver = new ScreamReceiver(network);
using var player = new ScreamPlayer(settings, receiver, output);

audio.OnDefaultDeviceChanged += (newDefaultDevice) =>
{
    player.Device = newDefaultDevice;
};

player.Start();

Console.WriteLine("Playing audio...");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

player.Stop();
