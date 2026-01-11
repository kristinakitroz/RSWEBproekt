using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RSWEBproekt.Data;
using RSWEBproekt.Models;
using RSWEBproekt.Models.ViewModels;
using System.Text;

namespace RSWEBproekt.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        // konstruktor
        public AccountController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        
        // Force change password (koga korisnikot e najaven)
        [HttpGet]
        public IActionResult ForceChangePassword()
        {
            return View(new ForceChangePasswordVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceChangePassword(ForceChangePasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var result = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(vm);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            
            return RedirectToAction("MyCourses", "Professor");
        }

        
        // Reset password (simuliran email link)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return BadRequest("Invalid password reset link.");

            var vm = new ResetPasswordVM
            {
                Email = email,
                Token = token
            };

            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(vm);
            }

            // decode token
            string tokenDecoded;
            try
            {
                var bytes = WebEncoders.Base64UrlDecode(vm.Token);
                tokenDecoded = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid token.");
                return View(vm);
            }

            var result = await _userManager.ResetPasswordAsync(user, tokenDecoded, vm.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(vm);
            }

            //po uspesen resetPassword -> izbrisi PendingResetLink kaj Teacher i/ili Student
            bool changed = false;

            if (user.TeacherId != null)
            {
                var teacher = await _context.Teachers.FindAsync(user.TeacherId.Value);
                if (teacher != null)
                {
                    teacher.PendingResetLink = null;
                    teacher.ResetLinkCreatedOn = null;
                    _context.Update(teacher);
                    changed = true;
                }
            }

            if (user.StudentId != null)
            {
                var student = await _context.Students.FindAsync(user.StudentId.Value);
                if (student != null)
                {
                    student.PendingResetLink = null;
                    student.ResetLinkCreatedOn = null;
                    _context.Update(student);
                    changed = true;
                }
            }

            if (changed)
                await _context.SaveChangesAsync();

            TempData["Message"] = "Password set successfully. You can now login.";
            return Redirect("/Identity/Account/Login");
        }
    }
}
