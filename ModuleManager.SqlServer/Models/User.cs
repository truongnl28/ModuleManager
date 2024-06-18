namespace ModuleManager.SqlServer.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public bool IsActive { get; set; }

		public DateTime CreatedAt { get; set; }

		public string ImageUrl { get; set; }

		public ICollection<UserRole> UserRoles { get; set; }
    }
}
