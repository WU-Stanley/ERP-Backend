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
        public DbSet<JobCategory> JobCategories { get; set; }

        // public DbSet<AuditLog> AuditLogs { get; set; }

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
        }
    }
}
