using FakeItEasy;
using ScreamReaderCore.Networking;

namespace ScreamReaderCore.Lib.Tests;

public class ScreamReceiverTests
{
    private INetworkProvider _provider = null!;
    private ScreamReceiver _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _provider = A.Fake<INetworkProvider>();
        _sut = new ScreamReceiver(_provider);
    }

    [TearDown]
    public void TearDown()
    {
        _provider.Dispose();
        _sut.Dispose();
    }
}