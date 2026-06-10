using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface ILeaveTypeService
    {
        Task<LeaveType> CreateLeaveType(CreateLeaveTypeDto createLeaveTypeDto);
        Task<LeaveType> UpdateLeaveType(LeaveType leaveType);
        Task<LeaveType> DeleteLeaveType(Guid leaveTypeId);
        Task<LeaveType> GetLeaveTypeById(Guid leaveTypeId);
        Task<IEnumerable<LeaveType>> GetAllLeaveTypes(); 
        Task<IEnumerable<LeaveType>> GetAvailableLeaveTypes();
    }
}