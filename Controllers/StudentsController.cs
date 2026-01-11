using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using RSWEBproekt.Data;
using RSWEBproekt.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSWEBproekt.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Students
        public async Task<IActionResult> Index(string index, string firstName, string lastName)
        {
            var students = from s in _context.Students select s;

            if (!string.IsNullOrEmpty(index))
                students = students.Where(s => s.Index.Contains(index));

            if (!string.IsNullOrEmpty(firstName))
                students = students.Where(s => s.FirstName.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                students = students.Where(s => s.LastName.Contains(lastName));

            return View(await students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.StudentId == student.Id);

            bool hasAccount = user != null;
            bool hasPassword = false;

            if (user != null)
                hasPassword = await _userManager.HasPasswordAsync(user);

            ViewBag.HasAccount = hasAccount;
            ViewBag.HasPassword = hasPassword;

            return View(student);
        }


        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                        Console.WriteLine($"{kvp.Key}: {error.ErrorMessage}");
                }
                return View(student);
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);

                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images",
                    fileName
                );

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                student.ImagePath = "/images/" + fileName;
            }

            _context.Add(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Index,FirstName,LastName,EnrollmentYear,CurrentSemester,LevelOfEducation")] Student student)
        {
            if (id != student.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
                _context.Students.Remove(student);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }

        // 
        //  Kreiranje student account (EMAIL + RESET LINK) - isto kako Teacher
        

        // GET: Students/CreateAccount/5
        [HttpGet]
        public async Task<IActionResult> CreateAccount(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            // ako vekje ima account vrzan so student
            var already = await _context.Users.AnyAsync(u => u.StudentId == student.Id);
            if (already)
            {
                return RedirectToAction(nameof(Details), new { id = student.Id });
            }

            ViewBag.StudentName = $"{student.FirstName} {student.LastName}";
            ViewBag.StudentId = student.Id;

            return View();
        }

        // POST: Students/CreateAccount/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(int id, string email)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            // student da nema vekje akaunt
            var already = await _context.Users.AnyAsync(u => u.StudentId == student.Id);
            if (already)
            {
                ModelState.AddModelError("", "This student already has an account.");
            }

            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("", "Email is required.");

            if (!ModelState.IsValid)
            {
                ViewBag.StudentName = $"{student.FirstName} {student.LastName}";
                ViewBag.StudentId = student.Id;
                return View();
            }

            // osiguraj role Student
            if (!await _roleManager.RoleExistsAsync("Student"))
                await _roleManager.CreateAsync(new IdentityRole("Student"));

            // ako vekje postoi user so toj email
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                ModelState.AddModelError("", "User with this email already exists.");
                ViewBag.StudentName = $"{student.FirstName} {student.LastName}";
                ViewBag.StudentId = student.Id;
                return View();
            }

            //  1) Kreiraj user WITHOUT password
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                StudentId = student.Id,
                MustChangePassword = false
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                    ModelState.AddModelError("", err.Description);

                ViewBag.StudentName = $"{student.FirstName} {student.LastName}";
                ViewBag.StudentId = student.Id;
                return View();
            }

            await _userManager.AddToRoleAsync(user, "Student");

            // 2) Generiraj password reset token i napravi link
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { email = user.Email, token = tokenEncoded },
                protocol: Request.Scheme);

            // 3) Zacuvaj go linkot vo Student (da ne iscezne)
            student.PendingResetLink = resetLink;
            student.ResetLinkCreatedOn = DateTime.Now;

            _context.Update(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = student.Id });
        }
        
        // STUDENT – My Enrolled Courses
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyEnrolledCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StudentId == null)
                return Forbid();

            var studentId = user.StudentId.Value;

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.Year)
                .ThenBy(e => e.Semester)
                .ToListAsync();

            return View(enrollments);
        }


    }
}
