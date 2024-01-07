namespace Friday.ERP.Client.Data.DataTransferObjects;

public record NotificationCreateDto(
    string Heading,
    string Body
);

public record NotificationViewDto(
    Guid Guid,
    string Heading,
    string Body,
    bool HaveRead,
    DateTime SentAt
);

public record SettingViewDto(
    string? ShopImage,
    string ShopName,
    string? ShopDescription,
    string? AddressOne,
    string? AddressTwo,
    string? PhoneOne,
    string? PhoneTwo,
    string? PhoneThree,
    string? PhoneFour,
    int DefaultProfitPercent,
    int DefaultProfitPercentForWholeSale,
    int MinimumStockMargin,
    bool? SuggestSalePrice);

public record SettingUpdateDto(
    int? DefaultProfitPercent,
    int? DefaultProfitPercentForWholeSale,
    int? MinimumStockMargin,
    bool? SuggestSalePrice
);