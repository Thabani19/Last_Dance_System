using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace Last_Dance_System.Services
{
    public class EmailService
    {
        private readonly string emailFrom;
        private readonly string emailPassword;
        private readonly string smtpHost;
        private readonly int smtpPort;

        public EmailService()
        {
            emailFrom = ConfigurationManager.AppSettings["EmailFrom"];
            emailPassword = ConfigurationManager.AppSettings["EmailPassword"];
            smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            smtpPort = int.Parse(
                ConfigurationManager.AppSettings["SmtpPort"]
            );
        }

        public void SendEmail(
            string recipientEmail,
            string subject,
            string body)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom);

                mail.To.Add(recipientEmail);

                mail.Subject = subject;

                mail.Body = body;

                mail.IsBodyHtml = false;

                using (SmtpClient smtp =
                    new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.EnableSsl = true;

                    smtp.Credentials =
                        new NetworkCredential(
                            emailFrom,
                            emailPassword
                        );

                    smtp.Send(mail);
                }
            }
        }
    }
}