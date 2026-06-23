using System;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;
namespace WUIAM.Services
{

    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IAuthService _authService;
        private readonly IAuthRepository _authRepository;
        private readonly WUIAMDbContext _context;
        public EmployeeService(WUIAMDbContext wUIAMDbContext, IEmployeeRepository employeeRepo, IAuthService authService, IAuthRepository authRepository)
        {
            _employeeRepo = employeeRepo;
            _authService = authService;
            _authRepository = authRepository;
            _context = wUIAMDbContext;
        }

        public async Task<EmployeeDetails?> GetEmployeeProfileAsync(Guid employeeId)
            => await _employeeRepo.GetByIdAsync(employeeId);

        public async Task<EmployeeDetails?> GetEmployeeByUserIdAsync(Guid userId)
            => await _employeeRepo.GetByUserIdAsync(userId);

        public async Task<IEnumerable<EmployeeDirectoryDto>> GetEmployeeDirectoryAsync()
        {
            var employees = await _employeeRepo.GetAllAsync();
            return MapEmployeeDirectory(employees);
        }

        public async Task<IEnumerable<EmployeeDirectoryDto>> GetEmployeeDirectoryByDepartmentAsync(Guid departmentId)
        {
            var employees = await _employeeRepo.GetAllAsync();
            return MapEmployeeDirectory(employees)
                .Where(employee => employee.DepartmentId == departmentId)
                .ToList();
        }

        public async Task<EmployeeDetails?> UpdateOwnProfileAsync(Guid userId, EmployeeSelfServiceUpdateDto update)
        {
            var employee = await _employeeRepo.GetByUserIdAsync(userId);
            if (employee == null)
            {
                return null;
            }

            employee.Address = update.Address;
            employee.PhoneNumber = update.PhoneNumber;
            employee.EmergencyContactName = update.EmergencyContactName;
            employee.EmergencyContactPhone = update.EmergencyContactPhone;
            employee.Relationship = update.Relationship;
            employee.BankName = update.BankName;
            employee.BankAccountNumber = update.BankAccountNumber;
            employee.UpdatedAt = DateTime.UtcNow;

            return await _employeeRepo.UpdateAsync(employee);
        }

        public async Task<EmployeeProfileUpdateRequestDto?> SubmitOwnProfileUpdateRequestAsync(Guid userId, EmployeeSelfServiceUpdateDto update)
        {
            var employee = await _employeeRepo.GetByUserIdAsync(userId);
            if (employee == null)
            {
                return null;
            }

            var current = MapSelfServiceUpdate(employee);
            var request = new EmployeeProfileUpdateRequest
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.EmployeeId,
                RequestedByUserId = userId,
                CurrentValuesJson = JsonSerializer.Serialize(current),
                ProposedValuesJson = JsonSerializer.Serialize(update),
                Status = StatusConstants.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.EmployeeProfileUpdateRequests.Add(request);
            await _context.SaveChangesAsync();
            request.Employee = employee;

            return MapProfileUpdateRequest(request);
        }

        public async Task<IEnumerable<EmployeeProfileUpdateRequestDto>> GetOwnProfileUpdateRequestsAsync(Guid userId)
        {
            var employee = await _employeeRepo.GetByUserIdAsync(userId);
            if (employee == null)
            {
                return Enumerable.Empty<EmployeeProfileUpdateRequestDto>();
            }

            var requests = await _context.EmployeeProfileUpdateRequests
                .Include(request => request.Employee)
                .Where(request => request.EmployeeId == employee.EmployeeId)
                .OrderByDescending(request => request.RequestedAt)
                .ToListAsync();

            return requests.Select(MapProfileUpdateRequest).ToList();
        }

        public async Task<IEnumerable<EmployeeProfileUpdateRequestDto>> GetProfileUpdateRequestsAsync(string? status)
        {
            var query = _context.EmployeeProfileUpdateRequests
                .Include(request => request.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(request => request.Status.ToLower() == status.ToLower());
            }

            var requests = await query
                .OrderByDescending(request => request.RequestedAt)
                .ToListAsync();

            return requests.Select(MapProfileUpdateRequest).ToList();
        }

