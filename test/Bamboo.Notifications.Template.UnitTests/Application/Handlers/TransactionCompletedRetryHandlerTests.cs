using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using Bamboo.Notifications.Template.Application.Configurations;
using Bamboo.Notifications.Template.Application.Contracts;
using Bamboo.Notifications.Template.Application.Handlers;
using Bamboo.SQSForRetries.Models;
using Bamboo.Notifications.Template.Domain.Entities.Events;
using Xunit;
using Bamboo.Notifications.Template.UnitTests.Application.Handlers.Base;

namespace Bamboo.Notifications.Template.UnitTests.Application.Handlers;

public class TransactionCompletedRetryHandlerTests : HandlerTestsBase
{
    private readonly TransactionCompletedRetryHandler _sut;
    private readonly Mock<IAntifraudService> _AntifraudServiceMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOptions<BrokerConfig>> _brokerConfigOptionsMock;

    public TransactionCompletedRetryHandlerTests()
    {
        _AntifraudServiceMock = new Mock<IAntifraudService>();
        _loggerMock = new Mock<ILogger>();
        _brokerConfigOptionsMock = new Mock<IOptions<BrokerConfig>>();
        _brokerConfigOptionsMock.Setup(x => x.Value).Returns(GetBrokerConfig());
        _sut = new TransactionCompletedRetryHandler(
            _brokerConfigOptionsMock.Object,
            _AntifraudServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_HappyPath_ReturnsSuccess()
    {
        // Arrange
        _AntifraudServiceMock.Setup(x => x.SendPurchaseAsync(It.IsAny<CompleteEventWebModelRequest>()))
                               .ReturnsAsync(GetAntifraudOkResponse());

        var message = new RetryEvent { PurchaseId = 123, ReceiveCount = 1 };

        // Act
        var result = await _sut.Handle(message, CancellationToken.None);

        // Assert
        Assert.Equal(ResultAction.Success, result.ResultAction);
    }

    [Fact]
    public async Task Handle_AntifraudFails_ReturnsReprocess()
    {
        // Arrange
        _AntifraudServiceMock.Setup(x => x.SendPurchaseAsync(It.IsAny<CompleteEventWebModelRequest>()))
                               .ReturnsAsync(GetAntifraudErrorResponse());

        var message = new RetryEvent { PurchaseId = 123, ReceiveCount = 1 };

        // Act
        var result = await _sut.Handle(message, CancellationToken.None);

        // Assert
        Assert.Equal(ResultAction.Reprocess, result.ResultAction);
    }

    [Fact]
    public async Task Handle_WithExceededMaxRetries_ReturnsDismiss()
    {
        // Arrange
        var message = new RetryEvent { PurchaseId = 123, ReceiveCount = 3 };

        // Act
        var result = await _sut.Handle(message, CancellationToken.None);

        // Assert
        Assert.Equal(ResultAction.Dismiss, result.ResultAction);
    }

    private BrokerConfig GetBrokerConfig()
    {
        return new BrokerConfig
        {
            BootstrapServers = "example.kafka.com",
            UserName = "username",
            Password = "password",
            ConsumerConfig = new BrokerConsumerConfig
            {
                GroupId = "consumer-group",
                Topics = new[] { "topic1", "topic2" },
                Retries = new short[] { 1, 2 },
                MaxPollIntervalMs = 5000,
                EnableAutoCommit = true,
                EnableAutoOffsetStore = true
            }
        };
    }
}
