using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ModuleManager.SqlServer.Settings;

public class EmailService
{
	private readonly SmtpSettings _smtpSettings;
	private readonly static CancellationToken ct = new CancellationToken();

	public EmailService(IOptions<SmtpSettings> smtpSettings)
	{
		_smtpSettings = smtpSettings.Value;
	}

	public async Task SendEmail(string userEmail, string title, string message)
	{
		try
		{
			var mail = new MimeMessage();

			// Người gửi
			mail.From.Add(new MailboxAddress(_smtpSettings.NameApp, _smtpSettings.EMailAddress));
			mail.Sender = new MailboxAddress(_smtpSettings.NameApp, _smtpSettings.EMailAddress);

			// Người nhận
			mail.To.Add(MailboxAddress.Parse(userEmail));

			// Nội dung gửi
			var body = new BodyBuilder();
			mail.Subject = $"{title}";

			// Có thể dùng HTML để format nội dung gửi
			body.HtmlBody = $"<p>{message}</p>";
			mail.Body = body.ToMessageBody();

			using var smtp = new MailKit.Net.Smtp.SmtpClient();

			if (_smtpSettings.UseSSL)
			{
				await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.SslOnConnect, ct);
			}
			else if (_smtpSettings.UseStartTls)
			{
				await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls, ct);
			}

			await smtp.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password, ct);
			await smtp.SendAsync(mail, ct);
			await smtp.DisconnectAsync(true, ct);
		}
		catch (Exception ex)
		{
			// Log the exception or handle it as needed
			Console.WriteLine($"Error sending email: {ex.Message}");
		}
	}

	public async Task SendRegistrationConfirmationEmail(string email, string userName, string password)
	{
		var subject = "Xác nhận đăng ký";
		var message = $"<p>Chào {userName},</p>" +
					  $"<p>Bạn đã đăng ký thành công với tài khoản:</p>" +
					  $"<p><strong>Email:</strong> {email}</p>" +
					  $"<p><strong>Mật khẩu:</strong> {password}</p>";

		await SendEmail(email, subject, message);
	}

	public async Task SendAccountDeletionEmail(string email, string userName)
	{
		var subject = "Tài khoản của bạn đã bị xóa";
		var message = $"<p>Chào {userName},</p>" +
					  $"<p>Tài khoản của bạn đã bị xóa bởi quản trị viên.</p>";

		await SendEmail(email, subject, message);
	}
}
