using Friday.ERP.Client.Data.RequestFeatures;

namespace Friday.ERP.Client.Data.DataTransferObjects;

public record SuggestedProductPriceViewDto(
    long SalePrice,
    long SalePriceForWholeSale
);

public record ProductPriceViewDto(
    Guid Guid,
    bool? IsWholeSale,
    long SalePrice
);

public record ProductPriceCreateDto(
    bool IsWholeSale,
    long SalePrice
);

public record ProductViewDto(
    Guid Guid,
    string Code,
    string Name,
    int TotalStockLeft,
    string? Image,
    Guid CategoryGuid,
    string CategoryName,
    List<ProductPriceViewDto> Prices);

public record ProductCreateDto(
    string Code,
    Guid CategoryGuid,
    string Name,
    int TotalStockLeft,
    string? Image
);

public record ProductUpdateDto(
    Guid? CategoryGuid,
    string? Name,
    int? TotalStockLeft,
    string? Image
);

public record ProductPurchasePriceViewDto(
    Guid ProductGuid,
    long BuyPrice
);

public class ProductParameter : RequestParameters
{
    public ProductParameter()
    {
        OrderBy = "Name";
    }

    public string? SearchTerm { get; set; }

    public Guid? CategoryGuid { get; set; }
}

public class ProductPurchaseAddToCartViewDto
{
    public Guid ProductGuid { get; set; }
    public string? Image { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int Stock { get; set; }
    public long BuyPrice { get; set; }
    public int Quantity { get; set; }
    public long TotalPrice { get; set; }
}

public class ProductSaleAddToCartViewDto
{
    public Guid ProductGuid { get; set; }
    public string? Image { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int Stock { get; set; }
    public Guid SalePriceGuid { get; set; }
    public long SalePrice { get; set; }

    public bool SalePriceIsWholeSale { get; set; }
    public int Quantity { get; set; }
    public long TotalPrice { get; set; }
}