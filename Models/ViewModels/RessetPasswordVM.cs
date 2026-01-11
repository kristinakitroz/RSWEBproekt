using System.ComponentModel.DataAnnotations;

namespace RSWEBproekt.Models.ViewModels
{
    public class ResetPasswordVM
    {
        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string Token { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string NewPassword { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = "";
    }
}
