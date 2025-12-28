using ScreamReaderCore.Contract.Models;
using Shouldly;

namespace ScreamReaderCore.Contract.Tests.Models;

[TestFixture]
public class PcmMessageTests
{
    [Test]
    public void Should_be_able_to_create_scream_message_from_valid_data()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        // Act
        var message = new PcmMessage(data);
        // Assert
        message.RawData.ShouldBeEquivalentTo(data);
        message.Header.CurrentRate.ShouldBe(1); 
        message.Header.CurrentWidth.ShouldBe(2);
        message.Header.CurrentChannels.ShouldBe(3);
        message.Header.CurrentChannelsMapLsb.ShouldBe(4);
        message.Header.CurrentChannelsMapMsb.ShouldBe(5);
        message.Data[0].ShouldBe(data[5]);
    }

    [Test]
    public void Should_throw_exception_when_creating_scream_message_with_null_data()
    {
        byte[]? data = null!;

        Should.Throw<ArgumentNullException>(() => new PcmMessage(data));
    }

    [Test]
    public void Should_throw_exception_when_creating_scream_header_with_insufficient_data()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4 }; // Less than 5 bytes
        // Act & Assert
        Should.Throw<ArgumentException>(() => new PcmHeader(data));
    }

    [Test]
    public void Should_be_able_to_compare_scream_headers()
    {
        // Arrange
        var data1 = new byte[] { 1, 2, 3, 4, 5 };
        var data2 = new byte[] { 1, 2, 3, 4, 5 };
        var data3 = new byte[] { 6, 7, 8, 9, 10 };
        var header1 = new PcmHeader(data1);
        var header2 = new PcmHeader(data2);
        var header3 = new PcmHeader(data3);
        // Act & Assert
        header1.Equals(header2).ShouldBeTrue();
        header1.Equals(header3).ShouldBeFalse();

        (header1 == header2).ShouldBeTrue();
    }
}
