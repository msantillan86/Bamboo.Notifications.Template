using Moq;
using Xunit;
using Serilog;
using Bamboo.SQSForRetries.Contracts;
using Bamboo.Notifications.Template.Application.Handlers;
using Bamboo.Notifications.Template.Application.Contracts;
using Bamboo.Notifications.Template.Domain.Entities.Events;
using Bamboo.Notifications.Template.UnitTests.Application.Handlers.Base;

namespace Bamboo.Notifications.Template.UnitTests.Application.Handlers;

public class TransactionCompletedHandlerTests : HandlerTestsBase
{
    private readonly TransactionCompletedHandler _sut;
    private readonly Mock<ITemplateService> _templateServiceMock;
    private readonly Mock<ISqsPublisherService> _publisherServiceMock;
    private readonly Mock<ILogger> _loggerMock;

    public TransactionCompletedHandlerTests()
    {
        _templateServiceMock = new Mock<ITemplateService>();
        _publisherServiceMock = new Mock<ISqsPublisherService>();
        _loggerMock = new Mock<ILogger>();

        _sut = new TransactionCompletedHandler(
            _templateServiceMock.Object,
            _publisherServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidMessage_ReturnsTrue()
    {
        // Arrange
        var validRawMessage = "{\"CommerceAccountId\":1,\"PurchaseId\":1}";

        _templateServiceMock.Setup(x => x.SendPurchaseAsync(It.IsAny<CompleteEventWebModelRequest>()))
            .ReturnsAsync(GetTemplateOkResponse());

        // Act
        var result = await _sut.Handle(validRawMessage);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Handle_WithValidMessage_NeverCallsPublishAsync()
    {
        // Arrange
        var validRawMessage = "{\"CommerceAccountId\":1,\"PurchaseId\":1}";

        _templateServiceMock.Setup(x => x.SendPurchaseAsync(It.IsAny<CompleteEventWebModelRequest>()))
            .ReturnsAsync(GetTemplateOkResponse());

        // Act
        await _sut.Handle(validRawMessage);

        // Assert
        _publisherServiceMock.Verify(x => x.PublishAsync(It.IsAny<RetryEvent>()), Times.Never);
    }

    [Theory]
    [InlineData("{\"CommerceAccountId\":0,\"PurchaseId\":0}")]
    [InlineData("{\"AnotherJson\":333}")]
    [InlineData("AnotherFormat")]
    [InlineData(null)]
    public async Task Handle_WithInvalidMessage_ReturnsFalse(string rawMessage)
    {
        // Arrange
        var invalidRawMessage = rawMessage;

        _templateServiceMock.Setup(x => x.SendPurchaseAsync(It.IsAny<CompleteEventWebModelRequest>()))
            .ReturnsAsync(GetTemplateOkResponse());

        // Act
        var result = await _sut.Handle(invalidRawMessage);

        // Assert
        Assert.False(result);
    }
}