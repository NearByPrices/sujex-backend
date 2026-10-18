using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Foundation;

public class CustomFieldDefinition : TenantEntity
{
    public string EntityType { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string FieldType { get; set; } = "Text"; // Text/Number/Date/Dropdown/Checkbox
    public bool IsRequired { get; set; } = false;
    public int DisplayOrder { get; set; }
    public string? Options { get; set; }
    public bool IsActive { get; set; } = true;
}
