using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using RSWEBproekt.Models;

namespace RSWEBproekt.Models.ViewModels
{
    public class StudentEditEnrollmentLinksVM
    {
        public int EnrollmentId { get; set; }

        public string CourseTitle { get; set; } = "";
        public int Year { get; set; }
        public Semester Semester { get; set; }

        // read-only fields
        public int? Points { get; set; }
        public int? Grade { get; set; }
        public DateTime? EnrolledOn { get; set; }
        public DateTime? PassedOn { get; set; }
        public DateTime? EndDate { get; set; }

        // editable
        public string? DocumentPath { get; set; }

        [Url(ErrorMessage = "Enter a valid URL.")]
        public string? ProjectUrl { get; set; }

        public IFormFile? DocumentFile { get; set; }
    }
}
