using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SE4458_Midterm_20070006074.Models;

namespace SE4458_Midterm_20070006074.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<Usage> Usages { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BillDetail> BillDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.LogTo(message => _logger.LogInformation(message));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Subscriber
            modelBuilder.Entity<Subscriber>(entity =>
            {
                entity.HasKey(e => e.SubscriberId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Surname).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
            });

            // Configure Bill
            modelBuilder.Entity<Bill>(entity =>
            {
                entity.HasKey(e => e.BillId);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(b => b.Subscriber)
                    .WithMany(s => s.Bills)
                    .HasForeignKey(b => b.SubscriberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BillDetail
            modelBuilder.Entity<BillDetail>(entity =>
            {
                entity.HasKey(e => e.DetailId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(bd => bd.Bill)
                    .WithMany(b => b.BillDetails)
                    .HasForeignKey(bd => bd.BillId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Usage
            modelBuilder.Entity<Usage>(entity =>
            {
                entity.HasKey(e => e.UsageId);
                entity.HasOne(u => u.Subscriber)
                    .WithMany(s => s.Usages)
                    .HasForeignKey(u => u.SubscriberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasOne(p => p.Bill)
                    .WithMany(b => b.Payments)
                    .HasForeignKey(p => p.BillId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
} 