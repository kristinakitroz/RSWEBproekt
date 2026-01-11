using System.ComponentModel.DataAnnotations;

namespace RSWEBproekt.Models.ViewModels
{
    public class ForceChangePasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = "";

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
