//ovoj fajl e mostot megju c# kodot i bazata na podatoci
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RSWEBproekt.Models;

namespace RSWEBproekt.Data
{
    //so ova kazuvas ovaa klasa upravuva so baza na podatoci
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        //konstruktor , podesuvanja za konekcija so baza na podatoci
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //vo bazata ke ima tabela Teachers so podatoci spored modelot Teacher
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        //kako se povrzani modelite
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //nadvoresen kluc prv profesor
            modelBuilder.Entity<Course>()
                .HasOne(c => c.FirstTeacher)
                .WithMany(t => t.FirstTeacherCourses)
                .HasForeignKey(c => c.FirstTeacherId)
                .OnDelete(DeleteBehavior.Restrict);
            //nadvoresen kluc vtorprofesor
            modelBuilder.Entity<Course>()
                .HasOne(c => c.SecondTeacher)
                .WithMany(t => t.SecondTeacherCourses)
                .HasForeignKey(c => c.SecondTeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
