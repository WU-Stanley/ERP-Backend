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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public ICollection<HelpdeskTicketComment> Comments { get; set; } = new List<HelpdeskTicketComment>();
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
        public DateTime? WarrantyExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
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
}
