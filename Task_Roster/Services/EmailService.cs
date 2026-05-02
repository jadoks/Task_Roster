using MailKit.Net.Smtp;
using MimeKit;

namespace Task_Roster.Services;

public class EmailService
{
    // USE YOUR GMAIL HERE
    private const string SenderEmail = "dalevelasco2005@gmail.com";

    // USE YOUR GOOGLE APP PASSWORD HERE
    private const string SenderPassword = "nprlfkgmuqmtoqxs";

    public async Task<bool> SendOtpEmailAsync(
        string receiverEmail,
        string otp)
    {
        try
        {
            MimeMessage email = new();

            email.From.Add(
                MailboxAddress.Parse(SenderEmail));

            email.To.Add(
                MailboxAddress.Parse(receiverEmail));

            email.Subject = "TaskRoster Password Reset OTP";

            email.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family:Arial;padding:20px;'>
                    <h2 style='color:#165A3A;'>TaskRoster</h2>

                    <p>Your password reset OTP is:</p>

                    <h1 style='letter-spacing:5px;color:#165A3A;'>
                        {otp}
                    </h1>

                    <p>
                        This OTP will expire after use.
                    </p>
                </div>"
            };

            using SmtpClient smtp = new();

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                MailKit.Security.SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                SenderEmail,
                SenderPassword);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);

            return true;
        }
        catch
        {
            return false;
        }
    }
}