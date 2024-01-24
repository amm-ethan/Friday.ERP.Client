using Friday.ERP.Shared.DataTransferObjects;
using Friday.ERP.Shared.RequestFeatures;

namespace Friday.ERP.Client.Data;

public interface IHttpRepository
{
    # region Auth API

    public Task<(bool isSuccess, LoginResultDto? loginResultDto)> Login(LoginDto loginDto);

    public Task<string?> GetRefreshToken();

    Task<bool> Logout();

    #endregion

    #region System API

    public Task<(bool isSuccess, List<NotificationViewDto>? notificationViewDtos)> GetAllNotifications();

    public Task<bool> ReadNotificationByGuid(Guid guid);

    public Task<(bool isSuccess, SettingViewDto? settingViewDto)> GetSettings();

    public Task<(bool isSuccess, SettingViewDto? settingViewDto)> UpdateSettings(SettingUpdateDto settingUpdateDto);

    #endregion

    #region User API

    public Task<(bool isSuccess, PagingResponse<UserViewDto>? userViewDtos)> GetAllUsers(UserParameter userParameter);

    public Task<(bool isSuccess, UserViewDto? userViewDto)> CreateUser(UserCreateDto userCreateDto);

    public Task<(bool isSuccess, UserViewDto? userViewDto)> UpdateUserByGuid(Guid guid, UserUpdateDto userUpdateDto);

    public Task<(bool isSuccess, List<UserRoleViewDto>? userRoleViewDtos)> GetAllUserRoles(string? searchTerm);

    #endregion

    #region Customer API

    public Task<(bool isSuccess, PagingResponse<CustomerVendorViewDto>? customerVendorViewDtos)> GetAllCustomers(
        CustomerVendorParameter customerVendorParameter);

    public Task<(bool isSuccess, CustomerVendorViewDto? CustomerVendorViewDto)> CreateCustomer(
        CustomerVendorCreateDto customerVendorCreate);

    public Task<(bool isSuccess, CustomerVendorViewDto? CustomerVendorViewDto)> UpdateCustomer(Guid guid,
        CustomerVendorUpdateDto customerVendorUpdateDto);

    public Task<(bool isSuccess, List<CustomerVendorViewDto>? CustomerVendorViewDtos)> SearchCustomers(
        string? searchTerm);

    #endregion

    #region Vendor API

    public Task<(bool isSuccess, PagingResponse<CustomerVendorViewDto>? customerVendorViewDtos)> GetAllVendors(
        CustomerVendorParameter customerVendorParameter);

    public Task<(bool isSuccess, CustomerVendorViewDto? CustomerVendorViewDto)> CreateVendor(
        CustomerVendorCreateDto customerVendorCreate);

    public Task<(bool isSuccess, CustomerVendorViewDto? CustomerVendorViewDto)> UpdateVendor(Guid guid,
        CustomerVendorUpdateDto customerVendorUpdateDto);

    public Task<(bool isSuccess, List<CustomerVendorViewDto>? CustomerVendorViewDtos)>
        SearchVendors(string? searchTerm);

    #endregion

    #region Category API

    public Task<(bool isSuccess, CategoryViewDto? categoryViewDto)> CreateCategory(
        CategoryCreateDto categoryCreateDto);

    public Task<(bool isSuccess, PagingResponse<CategoryViewDto>? categoryViewDtos)> GetAllCategories(
        CategoryParameter categoryParameter);

    public Task<(bool isSuccess, List<CategoryViewDto>? categoryViewDtos)> SearchCategories(
        string? searchTerm);

    public Task<bool> DisableCategory(Guid guid);

    public Task<(bool isSuccess, CustomerVendorViewDto? customerVendorViewDto)> UpdateCategory(
        Guid guid, CategoryUpdateDto categoryUpdateDto);

    #endregion

    #region Product API

    public Task<(bool isSuccess, ProductViewDto? productViewDto)> CreateProduct(
        ProductCreateDto productCreateDto);

    public Task<(bool isSuccess, ProductViewDto? productViewDto)> GetProductByGuid(Guid guid);

    public Task<(bool isSuccess, PagingResponse<ProductViewDto>? productViewDtos)> GetAllProducts(
        ProductParameter productParameter);

    public Task<(bool isSuccess, List<ProductViewDto>? productViewDtos)> SearchProducts(
        string? searchTerm);

    public Task<bool> DisableProduct(Guid guid);

    public Task<(bool isSuccess, ProductViewDto? productViewDto)> UpdateProduct(
        Guid guid, ProductUpdateDto productUpdateDto);

    public Task<(bool isSuccess, List<ProductPriceViewDto>? productPriceViewDtos)> GetProductPriceHistory(Guid guid);

    public Task<(bool isSuccess, ProductPriceViewDto?)> UpdateProductPrice(Guid guid,
        ProductPriceCreateDto productPriceCreateDto);

    public Task<bool> DisableProductPrice(Guid guid, Guid productGuid);

    public Task<(bool isSuccess, SuggestedProductPriceViewDto? suggestedProductPriceViewDto)> SuggestProductPrice(
        Guid guid
    );

    public Task<(bool isSuccess, ProductPurchasePriceViewDto? productPurchasePriceViewDto)>
        GetLastPurchasedPriceOfProduct(
            Guid guid
        );

    #endregion

    #region Dashboard API

    public Task<(bool isSuccess, CustomerDebitSummaryViewDto? customerDebitSummaryViewDto)> GetCustomersWithDebit();

    public Task<(bool isSuccess, List<InvoiceSaleForFrequentCustomerViewDto>? invoiceSaleForFrequentCustomerViewDtos)>
        GetFrequentCustomers();

    public Task<(bool isSuccess, List<TopSellingProductsViewDto>? topSellingProductsViewDtos)> GetTopSellingProducts();
    public Task<(bool isSuccess, List<MonthlySaleFigureViewDto>? monthlySaleFigureViewDtos)> GetMonthlySummary();

    #endregion

    #region Invoice API

    public Task<(bool isSuccess, InvoiceIdViewDto? invoiceIdViewDto)> GetNextPurchaseInvoiceNumber();

    public Task<(bool isSuccess, InvoicePurchaseViewDto? invoicePurchaseViewDto)>
        CreatePurchaseInvoice(InvoicePurchaseCreateDto invoicePurchaseCreateDto);

    public Task<(bool isSuccess, PagingResponse<InvoicePurchaseTableViewDto>? invoicePurchaseViewDtos)>
        GetAllPurchasesInvoices(InvoiceParameter invoiceParameter);

    public Task<(bool isSuccess, InvoicePurchaseViewDto? invoicePurchaseViewDto)> GetPurchaseInvoice(Guid guid);
    public Task<(bool isSuccess, InvoiceSaleViewDto? invoiceSaleViewDto)> GetSaleInvoice(Guid guid);

    public Task<(bool isSuccess, PagingResponse<InvoiceSaleTableViewDto>? invoiceSaleViewDtos)>
        GetAllSalesInvoices(InvoiceParameter invoiceParameter);

    public Task<(bool isSuccess, InvoiceIdViewDto? invoiceIdViewDto)> GetNextSaleInvoiceNumber();

    public Task<(bool isSuccess, Stream? stream)>
        CreateSaleInvoice(InvoiceSaleCreateDto invoiceSaleCreateDto);

    public Task<(bool isSuccess, Stream? stream)>
        DownloadSaleInvoice(Guid guid);

    #endregion
}