using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RSWEBproekt.Models.ViewModels
{
    public class EnrollmentDeactivateSelectVM
    {
        [Required]
        public int CourseId { get; set; }
        public List<SelectListItem> Courses { get; set; } = new();
    }
}
