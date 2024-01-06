namespace Friday.ERP.Client.Data.DataTransferObjects;

public record MonthlySaleFigureViewDto(
    string? Month,
    int Year,
    long TotalSaleAmount,
    long TotalPurchaseAmount,
    long TotalProfitAmount
);

public record TopSellingProductsViewDto(
    Guid ProductGuid,
    string ProductName,
    string CategoryName,
    string CategoryColor,
    int TotalTimes,
    long PurchaseCount
);

public record CustomerWithDebitViewDto(
    Guid Guid,
    string CustomerName,
    long TotalCreditDebitLeft
);

public record CustomerDebitSummaryViewDto(
    long TotalDebits,
    List<CustomerWithDebitViewDto> CustomersWithDebits
);

public record InvoiceSaleForFrequentCustomerViewDto(
    Guid CustomerGuid,
    string? CustomerName,
    int PurchaseCount,
    long Total
);