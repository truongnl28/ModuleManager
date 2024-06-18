using System.ComponentModel.DataAnnotations;

namespace ModuleManager.SqlServer.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Tên người dùng")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Hình ảnh")]
        public IFormFile Image { get; set; }
    }
}
