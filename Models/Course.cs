using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RSWEBproekt.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public int Credits { get; set; }

        public int Semester { get; set; }

        [StringLength(50)]
        public string? StudyProgram { get; set; }

        [StringLength(50)]
        public string? EducationLevel { get; set; }

        // FK кон Teachers 
        //int? znaci moze da bide null vrednost
        //bazata ke cuva broevi , ovie se foreign keys
        public int? FirstTeacherId { get; set; }
        public int? SecondTeacherId { get; set; }

        // Navigation properties
        //ova se vsunost pateki, go povrzuva teacher so firstteacher i secondteacher
        //tehnicki EF go gleda FirstTeacherId,odi vo tabela Teachers,go zema redot so Id=3 i go stava vo FirstTeacher
        //eden predmet moze da ima 2 razlicni nastavnici
        public Teacher? FirstTeacher { get; set; }
        public Teacher? SecondTeacher { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }

    }
}
