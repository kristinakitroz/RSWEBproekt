using System;
using System.ComponentModel.DataAnnotations;

namespace RSWEBproekt.Models.ViewModels
{
    public class ProfessorUpdateEnrollmentVM
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public int Year { get; set; }

        public string StudentName { get; set; } = "";
        public string CourseTitle { get; set; } = "";

        [Range(0, 100)]
        public int? Points { get; set; }

        [Range(5, 10)]
        public int? Grade { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PassedOn { get; set; }

        public string? DocumentPath { get; set; } // read-only
    }
}
