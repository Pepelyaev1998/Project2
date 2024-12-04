using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Project2.Controllers
{
    public class PackageController : Controller
    {
        private DBContext db;

        public PackageController(DBContext context)
        {
            db = context;
        }

        [HttpGet]
        public async Task<IActionResult> Packages(SortState sortOrder)
        {
            var packages = await db.Packages.ToListAsync();
            ViewData["LastDateOfUpdate"] = sortOrder == SortState.LastDateOfUpdateAsc ? SortState.LastDateOfUpdateDesc : SortState.LastDateOfUpdateAsc;
            packages = sortOrder switch
            {
                SortState.LastDateOfUpdateDesc => packages.OrderByDescending(p => p.LastDateOfUpdate).ToList(),
                SortState.LastDateOfUpdateAsc => packages.OrderBy(p => p.LastDateOfUpdate).ToList(),
                _ => packages.OrderBy(p => p.TrackNumber).ToList(),
            };
            return View(packages);
        }

        [HttpPost]
        public async Task<IActionResult> Packages(string searchString)
        {
            var packages = await db.Packages.ToListAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                packages = packages.
                    FindAll(p => p.TrackNumber.ToUpper().Contains(searchString.ToUpper()));
            }

            return View(packages);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> EditPackage(int? id)
        {
            if (id != null)
            {
                var package = await db.Packages.FirstOrDefaultAsync(p => p.Id == id);
                if (package != null)
                    return View(package);
            }
            return NotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> EditPackage(Package package)
        {
            if (ModelState.IsValid)
            {
                package.LastDateOfUpdate = DateTime.Now;
                db.Packages.Update(package);
                await db.SaveChangesAsync();
                return RedirectToAction("Package");
            }

            return View(package);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> DeletePackage(int? id)
        {
            if (id != null)
            {
                var package = await db.Packages.FirstOrDefaultAsync(p => p.Id == id);
                if (package != null)
                {
                    db.Packages.Remove(package);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Packages");
                }
            }
            return NotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult AddPackage()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPackage(Package packageModel)
        {
            if (ModelState.IsValid)
            {
                var package = await db.Packages.FirstOrDefaultAsync(p => p.TrackNumber.Equals(packageModel.TrackNumber));
                if (package == null)
                {
                    packageModel.LastDateOfUpdate = DateTime.Now;
                    db.Packages.Add(packageModel);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Packages");
                }
                else
                    ModelState.AddModelError("", "Icorrect data");
            }
            return View(packageModel);
        }
    }
}
