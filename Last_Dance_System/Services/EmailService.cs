using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Last_Dance_System.Models;

namespace Last_Dance_System.Services
{
    public class EmailService
    {
        public async Task SendBookingConfirmationAsync(Registration registration, Booking booking)
        {
            // Read SMTP from Web.config via <system.net> or appSettings; this uses simple settings from appSettings for brevity
            var smtpHost = System.Configuration.ConfigurationManager.AppSettings["Smtp:Host"];
            var smtpPort = int.TryParse(System.Configuration.ConfigurationManager.AppSettings["Smtp:Port"], out var p) ? p : 25;
            var smtpUser = System.Configuration.ConfigurationManager.AppSettings["Smtp:User"];
            var smtpPass = System.Configuration.ConfigurationManager.AppSettings["Smtp:Pass"];
            var from = System.Configuration.ConfigurationManager.AppSettings["Smtp:From"] ?? "no-reply@example.com";

            var to = registration.ApplicationUser != null ? registration.ApplicationUser.Email : null;

            if (string.IsNullOrWhiteSpace(to))
                return;

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                if (!string.IsNullOrEmpty(smtpUser))
                {
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.EnableSsl = true;
                }

                var subject = $"Booking confirmation #{booking.BookingId}";
                var body = $"Hello {registration.LastName},\n\n" +
                           $"Your booking (ID {booking.BookingId}) for {booking.LessonSchedule?.LessonType?.LessonTypeName ?? "lesson"} on {booking.LessonSchedule?.LessonDate:d} at {booking.LessonSchedule?.StartTime} has been confirmed.\n\n" +
                           "Thank you.";

                var mail = new MailMessage(from, to, subject, body);
                await client.SendMailAsync(mail);
            }
        }
    }
}