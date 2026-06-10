   namespace WUIAM.Enums
    {
        public enum Permissions
        {
            // System & Admin Access
            SuperAdminAccess,
            AdminAccess,
            AccessDashboard,
            ViewAuditLogs,
            ConfigureSystemSettings,

            // Global or Departmental User Management
            ManageUsers,
            ViewUsers,
            CreateUser,
            EditUser,
            DeleteUser,
            ActivateDeactivateUser,
            ResetUserPassword,
            ManageDepartmentUsers,
            ViewDepartmentUsers,

            // Role & Permission Management
            ManageRoles,
            ViewRoles,
            CreateRole,
            EditRole,
            DeleteRole,
            AssignRolesToUser,
            ManagePermissions,
            ViewPermissions,
            AssignPermissionsToRole,
            RevokePermissionsFromRole,

            // Approval Workflows (Generalized)
            ApproveRequests,
            RejectRequests,
            ViewPendingApprovals,

            // Department-Specific Approvals
            ApproveLeaveInDepartment,
            ApproveDocumentInDepartment,
            ApproveProfileUpdateInDepartment,

            // Own Profile & Self-Service
            ViewOwnProfile,
            UpdateOwnContactDetails,
            UpdateOwnBankDetails,
            UpdateOwnNextOfKin,
            RequestPermissionChange,
            ViewOwnPayslip,
            RequestLeave,
            ViewOwnAttendance,

            // HR: Recruitment & Job Management
            CreateJobPost,
            EditJobPost,
            DeleteJobPost,
            PublishJobPost,
            ViewJobApplicants,
            ScheduleInterview,
            UpdateInterviewStatus,
            SendOfferLetter,
            RejectApplicant,

            // HR: Employee Records & Management
            ViewEmployeeProfiles,
            ViewDepartmentEmployeeProfiles,
            UpdateEmployeeProfiles,
            UploadEmployeeDocuments,
            GenerateEmployeeReport,
            AssignSupervisor,

            // HR: Onboarding & Offboarding
            InitiateOnboarding,
            CompleteOnboarding,
            InitiateOffboarding,
            ProcessExitClearance,
            SendExitLetter,

            // HR: Leave & Attendance
            ManageLeaveRequests,
            ApproveLeave,
            RejectLeave,
            ViewLeaveCalendar,
            ViewAttendanceRecords,
            UpdateAttendance,
            ViewDepartmentAttendance,

            // HR: Payroll & Compensation
            ManagePayroll,
            ProcessPayroll,
            ViewPayslips,
            UpdateSalaryStructure,
            ManageBonuses,
            ManageDeductions,

            // Notifications & Communication
            SendInternalMessage,
            ManageAnnouncements,
            ReceiveNotifications,
            ManageTemplates,

            // Document Management
            UploadDocuments,
            ApproveDocuments,
            ArchiveDocuments,
            DeleteDocuments,
            ViewDepartmentDocuments,

            // Departmental Administration
            CreateDepartment,
            EditDepartment,
            DeleteDepartment,
            AssignDepartmentHead,
            ManageDepartmentStructure,

            // Reports & Analytics
            ViewHRReports,
            GenerateDepartmentReports,
            ExportData
        }
    }



