using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Project2.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project2.Services
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly string adminEmail = "admin@mail.ru";
        private DBContext db;

        public ChatHub(DBContext context)
        {
            db = context;
        }

        public async Task SendMessage(string message, string to)
        {
            if (string.IsNullOrEmpty(to)) to = adminEmail;
            var userName = Context.User.Identity.Name;
            var isAdmin = Context.User.FindFirst(x => x.Type.Equals(ClaimsIdentity.DefaultRoleClaimType)).Value.Equals("admin");
            var sentMessage = isAdmin ? message + " Message to: " + to : message;
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
            if (Context.UserIdentifier != to) // если получатель и текущий пользователь не совпадают
                await Clients.User(Context.UserIdentifier).SendAsync("ReceiveMessage", sentMessage, userName);
            await Clients.User(to).SendAsync("ReceiveMessage", sentMessage, userName);
            var CurrentUser = db.Users.FirstOrDefaultAsync(u => u.Email.Equals(userName));
            var toUser = db.Users.FirstOrDefaultAsync(u => u.Email.Equals(to));

            if (CurrentUser != null || toUser != null)
            {
                db.Messages.Add(new Message() { FromUserId = CurrentUser.GetAwaiter().GetResult().Id, ToUserId = toUser.GetAwaiter().GetResult().Id, SentMessage = message });
                await db.SaveChangesAsync();
            }
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.User(Context.UserIdentifier).SendAsync("Notify", $"Welcome to our technical support team.");
            var CurrentUserId = db.Users.FirstOrDefaultAsync(u => u.Email.Equals(Context.User.Identity.Name)).GetAwaiter().GetResult().Id;
            var messages = db.Messages.Where(m => m.FromUserId == CurrentUserId || m.ToUserId == CurrentUserId).ToList();
            var isAdmin = Context.User.FindFirst(x => x.Type.Equals(ClaimsIdentity.DefaultRoleClaimType)).Value.Equals("admin");
            foreach (var message in messages)
            {
                var ToUserEmail = db.Users.FirstOrDefaultAsync(u => u.Id == message.ToUserId).GetAwaiter().GetResult();
                var FromUserEmail = db.Users.FirstOrDefaultAsync(u => u.Id == message.FromUserId).GetAwaiter().GetResult();
                if (message.FromUserId == CurrentUserId)
                {
                    var sentMessage = isAdmin ? message.SentMessage + " Message to: " + ToUserEmail.Email : message.SentMessage;
                    await Clients.User(Context.UserIdentifier).SendAsync("ReceiveMessage", sentMessage, Context.User.Identity.Name);
                }
                else if (message.ToUserId == CurrentUserId)
                {
                    await Clients.User(Context.UserIdentifier).SendAsync("ReceiveMessage", message.SentMessage, FromUserEmail.Email);
                }
            }

            await base.OnConnectedAsync();
        }
    }
}