        public async Task<EmployeeProfileUpdateRequestDto?> ReviewProfileUpdateRequestAsync(Guid requestId, Guid reviewerUserId, ProfileUpdateDecisionDto decision)
        {
            var request = await _context.EmployeeProfileUpdateRequests
                .Include(item => item.Employee)
                .FirstOrDefaultAsync(item => item.Id == requestId);

            if (request == null || request.Status != StatusConstants.Pending)
            {
                return null;
            }

            request.Status = decision.IsApproved ? StatusConstants.Approved : StatusConstants.Rejected;
            request.Comment = decision.Comment;
            request.ReviewedByUserId = reviewerUserId;
            request.ReviewedAt = DateTime.UtcNow;

            if (decision.IsApproved && request.Employee != null)
            {
                ApplySelfServiceUpdate(request.Employee, DeserializeSelfServiceUpdate(request.ProposedValuesJson));
                request.Employee.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapProfileUpdateRequest(request);
        }

        private static List<EmployeeDirectoryDto> MapEmployeeDirectory(IEnumerable<EmployeeDetails> employees)
        {
            return employees
                .Select(employee =>
                {
                    var currentEmployment = employee.Employments
                        .OrderByDescending(employment => employment.IsActive)
                        .ThenByDescending(employment => employment.DateOfHire)
                        .FirstOrDefault();

                    return new EmployeeDirectoryDto
                    {
                        EmployeeId = employee.EmployeeId,
                        UserId = employee.UserId,
                        FullName = $"{employee.FirstName} {employee.LastName}".Trim(),
                        Email = employee.Email,
                        PhoneNumber = employee.PhoneNumber,
                        JobTitle = currentEmployment?.JobTitle ?? string.Empty,
                        DepartmentId = currentEmployment?.DepartmentId,
                        DepartmentName = currentEmployment?.Department?.Name ?? string.Empty,
                        EmploymentTypeName = currentEmployment?.EmploymentType?.Name ?? string.Empty,
                        EmploymentStatus = currentEmployment?.EmploymentStatus ?? string.Empty,
                        DateOfHire = currentEmployment?.DateOfHire,
                        IsActive = currentEmployment?.IsActive ?? false
                    };
                })
                .OrderBy(employee => employee.FullName)
                .ToList();
        }

        private static EmployeeSelfServiceUpdateDto MapSelfServiceUpdate(EmployeeDetails employee)
        {
            return new EmployeeSelfServiceUpdateDto
            {
                Address = employee.Address,
                PhoneNumber = employee.PhoneNumber,
                EmergencyContactName = employee.EmergencyContactName,
                EmergencyContactPhone = employee.EmergencyContactPhone,
                Relationship = employee.Relationship,
                BankName = employee.BankName,
                BankAccountNumber = employee.BankAccountNumber,
                CvUrl = employee.CvUrl,
                IdentificationUrl = employee.IdentificationUrl,
                CertificateUrl = employee.CertificateUrl
            };
        }

        private static void ApplySelfServiceUpdate(EmployeeDetails employee, EmployeeSelfServiceUpdateDto update)
        {
            employee.Address = update.Address;
            employee.PhoneNumber = update.PhoneNumber;
            employee.EmergencyContactName = update.EmergencyContactName;
            employee.EmergencyContactPhone = update.EmergencyContactPhone;
            employee.Relationship = update.Relationship;
            employee.BankName = update.BankName;
            employee.BankAccountNumber = update.BankAccountNumber;
            employee.CvUrl = update.CvUrl;
            employee.IdentificationUrl = update.IdentificationUrl;
            employee.CertificateUrl = update.CertificateUrl;
        }

        private static EmployeeProfileUpdateRequestDto MapProfileUpdateRequest(EmployeeProfileUpdateRequest request)
        {
            return new EmployeeProfileUpdateRequestDto
            {
                Id = request.Id,
                EmployeeId = request.EmployeeId,
                RequestedByUserId = request.RequestedByUserId,
                EmployeeName = request.Employee == null ? string.Empty : $"{request.Employee.FirstName} {request.Employee.LastName}".Trim(),
                EmployeeEmail = request.Employee?.Email ?? string.Empty,
                CurrentValues = DeserializeSelfServiceUpdate(request.CurrentValuesJson),
                ProposedValues = DeserializeSelfServiceUpdate(request.ProposedValuesJson),
                Status = request.Status,
                Comment = request.Comment,
                ReviewedByUserId = request.ReviewedByUserId,
                RequestedAt = request.RequestedAt,
                ReviewedAt = request.ReviewedAt
            };
        }

        private static EmployeeSelfServiceUpdateDto DeserializeSelfServiceUpdate(string json)
        {
            return JsonSerializer.Deserialize<EmployeeSelfServiceUpdateDto>(json) ?? new EmployeeSelfServiceUpdateDto();
        }

        private async Task<string> GenerateNextEmployeeCodeAsync()
        {
            var existingCodes = await _context.EmployeeDetails
                .Where(e => !string.IsNullOrEmpty(e.EmployeeCode) && e.EmployeeCode.StartsWith("WU-"))
                .Select(e => e.EmployeeCode)
                .ToListAsync();

            var maxNum = 0;
            foreach (var code in existingCodes)
            {
                var numStr = code.Substring(3);
                if (int.TryParse(numStr, out var val))
                {
                    if (val > maxNum)
                    {
                        maxNum = val;
                    }
                }
            }
            return $"WU-{maxNum + 1:D4}";
        }

        public async Task<EmployeeDetails> CreateEmployeeAsync(CreateUserDto userDto)
        {
            if (userDto.DepartmentId == null)
                throw new ArgumentException("DepartmentId is required", nameof(userDto.DepartmentId));

            var employee = new EmployeeDetails
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.UserEmail,
                EmployeeCode = await GenerateNextEmployeeCodeAsync(),
                CreatedAt = DateTime.UtcNow
                // UserId will be set after user creation
            };

            var employment = new EmploymentDetails
            {
                DepartmentId = userDto.DepartmentId.Value,
                EmploymentTypeId = userDto.EmploymentTypeId,
                DateOfHire = DateTime.UtcNow,
                IsActive = true,
                JobTitle = userDto.JobTitle,
                JobCategoryId = userDto.JobCategoryId,
                Employee = employee // Establish relationship
            };
            // Use a transaction to ensure both user and employee are created successfully
            // If either fails, the transaction is rolled back

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Get Staff UserType
                var userTypes = await _authRepository.getUserTypes();
                if (userTypes == null || !userTypes.Any(a => a.Name == "Staff"))
                {
                    throw new InvalidOperationException("Staff user type not found.");
                }
                var staff = userTypes.First(a => a.Name == "Staff");

                // Step 2: Create user account
                var foundUser = await _authService.getUserByEmailAsync(employee.Email);
                if (foundUser == null)
                {
                    var user = await _authService.RegisterAsync(new CreateUserDto
                    {
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        UserEmail = employee.Email,
                        UserTypeId = staff!.Id,
                        EmploymentTypeId = userDto.EmploymentTypeId,
                        Password = employee.FirstName, // ⚠️ You may want to improve this logic
                        SingleSignOnEnabled = userDto.SingleSignOnEnabled
                    });

                    // Step 3: Link employee with user
                    employee.UserId = user.Id;
                    employee.ProfilePicture = "default.png";
                }
                else
                {
                    // Step 3: Link employee with user
                    employee.UserId = foundUser.Id;
                    employee.ProfilePicture = "default.png";
                }               // Step 4: Persist employee
                var createdEmployee = await _employeeRepo.AddAsync(employee);

                // Step 5: Commit transaction
                await transaction.CommitAsync();

                return createdEmployee;
            }
            catch
            {
                // Rollback if anything fails
                await transaction.RollbackAsync();
                throw; // let the controller handle the error
            }
        }

        public async Task<BulkStaffUploadResultDto> BulkCreateEmployeesAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Upload file is required.", nameof(file));
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var rows = extension switch
            {
                ".xlsx" => await ReadXlsxRowsAsync(file),
                ".csv" => await ReadCsvRowsAsync(file),
                _ => throw new InvalidOperationException("Only .xlsx and .csv staff upload files are supported.")
            };

            var result = new BulkStaffUploadResultDto { TotalRows = rows.Count };

            foreach (var row in rows)
            {
                try
                {
                    var dto = await MapBulkStaffRowAsync(row.Values);
                    await CreateEmployeeAsync(dto);
                    result.CreatedRows++;
                }
                catch (Exception ex)
                {
                    result.FailedRows++;
                    result.Errors.Add(new BulkStaffUploadRowErrorDto
                    {
                        RowNumber = row.RowNumber,
                        Email = GetValue(row.Values, "email", "useremail", "user email"),
                        Message = ex.Message
                    });
                }
            }

            return result;
        }

        public async Task<EmployeeDetails> UpdateEmployeeAsync(EmployeeDetails employee)
            => await _employeeRepo.UpdateAsync(employee);

        public async Task<List<JobCategory>> GetJobCategoriesAsync()
            => await _employeeRepo.GetJobCategoriesAsync();

        private async Task<CreateUserDto> MapBulkStaffRowAsync(Dictionary<string, string> row)
        {
            var firstName = GetRequiredValue(row, "firstname", "first name");
            var lastName = GetRequiredValue(row, "lastname", "last name");
            var email = GetRequiredValue(row, "email", "useremail", "user email");

            return new CreateUserDto
            {
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                UserName = GetValue(row, "username", "user name"),
                UserEmail = email,
                Password = GetValue(row, "password") is { Length: > 0 } password ? password : firstName,
                JobTitle = GetValue(row, "jobtitle", "job title"),
                DateCreated = DateTime.UtcNow,
                DepartmentId = await ResolveDepartmentIdAsync(row),
                EmploymentTypeId = await ResolveEmploymentTypeIdAsync(row),
                JobCategoryId = await ResolveJobCategoryIdAsync(row),
                SingleSignOnEnabled = ParseBoolean(GetValue(row, "singlesignonenabled", "single sign on enabled", "sso"), true)
            };
        }

        private async Task<Guid> ResolveDepartmentIdAsync(Dictionary<string, string> row)
        {
            var value = GetRequiredValue(row, "department", "departmentid", "department id");
            if (Guid.TryParse(value, out var parsed))
            {
                return parsed;
            }

            var normalized = NormalizeLookup(value);
            var department = await _context.Departments
                .Where(item => item.Name != null)
                .FirstOrDefaultAsync(item => item.Name.ToLower().Replace(" ", "") == normalized);

            return department?.Id ?? throw new InvalidOperationException($"Department '{value}' was not found.");
        }

        private async Task<Guid> ResolveEmploymentTypeIdAsync(Dictionary<string, string> row)
        {
            var value = GetRequiredValue(row, "employmenttype", "employment type", "employmenttypeid", "employment type id");
            if (Guid.TryParse(value, out var parsed))
            {
                return parsed;
            }

            var normalized = NormalizeLookup(value);
            var employmentType = await _context.EmploymentTypes
                .Where(item => item.Name != null)
                .FirstOrDefaultAsync(item => item.Name.ToLower().Replace(" ", "") == normalized);

            return employmentType?.Id ?? throw new InvalidOperationException($"Employment type '{value}' was not found.");
        }

        private async Task<Guid> ResolveJobCategoryIdAsync(Dictionary<string, string> row)
        {
            var value = GetRequiredValue(row, "jobcategory", "job category", "jobcategoryid", "job category id");
            if (Guid.TryParse(value, out var parsed))
            {
                return parsed;
            }

            var normalized = NormalizeLookup(value);
            var jobCategory = await _context.JobCategories
                .Where(item => item.Name != null)
                .FirstOrDefaultAsync(item => item.Name.ToLower().Replace(" ", "") == normalized);

            return jobCategory?.Id ?? throw new InvalidOperationException($"Job category '{value}' was not found.");
        }

        private static async Task<List<BulkStaffRow>> ReadCsvRowsAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            var records = ParseCsv(content);
            return BuildRows(records);
        }

        private static async Task<List<BulkStaffRow>> ReadXlsxRowsAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
            var sharedStrings = ReadSharedStrings(archive);
            var worksheet = archive.GetEntry("xl/worksheets/sheet1.xml")
                ?? throw new InvalidOperationException("The workbook must contain a first worksheet.");

            await using var worksheetStream = worksheet.Open();
            var document = await XDocument.LoadAsync(worksheetStream, LoadOptions.None, CancellationToken.None);
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            var records = document.Descendants(ns + "row")
                .Select(row =>
                {
                    var cells = row.Elements(ns + "c")
                        .ToDictionary(
                            cell => GetColumnName(cell.Attribute("r")?.Value ?? string.Empty),
                            cell => ReadCellValue(cell, sharedStrings, ns),
                            StringComparer.OrdinalIgnoreCase);

                    if (cells.Count == 0)
                    {
                        return [];
                    }

                    var maxColumn = cells.Keys.Select(ColumnNameToNumber).DefaultIfEmpty(0).Max();
                    return Enumerable.Range(1, maxColumn)
                        .Select(index => cells.TryGetValue(ColumnNumberToName(index), out var value) ? value : string.Empty)
                        .ToList();
                })
                .Where(record => record.Any(value => !string.IsNullOrWhiteSpace(value)))
                .ToList();

            return BuildRows(records);
        }

        private static List<BulkStaffRow> BuildRows(List<List<string>> records)
        {
            if (records.Count < 2)
            {
                return [];
            }

            var headers = records[0].Select(NormalizeHeader).ToList();
            var rows = new List<BulkStaffRow>();

            for (var i = 1; i < records.Count; i++)
            {
                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var column = 0; column < headers.Count; column++)
                {
                    if (string.IsNullOrWhiteSpace(headers[column]))
                    {
                        continue;
                    }

                    values[headers[column]] = column < records[i].Count ? records[i][column].Trim() : string.Empty;
                }

                if (values.Values.Any(value => !string.IsNullOrWhiteSpace(value)))
                {
                    rows.Add(new BulkStaffRow(i + 1, values));
                }
            }

            return rows;
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");
            if (sharedStringsEntry == null)
            {
                return [];
            }

            using var stream = sharedStringsEntry.Open();
            var document = XDocument.Load(stream);
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            return document.Descendants(ns + "si")
                .Select(item => string.Concat(item.Descendants(ns + "t").Select(text => text.Value)))
                .ToList();
        }

        private static string ReadCellValue(XElement cell, List<string> sharedStrings, XNamespace ns)
        {
            var type = cell.Attribute("t")?.Value;
            if (type == "inlineStr")
            {
                return string.Concat(cell.Descendants(ns + "t").Select(text => text.Value));
            }

            var value = cell.Element(ns + "v")?.Value ?? string.Empty;
            if (type == "s" && int.TryParse(value, out var sharedStringIndex) && sharedStringIndex < sharedStrings.Count)
            {
                return sharedStrings[sharedStringIndex];
            }

            return value;
        }

        private static List<List<string>> ParseCsv(string content)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var value = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < content.Length; i++)
            {
                var current = content[i];
                if (current == '"')
                {
                    if (inQuotes && i + 1 < content.Length && content[i + 1] == '"')
                    {
                        value.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (current == ',' && !inQuotes)
                {
                    row.Add(value.ToString());
                    value.Clear();
                }
                else if ((current == '\n' || current == '\r') && !inQuotes)
                {
                    if (current == '\r' && i + 1 < content.Length && content[i + 1] == '\n')
                    {
                        i++;
                    }

                    row.Add(value.ToString());
                    value.Clear();
                    if (row.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                    {
                        rows.Add(row);
                    }
                    row = new List<string>();
                }
                else
                {
                    value.Append(current);
                }
            }

            row.Add(value.ToString());
            if (row.Any(cell => !string.IsNullOrWhiteSpace(cell)))
            {
                rows.Add(row);
            }

            return rows;
        }

        private static string GetColumnName(string cellReference)
            => new(cellReference.Where(char.IsLetter).ToArray());

        private static int ColumnNameToNumber(string columnName)
        {
            var number = 0;
            foreach (var character in columnName.ToUpperInvariant())
            {
                number = number * 26 + character - 'A' + 1;
            }

            return number;
        }

        private static string ColumnNumberToName(int columnNumber)
        {
            var name = string.Empty;
            while (columnNumber > 0)
            {
                var modulo = (columnNumber - 1) % 26;
                name = Convert.ToChar('A' + modulo) + name;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return name;
        }

        private static string NormalizeHeader(string value)
            => new(value.Trim().ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

        private static string NormalizeLookup(string value)
            => value.Trim().ToLowerInvariant().Replace(" ", string.Empty);

        private static string GetRequiredValue(Dictionary<string, string> row, params string[] keys)
        {
            var value = GetValue(row, keys);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{keys[0]} is required.");
            }

            return value;
        }

        private static string GetValue(Dictionary<string, string> row, params string[] keys)
        {
            foreach (var key in keys.Select(NormalizeHeader))
            {
                if (row.TryGetValue(key, out var value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static Guid ParseRequiredGuid(Dictionary<string, string> row, params string[] keys)
        {
            var value = GetRequiredValue(row, keys);
            if (!Guid.TryParse(value, out var parsed))
            {
                throw new InvalidOperationException($"{keys[0]} must be a valid GUID.");
            }

            return parsed;
        }

        private static bool ParseBoolean(string value, bool defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return bool.TryParse(value, out var parsed) ? parsed : value.Trim().ToLowerInvariant() switch
            {
                "1" => true,
                "yes" => true,
                "y" => true,
                "0" => false,
                "no" => false,
                "n" => false,
                _ => defaultValue
            };
        }

        private record BulkStaffRow(int RowNumber, Dictionary<string, string> Values);
    }
}
