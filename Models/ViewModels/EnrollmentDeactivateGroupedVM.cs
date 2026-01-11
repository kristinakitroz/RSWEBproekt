using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RSWEBproekt.Models;

namespace RSWEBproekt.Models.ViewModels
{
    public class EnrollmentDeactivateGroupedVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        public List<EnrollmentGroupVM> Groups { get; set; } = new();
    }

    public class EnrollmentGroupVM
    {
        public int Year { get; set; }
        public Semester Semester { get; set; }
        public List<EnrollmentDeactivateItemVM> Items { get; set; } = new();
    }

    public class EnrollmentDeactivateItemVM
    {
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; } = "";
        public bool IsSelected { get; set; }
    }
}
