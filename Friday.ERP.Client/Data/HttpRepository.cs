using System.Globalization;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Friday.ERP.Client.Data.DataTransferObjects;
using Friday.ERP.Client.Data.RequestFeatures;
using Friday.ERP.Client.Utilities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using Newtonsoft.Json;

namespace Friday.ERP.Client.Data;

public class HttpRepository(
    HttpClient client,
    AuthenticationStateProvider authStateProvider,
    ILocalStorageService localStorage,
    ISnackbar snackbar) : IHttpRepository
{
    #region Private Methods

    private async Task<(bool isSuccess, T? dynamicObject)> CustomHttpGet<T>(string requestUrl) where T : class
    {
        try
        {
            var response = await client.GetAsync(requestUrl);
            var stringObject = await response.Content.ReadAsStringAsync();

            return !response.IsSuccessStatusCode
                ? (false, null)
                : (true, JsonConvert.DeserializeObject<T>(stringObject));
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    private async Task<(bool isSuccess, PagingResponse<T>? dynamicObject)>
        CustomHttpGetWithPagination<T>(string requestUrl) where T : class
    {
        try
        {
            var response = await client.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
                return (false, null);

            var stringObject = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<List<T>>(stringObject);
            var metaData = JsonConvert.DeserializeObject<MetaData>(response.Headers.GetValues("X-Pagination").First());

            return (true, new PagingResponse<T>
            {
                Items = items,
                MetaData = metaData
            });
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    private async Task<(bool isSuccess, T? dynamicObject)> CustomHttpPost<T, T2>(string requestUrl, T2 bodyObject)
        where T : class
        where T2 : class
    {
        try
        {
            var response = await client.PostAsJsonAsync(requestUrl, bodyObject);
            var stringObject = await response.Content.ReadAsStringAsync();

            return !response.IsSuccessStatusCode
                ? (false, null)
                : (true, JsonConvert.DeserializeObject<T>(stringObject));
        }
        catch (Exception)
        {
            return (false, null);
        }
    }


    private async Task<bool> CustomHttpPostWithoutReturnBody(string requestUrl)
    {
        try
        {
            var response = await client.PostAsync(requestUrl, null);

            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<(bool isSuccess, T? dynamicObject)> CustomHttpPut<T, T2>(string requestUrl,
        T2 bodyObject)
        where T : class
        where T2 : class
    {
        try
        {
            var response = await client.PutAsJsonAsync(requestUrl, bodyObject);
            var stringObject = await response.Content.ReadAsStringAsync();

            return !response.IsSuccessStatusCode
                ? (false, null)
                : (true, JsonConvert.DeserializeObject<T>(stringObject));
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    #endregion

    #region Auth API

    public async Task<(bool isSuccess, LoginResultDto? loginResultDto)> Login(LoginDto loginDto)
    {
        var result = await CustomHttpPost<LoginResultDto, LoginDto>("authentication/login", loginDto);
        if (!result.isSuccess || result.dynamicObject is null) return result;
        await localStorage.SetItemAsync("accessToken", result!.dynamicObject.AccessToken);
        await localStorage.SetItemAsync("refreshToken", result!.dynamicObject.RefreshToken);
        await localStorage.SetItemAsync("refreshTokenExpiryTime",
            result!.dynamicObject.RefreshTokenExpiredAt.ToString(CultureInfo.CurrentCulture));

        ((AuthStateProvider)authStateProvider).NotifyUserAuthentication(result!.dynamicObject.AccessToken);

        return result;
    }

    public async Task<string?> GetRefreshToken()
    {
        var expired = await localStorage.GetItemAsync<string>("refreshTokenExpiryTime");
        var expiredDateTime = DateTime.Parse(expired);
        if (DateTime.Now > expiredDateTime)
        {
            // RefreshToken Expired
            await DeleteLocalStorageAndLogout();
            snackbar.Add("Login Expired.", Severity.Error);
            return null;
        }

        var refreshTokenDto = new RefreshTokenDto(
            await localStorage.GetItemAsync<string>("accessToken"),
            await localStorage.GetItemAsync<string>("refreshToken")
        );

        var result = await CustomHttpPost<LoginResultDto, RefreshTokenDto>("authentication/refresh", refreshTokenDto);
        if (result.isSuccess)
        {
            await localStorage.RemoveItemAsync("accessToken");
            await localStorage.RemoveItemAsync("refreshToken");
            await localStorage.RemoveItemAsync("refreshTokenExpiryTime");

            await localStorage.SetItemAsync("accessToken", result!.dynamicObject!.AccessToken);
            await localStorage.SetItemAsync("refreshToken", result!.dynamicObject.RefreshToken);
            await localStorage.SetItemAsync("refreshTokenExpiryTime",
                result!.dynamicObject.RefreshTokenExpiredAt.ToString(CultureInfo.CurrentCulture));

            return result!.dynamicObject!.AccessToken;
        }

        await DeleteLocalStorageAndLogout();
        snackbar.Add("Login Expired.", Severity.Error);
        return null;
    }

    public async Task<bool> Logout()
    {
        var result = await CustomHttpPostWithoutReturnBody("authentication/logout");

        await DeleteLocalStorageAndLogout();
        return result;
    }

    private async Task DeleteLocalStorageAndLogout()
    {
        await localStorage.RemoveItemAsync("accessToken");
        await localStorage.RemoveItemAsync("refreshToken");
        await localStorage.RemoveItemAsync("refreshTokenExpiryTime");

        ((AuthStateProvider)authStateProvider).NotifyUserLogout();
    }

    #endregion

    #region System API

    public async Task<(bool isSuccess, List<NotificationViewDto>? notificationViewDtos)> GetAllNotifications()
    {
        return await CustomHttpGet<List<NotificationViewDto>>("system-management/notifications");
    }

    public async Task<bool> ReadNotificationByGuid(Guid guid)
    {
        return await CustomHttpPostWithoutReturnBody($"system-management/notifications/{guid}");
    }

    public async Task<(bool isSuccess, SettingViewDto? settingViewDto)> GetSettings()
    {
        return await CustomHttpGet<SettingViewDto>("system-management/system");
    }

    public async Task<(bool isSuccess, SettingViewDto? settingViewDto)> UpdateSettings(
        SettingUpdateDto settingUpdateDto)
    {
        return await CustomHttpPut<SettingViewDto, SettingUpdateDto>("system-management/system", settingUpdateDto);
    }

    #endregion

    #region User API

    public async Task<(bool isSuccess, PagingResponse<UserViewDto>? userViewDtos)> GetAllUsers(
        UserParameter userParameter)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["pageNumber"] = userParameter.PageNumber.ToString(),
            ["pageSize"] = userParameter.PageSize.ToString(),
            ["searchTerm"] = userParameter.SearchTerm ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("account-management/users", queryStringParam!);
        return await CustomHttpGetWithPagination<UserViewDto>(url);
    }

    public async Task<(bool isSuccess, UserViewDto? userViewDto)> CreateUser(UserCreateDto userCreateDto)
    {
        return await CustomHttpPost<UserViewDto, UserCreateDto>("account-management/users", userCreateDto);
    }

    public async Task<(bool isSuccess, UserViewDto? userViewDto)> UpdateUserByGuid(Guid guid,
        UserUpdateDto userUpdateDto)
    {
        return await CustomHttpPut<UserViewDto, UserUpdateDto>($"account-management/users/{guid}",
            userUpdateDto);
    }

    public async Task<(bool isSuccess, List<UserRoleViewDto>? userRoleViewDtos)> GetAllUserRoles(string? searchTerm)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["searchTerm"] = searchTerm ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("account-management/roles", queryStringParam!);
        return await CustomHttpGet<List<UserRoleViewDto>>(url);
    }

    #endregion

    #region Customer API

    public async Task<(bool isSuccess, PagingResponse<CustomerVendorViewDto>? customerVendorViewDtos)> GetAllCustomers(
        CustomerVendorParameter customerVendorParameter)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["pageNumber"] = customerVendorParameter.PageNumber.ToString(),
            ["pageSize"] = customerVendorParameter.PageSize.ToString(),
            ["searchTerm"] = customerVendorParameter.SearchTerm ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("customer-management/customers", queryStringParam!);
        return await CustomHttpGetWithPagination<CustomerVendorViewDto>(url);
    }

    public async Task<(bool isSuccess, CustomerVendorViewDto? CustomerVendorViewDto)> CreateCustomer(
        CustomerVendorCreateDto customerVendorCreate)
    {
        return await CustomHttpPost<CustomerVendorViewDto, CustomerVendorCreateDto>("customer-management/customers",
            customerVendorCreate);
    }

    public async Task<(bool isSuccess, CustomerVendorViewDto? CustomerVendorViewDto)> UpdateCustomer(Guid guid,
        CustomerVendorUpdateDto customerVendorUpdateDto)
    {
        return await CustomHttpPut<CustomerVendorViewDto, CustomerVendorUpdateDto>(
            $"customer-management/customers/{guid}",
            customerVendorUpdateDto);
    }

    public async Task<(bool isSuccess, List<CustomerVendorViewDto>? CustomerVendorViewDtos)> SearchCustomers(
        string? searchTerm)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["searchTerm"] = searchTerm ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("customer-management/customers/search", queryStringParam!);
        return await CustomHttpGet<List<CustomerVendorViewDto>>(url);
    }

    #endregion

    #region Vendor API

    public async Task<(bool isSuccess, PagingResponse<CustomerVendorViewDto>? customerVendorViewDtos)> GetAllVendors(
        CustomerVendorParameter customerVendorParameter)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["pageNumber"] = customerVendorParameter.PageNumber.ToString(),
            ["pageSize"] = customerVendorParameter.PageSize.ToString(),
            ["searchTerm"] = customerVendorParameter.SearchTerm ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("vendor-management/vendors", queryStringParam!);
        return await CustomHttpGetWithPagination<CustomerVendorViewDto>(url);
    }

    public async Task<(bool isSuccess, CustomerVendorViewDto? CustomerVendorViewDto)> CreateVendor(
        CustomerVendorCreateDto customerVendorCreate)
    {
        return await CustomHttpPost<CustomerVendorViewDto, CustomerVendorCreateDto>("vendor-management/vendors",
            customerVendorCreate);
    }

    public async Task<(bool isSuccess, CustomerVendorViewDto? CustomerVendorViewDto)> UpdateVendor(Guid guid,
        CustomerVendorUpdateDto customerVendorUpdateDto)
    {
        return await CustomHttpPut<CustomerVendorViewDto, CustomerVendorUpdateDto>(
            $"vendor-management/vendors/{guid}",
            customerVendorUpdateDto);
    }

    public async Task<(bool isSuccess, List<CustomerVendorViewDto>? CustomerVendorViewDtos)> SearchVendors(
        string? searchTerm)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["searchTerm"] = searchTerm ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("vendor-management/vendors/search", queryStringParam!);
        return await CustomHttpGet<List<CustomerVendorViewDto>>(url);
    }

    #endregion

    #region Category API

    public async Task<(bool isSuccess, CategoryViewDto? categoryViewDto)> CreateCategory(
        CategoryCreateDto categoryCreateDto)
    {
        return await CustomHttpPost<CategoryViewDto, CategoryCreateDto>("inventory-management/categories",
            categoryCreateDto);
    }

    public async Task<(bool isSuccess, PagingResponse<CategoryViewDto>? categoryViewDtos)> GetAllCategories(
        CategoryParameter categoryParameter)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["pageNumber"] = categoryParameter.PageNumber.ToString(),
            ["pageSize"] = categoryParameter.PageSize.ToString(),
            ["searchTerm"] = categoryParameter.SearchTerm ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("inventory-management/categories", queryStringParam!);
        return await CustomHttpGetWithPagination<CategoryViewDto>(url);
    }

    public async Task<(bool isSuccess, List<CategoryViewDto>? categoryViewDtos)> SearchCategories(
        string? searchTerm)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["searchTerm"] = searchTerm ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("inventory-management/categories/search", queryStringParam!);
        return await CustomHttpGet<List<CategoryViewDto>>(url);
    }

    public async Task<bool> DisableCategory(Guid guid)
    {
        return await CustomHttpPostWithoutReturnBody($"inventory-management/categories/{guid}/disable");
    }

    public async Task<(bool isSuccess, CustomerVendorViewDto? customerVendorViewDto)> UpdateCategory(Guid guid,
        CategoryUpdateDto categoryUpdateDto)
    {
        return await CustomHttpPut<CustomerVendorViewDto, CategoryUpdateDto>(
            $"inventory-management/categories/{guid}",
            categoryUpdateDto);
    }

    #endregion

    #region Product API

    public async Task<(bool isSuccess, ProductViewDto? productViewDto)> CreateProduct(ProductCreateDto productCreateDto)
    {
        return await CustomHttpPost<ProductViewDto, ProductCreateDto>("inventory-management/products",
            productCreateDto);
    }

    public async Task<(bool isSuccess, PagingResponse<ProductViewDto>? productViewDtos)> GetAllProducts(
        ProductParameter productParameter)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["pageNumber"] = productParameter.PageNumber.ToString(),
            ["pageSize"] = productParameter.PageSize.ToString(),
            ["CategoryGuid"] = productParameter.CategoryGuid.ToString() ?? string.Empty,
            ["searchTerm"] = productParameter.SearchTerm ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("inventory-management/products", queryStringParam!);
        return await CustomHttpGetWithPagination<ProductViewDto>(url);
    }

    public async Task<(bool isSuccess, List<ProductViewDto>? productViewDtos)> SearchProducts(string? searchTerm)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["searchTerm"] = searchTerm ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("inventory-management/products", queryStringParam!);
        return await CustomHttpGet<List<ProductViewDto>>(url);
    }

    public async Task<bool> DisableProduct(Guid guid)
    {
        return await CustomHttpPostWithoutReturnBody($"inventory-management/products/{guid}/disable");
    }

    public async Task<(bool isSuccess, ProductViewDto? productViewDto)> UpdateProduct(Guid guid,
        ProductUpdateDto productUpdateDto)
    {
        return await CustomHttpPut<ProductViewDto, ProductUpdateDto>(
            $"inventory-management/products/{guid}",
            productUpdateDto);
    }

    public async Task<(bool isSuccess, List<ProductPriceViewDto>? productPriceViewDtos)>
        GetProductPriceHistory(Guid guid)
    {
        return await CustomHttpGet<List<ProductPriceViewDto>>(
            $"inventory-management/products/{guid}/prices");
    }

    public async Task<(bool isSuccess, ProductPriceViewDto?)> UpdateProductPrice(Guid guid,
        ProductPriceCreateDto productPriceCreateDto)
    {
        return await CustomHttpPut<ProductPriceViewDto, ProductPriceCreateDto>(
            $"inventory-management/products/{guid}/prices", productPriceCreateDto);
    }

    public async Task<bool> DisableProductPrice(Guid guid, Guid productGuid)
    {
        return await CustomHttpPostWithoutReturnBody(
            $"inventory-management/products/{guid}/prices/{productGuid}/disable");
    }

    public async Task<(bool isSuccess, SuggestedProductPriceViewDto? suggestedProductPriceViewDto)>
        SuggestProductPrice(Guid guid)
    {
        return await CustomHttpGet<SuggestedProductPriceViewDto>(
            $"inventory-management/products/{guid}/prices/suggest");
    }

    public async Task<(bool isSuccess, ProductPurchasePriceViewDto? productPurchasePriceViewDto)>
        GetLastPurchasedPriceOfProduct(Guid guid)
    {
        return await CustomHttpGet<ProductPurchasePriceViewDto>(
            $"invoice-management/purchases/product/{guid}/price");
    }

    #endregion

    #region Dashboard API

    public async Task<(bool isSuccess, CustomerDebitSummaryViewDto? customerDebitSummaryViewDto)>
        GetCustomersWithDebit()
    {
        return await CustomHttpGet<CustomerDebitSummaryViewDto>(
            "dashboard-management/customers-with-debit");
    }

    public async Task<(bool isSuccess, List<InvoiceSaleForFrequentCustomerViewDto>?
            invoiceSaleForFrequentCustomerViewDtos)>
        GetFrequentCustomers()
    {
        return await CustomHttpGet<List<InvoiceSaleForFrequentCustomerViewDto>>(
            "dashboard-management/frequent-customers");
    }

    public async Task<(bool isSuccess, List<TopSellingProductsViewDto>? topSellingProductsViewDtos)>
        GetTopSellingProducts()
    {
        return await CustomHttpGet<List<TopSellingProductsViewDto>>(
            "dashboard-management/top-selling-products");
    }

    public async Task<(bool isSuccess, List<MonthlySaleFigureViewDto>? monthlySaleFigureViewDtos)> GetMonthlySummary()
    {
        return await CustomHttpGet<List<MonthlySaleFigureViewDto>>(
            "dashboard-management/monthly-summary");
    }

    #endregion

    #region Invoice API

    public async Task<(bool isSuccess, InvoiceIdViewDto? invoiceIdViewDto)> GetNextPurchaseInvoiceNumber()
    {
        return await CustomHttpGet<InvoiceIdViewDto>(
            "invoice-management/purchases/next-invoice-no");
    }

    public async Task<(bool isSuccess, InvoicePurchaseViewDto? invoicePurchaseViewDto)> CreatePurchaseInvoice(
        InvoicePurchaseCreateDto invoicePurchaseCreateDto)
    {
        return await CustomHttpPost<InvoicePurchaseViewDto, InvoicePurchaseCreateDto>(
            "invoice-management/purchases", invoicePurchaseCreateDto);
    }

    public async Task<(bool isSuccess, PagingResponse<InvoicePurchaseTableViewDto>? invoicePurchaseViewDtos)>
        GetAllPurchasesInvoices(InvoiceParameter invoiceParameter)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["pageNumber"] = invoiceParameter.PageNumber.ToString(),
            ["pageSize"] = invoiceParameter.PageSize.ToString(),
            ["CustomerVendorGuid"] = invoiceParameter.CustomerVendorGuid.ToString() ?? string.Empty,
            ["searchTerm"] = invoiceParameter.SearchTerm ?? string.Empty,
            ["FromPurchasedDate"] = invoiceParameter.FromPurchasedDate.ToString() ?? string.Empty,
            ["ToPurchasedDate"] = invoiceParameter.ToPurchasedDate.ToString() ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("invoice-management/purchases", queryStringParam!);
        return await CustomHttpGetWithPagination<InvoicePurchaseTableViewDto>(url);
    }

    public async Task<(bool isSuccess, InvoicePurchaseViewDto? invoicePurchaseViewDto)> GetPurchaseInvoice(Guid guid)
    {
        return await CustomHttpGet<InvoicePurchaseViewDto>(
            $"invoice-management/purchases/{guid}");
    }

    public async Task<(bool isSuccess, InvoiceSaleViewDto? invoiceSaleViewDto)> GetSaleInvoice(Guid guid)
    {
        return await CustomHttpGet<InvoiceSaleViewDto>(
            $"invoice-management/sales/{guid}");
    }

    public async Task<(bool isSuccess, PagingResponse<InvoiceSaleTableViewDto>? invoiceSaleViewDtos)>
        GetAllSalesInvoices(InvoiceParameter invoiceParameter)
    {
        var queryStringParam = new Dictionary<string, string>
        {
            ["pageNumber"] = invoiceParameter.PageNumber.ToString(),
            ["pageSize"] = invoiceParameter.PageSize.ToString(),
            ["CustomerVendorGuid"] = invoiceParameter.CustomerVendorGuid.ToString() ?? string.Empty,
            ["searchTerm"] = invoiceParameter.SearchTerm ?? string.Empty,
            ["FromPurchasedDate"] = invoiceParameter.FromPurchasedDate.ToString() ?? string.Empty,
            ["ToPurchasedDate"] = invoiceParameter.ToPurchasedDate.ToString() ?? string.Empty
        };
        var url = QueryHelpers.AddQueryString("invoice-management/sales", queryStringParam!);
        return await CustomHttpGetWithPagination<InvoiceSaleTableViewDto>(url);
    }

    public async Task<(bool isSuccess, InvoiceIdViewDto? invoiceIdViewDto)> GetNextSaleInvoiceNumber()
    {
        return await CustomHttpGet<InvoiceIdViewDto>(
            "invoice-management/sales/next-invoice-no");
    }

    public async Task<(bool isSuccess, Stream? stream)> CreateSaleInvoice(InvoiceSaleCreateDto invoiceSaleCreateDto)
    {
        try
        {
            var response =
                await client.PostAsJsonAsync("invoice-management/sales", invoiceSaleCreateDto);
            return response.IsSuccessStatusCode ? (true, await response.Content.ReadAsStreamAsync()) : (false, null);
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    public async Task<(bool isSuccess, Stream? stream)> DownloadSaleInvoice(Guid guid)
    {
        try
        {
            var response =
                await client.GetAsync($"download/invoices/sales/{guid}");
            return response.IsSuccessStatusCode ? (true, await response.Content.ReadAsStreamAsync()) : (false, null);
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    #endregion
}