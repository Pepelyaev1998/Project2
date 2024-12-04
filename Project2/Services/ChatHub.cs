using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project2.Services
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly string adminEmail = "admin@mail.ru";

        public async Task SendMessage(string message, string to)
        {
            if (string.IsNullOrEmpty(to)) to = adminEmail;
            var userName = Context.User.Identity.Name;
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
            if (Context.UserIdentifier != to) // если получатель и текущий пользователь не совпадают
                await Clients.User(Context.UserIdentifier).SendAsync("ReceiveMessage", message, userName);
            await Clients.User(to).SendAsync("ReceiveMessage", message, userName);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.User(Context.UserIdentifier).SendAsync("Notify", $"Welcome to our technical support team. Describe your problem.");
            await base.OnConnectedAsync();
        }
    }
}
