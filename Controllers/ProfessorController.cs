using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSWEBproekt.Data;
using RSWEBproekt.Models;
using RSWEBproekt.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RSWEBproekt.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfessorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Professor/MyCourses
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.TeacherId == null) return Forbid();

            var teacherId = user.TeacherId.Value;

            var courses = await _context.Courses
                .Where(c => c.FirstTeacherId == teacherId || c.SecondTeacherId == teacherId)
                .OrderBy(c => c.Title)
                .Select(c => new ProfessorCourseVM
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Credits = c.Credits,
                    Semester = c.Semester
                })
                .ToListAsync();

            return View(courses);
        }

        // GET: /Professor/CourseStudents?courseId=1&year=2025
        public async Task<IActionResult> CourseStudents(int courseId, int? year)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.TeacherId == null) return Forbid();
            var teacherId = user.TeacherId.Value;

            // Security: course mora da e negov
            var course = await _context.Courses.FirstOrDefaultAsync(c =>
                c.Id == courseId && (c.FirstTeacherId == teacherId || c.SecondTeacherId == teacherId));

            if (course == null) return Forbid();

            
            //years dropdown - site godini kade ima enrollments
            var years = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => e.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            var selectedYear = year ?? (years.Count > 0 ? years.First() : DateTime.Now.Year);

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId && e.Year == selectedYear)
                .OrderBy(e => e.Student!.LastName).ThenBy(e => e.Student!.FirstName)
                .Select(e => new ProfessorEnrollmentRowVM
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student!.FirstName + " " + e.Student!.LastName,
                    StudentIndex = e.Student!.Index,
                    IsActive = e.PassedOn == null,
                    Points = e.Points,
                    Grade = e.Grade,
                    PassedOn = e.PassedOn,
                    DocumentPath = e.DocumentPath
                })
                .ToListAsync();

            var vm = new ProfessorCourseStudentsVM
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                SelectedYear = selectedYear,
                Years = years,
                Enrollments = enrollments
            };

            return View("CourseStudents", vm);
        }

        // GET: /Professor/EditEnrollment/5
        public async Task<IActionResult> EditEnrollment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.TeacherId == null) return Forbid();
            var teacherId = user.TeacherId.Value;

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null) return NotFound();

            
            if (enrollment.Course == null ||
                !(enrollment.Course.FirstTeacherId == teacherId || enrollment.Course.SecondTeacherId == teacherId))
                return Forbid();

            // samo aktivni studenti se editable
            if (enrollment.PassedOn != null)
            {
                TempData["Message"] = "Cannot edit inactive enrollment.";
                return RedirectToAction(nameof(CourseStudents), new { courseId = enrollment.CourseId, year = enrollment.Year });
            }

            var vm = new ProfessorUpdateEnrollmentVM
            {
                EnrollmentId = enrollment.Id,
                CourseId = enrollment.CourseId,
                Year = enrollment.Year,
                CourseTitle = enrollment.Course!.Title,
                StudentName = enrollment.Student!.FirstName + " " + enrollment.Student!.LastName,
                Points = enrollment.Points,
                Grade = enrollment.Grade,
                PassedOn = enrollment.PassedOn,
                DocumentPath = enrollment.DocumentPath
            };

            return View(vm);
        }

        // POST: /Professor/EditEnrollment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEnrollment(ProfessorUpdateEnrollmentVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.TeacherId == null) return Forbid();
            var teacherId = user.TeacherId.Value;

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == vm.EnrollmentId);

            if (enrollment == null) return NotFound();

            if (enrollment.Course == null ||
                !(enrollment.Course.FirstTeacherId == teacherId || enrollment.Course.SecondTeacherId == teacherId))
                return Forbid();

            
            if (enrollment.PassedOn != null)
            {
                TempData["Message"] = "Cannot edit inactive enrollment.";
                return RedirectToAction(nameof(CourseStudents), new { courseId = enrollment.CourseId, year = enrollment.Year });
            }

            if (!ModelState.IsValid)
                return View(vm);

            enrollment.Points = vm.Points;
            enrollment.Grade = vm.Grade;
            enrollment.PassedOn = vm.PassedOn;
            
            enrollment.EndDate = vm.PassedOn;  


            _context.Update(enrollment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CourseStudents), new { courseId = enrollment.CourseId, year = enrollment.Year });
        }
    }
}
