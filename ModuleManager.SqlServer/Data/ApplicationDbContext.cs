using Microsoft.EntityFrameworkCore;
using ModuleManager.SqlServer.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ModuleManager.SqlServer.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

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

			// Tạo một vai trò Admin mặc định
			var adminRole = new Role { RoleId = 1, RoleName = "Admin" };
			modelBuilder.Entity<Role>().HasData(adminRole);

			// Tạo một tài khoản Admin mặc định
			var adminUser = new User
			{
				UserId = 1,
				UserName = "admin",
				Email = "admin@example.com",
				PasswordHash = HashPassword("admin123"), 
				IsActive = true,
				CreatedAt = DateTime.Now,
				ImageUrl = "https://res.cloudinary.com/demo/image/upload/v1600123456/sample.jpg" // URL cụ thể cho hình ảnh
			};
			modelBuilder.Entity<User>().HasData(adminUser);

			// Gán vai trò Admin cho tài khoản Admin
			var adminUserRole = new UserRole { UserId = 1, RoleId = 1 };
			modelBuilder.Entity<UserRole>().HasData(adminUserRole);
		}

		private string HashPassword(string password)
		{
			using (var sha256 = SHA256.Create())
			{
				var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
				var builder = new StringBuilder();
				foreach (var t in bytes)
				{
					builder.Append(t.ToString("x2"));
				}
				return builder.ToString();
			}
		}
	}
}
