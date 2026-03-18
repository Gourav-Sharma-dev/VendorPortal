using Microsoft.EntityFrameworkCore;
using Server.Model;
using Server.Model.Enums;

namespace Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorBankDetail> VendorBankDetails { get; set; }
        public DbSet<VendorDocument> VendorDocuments { get; set; }
        public DbSet<VendorStatusHistory> VendorStatusHistories { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Vendor entity
            modelBuilder.Entity<Vendor>(entity =>
            {
                // Convert VendorType enum to string in database
                entity.Property(v => v.VendorType)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                // Convert VendorStatus enum to string in database
                entity.Property(v => v.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                // String column lengths
                entity.Property(v => v.VendorName).HasMaxLength(100);
                entity.Property(v => v.VendorCode).HasMaxLength(30);
                entity.Property(v => v.ContactPerson).HasMaxLength(100);
                entity.Property(v => v.MobileNumber).HasMaxLength(15);
                entity.Property(v => v.EmailAddress).HasMaxLength(100);
                entity.Property(v => v.CompanyName).HasMaxLength(150);
                entity.Property(v => v.CompanyAddress).HasMaxLength(250);
                entity.Property(v => v.City).HasMaxLength(50);
                entity.Property(v => v.State).HasMaxLength(50);
                entity.Property(v => v.Country).HasMaxLength(50);
                entity.Property(v => v.PinCode).HasMaxLength(10);
                entity.Property(v => v.GSTNumber).HasMaxLength(20);
                entity.Property(v => v.PANNumber).HasMaxLength(10);

                // Unique indexes
                entity.HasIndex(v => v.VendorCode).IsUnique();
                entity.HasIndex(v => v.EmailAddress).IsUnique();

                // Set default values
                entity.Property(v => v.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(v => v.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Configure relationships
                entity.HasMany(v => v.BankDetails)
                    .WithOne(b => b.Vendor)
                    .HasForeignKey(b => b.VendorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.Documents)
                    .WithOne(d => d.Vendor)
                    .HasForeignKey(d => d.VendorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.StatusHistories)
                    .WithOne(s => s.Vendor)
                    .HasForeignKey(s => s.VendorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure VendorDocument entity
            modelBuilder.Entity<VendorDocument>(entity =>
            {
                // Convert DocumentType enum to string in database
                entity.Property(d => d.DocumentType)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(d => d.FileName).HasMaxLength(255);
                entity.Property(d => d.FileFormat).HasMaxLength(10);

                entity.Property(d => d.UploadedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure VendorStatusHistory entity
            modelBuilder.Entity<VendorStatusHistory>(entity =>
            {
                // Configure relationship to User for ActionBy
                entity.HasOne(h => h.ActionUser)
                    .WithMany()
                    .HasForeignKey(h => h.ActionBy)
                    .OnDelete(DeleteBehavior.Restrict);

                // Convert OldStatus enum to string in database
                entity.Property(h => h.OldStatus)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                // Convert NewStatus enum to string in database
                entity.Property(h => h.NewStatus)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                // Convert ApprovalLevel enum to string in database (nullable)
                entity.Property(h => h.ApprovalLevel)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.Property(h => h.Remarks).HasMaxLength(500);

                entity.Property(h => h.ActionDate)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Username).HasMaxLength(50);
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.Property(u => u.FullName).HasMaxLength(100);

                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(u => u.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.RoleName).IsUnique();
                entity.Property(r => r.RoleName).HasMaxLength(50);
                entity.Property(r => r.RoleDescription).HasMaxLength(200);
            });

            // Configure UserRole relationship
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AuditLog entity
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.Property(a => a.Action).HasMaxLength(100);
                entity.Property(a => a.Entity).HasMaxLength(100);
                entity.Property(a => a.Details).HasMaxLength(1000);

                entity.Property(a => a.Timestamp)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Seed Fixed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleId = Guid.Parse("a0251ea3-4f96-4d7a-99da-05f0e0b57929"),
                    RoleName = "Admin",
                    RoleDescription = "Full system access and user management"
                },
                new Role
                {
                    RoleId = Guid.Parse("76172ad1-b7ef-47fa-b0b4-4ace4c5e7388"),
                    RoleName = "Vendor",
                    RoleDescription = "Submit registration request and upload documents"
                },
                new Role
                {
                    RoleId = Guid.Parse("7b331adb-f343-456e-aa03-cfed7fc391a6"),
                    RoleName = "Procurement",
                    RoleDescription = "Review and approve vendor information"
                },
                new Role
                {
                    RoleId = Guid.Parse("64f10715-a8d1-4070-8c90-432b7a19a6fa"),
                    RoleName = "Finance",
                    RoleDescription = "Verify financial and tax details"
                }
            );
        }
    }
}