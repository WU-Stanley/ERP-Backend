using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WUIAM.Models
{
    public class SalaryStructure
    {
        [Key]
        public Guid Id { get; set; }
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class PayrollRun
    {
        [Key]
        public Guid Id { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Status { get; set; } = "Draft";
        public int EmployeeCount { get; set; }
        public decimal GrossPayTotal { get; set; }
        public decimal NetPayTotal { get; set; }
        public Guid? ProcessedByUserId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class Payslip
    {
        [Key]
        public Guid Id { get; set; }
        public Guid PayrollRunId { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public int Step { get; set; }
        public decimal BasePay { get; set; }
        public decimal AllowancesTotal { get; set; }
        public decimal BonusesTotal { get; set; }
        public decimal DeductionsTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal PensionAmount { get; set; }
        public decimal GrossPay { get; set; }
        public decimal NetPay { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PayrollAdjustment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Bonus" or "Deduction"
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ApplicableMonth { get; set; }
        public bool IsProcessed { get; set; }
        public Guid? PayrollRunId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class ProcurementRequest
    {
        [Key]
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public decimal EstimatedAmount { get; set; }
        public string Priority { get; set; } = "Normal";
        public string Status { get; set; } = "Draft";
        public DateTime? NeededBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class InventoryItem
    {
        [Key]
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public decimal UnitCost { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = "Available";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class Vendor
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active, Inactive
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class PurchaseOrder
    {
        [Key]
        public Guid Id { get; set; }
        public string PoNumber { get; set; } = string.Empty;
        public Guid? ProcurementRequestId { get; set; }
        public Guid VendorId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Sent, Delivered, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        
        public ICollection<PurchaseOrderLineItem> LineItems { get; set; } = new List<PurchaseOrderLineItem>();
    }

    public class PurchaseOrderLineItem
    {
        [Key]
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public Guid? InventoryItemId { get; set; } // If mapped to an inventory item
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class GoodsReceivedNote
    {
        [Key]
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
        public Guid? ReceivedByUserId { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class InventoryTransaction
    {
        [Key]
        public Guid Id { get; set; }
        public Guid InventoryItemId { get; set; }
        public string TransactionType { get; set; } = "In"; // In, Out, Adjustment, Initial
        public int Quantity { get; set; }
        public Guid? ReferenceId { get; set; } // e.g. GoodsReceivedNote Id or Request Id
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DocumentRecord
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Guid? OwnerDepartmentId { get; set; }
        public Guid? OwnerUserId { get; set; }
        public string StorageUrl { get; set; } = string.Empty;
        public string Confidentiality { get; set; } = "Internal";
        public string Status { get; set; } = "Draft";
        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class HelpdeskTicket
    {
        [Key]
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public ICollection<HelpdeskTicketComment> Comments { get; set; } = new List<HelpdeskTicketComment>();

        [ForeignKey("FacilityAssetId")]
        public FacilityAsset? FacilityAsset { get; set; }
    }

    public class HelpdeskTicketComment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        [ForeignKey("TicketId")]
        public HelpdeskTicket? Ticket { get; set; }
    }

    public class FacilityAsset
    {
        [Key]
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
        public int ExpectedLifeSpanMonths { get; set; } = 60; // default 5 years
        public decimal SalvageValue { get; set; } = 0;
        public DateTime? WarrantyExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        
        public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
        public ICollection<AssetMaintenanceRecord> MaintenanceRecords { get; set; } = new List<AssetMaintenanceRecord>();
    }

    public class AssetAssignment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnedAt { get; set; }
        public string ConditionAtAssignment { get; set; } = "Good";
        public string ConditionAtReturn { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        [ForeignKey("AssetId")]
        public FacilityAsset? Asset { get; set; }
    }

    public class AssetMaintenanceRecord
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal Cost { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled
        public string PerformedBy { get; set; } = string.Empty;

        [ForeignKey("AssetId")]
        public FacilityAsset? Asset { get; set; }
    }


    public class RegistryIntegrationRecord
    {
        [Key]
        public Guid Id { get; set; }
        public string SystemName { get; set; } = string.Empty;
        public string IntegrationType { get; set; } = string.Empty;
        public string ExternalUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Planned";
        public DateTime? LastSyncedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class RegistrySyncLog
    {
        [Key]
        public Guid Id { get; set; }
        public Guid IntegrationId { get; set; }
        public string ActionType { get; set; } = string.Empty; // e.g. Ping, Sync
        public string Status { get; set; } = string.Empty; // e.g. Success, Failed
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey("IntegrationId")]
        public RegistryIntegrationRecord? Integration { get; set; }
    }
}
