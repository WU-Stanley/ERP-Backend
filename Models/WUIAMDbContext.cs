using Microsoft.EntityFrameworkCore;

namespace WUIAM.Models
{
    public class WUIAMDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public WUIAMDbContext() { }
        public WUIAMDbContext(DbContextOptions<WUIAMDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<MFAToken> MFATokens { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ApprovalStep> ApprovalSteps { get; set; }
        public DbSet<ApprovalFlow> ApprovalFlows { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequestApproval> LeaveRequestApprovals { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<PublicHoliday> PublicHolidays { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<EmploymentType> EmploymentTypes { get; set; }
        public DbSet<LeaveTypeVisibility> LeaveTypeVisibilities { get; set; }
        public DbSet<ApprovalDelegation> ApprovalDelegations { get; set; }
        public DbSet<LeavePolicy> LeavePolicies { get; set; }
        //public DbSet<LeavePolicy> MyLeavePolicies { get; set; }
        public DbSet<College> Colleges { get; set; }
        public DbSet<AcademicProgram> Programs { get; set; }
        public DbSet<EmployeeDetails> EmployeeDetails { get; set; }
        public DbSet<EmploymentDetails> EmploymentDetails { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<EmployeeProfileUpdateRequest> EmployeeProfileUpdateRequests { get; set; }
        public DbSet<JobCategory> JobCategories { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SalaryStructure> SalaryStructures { get; set; }
        public DbSet<PayrollRun> PayrollRuns { get; set; }
        public DbSet<ProcurementRequest> ProcurementRequests { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<DocumentRecord> DocumentRecords { get; set; }
        public DbSet<HelpdeskTicket> HelpdeskTickets { get; set; }
        public DbSet<HelpdeskTicketComment> HelpdeskTicketComments { get; set; }
        public DbSet<FacilityAsset> FacilityAssets { get; set; }
        public DbSet<RegistryIntegrationRecord> RegistryIntegrationRecords { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Recruitment Management
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<ApplicationScore> ApplicationScores { get; set; }
        public DbSet<InterviewSchedule> InterviewSchedules { get; set; }
        public DbSet<InterviewInterviewer> InterviewInterviewers { get; set; }
        public DbSet<OfferLetter> OfferLetters { get; set; }
        public DbSet<ApplicantQuery> ApplicantQueries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
            .HasIndex(u => u.UserEmail)
            .IsUnique();
            // UserRole: composite key
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // RolePermission: composite key
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // Department to User (one-to-many)
            //modelBuilder.Entity<User>()
            //    .HasOne(u => u.Department)
            //    .WithMany(d => d.Users)
            //    .HasForeignKey(u => u.DeptId);

            // User to MFAToken (one-to-many)
            modelBuilder.Entity<MFAToken>()
                .HasOne(m => m.User)
                .WithMany(u => u.MFATokens)
                .HasForeignKey(m => m.UserId);

            // Optionally, configure Permission navigation to RolePermission
            modelBuilder.Entity<Permission>()
                .HasMany(p => p.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId);

            // Optionally, configure Role navigation to RolePermission and UserRole
            modelBuilder.Entity<Role>()
                .HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<Role>()
                .HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId);

            // Optionally, configure User navigation to UserRole and MFAToken
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.MFATokens)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId);

            // Optionally, configure Department navigation to Users
            //modelBuilder.Entity<Department>()
            //    .HasMany(d => d.Users)
            //    .WithOne(u => u.Department)
            //    .HasForeignKey(u => u.DeptId);
            modelBuilder.Entity<LeaveBalance>()
        //.HasIndex(lb => new { lb.UserId, lb.LeaveTypeId, lb.ValidFrom })
        .HasIndex(lb => lb.UserId) .IsUnique(false)
        .IsUnique();
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.LeaveType)
                .WithMany()
                .HasForeignKey(lr => lr.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveBalance>()
        .HasIndex(lb => lb.UserId)
        .IsUnique(false); 


            modelBuilder.Entity<Leave>()
                .HasOne(l => l.LeaveRequest)
                .WithMany()
                .HasForeignKey(l => l.LeaveRequestId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<LeaveRequestApproval>()
                .HasOne(a => a.ApproverPerson)
                .WithMany()
                .HasForeignKey(a => a.ApproverPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApprovalDelegation>()
                .HasOne(d => d.ApproverPerson)
                .WithMany()
                .HasForeignKey(d => d.ApproverPersonId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApprovalDelegation>()
                .HasOne(d => d.DelegatePerson)
                .WithMany()
                .HasForeignKey(d => d.DelegatePersonId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LeaveType>()
                .HasMany(l => l.VisibilityRules)
                .WithOne(v => v.LeaveType) // explicitly specify the navigation property
                .HasForeignKey(v => v.LeaveTypeId) // explicitly specify the foreign key
                .OnDelete(DeleteBehavior.Cascade);
            // This enables database-level cascade delete

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);

                // Self-referencing relationship
                entity.HasOne(d => d.ParentDepartment)
                      .WithMany(d => d.SubDepartments)
                      .HasForeignKey(d => d.ParentDepartmentId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

                // Relationship with College (only for academic departments)
                entity.HasOne(d => d.College)
                      .WithMany(c => c.Departments)
                      .HasForeignKey(d => d.CollegeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relationship with Head of Department
                entity.HasOne(d => d.Head)
                      .WithMany()
                      .HasForeignKey(d => d.HeadId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Required properties
                entity.Property(d => d.Code)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(d => d.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(d => d.DepartmentType)
                      .IsRequired()
                      .HasMaxLength(20);
            });

            modelBuilder.Entity<EmploymentDetails>(entity =>
            {
                entity.HasKey(e => e.EmploymentId);

                // Explicit mapping Employee ↔ Employment
                entity.HasOne(e => e.Employee)                   // nav on EmploymentDetails
                .WithMany(emp => emp.Employments)          // nav on EmployeeDetails
                .HasForeignKey(e => e.EmployeeId)          // FK property
                .OnDelete(DeleteBehavior.Restrict);        // prevents cascade delete
            });
            modelBuilder.Entity<EmployeeDetails>(entity =>
            {
               entity.HasKey(e => e.EmployeeId);

               entity.HasOne(e => e.User)                     // EmployeeDetails → User
                .WithOne(u => u.Employee)                // User → EmployeeDetails
                .HasForeignKey<EmployeeDetails>(e => e.UserId); // FK is in EmployeeDetails

               entity.Property(e => e.Gender)
                   .HasConversion<string>();

               entity.Property(e => e.MaritalStatus)
                   .HasConversion<string>();
            });

            // Student → Colleges and Student → Programs both cascade,
            // but Programs → Colleges also cascades, causing a cycle.
            // Suppress cascade on Student → Colleges to avoid it.
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasOne(s => s.College)
                    .WithMany()
                    .HasForeignKey(s => s.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Program)
                    .WithMany()
                    .HasForeignKey(s => s.ProgramId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<HelpdeskTicketComment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasOne(c => c.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(c => c.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
            });

            // Recruitment entity configurations
            modelBuilder.Entity<JobPosting>(entity =>
            {
                entity.HasKey(j => j.Id);
                entity.HasOne(j => j.Department)
                    .WithMany()
                    .HasForeignKey(j => j.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(j => j.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(j => j.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.Property(j => j.Title).IsRequired().HasMaxLength(200);
                entity.Property(j => j.Description).IsRequired();
                entity.Property(j => j.Status).HasMaxLength(50);
                entity.Property(j => j.EmploymentType).HasMaxLength(50);
            });

            modelBuilder.Entity<JobApplication>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasOne(a => a.JobPosting)
                    .WithMany()
                    .HasForeignKey(a => a.JobPostingId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(a => a.AssignedToUser)
                    .WithMany()
                    .HasForeignKey(a => a.AssignedTo)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.Property(a => a.ApplicantName).IsRequired().HasMaxLength(200);
                entity.Property(a => a.Email).IsRequired().HasMaxLength(200);
                entity.Property(a => a.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<ApplicationScore>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasOne(s => s.Application)
                    .WithMany()
                    .HasForeignKey(s => s.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(s => s.TechnicalMatch).HasPrecision(5, 2);
                entity.Property(s => s.CulturalFit).HasPrecision(5, 2);
                entity.Property(s => s.EducationMatch).HasPrecision(5, 2);
                entity.Property(s => s.ExperienceMatch).HasPrecision(5, 2);
                entity.Property(s => s.OverallMatch).HasPrecision(5, 2);
            });

            modelBuilder.Entity<InterviewSchedule>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasOne(i => i.Application)
                    .WithMany()
                    .HasForeignKey(i => i.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(i => i.Type).HasMaxLength(50);
                entity.Property(i => i.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<InterviewInterviewer>(entity =>
            {
                entity.HasKey(ii => ii.Id);
                entity.HasOne(ii => ii.InterviewSchedule)
                    .WithMany(s => s.Interviewers)
                    .HasForeignKey(ii => ii.InterviewScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ii => ii.Employee)
                    .WithMany()
                    .HasForeignKey(ii => ii.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<OfferLetter>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.HasOne(o => o.Application)
                    .WithMany()
                    .HasForeignKey(o => o.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(o => o.Status).HasMaxLength(50);
                entity.Property(o => o.CompanyName).HasMaxLength(200);
                entity.Property(o => o.Position).HasMaxLength(200);
                entity.Property(o => o.Salary).HasPrecision(18, 2);
            });

            modelBuilder.Entity<ApplicantQuery>(entity =>
            {
                entity.HasKey(q => q.Id);
                entity.HasOne(q => q.Application)
                    .WithMany()
                    .HasForeignKey(q => q.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(q => q.FromUser)
                    .WithMany()
                    .HasForeignKey(q => q.FromUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.Property(q => q.MessageFrom).HasMaxLength(50);
            });
        }
    }
}
