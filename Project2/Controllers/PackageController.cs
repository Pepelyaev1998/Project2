﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Project2.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Project2.Controllers
{
    public class PackageController : Controller
    {
        private DBContext db;
        private readonly IHostingEnvironment hostingEnvironment;

        public PackageController(DBContext context, IHostingEnvironment hostingEnvironment)
        {
            db = context;
            this.hostingEnvironment = hostingEnvironment;
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

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> ExportToFile()
        {
            string webRootFolder = hostingEnvironment.WebRootPath;
            string fileName = @"Package.xlsx";
            var memory = new MemoryStream();
            var workbook = new XSSFWorkbook();
            var excelSheet = workbook.CreateSheet("Packages");
            excelSheet.DefaultColumnWidth = 30;
            var packages = await db.Packages.ToListAsync();

            using (var fs = new FileStream(Path.Combine(webRootFolder, fileName), FileMode.Create, FileAccess.Write))
            {

                IRow row = excelSheet.CreateRow(0);
                row.CreateCell(0).SetCellValue("Track Number");
                row.CreateCell(1).SetCellValue("Starting Point");
                row.CreateCell(2).SetCellValue("End Point");
                row.CreateCell(3).SetCellValue("Last Location");
                row.CreateCell(4).SetCellValue("Current State");
                row.CreateCell(5).SetCellValue("Last date of update");

                for (int r = 1; r <= packages.Count; r++)
                {
                    row = excelSheet.CreateRow(r);
                    var item = packages[r - 1];
                    row.CreateCell(0).SetCellValue(item.TrackNumber);
                    row.CreateCell(1).SetCellValue(item.StartingPoint);
                    row.CreateCell(2).SetCellValue(item.EndPoint);
                    //row.GetCell(2).CellStyle.Alignment = HorizontalAlignment.Left;
                    row.CreateCell(3).SetCellValue(item.LastLocation);
                    row.CreateCell(4).SetCellValue(item.CurrentState);
                    row.CreateCell(5).SetCellValue(item.LastDateOfUpdate.ToShortDateString() + " " + item.LastDateOfUpdate.ToShortTimeString());
                }
                workbook.Write(fs);
            }
            using (var stream = new FileStream(Path.Combine(webRootFolder, fileName), FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
