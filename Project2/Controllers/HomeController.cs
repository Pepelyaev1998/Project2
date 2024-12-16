using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project2.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Project2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private DBContext db;

        public HomeController(ILogger<HomeController> logger, DBContext context)
        {
            db = context;
        }

        public IActionResult Support()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete]
        public void DeleteMessages()
        {
            db.Messages.RemoveRange(db.Messages);
            db.SaveChanges();
        }

    }
}
