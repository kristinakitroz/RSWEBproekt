using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSWEBproekt.Data;
using RSWEBproekt.Models;
using RSWEBproekt.Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RSWEBproekt.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Student/MyEnrolledCourses
        public async Task<IActionResult> MyEnrolledCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StudentId == null) return Forbid();

            var studentId = user.StudentId.Value;

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.Year)
                .ThenBy(e => e.Semester)
                .ThenBy(e => e.Course!.Title)
                .Select(e => new StudentEnrollmentRowVM
                {
                    EnrollmentId = e.Id,
                    CourseTitle = e.Course != null ? e.Course.Title : "-",
                    Year = e.Year,
                    Semester = e.Semester,
                    IsActive = e.EndDate == null,
                    Points = e.Points,
                    Grade = e.Grade,
                    PassedOn = e.PassedOn,
                    EnrolledOn = e.EnrolledOn,
                    DocumentPath = e.DocumentPath,
                    ProjectUrl = e.ProjectUrl
                })
                .ToListAsync();

            return View("MyEnrolledCourses", enrollments);
        }

        // GET: /Student/EditEnrollmentLinks/5
        public async Task<IActionResult> EditEnrollmentLinks(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StudentId == null) return Forbid();
            var studentId = user.StudentId.Value;

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null) return NotFound();

            
            if (enrollment.StudentId != studentId) return Forbid();

           

            var vm = new StudentEditEnrollmentLinksVM
            {
                EnrollmentId = enrollment.Id,
                CourseTitle = enrollment.Course?.Title ?? "-",
                Year = enrollment.Year,
                Semester = enrollment.Semester,

                Points = enrollment.Points,
                Grade = enrollment.Grade,
                PassedOn = enrollment.PassedOn,
                EnrolledOn = enrollment.EnrolledOn,
                EndDate = enrollment.EndDate,

                DocumentPath = enrollment.DocumentPath,
                ProjectUrl = enrollment.ProjectUrl
            };

            return View("EditEnrollmentLinks", vm);
        }

        // POST: /Student/EditEnrollmentLinks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEnrollmentLinks(StudentEditEnrollmentLinksVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StudentId == null) return Forbid();
            var studentId = user.StudentId.Value;

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == vm.EnrollmentId);

            if (enrollment == null) return NotFound();
            if (enrollment.StudentId != studentId) return Forbid();

            if (!ModelState.IsValid)
                return View("EditEnrollmentLinks", vm);

            
            enrollment.ProjectUrl = string.IsNullOrWhiteSpace(vm.ProjectUrl) ? null : vm.ProjectUrl.Trim();

            // upload document (pdf/doc/docx)
            if (vm.DocumentFile != null && vm.DocumentFile.Length > 0)
            {
                var ext = Path.GetExtension(vm.DocumentFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".pdf", ".doc", ".docx" };

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError(nameof(vm.DocumentFile), "Allowed file types: pdf, doc, docx.");
                    return View("EditEnrollmentLinks", vm);
                }

                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "documents");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"enr_{enrollment.Id}_stud_{studentId}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var fullPath = Path.Combine(uploadsDir, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await vm.DocumentFile.CopyToAsync(stream);

                enrollment.DocumentPath = $"/uploads/documents/{fileName}";
            }

            _context.Update(enrollment);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Saved.";
            return RedirectToAction(nameof(MyEnrolledCourses));
        }
    }
}
