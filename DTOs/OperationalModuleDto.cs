namespace WUIAM.DTOs
{
    public class ModuleSummaryDto
    {
        public int SalaryStructureCount { get; set; }
        public int PayrollRunCount { get; set; }
        public int PendingProcurementRequestCount { get; set; }
        public int LowStockInventoryItemCount { get; set; }
        public int DocumentCount { get; set; }
        public int OpenHelpdeskTicketCount { get; set; }
        public int FacilityAssetCount { get; set; }
        public int RegistryIntegrationCount { get; set; }
    }

    public class UpdateOperationalStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }

    public class CreateSalaryStructureDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public decimal BasePay { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal TaxRatePercent { get; set; }
        public decimal PensionRatePercent { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreatePayrollRunDto
    {
        public string PeriodName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int EmployeeCount { get; set; }
        public decimal GrossPayTotal { get; set; }
        public decimal NetPayTotal { get; set; }
    }

    public class CreateProcurementRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public decimal EstimatedAmount { get; set; }
        public string Priority { get; set; } = "Normal";
        public DateTime? NeededBy { get; set; }
    }

    public class CreateInventoryItemDto
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public decimal UnitCost { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = "Available";
    }

    public class CreateDocumentRecordDto
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Guid? OwnerDepartmentId { get; set; }
        public Guid? OwnerUserId { get; set; }
        public string StorageUrl { get; set; } = string.Empty;
        public string Confidentiality { get; set; } = "Internal";
        public string Status { get; set; } = "Draft";
    }

    public class CreateHelpdeskTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Priority { get; set; } = "Normal";
        public Guid? RequesterUserId { get; set; }
        public Guid? AssigneeUserId { get; set; }
        public DateTime? DueAt { get; set; }
    }

    public class HelpdeskTicketCommentDto
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateHelpdeskCommentDto
    {
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
    }

    public class HelpdeskTicketDetailDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Priority { get; set; } = "Normal";
        public string Status { get; set; } = "Open";
        public Guid? RequesterUserId { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public Guid? AssigneeUserId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public DateTime? DueAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<HelpdeskTicketCommentDto> Comments { get; set; } = new();
    }

    public class CreateFacilityAssetDto
    {
        public string AssetTag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public Guid? CustodianEmployeeId { get; set; }
        public string Condition { get; set; } = "Good";
        public string Status { get; set; } = "InUse";
        public DateTime? PurchaseDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
    }

    public class CreateRegistryIntegrationDto
    {
        public string SystemName { get; set; } = string.Empty;
        public string IntegrationType { get; set; } = string.Empty;
        public string ExternalUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Planned";
        public DateTime? LastSyncedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
