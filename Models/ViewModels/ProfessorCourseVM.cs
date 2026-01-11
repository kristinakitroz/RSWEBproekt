namespace RSWEBproekt.Models.ViewModels
{
    public class ProfessorCourseVM
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = "";
        public int Credits { get; set; }
        public int Semester { get; set; }
    }
}
