using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RSWEBproekt.Data;
using RSWEBproekt.Models;
using System.IO;

namespace RSWEBproekt.Controllers
{
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
            var applicationDbContext = _context.Enrollments.Include(e => e.Course).Include(e => e.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title");
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName");
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     [Bind("Id,StudentId,CourseId,Grade,EnrolledOn")]
    Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                enrollment.EnrolledOn ??= DateTime.Now;

                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CourseId"] =
                new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] =
                new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);

            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,StudentId,CourseId,Grade,EnrolledOn")]
    Enrollment enrollment,
    IFormFile DocumentFile)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 🔹 ако има прикачен документ
                    if (DocumentFile != null && DocumentFile.Length > 0)
                    {
                        var extension = Path.GetExtension(DocumentFile.FileName).ToLower();

                        if (extension == ".pdf" || extension == ".doc" || extension == ".docx")
                        {
                            var fileName = Guid.NewGuid().ToString() + extension;
                            var path = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot/documents",
                                fileName
                            );

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await DocumentFile.CopyToAsync(stream);
                            }

                            enrollment.DocumentPath = "/documents/" + fileName;
                        }
                        else
                        {
                            ModelState.AddModelError("", "Only PDF, DOC or DOCX files are allowed.");

                            ViewData["CourseId"] =
                                new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
                            ViewData["StudentId"] =
                                new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);

                            return View(enrollment);
                        }
                    }

                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CourseId"] =
                new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] =
                new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);

            return View(enrollment);
        }


        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
    }
}
