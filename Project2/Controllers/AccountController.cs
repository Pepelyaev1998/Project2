using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2.Models;
using Project2.Services;
using Project2.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project2.Controllers
{
    public class AccountController : Controller
    {
        //private DBContext db;
        IEntityRepository entityRepository { get; set; }
        private readonly string error = "Incorrect login and/or password";
        private readonly EmailService emailService;

        //public AccountController(DBContext context)
        //{
        //    db = context;
        //}

        public AccountController(IEntityRepository entityRepository, EmailService emailService)
        {
            this.entityRepository = entityRepository;
            this.emailService = emailService;
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
                //var user = await db.Users
                //     .Include(u => u.Role)
                //     .FirstOrDefaultAsync(u => u.Email.Equals(model.Email) && u.Password.Equals(model.Password));
                var user = await entityRepository.Users
                     .Include(u => u.Role)
                     .FirstOrDefaultAsync(u => u.Email.Equals(model.Email) && u.Password.Equals(model.Password));
                if (user != null)
                {
                    await Authenticate(user);

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
            //var users = await db.Users.Include(u => u.Role).ToListAsync();
            var users = await entityRepository.Users.Include(u => u.Role).ToListAsync();
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
        [HttpGet]
        public async Task<IActionResult> AccountTable(SortState sortOrder)
        {
            //var users = await db.Users.Include(u => u.Role).ToListAsync();
            var users = await entityRepository.Users.Include(u => u.Role).ToListAsync();
            ViewData["Name"] = sortOrder == SortState.NameAsc ? SortState.NameDesc : SortState.NameAsc;
            users = sortOrder switch
            {
                SortState.NameDesc => users.OrderByDescending(u => u.Email).ToList(),
                SortState.NameAsc => users.OrderBy(u => u.Email).ToList(),
                _ => users.OrderBy(p => p.Id).ToList(),
            };

            return PartialView(users);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> AccountManager(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                //var users = await db.Users.Include(u => u.Role).ToListAsync();
                var users = await entityRepository.Users.Include(u => u.Role).ToListAsync();
                users = users.FindAll(u => u.Email.ToUpper().Contains(searchString.ToUpper()));
                return View(users);
            }

            //return View(await db.Users.Include(u => u.Role).ToListAsync());
            return View(await entityRepository.Users.Include(u => u.Role).ToListAsync());
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id != null)
            {
                //var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
                var user = await entityRepository.Users.FirstOrDefaultAsync(u => u.Id == id);
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
                //db.Users.Update(user);
                entityRepository.SaveEntity(user);
                //await db.SaveChangesAsync();
                await entityRepository.SaveChanges();
                return RedirectToAction("AccountManager");
            }

            return View(user);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int? id)
        {
            if (id != null)
            {
                var user = await entityRepository.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                {
                    entityRepository.DeleteEntity(user);
                    await entityRepository.SaveChanges();
                    return Ok("success");
                }
            }

            return BadRequest();
        }

        [Authorize(Roles = "admin")]
        [HttpPut]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await entityRepository.Users.FirstOrDefaultAsync(u => u.Email.Equals(model.Email));
                if (user == null)
                {
                    var userRole = await entityRepository.Roles.FirstOrDefaultAsync(r => r.Name.Equals("user"));
                    entityRepository.SaveEntity(new User { Email = model.Email, Password = model.Password, Role = userRole });
                    await entityRepository.SaveChanges();
                    //await emailService.SendEmailAsync(model.Email, model.Password);
                    //await Authenticate(user); // аутентификация

                    return Ok("success");
                }
            }

            ModelState.AddModelError("", error);

            return BadRequest();
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
