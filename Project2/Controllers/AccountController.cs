using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2.Models;
using Project2.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project2.Controllers
{
    public class AccountController : Controller
    {
        private DBContext db;
        private readonly string error = "Incorrect login and/or password";

        public AccountController(DBContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await db.Users
                     .Include(u => u.Role)
                     .FirstOrDefaultAsync(u => u.Email.Equals(model.Email) && u.Password.Equals(model.Password));
                if (user != null)
                {
                    await Authenticate(user); // аутентификация

                    return RedirectToAction("Packages", "Package");
                }
                ModelState.AddModelError("", error);
            }
            return View(model);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> AccountManager(SortState sortOrder)
        {
            var users = await db.Users.Include(u => u.Role).ToListAsync();
            ViewData["Name"] = sortOrder == SortState.NameAsc ? SortState.NameDesc : SortState.NameAsc;
            users = sortOrder switch
            {
                SortState.NameDesc => users.OrderByDescending(u => u.Email).ToList(),
                SortState.NameAsc => users.OrderBy(u => u.Email).ToList(),
                _ => users.OrderBy(p => p.Id).ToList(),
            };
            return View(users);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> AccountManager(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                var users = db.Users.Include(u => u.Role).ToListAsync();
                var filteredUsers = users.GetAwaiter().GetResult().
                    FindAll(u => u.Email.ToUpper().Contains(searchString.ToUpper()));
                return View(filteredUsers);
            }

            return View(await db.Users.Include(u => u.Role).ToListAsync());
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user != null)
                    return View(user);
            }
            return NotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> EditUser(User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Update(user);
                await db.SaveChangesAsync();
                return RedirectToAction("AccountManager");
            }

            return View(user);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> DeleteUser(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                {
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                    return RedirectToAction("AccountManager");
                }
            }
            return NotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email.Equals(model.Email));
                if (user == null)
                {
                    var userRole = await db.Roles.FirstOrDefaultAsync(r => r.Name.Equals("user"));
                    db.Users.Add(new User { Email = model.Email, Password = model.Password, Role = userRole });
                    await db.SaveChangesAsync();
                    //await Authenticate(user); // аутентификация
                    return RedirectToAction("AccountManager");
                }
                else
                    ModelState.AddModelError("", error);
            }
            return View(model);
        }

        private async Task Authenticate(User user)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.Name)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
