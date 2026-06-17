using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WUIAM.Attributes;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Hubs;
using WUIAM.Interfaces;
using WUIAM.Models;

namespace WUIAM.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/erp-modules")]
    [HasPermission(Permissions.AdminAccess, Permissions.AccessDashboard, Permissions.SuperAdminAccess)]
    public class OperationalModulesController : ControllerBase
    {
        private readonly WUIAMDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public OperationalModulesController(WUIAMDbContext context, INotificationService notificationService, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        [HttpGet("summary")]
        [AllowAuthenticatedUsers]
        public async Task<ActionResult<ApiResponse<ModuleSummaryDto>>> GetSummary()
        {
            var summary = new ModuleSummaryDto
            {
                SalaryStructureCount = await _context.SalaryStructures.CountAsync(),
                PayrollRunCount = await _context.PayrollRuns.CountAsync(),
                PendingProcurementRequestCount = await _context.ProcurementRequests.CountAsync(request => request.Status != "Completed" && request.Status != "Cancelled"),
                LowStockInventoryItemCount = await _context.InventoryItems.CountAsync(item => item.QuantityOnHand <= item.ReorderLevel),
                DocumentCount = await _context.DocumentRecords.CountAsync(),
                OpenHelpdeskTicketCount = await _context.HelpdeskTickets.CountAsync(ticket => ticket.Status != "Closed" && ticket.Status != "Cancelled"),
                FacilityAssetCount = await _context.FacilityAssets.CountAsync(),
                RegistryIntegrationCount = await _context.RegistryIntegrationRecords.CountAsync()
            };

            return Ok(ApiResponse<ModuleSummaryDto>.Success("ERP module summary retrieved successfully", summary));
        }

        [HttpGet("finance/salary-structures")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManagePayroll, Permissions.ViewPayslips, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SalaryStructure>>>> GetSalaryStructures()
        {
            var result = await _context.SalaryStructures
                .OrderBy(structure => structure.GradeLevel)
                .ThenBy(structure => structure.Name)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<SalaryStructure>>.Success("Salary structures retrieved successfully", result));
        }

        [HttpPost("finance/salary-structures")]
        [HasPermission(Permissions.AdminAccess, Permissions.UpdateSalaryStructure, Permissions.ManagePayroll, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<SalaryStructure>>> CreateSalaryStructure([FromBody] CreateSalaryStructureDto dto)
        {
            if (await _context.SalaryStructures.AnyAsync(structure => structure.Code == dto.Code))
                return BadRequest(ApiResponse<SalaryStructure>.Failure("A salary structure with this code already exists"));

            var salaryStructure = new SalaryStructure
            {
                Id = Guid.NewGuid(),
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim(),
                GradeLevel = dto.GradeLevel.Trim(),
                BasePay = dto.BasePay,
                HousingAllowance = dto.HousingAllowance,
                TransportAllowance = dto.TransportAllowance,
                OtherAllowance = dto.OtherAllowance,
                TaxRatePercent = dto.TaxRatePercent,
                PensionRatePercent = dto.PensionRatePercent,
                IsActive = dto.IsActive
            };

            _context.SalaryStructures.Add(salaryStructure);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSalaryStructures), ApiResponse<SalaryStructure>.Success("Salary structure created successfully", salaryStructure));
        }

        [HttpPut("finance/salary-structures/{id}")]
        [HasPermission(Permissions.AdminAccess, Permissions.UpdateSalaryStructure, Permissions.ManagePayroll, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<SalaryStructure>>> UpdateSalaryStructure(Guid id, [FromBody] CreateSalaryStructureDto dto)
        {
            var salaryStructure = await _context.SalaryStructures.FindAsync(id);
            if (salaryStructure == null)
                return NotFound(ApiResponse<SalaryStructure>.Failure("Salary structure not found"));

            if (await _context.SalaryStructures.AnyAsync(structure => structure.Id != id && structure.Code == dto.Code))
                return BadRequest(ApiResponse<SalaryStructure>.Failure("A salary structure with this code already exists"));

            salaryStructure.Code = dto.Code.Trim();
            salaryStructure.Name = dto.Name.Trim();
            salaryStructure.GradeLevel = dto.GradeLevel.Trim();
            salaryStructure.BasePay = dto.BasePay;
            salaryStructure.HousingAllowance = dto.HousingAllowance;
            salaryStructure.TransportAllowance = dto.TransportAllowance;
            salaryStructure.OtherAllowance = dto.OtherAllowance;
            salaryStructure.TaxRatePercent = dto.TaxRatePercent;
            salaryStructure.PensionRatePercent = dto.PensionRatePercent;
            salaryStructure.IsActive = dto.IsActive;
            salaryStructure.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<SalaryStructure>.Success("Salary structure updated successfully", salaryStructure));
        }

        [HttpGet("finance/payroll-runs")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManagePayroll, Permissions.ProcessPayroll, Permissions.ViewPayslips, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PayrollRun>>>> GetPayrollRuns()
        {
            var result = await _context.PayrollRuns
                .OrderByDescending(run => run.PeriodStart)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<PayrollRun>>.Success("Payroll runs retrieved successfully", result));
        }

        [HttpPost("finance/payroll-runs")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManagePayroll, Permissions.ProcessPayroll, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<PayrollRun>>> CreatePayrollRun([FromBody] CreatePayrollRunDto dto)
        {
            if (dto.PeriodEnd < dto.PeriodStart)
                return BadRequest(ApiResponse<PayrollRun>.Failure("Payroll period end date cannot be earlier than start date"));

            var payrollRun = new PayrollRun
            {
                Id = Guid.NewGuid(),
                PeriodName = dto.PeriodName.Trim(),
                PeriodStart = dto.PeriodStart,
                PeriodEnd = dto.PeriodEnd,
                EmployeeCount = dto.EmployeeCount,
                GrossPayTotal = dto.GrossPayTotal,
                NetPayTotal = dto.NetPayTotal,
                Status = "Draft"
            };

            _context.PayrollRuns.Add(payrollRun);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayrollRuns), ApiResponse<PayrollRun>.Success("Payroll run created successfully", payrollRun));
        }

        [HttpPatch("finance/payroll-runs/{id}/status")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManagePayroll, Permissions.ProcessPayroll, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<PayrollRun>>> UpdatePayrollRunStatus(Guid id, [FromBody] UpdateOperationalStatusDto dto)
        {
            var payrollRun = await _context.PayrollRuns.FindAsync(id);
            if (payrollRun == null)
                return NotFound(ApiResponse<PayrollRun>.Failure("Payroll run not found"));

            payrollRun.Status = dto.Status.Trim();
            payrollRun.UpdatedAt = DateTime.UtcNow;
            if (payrollRun.Status == "Processed")
            {
                payrollRun.ProcessedAt = DateTime.UtcNow;
                payrollRun.ProcessedByUserId = GetCurrentUserId();
            }

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<PayrollRun>.Success("Payroll run status updated successfully", payrollRun));
        }

        [HttpGet("procurement/requests")]
        [HasPermission(Permissions.AdminAccess, Permissions.ApproveRequests, Permissions.ViewPendingApprovals, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProcurementRequest>>>> GetProcurementRequests([FromQuery] string? status = null)
        {
            var query = _context.ProcurementRequests.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(request => request.Status == status);

            var result = await query.OrderByDescending(request => request.CreatedAt).ToListAsync();
            return Ok(ApiResponse<IEnumerable<ProcurementRequest>>.Success("Procurement requests retrieved successfully", result));
        }

        [HttpPost("procurement/requests")]
        [HasPermission(Permissions.AdminAccess, Permissions.ApproveRequests, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<ProcurementRequest>>> CreateProcurementRequest([FromBody] CreateProcurementRequestDto dto)
        {
            var request = new ProcurementRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = $"PR-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                DepartmentId = dto.DepartmentId,
                RequestedByUserId = dto.RequestedByUserId ?? GetCurrentUserId(),
                EstimatedAmount = dto.EstimatedAmount,
                Priority = dto.Priority.Trim(),
                NeededBy = dto.NeededBy,
                Status = "Submitted"
            };

            _context.ProcurementRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProcurementRequests), ApiResponse<ProcurementRequest>.Success("Procurement request submitted successfully", request));
        }

        [HttpPatch("procurement/requests/{id}/status")]
        [HasPermission(Permissions.AdminAccess, Permissions.ApproveRequests, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<ProcurementRequest>>> UpdateProcurementRequestStatus(Guid id, [FromBody] UpdateOperationalStatusDto dto)
        {
            var request = await _context.ProcurementRequests.FindAsync(id);
            if (request == null)
                return NotFound(ApiResponse<ProcurementRequest>.Failure("Procurement request not found"));

            request.Status = dto.Status.Trim();
            request.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<ProcurementRequest>.Success("Procurement request status updated successfully", request));
        }

        [HttpGet("procurement/inventory")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryItem>>>> GetInventoryItems([FromQuery] bool lowStockOnly = false)
        {
            var query = _context.InventoryItems.AsQueryable();
            if (lowStockOnly)
                query = query.Where(item => item.QuantityOnHand <= item.ReorderLevel);

            var result = await query.OrderBy(item => item.Name).ToListAsync();
            return Ok(ApiResponse<IEnumerable<InventoryItem>>.Success("Inventory items retrieved successfully", result));
        }

        [HttpPost("procurement/inventory")]
        [HasPermission(Permissions.AdminAccess, Permissions.ApproveRequests, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<InventoryItem>>> CreateInventoryItem([FromBody] CreateInventoryItemDto dto)
        {
            if (await _context.InventoryItems.AnyAsync(item => item.Sku == dto.Sku))
                return BadRequest(ApiResponse<InventoryItem>.Failure("An inventory item with this SKU already exists"));

            var item = new InventoryItem
            {
                Id = Guid.NewGuid(),
                Sku = dto.Sku.Trim(),
                Name = dto.Name.Trim(),
                Category = dto.Category.Trim(),
                QuantityOnHand = dto.QuantityOnHand,
                ReorderLevel = dto.ReorderLevel,
                UnitCost = dto.UnitCost,
                Location = dto.Location.Trim(),
                Status = dto.Status.Trim()
            };

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInventoryItems), ApiResponse<InventoryItem>.Success("Inventory item created successfully", item));
        }

        [HttpPut("procurement/inventory/{id}")]
        [HasPermission(Permissions.AdminAccess, Permissions.ApproveRequests, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<InventoryItem>>> UpdateInventoryItem(Guid id, [FromBody] CreateInventoryItemDto dto)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
                return NotFound(ApiResponse<InventoryItem>.Failure("Inventory item not found"));

            if (await _context.InventoryItems.AnyAsync(existing => existing.Id != id && existing.Sku == dto.Sku))
                return BadRequest(ApiResponse<InventoryItem>.Failure("An inventory item with this SKU already exists"));

            item.Sku = dto.Sku.Trim();
            item.Name = dto.Name.Trim();
            item.Category = dto.Category.Trim();
            item.QuantityOnHand = dto.QuantityOnHand;
            item.ReorderLevel = dto.ReorderLevel;
            item.UnitCost = dto.UnitCost;
            item.Location = dto.Location.Trim();
            item.Status = dto.Status.Trim();
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<InventoryItem>.Success("Inventory item updated successfully", item));
        }

        [HttpGet("documents")]
        [HasPermission(Permissions.AdminAccess, Permissions.UploadDocuments, Permissions.ViewDepartmentDocuments, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DocumentRecord>>>> GetDocuments([FromQuery] string? category = null)
        {
            var query = _context.DocumentRecords.AsQueryable();
            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(document => document.Category == category);

            var result = await query.OrderByDescending(document => document.CreatedAt).ToListAsync();
            return Ok(ApiResponse<IEnumerable<DocumentRecord>>.Success("Documents retrieved successfully", result));
        }

        [HttpPost("documents")]
        [HasPermission(Permissions.AdminAccess, Permissions.UploadDocuments, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<DocumentRecord>>> CreateDocument([FromBody] CreateDocumentRecordDto dto)
        {
            var document = new DocumentRecord
            {
                Id = Guid.NewGuid(),
                Title = dto.Title.Trim(),
                Category = dto.Category.Trim(),
                OwnerDepartmentId = dto.OwnerDepartmentId,
                OwnerUserId = dto.OwnerUserId ?? GetCurrentUserId(),
                StorageUrl = dto.StorageUrl.Trim(),
                Confidentiality = dto.Confidentiality.Trim(),
                Status = dto.Status.Trim()
            };

            _context.DocumentRecords.Add(document);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDocuments), ApiResponse<DocumentRecord>.Success("Document registered successfully", document));
        }

        [HttpPatch("documents/{id}/status")]
        [HasPermission(Permissions.AdminAccess, Permissions.ApproveDocuments, Permissions.ArchiveDocuments, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<DocumentRecord>>> UpdateDocumentStatus(Guid id, [FromBody] UpdateOperationalStatusDto dto)
        {
            var document = await _context.DocumentRecords.FindAsync(id);
            if (document == null)
                return NotFound(ApiResponse<DocumentRecord>.Failure("Document not found"));

            document.Status = dto.Status.Trim();
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<DocumentRecord>.Success("Document status updated successfully", document));
        }

        [HttpGet("helpdesk/tickets")]
        [AllowAuthenticatedUsers]
        public async Task<ActionResult<ApiResponse<IEnumerable<HelpdeskTicket>>>> GetHelpdeskTickets([FromQuery] string? status = null)
        {
            var query = _context.HelpdeskTickets.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(ticket => ticket.Status == status);

            var result = await query.OrderByDescending(ticket => ticket.CreatedAt).ToListAsync();
            return Ok(ApiResponse<IEnumerable<HelpdeskTicket>>.Success("Helpdesk tickets retrieved successfully", result));
        }

        [HttpPost("helpdesk/tickets")]
        [AllowAuthenticatedUsers]
        public async Task<ActionResult<ApiResponse<HelpdeskTicket>>> CreateHelpdeskTicket([FromBody] CreateHelpdeskTicketDto dto)
        {
            var ticket = new HelpdeskTicket
            {
                Id = Guid.NewGuid(),
                TicketNumber = $"HD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                Category = dto.Category.Trim(),
                Priority = dto.Priority.Trim(),
                RequesterUserId = dto.RequesterUserId ?? GetCurrentUserId(),
                AssigneeUserId = dto.AssigneeUserId,
                DueAt = dto.DueAt,
                Status = "Open"
            };

            _context.HelpdeskTickets.Add(ticket);
            await _context.SaveChangesAsync();

            var actorId = GetCurrentUserId();
            var adminNotifications = await _notificationService.NotifyAdminsAsync(
                "New Helpdesk ticket",
                $"Ticket {ticket.TicketNumber} was created in {ticket.Category}.",
                "info",
                "HelpdeskTicket",
                ticket.Id);
            await BroadcastNotifications(adminNotifications);

            if (actorId.HasValue && actorId.Value != ticket.RequesterUserId && ticket.RequesterUserId.HasValue)
            {
                var requesterNotification = await _notificationService.NotifyUserAsync(
                    ticket.RequesterUserId.Value,
                    "Helpdesk ticket submitted",
                    $"Your ticket {ticket.TicketNumber} has been submitted successfully.",
                    "success",
                    "HelpdeskTicket",
                    ticket.Id);
                await BroadcastNotification(requesterNotification);
            }

            return CreatedAtAction(nameof(GetHelpdeskTickets), ApiResponse<HelpdeskTicket>.Success("Helpdesk ticket created successfully", ticket));
        }

        [HttpPatch("helpdesk/tickets/{id}/status")]
        [AllowAuthenticatedUsers]
        public async Task<ActionResult<ApiResponse<HelpdeskTicket>>> UpdateHelpdeskTicketStatus(Guid id, [FromBody] UpdateOperationalStatusDto dto)
        {
            var ticket = await _context.HelpdeskTickets.FindAsync(id);
            if (ticket == null)
                return NotFound(ApiResponse<HelpdeskTicket>.Failure("Helpdesk ticket not found"));

            ticket.Status = dto.Status.Trim();
            ticket.UpdatedAt = DateTime.UtcNow;
            if (ticket.Status == "Closed")
                ticket.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var actorId = GetCurrentUserId();
            var adminNotifications = await _notificationService.NotifyAdminsAsync(
                "Helpdesk ticket updated",
                $"Ticket {ticket.TicketNumber} status changed to {ticket.Status}.",
                "info",
                "HelpdeskTicket",
                ticket.Id);
            await BroadcastNotifications(adminNotifications);

            if (ticket.RequesterUserId.HasValue && (!actorId.HasValue || actorId.Value != ticket.RequesterUserId.Value))
            {
                var requesterNotification = await _notificationService.NotifyUserAsync(
                    ticket.RequesterUserId.Value,
                    "Helpdesk ticket updated",
                    $"Your ticket {ticket.TicketNumber} is now {ticket.Status}.",
                    "info",
                    "HelpdeskTicket",
                    ticket.Id);
                await BroadcastNotification(requesterNotification);
            }

            return Ok(ApiResponse<HelpdeskTicket>.Success("Helpdesk ticket status updated successfully", ticket));
        }

        [HttpGet("helpdesk/tickets/{id:guid}")]
        [AllowAuthenticatedUsers]
        public async Task<ActionResult<ApiResponse<HelpdeskTicketDetailDto>>> GetHelpdeskTicketDetail(Guid id)
        {
            var ticket = await _context.HelpdeskTickets.FindAsync(id);
            if (ticket == null)
                return NotFound(ApiResponse<HelpdeskTicketDetailDto>.Failure("Helpdesk ticket not found"));

            var requester = ticket.RequesterUserId.HasValue ? await _context.Users.FindAsync(ticket.RequesterUserId.Value) : null;
            var assignee = ticket.AssigneeUserId.HasValue ? await _context.Users.FindAsync(ticket.AssigneeUserId.Value) : null;

            var comments = await _context.HelpdeskTicketComments
                .Where(c => c.TicketId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var commentDtos = new List<HelpdeskTicketCommentDto>();
            foreach (var c in comments)
            {
                var user = await _context.Users.FindAsync(c.UserId);
                commentDtos.Add(new HelpdeskTicketCommentDto
                {
                    Id = c.Id,
                    TicketId = c.TicketId,
                    UserId = c.UserId,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
                    Comment = c.Comment,
                    IsInternal = c.IsInternal,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                });
            }

            var detail = new HelpdeskTicketDetailDto
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Description = ticket.Description,
                Category = ticket.Category,
                Priority = ticket.Priority,
                Status = ticket.Status,
                RequesterUserId = ticket.RequesterUserId,
                RequesterName = requester != null ? $"{requester.FirstName} {requester.LastName}".Trim() : "Unknown",
                AssigneeUserId = ticket.AssigneeUserId,
                AssigneeName = assignee != null ? $"{assignee.FirstName} {assignee.LastName}".Trim() : "Unassigned",
                DueAt = ticket.DueAt,
                ClosedAt = ticket.ClosedAt,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                Comments = commentDtos
            };

            return Ok(ApiResponse<HelpdeskTicketDetailDto>.Success("Helpdesk ticket detail retrieved successfully", detail));
        }

        [HttpPost("helpdesk/tickets/{id:guid}/comments")]
        [AllowAuthenticatedUsers]
        public async Task<ActionResult<ApiResponse<HelpdeskTicketCommentDto>>> AddHelpdeskTicketComment(Guid id, [FromBody] CreateHelpdeskCommentDto dto)
        {
            var ticket = await _context.HelpdeskTickets.FindAsync(id);
            if (ticket == null)
                return NotFound(ApiResponse<HelpdeskTicketCommentDto>.Failure("Helpdesk ticket not found"));

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<HelpdeskTicketCommentDto>.Failure("User identity is missing."));

            var comment = new HelpdeskTicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = id,
                UserId = userId.Value,
                Comment = dto.Comment?.Trim() ?? string.Empty,
                IsInternal = dto.IsInternal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.HelpdeskTicketComments.Add(comment);
            await _context.SaveChangesAsync();

            var adminNotifications = await _notificationService.NotifyAdminsAsync(
                "Helpdesk ticket comment added",
                $"A new comment was added to ticket {ticket.TicketNumber}.",
                "info",
                "HelpdeskTicket",
                ticket.Id);
            await BroadcastNotifications(adminNotifications);

            if (ticket.RequesterUserId.HasValue && ticket.RequesterUserId.Value != userId.Value)
            {
                var requesterNotification = await _notificationService.NotifyUserAsync(
                    ticket.RequesterUserId.Value,
                    "Helpdesk ticket comment",
                    $"A new comment was added to your ticket {ticket.TicketNumber}.",
                    "info",
                    "HelpdeskTicket",
                    ticket.Id);
                await BroadcastNotification(requesterNotification);
            }

            if (ticket.AssigneeUserId.HasValue && ticket.AssigneeUserId.Value != userId.Value)
            {
                var assigneeNotification = await _notificationService.NotifyUserAsync(
                    ticket.AssigneeUserId.Value,
                    "Helpdesk ticket comment",
                    $"A new comment was added to ticket {ticket.TicketNumber}.",
                    "info",
                    "HelpdeskTicket",
                    ticket.Id);
                await BroadcastNotification(assigneeNotification);
            }

            var user = await _context.Users.FindAsync(userId.Value);
            var commentDto = new HelpdeskTicketCommentDto
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                UserId = comment.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
                Comment = comment.Comment,
                IsInternal = comment.IsInternal,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };

            return Ok(ApiResponse<HelpdeskTicketCommentDto>.Success("Comment added successfully", commentDto));
        }

        [HttpGet("facilities/assets")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FacilityAsset>>>> GetFacilityAssets([FromQuery] string? status = null)
        {
            var query = _context.FacilityAssets.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(asset => asset.Status == status);

            var result = await query.OrderBy(asset => asset.AssetTag).ToListAsync();
            return Ok(ApiResponse<IEnumerable<FacilityAsset>>.Success("Facility assets retrieved successfully", result));
        }

        [HttpPost("facilities/assets")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManageDepartmentStructure, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<FacilityAsset>>> CreateFacilityAsset([FromBody] CreateFacilityAssetDto dto)
        {
            if (await _context.FacilityAssets.AnyAsync(asset => asset.AssetTag == dto.AssetTag))
                return BadRequest(ApiResponse<FacilityAsset>.Failure("An asset with this tag already exists"));

            var asset = new FacilityAsset
            {
                Id = Guid.NewGuid(),
                AssetTag = dto.AssetTag.Trim(),
                Name = dto.Name.Trim(),
                Category = dto.Category.Trim(),
                Location = dto.Location.Trim(),
                CustodianEmployeeId = dto.CustodianEmployeeId,
                Condition = dto.Condition.Trim(),
                Status = dto.Status.Trim(),
                PurchaseDate = dto.PurchaseDate,
                PurchaseCost = dto.PurchaseCost,
                WarrantyExpiryDate = dto.WarrantyExpiryDate
            };

            _context.FacilityAssets.Add(asset);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFacilityAssets), ApiResponse<FacilityAsset>.Success("Facility asset created successfully", asset));
        }

        [HttpPut("facilities/assets/{id}")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManageDepartmentStructure, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<FacilityAsset>>> UpdateFacilityAsset(Guid id, [FromBody] CreateFacilityAssetDto dto)
        {
            var asset = await _context.FacilityAssets.FindAsync(id);
            if (asset == null)
                return NotFound(ApiResponse<FacilityAsset>.Failure("Facility asset not found"));

            if (await _context.FacilityAssets.AnyAsync(existing => existing.Id != id && existing.AssetTag == dto.AssetTag))
                return BadRequest(ApiResponse<FacilityAsset>.Failure("An asset with this tag already exists"));

            asset.AssetTag = dto.AssetTag.Trim();
            asset.Name = dto.Name.Trim();
            asset.Category = dto.Category.Trim();
            asset.Location = dto.Location.Trim();
            asset.CustodianEmployeeId = dto.CustodianEmployeeId;
            asset.Condition = dto.Condition.Trim();
            asset.Status = dto.Status.Trim();
            asset.PurchaseDate = dto.PurchaseDate;
            asset.PurchaseCost = dto.PurchaseCost;
            asset.WarrantyExpiryDate = dto.WarrantyExpiryDate;
            asset.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<FacilityAsset>.Success("Facility asset updated successfully", asset));
        }

        [HttpGet("registry/integrations")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RegistryIntegrationRecord>>>> GetRegistryIntegrations()
        {
            var result = await _context.RegistryIntegrationRecords
                .OrderBy(integration => integration.SystemName)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<RegistryIntegrationRecord>>.Success("Registry integrations retrieved successfully", result));
        }

        [HttpPost("registry/integrations")]
        [HasPermission(Permissions.AdminAccess, Permissions.ConfigureSystemSettings, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<RegistryIntegrationRecord>>> CreateRegistryIntegration([FromBody] CreateRegistryIntegrationDto dto)
        {
            var integration = new RegistryIntegrationRecord
            {
                Id = Guid.NewGuid(),
                SystemName = dto.SystemName.Trim(),
                IntegrationType = dto.IntegrationType.Trim(),
                ExternalUrl = dto.ExternalUrl.Trim(),
                Status = dto.Status.Trim(),
                LastSyncedAt = dto.LastSyncedAt,
                Notes = dto.Notes.Trim()
            };

            _context.RegistryIntegrationRecords.Add(integration);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRegistryIntegrations), ApiResponse<RegistryIntegrationRecord>.Success("Registry integration created successfully", integration));
        }

        [HttpPatch("registry/integrations/{id}/status")]
        [HasPermission(Permissions.AdminAccess, Permissions.ConfigureSystemSettings, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<RegistryIntegrationRecord>>> UpdateRegistryIntegrationStatus(Guid id, [FromBody] UpdateOperationalStatusDto dto)
        {
            var integration = await _context.RegistryIntegrationRecords.FindAsync(id);
            if (integration == null)
                return NotFound(ApiResponse<RegistryIntegrationRecord>.Failure("Registry integration not found"));

            integration.Status = dto.Status.Trim();
            integration.LastSyncedAt = integration.Status == "Synced" ? DateTime.UtcNow : integration.LastSyncedAt;
            integration.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<RegistryIntegrationRecord>.Success("Registry integration status updated successfully", integration));
        }

        private async Task BroadcastNotification(NotificationDto notification)
        {
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", notification);
        }

        private async Task BroadcastNotifications(IEnumerable<NotificationDto> notifications)
        {
            foreach (var notification in notifications)
            {
                await BroadcastNotification(notification);
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }
    }
}
