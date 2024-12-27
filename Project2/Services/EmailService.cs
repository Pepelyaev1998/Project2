using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace Project2.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string email, string password)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("The delivery company", "admin@mail.ru"));
            message.To.Add(new MailboxAddress(email, ""));
            message.Subject = "Welcome to our company!";
            message.Body = new BodyBuilder() { HtmlBody = "<div style=\"color: black;\">Your email: " + email + "Your password: +" + password + "</div>" }.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true); //либо использум порт 465
                await client.AuthenticateAsync("login", "password");
                await client.SendAsync(message);

                await client.DisconnectAsync(true);
            }

        }
    }
}
