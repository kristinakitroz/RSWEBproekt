using System;
using RSWEBproekt.Models;

namespace RSWEBproekt.Models.ViewModels
{
    public class StudentEnrollmentRowVM
    {
        public int EnrollmentId { get; set; }
        public string CourseTitle { get; set; } = "";
        public int Year { get; set; }
        public Semester Semester { get; set; }

        public bool IsActive { get; set; }
        public int? Points { get; set; }
        public int? Grade { get; set; }
        public DateTime? EnrolledOn { get; set; }
        public DateTime? PassedOn { get; set; }

        public string? DocumentPath { get; set; }
        public string? ProjectUrl { get; set; }
    }
}
