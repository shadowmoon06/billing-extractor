using BillingExtractor.Business.Interfaces;
using BillingExtractor.Business.Models;
using BillingExtractor.Business.Services;
using BillingExtractor.Data.Entities;
using BillingExtractor.Data.Repositories.SqlRepositories.Interfaces;
using FluentAssertions;
using Moq;

namespace BillingExtractor.Business.Tests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IInvoiceCacheService> _cacheServiceMock;
    private readonly InvoiceService _sut;

    public InvoiceServiceTests()
    {
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _cacheServiceMock = new Mock<IInvoiceCacheService>();
        _sut = new InvoiceService(_invoiceRepositoryMock.Object, _cacheServiceMock.Object);
    }

    private static Invoice CreateTestInvoice(string invoiceNumber = "INV-001") => new()
    {
        InvoiceNumber = invoiceNumber,
        IssuedDate = new DateTime(2025, 1, 15),
        VendorName = "Test Vendor",
        TotalAmount = 1000.00m,
        CreatedAt = new DateTime(2025, 1, 15),
        Items =
        [
            new InvoiceItem
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
            new InvoiceAdjustment
            {
                Description = "Discount",
                Amount = -50.00m
            }
        ]
    };

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

    #region GetByInvoiceNumberAsync Tests

    [Fact]
    public async Task GetByInvoiceNumberAsync_WhenCacheHit_ReturnsCachedData()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        var cachedDetail = CreateTestDetailDto(invoiceNumber);
        _cacheServiceMock.Setup(x => x.GetDetailAsync(invoiceNumber))
            .ReturnsAsync(cachedDetail);

        // Act
        var result = await _sut.GetByInvoiceNumberAsync(invoiceNumber);

        // Assert
        result.Should().BeEquivalentTo(cachedDetail);
        _invoiceRepositoryMock.Verify(x => x.GetByInvoiceNumberAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetByInvoiceNumberAsync_WhenCacheMiss_FetchesFromSqlAndCaches()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        var invoice = CreateTestInvoice(invoiceNumber);

        _cacheServiceMock.Setup(x => x.GetDetailAsync(invoiceNumber))
            .ReturnsAsync((InvoiceDetailDto?)null);
        _invoiceRepositoryMock.Setup(x => x.GetByInvoiceNumberAsync(invoiceNumber))
            .ReturnsAsync(invoice);

        // Act
        var result = await _sut.GetByInvoiceNumberAsync(invoiceNumber);

        // Assert
        result.Should().NotBeNull();
        result!.InvoiceNumber.Should().Be(invoiceNumber);
        result.VendorName.Should().Be("Test Vendor");

        _cacheServiceMock.Verify(x => x.SetDetailAsync(invoiceNumber, It.IsAny<InvoiceDetailDto>()), Times.Once);
        _cacheServiceMock.Verify(x => x.SetSummaryAsync(invoiceNumber, It.IsAny<InvoiceSummaryDto>()), Times.Once);
    }

    [Fact]
    public async Task GetByInvoiceNumberAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var invoiceNumber = "NON-EXISTENT";
        _cacheServiceMock.Setup(x => x.GetDetailAsync(invoiceNumber))
            .ReturnsAsync((InvoiceDetailDto?)null);
        _invoiceRepositoryMock.Setup(x => x.GetByInvoiceNumberAsync(invoiceNumber))
            .ReturnsAsync((Invoice?)null);

        // Act
        var result = await _sut.GetByInvoiceNumberAsync(invoiceNumber);

        // Assert
        result.Should().BeNull();
        _cacheServiceMock.Verify(x => x.SetDetailAsync(It.IsAny<string>(), It.IsAny<InvoiceDetailDto>()), Times.Never);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenCacheHit_ReturnsCachedData()
    {
        // Arrange
        var cachedSummaries = new List<InvoiceSummaryDto>
        {
            CreateTestSummaryDto("INV-001"),
            CreateTestSummaryDto("INV-002")
        };
        _cacheServiceMock.Setup(x => x.GetAllSummariesAsync())
            .ReturnsAsync(cachedSummaries);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        _invoiceRepositoryMock.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WhenCacheMiss_FetchesFromSqlAndCaches()
    {
        // Arrange
        var invoices = new List<Invoice>
        {
            CreateTestInvoice("INV-001"),
            CreateTestInvoice("INV-002")
        };

        _cacheServiceMock.Setup(x => x.GetAllSummariesAsync())
            .ReturnsAsync((IEnumerable<InvoiceSummaryDto>?)null);
        _invoiceRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(invoices);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        _cacheServiceMock.Verify(x => x.SetAllSummariesAsync(It.IsAny<IEnumerable<InvoiceSummaryDto>>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoInvoices_ReturnsEmptyList()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.GetAllSummariesAsync())
            .ReturnsAsync((IEnumerable<InvoiceSummaryDto>?)null);
        _invoiceRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Invoice>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_CreatesInSqlAndCaches()
    {
        // Arrange
        var invoice = CreateTestInvoice("INV-NEW");
        _invoiceRepositoryMock.Setup(x => x.CreateAsync(invoice))
            .ReturnsAsync(invoice);

        // Act
        var result = await _sut.CreateAsync(invoice);

        // Assert
        result.Should().Be(invoice);
        _invoiceRepositoryMock.Verify(x => x.CreateAsync(invoice), Times.Once);
        _cacheServiceMock.Verify(x => x.SetDetailAsync(invoice.InvoiceNumber, It.IsAny<InvoiceDetailDto>()), Times.Once);
        _cacheServiceMock.Verify(x => x.SetSummaryAsync(invoice.InvoiceNumber, It.IsAny<InvoiceSummaryDto>()), Times.Once);
        _cacheServiceMock.Verify(x => x.InvalidateAllSummariesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidatesAllSummariesCache()
    {
        // Arrange
        var invoice = CreateTestInvoice("INV-NEW");
        _invoiceRepositoryMock.Setup(x => x.CreateAsync(invoice))
            .ReturnsAsync(invoice);

        // Act
        await _sut.CreateAsync(invoice);

        // Assert
        _cacheServiceMock.Verify(x => x.InvalidateAllSummariesAsync(), Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_RemovesFromCache()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        _invoiceRepositoryMock.Setup(x => x.DeleteAsync(invoiceNumber))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(invoiceNumber);

        // Assert
        result.Should().BeTrue();
        _cacheServiceMock.Verify(x => x.DeleteAsync(invoiceNumber), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_DoesNotRemoveFromCache()
    {
        // Arrange
        var invoiceNumber = "NON-EXISTENT";
        _invoiceRepositoryMock.Setup(x => x.DeleteAsync(invoiceNumber))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteAsync(invoiceNumber);

        // Assert
        result.Should().BeFalse();
        _cacheServiceMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Mapping Tests

    [Fact]
    public async Task GetByInvoiceNumberAsync_MapsInvoiceToDetailDtoCorrectly()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        var invoice = CreateTestInvoice(invoiceNumber);

        _cacheServiceMock.Setup(x => x.GetDetailAsync(invoiceNumber))
            .ReturnsAsync((InvoiceDetailDto?)null);
        _invoiceRepositoryMock.Setup(x => x.GetByInvoiceNumberAsync(invoiceNumber))
            .ReturnsAsync(invoice);

        // Act
        var result = await _sut.GetByInvoiceNumberAsync(invoiceNumber);

        // Assert
        result.Should().NotBeNull();
        result!.InvoiceNumber.Should().Be(invoice.InvoiceNumber);
        result.IssuedDate.Should().Be(invoice.IssuedDate);
        result.VendorName.Should().Be(invoice.VendorName);
        result.TotalAmount.Should().Be(invoice.TotalAmount);
        result.LastEdited.Should().Be(invoice.CreatedAt);
        result.Items.Should().HaveCount(1);
        result.Items[0].ItemId.Should().Be("ITEM-001");
        result.Adjustments.Should().HaveCount(1);
        result.Adjustments[0].Description.Should().Be("Discount");
    }

    [Fact]
    public async Task GetByInvoiceNumberAsync_WhenUpdatedAt_UsesUpdatedAtForLastEdited()
    {
        // Arrange
        var invoiceNumber = "INV-001";
        var invoice = CreateTestInvoice(invoiceNumber);
        invoice.UpdatedAt = new DateTime(2025, 1, 20);

        _cacheServiceMock.Setup(x => x.GetDetailAsync(invoiceNumber))
            .ReturnsAsync((InvoiceDetailDto?)null);
        _invoiceRepositoryMock.Setup(x => x.GetByInvoiceNumberAsync(invoiceNumber))
            .ReturnsAsync(invoice);

        // Act
        var result = await _sut.GetByInvoiceNumberAsync(invoiceNumber);

        // Assert
        result!.LastEdited.Should().Be(invoice.UpdatedAt.Value);
    }

    #endregion
}
