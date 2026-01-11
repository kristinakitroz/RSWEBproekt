using System;
using System.ComponentModel.DataAnnotations;
//se kreira bidejka vrskata megju student i course e MANY TO MANY
//mora da se kreira nova tabela,bidejki nema kade da se stavi nadvoresen kluc
//vaka imame one to many vrska od student kon enrollment, i one to many vrska od course do enrollment
namespace RSWEBproekt.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        // Foreign keys
        public int StudentId { get; set; }
        public int CourseId { get; set; }

        // navigation properties
        //treba da moze da se null inace nema da se kreira enrollment,bidejki formata prajka student id ne student objekt pa ke go registrira kako null
        //vo post metodot na create modelstate ke se napravi false i nema da se kreria enrollment
        public Student? Student { get; set; } 
        public Course? Course { get; set; }

        
        public int Year { get; set; }                 // pr. 2025
        public Semester Semester { get; set; }        // Winter/Summer

        //Dopolnitelni polinja
        public int? Grade { get; set; }
        public DateTime? EnrolledOn { get; set; }

        // For deactivation / unenroll
        public DateTime? EndDate { get; set; }        // ako e null -> aktiven


        
        public string? DocumentPath { get; set; }
        //ke moze samo profesor da gi meenuva
        public int? Points { get; set; }
        public DateTime? PassedOn { get; set; }
        public string? ProjectUrl { get; set; }   // GitHub repository link



    }
}
