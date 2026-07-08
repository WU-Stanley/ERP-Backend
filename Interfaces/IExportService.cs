using System.Threading.Tasks;

namespace WUIAM.Interfaces
{
    public interface IExportService
    {
        Task<byte[]> ExportEmployeesCsvAsync();
        Task<byte[]> ExportDepartmentsCsvAsync();
        Task<byte[]> ExportLeaveRequestsCsvAsync();
    }
}
