using Microsoft.AspNetCore.Identity;

namespace RSWEBproekt.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public bool MustChangePassword { get; set; } = false;
        public int? StudentId { get; set; }
        public Student? Student { get; set; }


    }
}
