using Microsoft.EntityFrameworkCore;
using Models;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoan { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.ToTable("TaiKhoan");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.TenDangNhap)
                      .IsUnique();

                entity.Property(e => e.TenDangNhap)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.MatKhau)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(e => e.HoTen)
                      .HasMaxLength(200);

                entity.Property(e => e.Quyen)
                      .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true);
            });
        }
    }
}
