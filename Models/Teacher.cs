using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RSWEBproekt.Models
{
    //EF ke kreira tabela Teachers vo bazata na podatoci spored ovoj model
    public class Teacher
    {
        //primaren kluc
        public int Id { get; set; }

        // svojstva na modelot Teacher
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;    

        [StringLength(50)]
        public string? Degree { get; set; }

        [StringLength(25)]
        public string? AcademicRank { get; set; }

        [StringLength(10)]
        public string? OfficeNumber { get; set; }

        public DateTime? HireDate { get; set; }

        // Navigation properties 
        //sluzat za relacii , ke se koristat koga ke go kreirame Course4
        //vo koj kurs e prv profesork,vo koj vtor
        public ICollection<Course>? FirstTeacherCourses { get; set; }
       public ICollection<Course>? SecondTeacherCourses { get; set; }

        public string? ImagePath { get; set; }

        public string? PendingResetLink { get; set; }   // reset link za profesorot (se brise otkako ke si postavi password)
        public DateTime? ResetLinkCreatedOn { get; set; }


    }
}
