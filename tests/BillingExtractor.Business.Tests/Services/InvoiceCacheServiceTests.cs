using System.Text;
using System.Text.Json;
using BillingExtractor.Business.Models;
using BillingExtractor.Business.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace BillingExtractor.Business.Tests.Services;

public class InvoiceCacheServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly InvoiceCacheService _sut;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public InvoiceCacheServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _sut = new InvoiceCacheService(_cacheMock.Object);
    }

    private static InvoiceDetailDto CreateTestDetailDto(string invoiceNumber = "INV-001") => new()
    {
        InvoiceNumber = invoiceNumber,
        IssuedDate = new DateTime(2025, 1, 15),
        VendorName = "Test Vendor",
        TotalAmount = 1000.00m,
        LastEdited = new DateTime(2025, 1, 15),
        Items =
        [
            new InvoiceItemDto
            {
                ItemId = "ITEM-001",
                Description = "Test Item",
                Quantity = 10,
                UnitPrice = 100.00m,
                Unit = "pcs",
                Amount = 1000.00m
            }
        ],
        Adjustments =
        [
            new InvoiceAdjustmentDto
            {
                Description = "Discount",
                Amount = -50.00m
            }
        ]
    };

    private static InvoiceSummaryDto CreateTestSummaryDto(string invoiceNumber = "INV-001") => new()
    {
        InvoiceNumber = invoiceNumber,
        IssuedDate = new DateTime(2025, 1, 15),
        VendorName = "Test Vendor",
        TotalAmount = 1000.00m,
        LastEdited = new DateTime(2025, 1, 15)
    };

    private static byte[] SerializeToBytes<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        return Encoding.UTF8.GetBytes(json);
    }

    #region GetDetailAsync Tests

    [Fact]
    public async Task GetDetailAsync_WhenCacheHit_ReturnsDeserializedData()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        var detail = CreateTestDetailDto(invoiceNumber);
        var cachedBytes = SerializeToBytes(detail);

        _cacheMock.Setup(x => x.GetAsync($"invoice:detail:{invoiceNumber}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var result = await _sut.GetDetailAsync(invoiceNumber);

        // Assert
        result.Should().NotBeNull();
        result!.InvoiceNumber.Should().Be(invoiceNumber);
        result.VendorName.Should().Be("Test Vendor");
    }

    [Fact]
    public async Task GetDetailAsync_WhenCacheMiss_ReturnsNull()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        _cacheMock.Setup(x => x.GetAsync($"invoice:detail:{invoiceNumber}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetDetailAsync(invoiceNumber);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetSummaryAsync Tests

    [Fact]
    public async Task GetSummaryAsync_WhenCacheHit_ReturnsDeserializedData()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        var summary = CreateTestSummaryDto(invoiceNumber);
        var cachedBytes = SerializeToBytes(summary);

        _cacheMock.Setup(x => x.GetAsync($"invoice:summary:{invoiceNumber}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var result = await _sut.GetSummaryAsync(invoiceNumber);

        // Assert
        result.Should().NotBeNull();
        result!.InvoiceNumber.Should().Be(invoiceNumber);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenCacheMiss_ReturnsNull()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        _cacheMock.Setup(x => x.GetAsync($"invoice:summary:{invoiceNumber}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetSummaryAsync(invoiceNumber);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllSummariesAsync Tests

    [Fact]
    public async Task GetAllSummariesAsync_WhenCacheHit_ReturnsDeserializedList()
    {
        // Arrange
        var summaries = new List<InvoiceSummaryDto>
        {
            CreateTestSummaryDto("INV-001"),
            CreateTestSummaryDto("INV-002")
        };
        var cachedBytes = SerializeToBytes(summaries);

        _cacheMock.Setup(x => x.GetAsync("invoice:all_summaries", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var result = await _sut.GetAllSummariesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllSummariesAsync_WhenCacheMiss_ReturnsNull()
    {
        // Arrange
        _cacheMock.Setup(x => x.GetAsync("invoice:all_summaries", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetAllSummariesAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SetDetailAsync Tests

    [Fact]
    public async Task SetDetailAsync_SerializesAndStoresInCache()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        var detail = CreateTestDetailDto(invoiceNumber);

        // Act
        await _sut.SetDetailAsync(invoiceNumber, detail);

        // Assert
        _cacheMock.Verify(x => x.SetAsync(
            $"invoice:detail:{invoiceNumber}",
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromDays(1)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region SetSummaryAsync Tests

    [Fact]
    public async Task SetSummaryAsync_SerializesAndStoresInCache()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        var summary = CreateTestSummaryDto(invoiceNumber);

        // Act
        await _sut.SetSummaryAsync(invoiceNumber, summary);

        // Assert
        _cacheMock.Verify(x => x.SetAsync(
            $"invoice:summary:{invoiceNumber}",
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromDays(1)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region SetAllSummariesAsync Tests

    [Fact]
    public async Task SetAllSummariesAsync_SerializesAndStoresInCache()
    {
        // Arrange
        var summaries = new List<InvoiceSummaryDto>
        {
            CreateTestSummaryDto("INV-001"),
            CreateTestSummaryDto("INV-002")
        };

        // Act
        await _sut.SetAllSummariesAsync(summaries);

        // Assert
        _cacheMock.Verify(x => x.SetAsync(
            "invoice:all_summaries",
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromDays(1)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_RemovesAllRelatedKeysFromCache()
    {
        // Arrange
        var invoiceNumber = "INV-001";

        // Act
        await _sut.DeleteAsync(invoiceNumber);

        // Assert
        _cacheMock.Verify(x => x.RemoveAsync($"invoice:summary:{invoiceNumber}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync($"invoice:detail:{invoiceNumber}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync("invoice:all_summaries", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region InvalidateAllSummariesAsync Tests

    [Fact]
    public async Task InvalidateAllSummariesAsync_RemovesAllSummariesKey()
    {
        // Act
        await _sut.InvalidateAllSummariesAsync();

        // Assert
        _cacheMock.Verify(x => x.RemoveAsync("invoice:all_summaries", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region TTL Tests

    [Fact]
    public async Task SetDetailAsync_UseOneDayTtl()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        var detail = CreateTestDetailDto(invoiceNumber);

        // Act
        await _sut.SetDetailAsync(invoiceNumber, detail);

        // Assert
        _cacheMock.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromDays(1)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
