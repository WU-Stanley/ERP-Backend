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
        public int Step { get; set; } = 1;
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

    public class CreatePayrollAdjustmentDto
    {
        public Guid EmployeeId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ApplicableMonth { get; set; }
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

    public class CreateVendorDto
    {
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class CreatePurchaseOrderDto
    {
        public Guid? ProcurementRequestId { get; set; }
        public Guid VendorId { get; set; }
        public List<CreatePurchaseOrderLineItemDto> LineItems { get; set; } = new();
    }

    public class CreatePurchaseOrderLineItemDto
    {
        public Guid? InventoryItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ReceiveGoodsDto
    {
        public Guid? ReceivedByUserId { get; set; }
        public string Remarks { get; set; } = string.Empty;
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

    
    public class UploadNewDocumentVersionDto
    {
        public string StorageUrl { get; set; } = string.Empty;
        public string? Remarks { get; set; }
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

    public class HelpdeskTicketDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Priority { get; set; } = "Normal";
        public string Status { get; set; } = "Open";
        public Guid? RequesterUserId { get; set; }
        public Guid? AssigneeUserId { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public Guid? FacilityAssetId { get; set; }
        public string FacilityAssetName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    
    public class AssignHelpdeskTicketDto
    {
        public Guid AssigneeUserId { get; set; }
        public string? Notes { get; set; }
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
        public Guid? FacilityAssetId { get; set; }
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
        public Guid? FacilityAssetId { get; set; }
        public string FacilityAssetName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<HelpdeskTicketCommentDto> Comments { get; set; } = new();
    }



    public class FacilityAssetDto
    {
        public Guid Id { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public Guid? CustodianEmployeeId { get; set; }
        public string Condition { get; set; } = "Good";
        public string Status { get; set; } = "InUse";
        public DateTime? PurchaseDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public int ExpectedLifeSpanMonths { get; set; }
        public decimal SalvageValue { get; set; }
        public decimal CurrentValue { get; set; } // Computed
        public DateTime? WarrantyExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
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
        public int ExpectedLifeSpanMonths { get; set; } = 60;
        public decimal SalvageValue { get; set; } = 0;
        public DateTime? WarrantyExpiryDate { get; set; }
    }

    public class AssetAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public string ConditionAtAssignment { get; set; } = string.Empty;
        public string ConditionAtReturn { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class CreateAssetAssignmentDto
    {
        public Guid EmployeeId { get; set; }
        public string ConditionAtAssignment { get; set; } = "Good";
        public string Notes { get; set; } = string.Empty;
    }

    public class ReturnAssetDto
    {
        public string ConditionAtReturn { get; set; } = "Good";
        public string Notes { get; set; } = string.Empty;
    }

    public class AssetMaintenanceRecordDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal Cost { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
    }

    public class CreateAssetMaintenanceRecordDto
    {
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal Cost { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public string PerformedBy { get; set; } = string.Empty;
    }

    public class RegistryIntegrationRecordDto
    {
        public Guid Id { get; set; }
        public string SystemName { get; set; } = string.Empty;
        public string IntegrationType { get; set; } = string.Empty;
        public string ExternalUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastSyncedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RegistrySyncLogDto
    {
        public Guid Id { get; set; }
        public Guid IntegrationId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
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

    public class VendorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PurchaseOrderLineItemDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public Guid? InventoryItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class PurchaseOrderDto
    {
        public Guid Id { get; set; }
        public string PoNumber { get; set; } = string.Empty;
        public Guid? ProcurementRequestId { get; set; }
        public string ProcurementRequestTitle { get; set; } = string.Empty;
        public Guid VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PurchaseOrderLineItemDto> LineItems { get; set; } = new();
    }

    public class GoodsReceivedNoteDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public string PoNumber { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; }
        public Guid? ReceivedByUserId { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class InventoryTransactionDto
    {
        public Guid Id { get; set; }
        public Guid InventoryItemId { get; set; }
        public string InventoryItemName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = "In";
        public int Quantity { get; set; }
        public Guid? ReferenceId { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
