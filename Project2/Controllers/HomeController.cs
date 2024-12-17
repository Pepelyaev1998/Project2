using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project2.Models;
using Project2.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Project2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        IEntityRepository entityRepository { get; set; }

        public HomeController(IEntityRepository entityRepository)
        {
            this.entityRepository = entityRepository;
        }

        public IActionResult Support()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete]
        public async Task DeleteMessages()
        {
            await entityRepository.Messages.ForEachAsync(message => entityRepository.DeleteEntity(message));
            await entityRepository.SaveChanges();
        }

    }
}
