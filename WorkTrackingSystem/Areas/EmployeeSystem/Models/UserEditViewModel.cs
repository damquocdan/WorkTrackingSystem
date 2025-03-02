using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Areas.EmployeeSystem.Models
{
    public class UserEditViewModel
    {
        public long Id { get; set; } // Không cho người dùng sửa
        public string? UserName { get; set; } // Chỉ đọc
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới không khớp.")]
        public string? ConfirmPassword { get; set; }
    }

}
