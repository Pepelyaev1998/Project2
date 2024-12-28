using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Project2.Models;
using Project2.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Project2.Controllers
{
    public class PackageController : Controller
    {
        IEntityRepository entityRepository { get; set; }
        private readonly IHostingEnvironment hostingEnvironment;

        public PackageController(IEntityRepository entityRepository, IHostingEnvironment hostingEnvironment)
        {
            this.entityRepository = entityRepository;
            this.hostingEnvironment = hostingEnvironment;

        }

        [HttpGet]
        public async Task<IActionResult> Packages()
        {
            var packages = await entityRepository.Packages.ToListAsync();

            return View(packages);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> PackageTable()
        {
            var packages = await entityRepository.Packages.ToListAsync();

            return PartialView(packages);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> EditPackage(int? id)
        {
            if (id != null)
            {
                var package = await entityRepository.Packages.FirstOrDefaultAsync(p => p.Id == id);
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
                entityRepository.SaveEntity(package);
                await entityRepository.SaveChanges();

                return RedirectToAction("Packages");
            }

            return View(package);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete]
        public async Task<IActionResult> DeletePackage(int? id)
        {
            if (id != null)
            {
                var package = await entityRepository.Packages.FirstOrDefaultAsync(p => p.Id == id);
                if (package != null)
                {
                    entityRepository.DeleteEntity(package);
                    await entityRepository.SaveChanges();

                    return Ok("success");
                }
            }
            return BadRequest();
        }

        [Authorize(Roles = "admin")]
        [HttpPut]
        public async Task<IActionResult> AddPackage(Package packageModel)
        {
            if (ModelState.IsValid)
            {
                var package = await entityRepository.Packages.FirstOrDefaultAsync(p => p.TrackNumber.Equals(packageModel.TrackNumber));
                if (package == null)
                {
                    packageModel.LastDateOfUpdate = DateTime.Now;
                    entityRepository.SaveEntity(packageModel);
                    await entityRepository.SaveChanges();
                    return Ok("success");
                }
            }

            ModelState.AddModelError("", "Icorrect data");

            return BadRequest();
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
            var packages = await entityRepository.Packages.ToListAsync();

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
