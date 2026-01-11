using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSWEBproekt.Data;
using RSWEBproekt.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace RSWEBproekt.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TeachersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Teachers
        // za filtriranje na nastavnicite spored ime, prezime, degree i akademski rang
        public async Task<IActionResult> Index(string firstName, string lastName, string degree, string academicRank)
        {
            var teachers = from t in _context.Teachers select t;

            if (!string.IsNullOrEmpty(firstName))
                teachers = teachers.Where(t => t.FirstName.Contains(firstName));

            if (!string.IsNullOrEmpty(lastName))
                teachers = teachers.Where(t => t.LastName.Contains(lastName));

            if (!string.IsNullOrEmpty(degree))
                teachers = teachers.Where(t => t.Degree.Contains(degree));

            if (!string.IsNullOrEmpty(academicRank))
                teachers = teachers.Where(t => t.AcademicRank.Contains(academicRank));

            return View(await teachers.ToListAsync());
        }

        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null) return NotFound();

            // dali ima kreiran account (user) vrzan so TeacherId
            var hasAccount = await _context.Users.AnyAsync(u => u.TeacherId == teacher.Id);
            ViewBag.HasAccount = hasAccount;

            return View(teacher);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")]
            Teacher teacher,
            IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/images",
                        fileName
                    );

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    teacher.ImagePath = "/images/" + fileName;
                }

                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // POST: Teachers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")]
            Teacher teacher)
        {
            if (id != teacher.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
                _context.Teachers.Remove(teacher);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }

        //  Admin pregled na predmeti na nastavnikot
        public async Task<IActionResult> Courses(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.FirstTeacherCourses)
                .Include(t => t.SecondTeacherCourses)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null) return NotFound();

            return View(teacher);
        }

        
        //  Kreiranje professor account za konkreten Teacher (EMAIL + RESET LINK)
        

        // GET: Teachers/CreateAccount/5
        [HttpGet]
        public async Task<IActionResult> CreateAccount(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            // ako vekje ima account vrzan so teacher
            var already = await _context.Users.AnyAsync(u => u.TeacherId == teacher.Id);
            if (already)
            {
                return RedirectToAction(nameof(Details), new { id = teacher.Id });
            }

            ViewBag.TeacherName = $"{teacher.FirstName} {teacher.LastName}";
            ViewBag.TeacherId = teacher.Id;

            return View();
        }

        // POST: Teachers/CreateAccount/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(int id, string email)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            // teacher da nema vekje akaunt
            var already = await _context.Users.AnyAsync(u => u.TeacherId == teacher.Id);
            if (already)
            {
                ModelState.AddModelError("", "This teacher already has an account.");
            }

            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("", "Email is required.");

            if (!ModelState.IsValid)
            {
                ViewBag.TeacherName = $"{teacher.FirstName} {teacher.LastName}";
                ViewBag.TeacherId = teacher.Id;
                return View();
            }

            // osiguraj role Professor
            if (!await _roleManager.RoleExistsAsync("Professor"))
                await _roleManager.CreateAsync(new IdentityRole("Professor"));

            // ako vekje postoi user so toj email
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                ModelState.AddModelError("", "User with this email already exists.");
                ViewBag.TeacherName = $"{teacher.FirstName} {teacher.LastName}";
                ViewBag.TeacherId = teacher.Id;
                return View();
            }

            //  1) Kreiraj user WITHOUT password
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TeacherId = teacher.Id,
                MustChangePassword = false
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                    ModelState.AddModelError("", err.Description);

                ViewBag.TeacherName = $"{teacher.FirstName} {teacher.LastName}";
                ViewBag.TeacherId = teacher.Id;
                return View();
            }

            await _userManager.AddToRoleAsync(user, "Professor");

            //  2) Generiraj password reset token i napravi link
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { email = user.Email, token = tokenEncoded },
                protocol: Request.Scheme);

            //  3) Zacuvaj go linkot vo Teacher (da ne iscezne)
            teacher.PendingResetLink = resetLink;
            teacher.ResetLinkCreatedOn = DateTime.Now;

            _context.Update(teacher);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = teacher.Id });
        }
    }
}
