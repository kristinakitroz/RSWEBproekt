using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RSWEBproekt.Models.ViewModels
{
    public class EnrollmentCreateViewModel
    {
        [Required]
        public int CourseId { get; set; }

        //public int? Grade { get; set; }
        public DateTime? EnrolledOn { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; } = DateTime.Now.Year;


        [Required]
        public Semester Semester { get; set; } = Semester.Winter;

        public List<SelectListItem> Courses { get; set; } = new();

        //  Checkbox list
        public List<StudentCheckboxItem> Students { get; set; } = new();
    }

    public class StudentCheckboxItem
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
