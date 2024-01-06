using Friday.ERP.Client.Data.RequestFeatures;

namespace Friday.ERP.Client.Data.DataTransferObjects;

public record CategoryViewDto(
    Guid Guid,
    string Name,
    string Color
);

public record CategoryCreateDto(
    string Name,
    string Color
);

public record CategoryUpdateDto(
    string? Name,
    string? Color
);

public class CategoryParameter : RequestParameters
{
    public CategoryParameter()
    {
        OrderBy = "Name";
    }

    public string? SearchTerm { get; set; }
}