using System;
using System.Collections.Generic;

namespace RSWEBproekt.Models.ViewModels
{
    public class ProfessorCourseStudentsVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";

        public int SelectedYear { get; set; }
        public List<int> Years { get; set; } = new();

        public List<ProfessorEnrollmentRowVM> Enrollments { get; set; } = new();
    }

    public class ProfessorEnrollmentRowVM
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentIndex { get; set; } = "";

        public bool IsActive { get; set; }          // Active = PassedOn == null

        public int? Points { get; set; }
        public int? Grade { get; set; }
        public DateTime? PassedOn { get; set; }

        public string? DocumentPath { get; set; }   // read-only
    }
}
