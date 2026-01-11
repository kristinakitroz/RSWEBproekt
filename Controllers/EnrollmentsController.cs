using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RSWEBproekt.Data;
using RSWEBproekt.Models;
using RSWEBproekt.Models.ViewModels;
//ke mozit samo admin da pristapi
using Microsoft.AspNetCore.Authorization;

namespace RSWEBproekt.Controllers
{
    //samo admin kje pristapit
    [Authorize(Roles = "Admin")]
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (enrollment == null) return NotFound();

            return View(enrollment);
        }

        // GET: Enrollments/Create  (bulk create: one course + many students)
        public IActionResult Create()
        {
            var vm = new EnrollmentCreateViewModel
            {
                Courses = _context.Courses
                    .OrderBy(c => c.Title)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title })
                    .ToList(),

                Students = _context.Students
                    .OrderBy(s => s.FirstName).ThenBy(s => s.LastName)
                    .Select(s => new StudentCheckboxItem
                    {
                        Id = s.Id,
                        DisplayName = $"{s.FirstName} {s.LastName} ({s.Index})",
                        IsSelected = false
                    })
                    .ToList(),

                Year = DateTime.Now.Year,
                Semester = Semester.Winter,
                EnrolledOn = DateTime.Now
            };

            return View(vm);
        }


        // POST: Enrollments/Create  (bulk insert)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnrollmentCreateViewModel vm)
        {
            var selectedIds = vm.Students
                .Where(s => s.IsSelected)
                .Select(s => s.Id)
                .ToList();

            if (vm.CourseId == 0 || selectedIds.Count == 0)
            {
                if (vm.CourseId == 0)
                    ModelState.AddModelError(nameof(vm.CourseId), "Select a course.");
                if (selectedIds.Count == 0)
                    ModelState.AddModelError("", "Select at least one student.");

                // refill lists
                vm.Courses = _context.Courses
                    .OrderBy(c => c.Title)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title })
                    .ToList();

                vm.Students = _context.Students
                    .OrderBy(s => s.FirstName).ThenBy(s => s.LastName)
                    .Select(s => new StudentCheckboxItem
                    {
                        Id = s.Id,
                        DisplayName = $"{s.FirstName} {s.LastName} ({s.Index})",
                        IsSelected = vm.Students.Any(x => x.Id == s.Id && x.IsSelected)
                    })
                    .ToList();

                return View(vm);
            }

            var enrolledOn = vm.EnrolledOn ?? DateTime.Now;

            // spreci duplikati za istiot course+year+semester (samo aktivni)
            var existingStudentIds = await _context.Enrollments
                .Where(e => e.CourseId == vm.CourseId
                            && e.Year == vm.Year
                            && e.Semester == vm.Semester
                            && e.EndDate == null
                            && selectedIds.Contains(e.StudentId))
                .Select(e => e.StudentId)
                .ToListAsync();

            var toInsert = selectedIds.Except(existingStudentIds).ToList();

            if (toInsert.Count == 0)
            {
                ModelState.AddModelError("", "All selected students are already enrolled for this year/semester.");

                // refill lists
                vm.Courses = _context.Courses
                    .OrderBy(c => c.Title)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title })
                    .ToList();

                vm.Students = _context.Students
                    .OrderBy(s => s.FirstName).ThenBy(s => s.LastName)
                    .Select(s => new StudentCheckboxItem
                    {
                        Id = s.Id,
                        DisplayName = $"{s.FirstName} {s.LastName} ({s.Index})",
                        IsSelected = selectedIds.Contains(s.Id)
                    })
                    .ToList();

                return View(vm);
            }

            var newEnrollments = toInsert.Select(studentId => new Enrollment
            {
                CourseId = vm.CourseId,
                StudentId = studentId,

                Year = vm.Year,
                Semester = vm.Semester,

                EnrolledOn = enrolledOn,

                Grade = null,          
                DocumentPath = null,   
                EndDate = null        
            }).ToList();

            _context.Enrollments.AddRange(newEnrollments);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);

            if (enrollment.EndDate != null)
                return Forbid(); 
           
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5  (document upload stays here)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,StudentId,CourseId,Grade,EnrolledOn,DocumentPath")] Enrollment enrollment,
            IFormFile DocumentFile)
        {
            if (id != enrollment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // ako e dokument uploaded
                    if (DocumentFile != null && DocumentFile.Length > 0)
                    {
                        var extension = Path.GetExtension(DocumentFile.FileName).ToLowerInvariant();

                        if (extension == ".pdf" || extension == ".doc" || extension == ".docx")
                        {
                            var fileName = Guid.NewGuid().ToString() + extension;

                            var folderPath = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot",
                                "documents");

                            // osiguraj se deka postojt folder
                            if (!Directory.Exists(folderPath))
                                Directory.CreateDirectory(folderPath);

                            var filePath = Path.Combine(folderPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await DocumentFile.CopyToAsync(stream);
                            }

                            enrollment.DocumentPath = "/documents/" + fileName;
                        }
                        else
                        {
                            ModelState.AddModelError("", "Only PDF, DOC or DOCX files are allowed.");

                            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
                            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);

                            return View(enrollment);
                        }
                    }

                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);

            var existing = await _context.Enrollments.AsNoTracking()
    .FirstOrDefaultAsync(e => e.Id == id);

            if (existing == null) return NotFound();
            if (existing.EndDate != null) return Forbid();

            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (enrollment == null) return NotFound();

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
        // GET: Enrollments/DeactivateSelect
        [HttpGet]
        public IActionResult DeactivateSelect()
        {
            var vm = new EnrollmentDeactivateSelectVM
            {
                Courses = _context.Courses
                    .OrderBy(c => c.Title)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title })
                    .ToList()
            };

            return View(vm);
        }

        // POST: Enrollments/DeactivateSelect
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeactivateSelect(EnrollmentDeactivateSelectVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Courses = _context.Courses
                    .OrderBy(c => c.Title)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title })
                    .ToList();
                return View(vm);
            }

            return RedirectToAction(nameof(DeactivateGrouped), new { courseId = vm.CourseId });
        }
        //GET deactivategrouped(prikazi grupirano)
        [HttpGet]
        public async Task<IActionResult> DeactivateGrouped(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId && e.EndDate == null) // само активни
                .OrderByDescending(e => e.Year)
                .ThenBy(e => e.Semester)
                .ThenBy(e => e.Student!.FirstName)
                .ThenBy(e => e.Student!.LastName)
                .ToListAsync();

            var groups = enrollments
                .GroupBy(e => new { e.Year, e.Semester })
                .OrderByDescending(g => g.Key.Year)
                .ThenBy(g => g.Key.Semester)
                .Select(g => new EnrollmentGroupVM
                {
                    Year = g.Key.Year,
                    Semester = g.Key.Semester,
                    Items = g.Select(e => new EnrollmentDeactivateItemVM
                    {
                        EnrollmentId = e.Id,
                        StudentName = $"{e.Student!.FirstName} {e.Student!.LastName}",
                        IsSelected = false
                    }).ToList()
                })
                .ToList();

            var vm = new EnrollmentDeactivateGroupedVM
            {
                CourseId = courseId,
                CourseTitle = course.Title,
                EndDate = DateTime.Today,
                Groups = groups
            };

            return View(vm);
        }
        //post deactivategrouped(setira enddate na selektiranite)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateGrouped(EnrollmentDeactivateGroupedVM vm)
        {
            var selectedIds = vm.Groups
                .SelectMany(g => g.Items)
                .Where(i => i.IsSelected)
                .Select(i => i.EnrollmentId)
                .ToList();

            if (selectedIds.Count == 0)
            {
                ModelState.AddModelError("", "Select at least one student to deactivate.");

                
                return await DeactivateGrouped(vm.CourseId);
            }

            var enrollmentsToUpdate = await _context.Enrollments
                .Where(e => selectedIds.Contains(e.Id))
                .ToListAsync();

            foreach (var e in enrollmentsToUpdate)
                e.EndDate = vm.EndDate;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



    }
}
