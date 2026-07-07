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
                Step = dto.Step,
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
            salaryStructure.Step = dto.Step;
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

            // Fetch active employments with salary structures
            var employments = await _context.EmploymentDetails
                .Where(e => e.EmploymentStatus == "Active" && e.SalaryStructureId != Guid.Empty)
                .ToListAsync();

            var employeeIds = employments.Select(e => e.EmployeeId).ToList();
            var employees = await _context.EmployeeDetails
                .Where(e => employeeIds.Contains(e.UserId))
                .ToDictionaryAsync(e => e.UserId);

            var salaryStructureIds = employments.Select(e => e.SalaryStructureId).Distinct().ToList();
            var salaryStructures = await _context.SalaryStructures
                .Where(s => salaryStructureIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id);

            var adjustments = await _context.PayrollAdjustments
                .Where(a => !a.IsProcessed && a.ApplicableMonth.Month == dto.PeriodStart.Month && a.ApplicableMonth.Year == dto.PeriodStart.Year)
                .ToListAsync();

            int actualEmployeeCount = 0;
            decimal actualGrossPayTotal = 0;
            decimal actualNetPayTotal = 0;

            foreach (var employment in employments)
            {
                if (!salaryStructures.TryGetValue(employment.SalaryStructureId, out var structure))
                    continue;

                if (!employees.TryGetValue(employment.EmployeeId, out var employee))
                    continue;

                var empAdjustments = adjustments.Where(a => a.EmployeeId == employee.UserId).ToList();
                decimal bonuses = empAdjustments.Where(a => a.Type == "Bonus").Sum(a => a.Amount);
                decimal deductions = empAdjustments.Where(a => a.Type == "Deduction").Sum(a => a.Amount);

                decimal allowances = structure.HousingAllowance + structure.TransportAllowance + structure.OtherAllowance;
                decimal tax = (structure.BasePay + allowances) * (structure.TaxRatePercent / 100);
                decimal pension = (structure.BasePay + allowances) * (structure.PensionRatePercent / 100);

                decimal gross = structure.BasePay + allowances + bonuses;
                decimal net = gross - tax - pension - deductions;

                var payslip = new Payslip
                {
                    Id = Guid.NewGuid(),
                    PayrollRunId = payrollRun.Id,
                    EmployeeId = employee.UserId,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    GradeLevel = structure.GradeLevel,
                    Step = structure.Step,
                    BasePay = structure.BasePay,
                    AllowancesTotal = allowances,
                    BonusesTotal = bonuses,
                    DeductionsTotal = deductions,
                    TaxAmount = tax,
                    PensionAmount = pension,
                    GrossPay = gross,
                    NetPay = net
                };

                _context.Payslips.Add(payslip);

                foreach (var adj in empAdjustments)
                {
                    adj.IsProcessed = true;
                    adj.PayrollRunId = payrollRun.Id;
                    adj.UpdatedAt = DateTime.UtcNow;
                }

                actualEmployeeCount++;
                actualGrossPayTotal += gross;
                actualNetPayTotal += net;
            }

            // Update run totals with computed values
            payrollRun.EmployeeCount = actualEmployeeCount;
            payrollRun.GrossPayTotal = actualGrossPayTotal;
            payrollRun.NetPayTotal = actualNetPayTotal;

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

        [HttpGet("finance/payroll-runs/{runId}/payslips")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManagePayroll, Permissions.ProcessPayroll, Permissions.ViewPayslips, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Payslip>>>> GetPayslips(Guid runId)
        {
            var result = await _context.Payslips
                .Where(p => p.PayrollRunId == runId)
                .OrderBy(p => p.EmployeeName)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<Payslip>>.Success("Payslips retrieved successfully", result));
        }

        [HttpGet("finance/payroll/employee/{employeeId}/payslips")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManagePayroll, Permissions.ViewPayslips, Permissions.ViewOwnPayslip, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Payslip>>>> GetEmployeePayslips(Guid employeeId)
        {
            var result = await _context.Payslips
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.CreatedAt) // assuming newest first
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<Payslip>>.Success("Employee payslips retrieved successfully", result));
        }

        [HttpGet("finance/adjustments")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManagePayroll, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PayrollAdjustment>>>> GetPayrollAdjustments()
        {
            var result = await _context.PayrollAdjustments
                .OrderByDescending(a => a.ApplicableMonth)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<PayrollAdjustment>>.Success("Payroll adjustments retrieved successfully", result));
        }

        [HttpPost("finance/adjustments")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManagePayroll, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<PayrollAdjustment>>> CreatePayrollAdjustment([FromBody] CreatePayrollAdjustmentDto dto)
        {
            var employee = await _context.EmployeeDetails.FirstOrDefaultAsync(e => e.UserId == dto.EmployeeId);
            if (employee == null)
                return NotFound(ApiResponse<PayrollAdjustment>.Failure("Employee not found"));

            var adjustment = new PayrollAdjustment
            {
                Id = Guid.NewGuid(),
                EmployeeId = dto.EmployeeId,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Type = dto.Type.Trim(),
                Description = dto.Description.Trim(),
                Amount = dto.Amount,
                ApplicableMonth = dto.ApplicableMonth,
                IsProcessed = false
            };

            _context.PayrollAdjustments.Add(adjustment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayrollAdjustments), ApiResponse<PayrollAdjustment>.Success("Payroll adjustment created successfully", adjustment));
        }

        // ==========================================
        // PROCUREMENT & INVENTORY MODULE
        // ==========================================

        [HttpGet("procurement/vendors")]
        [HasPermission(Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VendorDto>>>> GetVendors()
        {
            var vendors = await _context.Vendors.OrderBy(v => v.Name).ToListAsync();
            var dtos = vendors.Select(v => new VendorDto
            {
                Id = v.Id,
                Name = v.Name,
                ContactEmail = v.ContactEmail,
                ContactPhone = v.ContactPhone,
                Address = v.Address,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            });
            return Ok(ApiResponse<IEnumerable<VendorDto>>.Success("Vendors retrieved successfully", dtos));
        }

        [HttpPost("procurement/vendors")]
        [HasPermission(Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<VendorDto>>> CreateVendor([FromBody] CreateVendorDto dto)
        {
            var vendor = new Vendor
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                ContactEmail = dto.ContactEmail.Trim(),
                ContactPhone = dto.ContactPhone.Trim(),
                Address = dto.Address.Trim(),
                Status = dto.Status.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            var resultDto = new VendorDto
            {
                Id = vendor.Id,
                Name = vendor.Name,
                ContactEmail = vendor.ContactEmail,
                ContactPhone = vendor.ContactPhone,
                Address = vendor.Address,
                Status = vendor.Status,
                CreatedAt = vendor.CreatedAt,
                UpdatedAt = vendor.UpdatedAt
            };

            return CreatedAtAction(nameof(GetVendors), ApiResponse<VendorDto>.Success("Vendor created successfully", resultDto));
        }

        [HttpGet("procurement/purchase-orders")]
        [HasPermission(Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PurchaseOrderDto>>>> GetPurchaseOrders()
        {
            var pos = await _context.PurchaseOrders
                .Include(po => po.LineItems)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();

            var vendorIds = pos.Select(po => po.VendorId).Distinct().ToList();
            var requestIds = pos.Where(po => po.ProcurementRequestId.HasValue).Select(po => po.ProcurementRequestId.Value).Distinct().ToList();

            var vendors = await _context.Vendors.Where(v => vendorIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id);
            var requests = await _context.ProcurementRequests.Where(r => requestIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id);

            var dtos = pos.Select(po => new PurchaseOrderDto
            {
                Id = po.Id,
                PoNumber = po.PoNumber,
                ProcurementRequestId = po.ProcurementRequestId,
                ProcurementRequestTitle = po.ProcurementRequestId.HasValue && requests.TryGetValue(po.ProcurementRequestId.Value, out var req) ? req.Title : string.Empty,
                VendorId = po.VendorId,
                VendorName = vendors.TryGetValue(po.VendorId, out var vendor) ? vendor.Name : string.Empty,
                TotalAmount = po.TotalAmount,
                Status = po.Status,
                CreatedAt = po.CreatedAt,
                UpdatedAt = po.UpdatedAt,
                LineItems = po.LineItems.Select(li => new PurchaseOrderLineItemDto
                {
                    Id = li.Id,
                    PurchaseOrderId = li.PurchaseOrderId,
                    InventoryItemId = li.InventoryItemId,
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    TotalPrice = li.TotalPrice
                }).ToList()
            });

            return Ok(ApiResponse<IEnumerable<PurchaseOrderDto>>.Success("Purchase orders retrieved successfully", dtos));
        }

        [HttpPost("procurement/purchase-orders")]
        [HasPermission(Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto)
        {
            if (dto.LineItems == null || !dto.LineItems.Any())
                return BadRequest(ApiResponse<PurchaseOrderDto>.Failure("A purchase order must have at least one line item."));

            var vendor = await _context.Vendors.FindAsync(dto.VendorId);
            if (vendor == null)
                return NotFound(ApiResponse<PurchaseOrderDto>.Failure("Vendor not found"));

            var po = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                PoNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                ProcurementRequestId = dto.ProcurementRequestId,
                VendorId = dto.VendorId,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            decimal totalAmount = 0;
            foreach (var itemDto in dto.LineItems)
            {
                var lineItem = new PurchaseOrderLineItem
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = po.Id,
                    InventoryItemId = itemDto.InventoryItemId,
                    Description = itemDto.Description.Trim(),
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.Quantity * itemDto.UnitPrice
                };
                totalAmount += lineItem.TotalPrice;
                po.LineItems.Add(lineItem);
            }

            po.TotalAmount = totalAmount;
            _context.PurchaseOrders.Add(po);

            if (dto.ProcurementRequestId.HasValue)
            {
                var request = await _context.ProcurementRequests.FindAsync(dto.ProcurementRequestId.Value);
                if (request != null && request.Status != "Approved")
                {
                    // Optionally auto-approve or simply let it be, depending on workflow.
                }
            }

            await _context.SaveChangesAsync();

            var resultDto = new PurchaseOrderDto
            {
                Id = po.Id,
                PoNumber = po.PoNumber,
                ProcurementRequestId = po.ProcurementRequestId,
                VendorId = po.VendorId,
                VendorName = vendor.Name,
                TotalAmount = po.TotalAmount,
                Status = po.Status,
                CreatedAt = po.CreatedAt,
                UpdatedAt = po.UpdatedAt,
                LineItems = po.LineItems.Select(li => new PurchaseOrderLineItemDto
                {
                    Id = li.Id,
                    PurchaseOrderId = li.PurchaseOrderId,
                    InventoryItemId = li.InventoryItemId,
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    TotalPrice = li.TotalPrice
                }).ToList()
            };

            return CreatedAtAction(nameof(GetPurchaseOrders), ApiResponse<PurchaseOrderDto>.Success("Purchase order created successfully", resultDto));
        }

        [HttpPost("procurement/purchase-orders/{id}/receive")]
        [HasPermission(Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<GoodsReceivedNoteDto>>> ReceiveGoods(Guid id, [FromBody] ReceiveGoodsDto dto)
        {
            var po = await _context.PurchaseOrders.Include(p => p.LineItems).FirstOrDefaultAsync(p => p.Id == id);
            if (po == null)
                return NotFound(ApiResponse<GoodsReceivedNoteDto>.Failure("Purchase order not found"));

            if (po.Status == "Delivered")
                return BadRequest(ApiResponse<GoodsReceivedNoteDto>.Failure("Purchase order is already delivered"));

            po.Status = "Delivered";
            po.UpdatedAt = DateTime.UtcNow;

            var grn = new GoodsReceivedNote
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = po.Id,
                ReceivedDate = DateTime.UtcNow,
                ReceivedByUserId = dto.ReceivedByUserId ?? GetCurrentUserId(),
                Remarks = dto.Remarks.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.GoodsReceivedNotes.Add(grn);

            // Process inventory logic for line items that are linked to Inventory Items
            foreach (var lineItem in po.LineItems)
            {
                if (lineItem.InventoryItemId.HasValue)
                {
                    var inventoryItem = await _context.InventoryItems.FindAsync(lineItem.InventoryItemId.Value);
                    if (inventoryItem != null)
                    {
                        inventoryItem.QuantityOnHand += lineItem.Quantity;
                        inventoryItem.UpdatedAt = DateTime.UtcNow;

                        var transaction = new InventoryTransaction
                        {
                            Id = Guid.NewGuid(),
                            InventoryItemId = inventoryItem.Id,
                            TransactionType = "In",
                            Quantity = lineItem.Quantity,
                            ReferenceId = grn.Id,
                            Remarks = $"Received via PO {po.PoNumber}",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.InventoryTransactions.Add(transaction);
                    }
                }
            }

            await _context.SaveChangesAsync();

            var resultDto = new GoodsReceivedNoteDto
            {
                Id = grn.Id,
                PurchaseOrderId = grn.PurchaseOrderId,
                PoNumber = po.PoNumber,
                ReceivedDate = grn.ReceivedDate,
                ReceivedByUserId = grn.ReceivedByUserId,
                Remarks = grn.Remarks,
                CreatedAt = grn.CreatedAt
            };

            return Ok(ApiResponse<GoodsReceivedNoteDto>.Success("Goods received successfully", resultDto));
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

            var transaction = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                InventoryItemId = item.Id,
                TransactionType = "Initial",
                Quantity = item.QuantityOnHand,
                Remarks = "Initial stock entry",
                CreatedAt = DateTime.UtcNow
            };
            _context.InventoryTransactions.Add(transaction);

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

            if (item.QuantityOnHand != dto.QuantityOnHand)
            {
                var difference = dto.QuantityOnHand - item.QuantityOnHand;
                var transaction = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = item.Id,
                    TransactionType = "Adjustment",
                    Quantity = difference,
                    Remarks = "Manual adjustment via UI",
                    CreatedAt = DateTime.UtcNow
                };
                _context.InventoryTransactions.Add(transaction);
            }

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

        [HttpGet("procurement/inventory/{id}/transactions")]
        [HasPermission(Permissions.AdminAccess, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryTransactionDto>>>> GetInventoryTransactions(Guid id)
        {
            var transactions = await _context.InventoryTransactions
                .Where(t => t.InventoryItemId == id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var item = await _context.InventoryItems.FindAsync(id);

            var dtos = transactions.Select(t => new InventoryTransactionDto
            {
                Id = t.Id,
                InventoryItemId = t.InventoryItemId,
                InventoryItemName = item?.Name ?? string.Empty,
                TransactionType = t.TransactionType,
                Quantity = t.Quantity,
                ReferenceId = t.ReferenceId,
                Remarks = t.Remarks,
                CreatedAt = t.CreatedAt
            });

            return Ok(ApiResponse<IEnumerable<InventoryTransactionDto>>.Success("Inventory transactions retrieved successfully", dtos));
        }

        [HttpPost("documents/upload")]
        [HasPermission(Permissions.AdminAccess, Permissions.UploadDocuments, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<string>>> UploadDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.Failure("No file was uploaded."));
            }

            try
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "documents");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"uploads/documents/{fileName}";
                return Ok(ApiResponse<string>.Success("File uploaded successfully", relativePath));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failure("File upload failed", ex.Message));
            }
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
        public async Task<ActionResult<ApiResponse<IEnumerable<HelpdeskTicketDto>>>> GetHelpdeskTickets([FromQuery] string? status = null)
        {
            var query = _context.HelpdeskTickets.Include(t => t.FacilityAsset).AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(ticket => ticket.Status == status);

            var tickets = await query.OrderByDescending(ticket => ticket.CreatedAt).ToListAsync();
            
            var result = tickets.Select(ticket => new HelpdeskTicketDto
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Description = ticket.Description,
                Category = ticket.Category,
                Priority = ticket.Priority,
                Status = ticket.Status,
                RequesterUserId = ticket.RequesterUserId,
                AssigneeUserId = ticket.AssigneeUserId,
                DueAt = ticket.DueAt,
                ClosedAt = ticket.ClosedAt,
                FacilityAssetId = ticket.FacilityAssetId,
                FacilityAssetName = ticket.FacilityAsset?.Name ?? string.Empty,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt
            });

            return Ok(ApiResponse<IEnumerable<HelpdeskTicketDto>>.Success("Helpdesk tickets retrieved successfully", result));
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
                FacilityAssetId = dto.FacilityAssetId,
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
        
        [HttpPost("helpdesk/tickets/{id}/assign")]
        public async Task<ActionResult<ApiResponse<HelpdeskTicket>>> AssignHelpdeskTicket(Guid id, [FromBody] AssignHelpdeskTicketDto dto)
        {
            var ticket = await _context.HelpdeskTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound(ApiResponse<HelpdeskTicket>.Failure("Helpdesk ticket not found"));
            }

            ticket.AssigneeUserId = dto.AssigneeUserId;
            if (ticket.Status == "Open" || ticket.Status == "New")
            {
                ticket.Status = "Assigned";
            }

            if (!string.IsNullOrEmpty(dto.Notes))
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdString, out Guid parsedUserId))
                {
                    _context.HelpdeskTicketComments.Add(new HelpdeskTicketComment
                    {
                        TicketId = ticket.Id,
                        UserId = parsedUserId,
                        Comment = $"Ticket assigned to {dto.AssigneeUserId}. Notes: {dto.Notes}",
                        IsInternal = true
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<HelpdeskTicket>.Success("Helpdesk ticket assigned successfully", ticket));
        }

        [HttpPatch("helpdesk/tickets/{id}/status")]
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
            var ticket = await _context.HelpdeskTickets.Include(t => t.FacilityAsset).FirstOrDefaultAsync(t => t.Id == id);
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
                FacilityAssetId = ticket.FacilityAssetId,
                FacilityAssetName = ticket.FacilityAsset != null ? ticket.FacilityAsset.Name : string.Empty,
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

private decimal CalculateCurrentValue(FacilityAsset asset)
        {
            if (asset.PurchaseDate == null || asset.ExpectedLifeSpanMonths <= 0) return asset.PurchaseCost;
            
            var ageInMonths = (DateTime.UtcNow.Year - asset.PurchaseDate.Value.Year) * 12 + DateTime.UtcNow.Month - asset.PurchaseDate.Value.Month;
            if (ageInMonths < 0) return asset.PurchaseCost;
            if (ageInMonths >= asset.ExpectedLifeSpanMonths) return asset.SalvageValue;
            
            var depreciationPerMonth = (asset.PurchaseCost - asset.SalvageValue) / asset.ExpectedLifeSpanMonths;
            var currentValue = asset.PurchaseCost - (depreciationPerMonth * ageInMonths);
            return Math.Max(asset.SalvageValue, currentValue);
        }

        [HttpGet("facilities/assets")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FacilityAssetDto>>>> GetFacilityAssets([FromQuery] string? status = null)
        {
            var query = _context.FacilityAssets.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(asset => asset.Status == status);

            var assets = await query.OrderBy(asset => asset.AssetTag).ToListAsync();
            var dtos = assets.Select(a => new FacilityAssetDto
            {
                Id = a.Id,
                AssetTag = a.AssetTag,
                Name = a.Name,
                Category = a.Category,
                Location = a.Location,
                CustodianEmployeeId = a.CustodianEmployeeId,
                Condition = a.Condition,
                Status = a.Status,
                PurchaseDate = a.PurchaseDate,
                PurchaseCost = a.PurchaseCost,
                ExpectedLifeSpanMonths = a.ExpectedLifeSpanMonths,
                SalvageValue = a.SalvageValue,
                CurrentValue = CalculateCurrentValue(a),
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });
            return Ok(ApiResponse<IEnumerable<FacilityAssetDto>>.Success("Facility assets retrieved successfully", dtos));
        }

        [HttpPost("facilities/assets")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManageDepartmentStructure, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<FacilityAssetDto>>> CreateFacilityAsset([FromBody] CreateFacilityAssetDto dto)
        {
            if (await _context.FacilityAssets.AnyAsync(asset => asset.AssetTag == dto.AssetTag))
                return BadRequest(ApiResponse<FacilityAssetDto>.Failure("An asset with this tag already exists"));

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
                ExpectedLifeSpanMonths = dto.ExpectedLifeSpanMonths,
                SalvageValue = dto.SalvageValue,
                WarrantyExpiryDate = dto.WarrantyExpiryDate
            };

            _context.FacilityAssets.Add(asset);
            await _context.SaveChangesAsync();

            var resultDto = new FacilityAssetDto
            {
                Id = asset.Id,
                AssetTag = asset.AssetTag,
                Name = asset.Name,
                Category = asset.Category,
                Location = asset.Location,
                CustodianEmployeeId = asset.CustodianEmployeeId,
                Condition = asset.Condition,
                Status = asset.Status,
                PurchaseDate = asset.PurchaseDate,
                PurchaseCost = asset.PurchaseCost,
                ExpectedLifeSpanMonths = asset.ExpectedLifeSpanMonths,
                SalvageValue = asset.SalvageValue,
                CurrentValue = CalculateCurrentValue(asset),
                WarrantyExpiryDate = asset.WarrantyExpiryDate,
                CreatedAt = asset.CreatedAt,
                UpdatedAt = asset.UpdatedAt
            };

            return CreatedAtAction(nameof(GetFacilityAssets), ApiResponse<FacilityAssetDto>.Success("Facility asset created successfully", resultDto));
        }

        [HttpPut("facilities/assets/{id}")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManageDepartmentStructure, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<FacilityAssetDto>>> UpdateFacilityAsset(Guid id, [FromBody] CreateFacilityAssetDto dto)
        {
            var asset = await _context.FacilityAssets.FindAsync(id);
            if (asset == null)
                return NotFound(ApiResponse<FacilityAssetDto>.Failure("Facility asset not found"));

            if (await _context.FacilityAssets.AnyAsync(existing => existing.Id != id && existing.AssetTag == dto.AssetTag))
                return BadRequest(ApiResponse<FacilityAssetDto>.Failure("An asset with this tag already exists"));

            asset.AssetTag = dto.AssetTag.Trim();
            asset.Name = dto.Name.Trim();
            asset.Category = dto.Category.Trim();
            asset.Location = dto.Location.Trim();
            asset.CustodianEmployeeId = dto.CustodianEmployeeId;
            asset.Condition = dto.Condition.Trim();
            asset.Status = dto.Status.Trim();
            asset.PurchaseDate = dto.PurchaseDate;
            asset.PurchaseCost = dto.PurchaseCost;
            asset.ExpectedLifeSpanMonths = dto.ExpectedLifeSpanMonths;
            asset.SalvageValue = dto.SalvageValue;
            asset.WarrantyExpiryDate = dto.WarrantyExpiryDate;
            asset.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            var resultDto = new FacilityAssetDto
            {
                Id = asset.Id,
                AssetTag = asset.AssetTag,
                Name = asset.Name,
                Category = asset.Category,
                Location = asset.Location,
                CustodianEmployeeId = asset.CustodianEmployeeId,
                Condition = asset.Condition,
                Status = asset.Status,
                PurchaseDate = asset.PurchaseDate,
                PurchaseCost = asset.PurchaseCost,
                ExpectedLifeSpanMonths = asset.ExpectedLifeSpanMonths,
                SalvageValue = asset.SalvageValue,
                CurrentValue = CalculateCurrentValue(asset),
                WarrantyExpiryDate = asset.WarrantyExpiryDate,
                CreatedAt = asset.CreatedAt,
                UpdatedAt = asset.UpdatedAt
            };
            
            return Ok(ApiResponse<FacilityAssetDto>.Success("Facility asset updated successfully", resultDto));
        }

        [HttpPost("facilities/assets/{id}/assign")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManageDepartmentStructure, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<AssetAssignmentDto>>> AssignAsset(Guid id, [FromBody] CreateAssetAssignmentDto dto)
        {
            var asset = await _context.FacilityAssets.FindAsync(id);
            if (asset == null) return NotFound(ApiResponse<AssetAssignmentDto>.Failure("Asset not found"));

            // Check if already assigned and not returned
            var activeAssignment = await _context.AssetAssignments.FirstOrDefaultAsync(a => a.AssetId == id && a.ReturnedAt == null);
            if (activeAssignment != null)
                return BadRequest(ApiResponse<AssetAssignmentDto>.Failure("Asset is already assigned and has not been returned."));

            var assignment = new AssetAssignment
            {
                Id = Guid.NewGuid(),
                AssetId = id,
                EmployeeId = dto.EmployeeId,
                ConditionAtAssignment = dto.ConditionAtAssignment,
                Notes = dto.Notes
            };

            asset.CustodianEmployeeId = dto.EmployeeId;
            asset.Status = "Assigned";
            asset.Condition = dto.ConditionAtAssignment;

            _context.AssetAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            var result = new AssetAssignmentDto
            {
                Id = assignment.Id,
                AssetId = assignment.AssetId,
                EmployeeId = assignment.EmployeeId,
                AssignedAt = assignment.AssignedAt,
                ConditionAtAssignment = assignment.ConditionAtAssignment,
                Notes = assignment.Notes
            };

            return Ok(ApiResponse<AssetAssignmentDto>.Success("Asset assigned successfully", result));
        }

        [HttpPost("facilities/assets/{id}/return")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManageDepartmentStructure, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<AssetAssignmentDto>>> ReturnAsset(Guid id, [FromBody] ReturnAssetDto dto)
        {
            var asset = await _context.FacilityAssets.FindAsync(id);
            if (asset == null) return NotFound(ApiResponse<AssetAssignmentDto>.Failure("Asset not found"));

            var activeAssignment = await _context.AssetAssignments.FirstOrDefaultAsync(a => a.AssetId == id && a.ReturnedAt == null);
            if (activeAssignment == null)
                return BadRequest(ApiResponse<AssetAssignmentDto>.Failure("Asset is not currently assigned."));

            activeAssignment.ReturnedAt = DateTime.UtcNow;
            activeAssignment.ConditionAtReturn = dto.ConditionAtReturn;
            if (!string.IsNullOrEmpty(dto.Notes))
                activeAssignment.Notes += " | Return Notes: " + dto.Notes;

            asset.CustodianEmployeeId = null;
            asset.Status = "InUse"; // Or "Available"
            asset.Condition = dto.ConditionAtReturn;

            await _context.SaveChangesAsync();

            var result = new AssetAssignmentDto
            {
                Id = activeAssignment.Id,
                AssetId = activeAssignment.AssetId,
                EmployeeId = activeAssignment.EmployeeId,
                AssignedAt = activeAssignment.AssignedAt,
                ReturnedAt = activeAssignment.ReturnedAt,
                ConditionAtAssignment = activeAssignment.ConditionAtAssignment,
                ConditionAtReturn = activeAssignment.ConditionAtReturn,
                Notes = activeAssignment.Notes
            };

            return Ok(ApiResponse<AssetAssignmentDto>.Success("Asset returned successfully", result));
        }

        [HttpGet("facilities/assets/{id}/assignments")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AssetAssignmentDto>>>> GetAssetAssignments(Guid id)
        {
            var assignments = await _context.AssetAssignments
                .Where(a => a.AssetId == id)
                .OrderByDescending(a => a.AssignedAt)
                .Select(a => new AssetAssignmentDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    EmployeeId = a.EmployeeId,
                    AssignedAt = a.AssignedAt,
                    ReturnedAt = a.ReturnedAt,
                    ConditionAtAssignment = a.ConditionAtAssignment,
                    ConditionAtReturn = a.ConditionAtReturn,
                    Notes = a.Notes
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<AssetAssignmentDto>>.Success("Assignments retrieved", assignments));
        }

        [HttpPost("facilities/assets/{id}/maintenance")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManageDepartmentStructure, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<AssetMaintenanceRecordDto>>> AddMaintenanceRecord(Guid id, [FromBody] CreateAssetMaintenanceRecordDto dto)
        {
            var asset = await _context.FacilityAssets.FindAsync(id);
            if (asset == null) return NotFound(ApiResponse<AssetMaintenanceRecordDto>.Failure("Asset not found"));

            var record = new AssetMaintenanceRecord
            {
                Id = Guid.NewGuid(),
                AssetId = id,
                ScheduledDate = dto.ScheduledDate,
                CompletedDate = dto.CompletedDate,
                Cost = dto.Cost,
                Description = dto.Description,
                Status = dto.Status,
                PerformedBy = dto.PerformedBy
            };

            _context.AssetMaintenanceRecords.Add(record);
            
            if (dto.Status == "Completed")
            {
                asset.Status = "InUse"; // Assuming it comes back from repair
            }
            else if (dto.Status == "Scheduled" || dto.Status == "InProgress")
            {
                asset.Status = "UnderMaintenance";
            }
            
            await _context.SaveChangesAsync();

            var result = new AssetMaintenanceRecordDto
            {
                Id = record.Id,
                AssetId = record.AssetId,
                ScheduledDate = record.ScheduledDate,
                CompletedDate = record.CompletedDate,
                Cost = record.Cost,
                Description = record.Description,
                Status = record.Status,
                PerformedBy = record.PerformedBy
            };

            return Ok(ApiResponse<AssetMaintenanceRecordDto>.Success("Maintenance record added", result));
        }

        [HttpGet("facilities/assets/{id}/maintenance")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AssetMaintenanceRecordDto>>>> GetMaintenanceRecords(Guid id)
        {
            var records = await _context.AssetMaintenanceRecords
                .Where(a => a.AssetId == id)
                .OrderByDescending(a => a.ScheduledDate)
                .Select(a => new AssetMaintenanceRecordDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    ScheduledDate = a.ScheduledDate,
                    CompletedDate = a.CompletedDate,
                    Cost = a.Cost,
                    Description = a.Description,
                    Status = a.Status,
                    PerformedBy = a.PerformedBy
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<AssetMaintenanceRecordDto>>.Success("Maintenance records retrieved", records));
        }

        [HttpPost("facilities/assets/{id}/write-off")]
        [HasPermission(Permissions.AdminAccess, Permissions.ManageDepartmentStructure, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<FacilityAssetDto>>> WriteOffAsset(Guid id, [FromBody] string reason)
        {
            var asset = await _context.FacilityAssets.FindAsync(id);
            if (asset == null) return NotFound(ApiResponse<FacilityAssetDto>.Failure("Asset not found"));

            asset.Status = "WrittenOff";
            asset.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var resultDto = new FacilityAssetDto
            {
                Id = asset.Id,
                AssetTag = asset.AssetTag,
                Name = asset.Name,
                Category = asset.Category,
                Location = asset.Location,
                CustodianEmployeeId = asset.CustodianEmployeeId,
                Condition = asset.Condition,
                Status = asset.Status,
                PurchaseDate = asset.PurchaseDate,
                PurchaseCost = asset.PurchaseCost,
                ExpectedLifeSpanMonths = asset.ExpectedLifeSpanMonths,
                SalvageValue = asset.SalvageValue,
                CurrentValue = CalculateCurrentValue(asset),
                WarrantyExpiryDate = asset.WarrantyExpiryDate,
                CreatedAt = asset.CreatedAt,
                UpdatedAt = asset.UpdatedAt
            };

            return Ok(ApiResponse<FacilityAssetDto>.Success("Asset written off successfully", resultDto));
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


        [HttpPost("registry/integrations/{id}/sync")]
        [HasPermission(Permissions.AdminAccess, Permissions.ConfigureSystemSettings, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<string>>> TriggerRegistrySync(Guid id)
        {
            var integration = await _context.RegistryIntegrationRecords.FindAsync(id);
            if (integration == null)
                return NotFound(ApiResponse<string>.Failure("Registry integration not found"));

            // Enqueue the sync operation in the background using Hangfire
            Hangfire.BackgroundJob.Enqueue<IRegistrySyncService>(service => service.SyncIntegrationAsync(id));

            return Ok(ApiResponse<string>.Success("Registry sync job enqueued successfully", "Job Enqueued"));
        }

        [HttpPost("registry/integrations/{id}/ping")]
        [HasPermission(Permissions.AdminAccess, Permissions.ConfigureSystemSettings, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<string>>> PingRegistryIntegration(Guid id, [FromServices] IRegistrySyncService syncService)
        {
            var integration = await _context.RegistryIntegrationRecords.FindAsync(id);
            if (integration == null)
                return NotFound(ApiResponse<string>.Failure("Registry integration not found"));

            // Perform ping synchronously to give immediate feedback
            await syncService.SyncIntegrationAsync(id);

            return Ok(ApiResponse<string>.Success("Registry integration pinged successfully", "Ping Completed"));
        }

        [HttpGet("registry/integrations/{id}/logs")]
        [HasPermission(Permissions.AdminAccess, Permissions.ConfigureSystemSettings, Permissions.SuperAdminAccess)]
        public async Task<ActionResult<ApiResponse<IEnumerable<RegistrySyncLog>>>> GetRegistrySyncLogs(Guid id)
        {
            var logs = await _context.RegistrySyncLogs
                .Where(l => l.IntegrationId == id)
                .OrderByDescending(l => l.Timestamp)
                .Take(50)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<RegistrySyncLog>>.Success("Registry sync logs retrieved successfully", logs));
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
