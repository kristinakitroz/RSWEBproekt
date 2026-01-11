using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RSWEBproekt.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Index { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public int EnrollmentYear { get; set; }

        public int CurrentSemester { get; set; }

        [StringLength(50)]
        public string? LevelOfEducation { get; set; }

        // Navigation properties 
        public ICollection<Enrollment>? Enrollments { get; set; }

        public string? ImagePath { get; set; }
        public string? PendingResetLink { get; set; }
        public DateTime? ResetLinkCreatedOn { get; set; }



    }
}
