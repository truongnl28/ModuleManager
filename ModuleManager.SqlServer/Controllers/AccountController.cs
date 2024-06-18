using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ModuleManager.SqlServer.Data;
using ModuleManager.SqlServer.Models;
using ModuleManager.SqlServer.Settings;
using ModuleManager.SqlServer.Services;

public class AccountController : Controller
{
	private readonly ApplicationDbContext _context;
	private readonly SmtpSettings _smtpSettings;
	private readonly EmailService _emailService;
	private readonly ImageService _imageService;

	public AccountController(ApplicationDbContext context, IOptions<SmtpSettings> smtpSettings, ImageService imageService, EmailService emailService)
	{
		_context = context;
		_smtpSettings = smtpSettings.Value;
		_imageService = imageService;
		_emailService = emailService;
	}

	[HttpGet]
	public IActionResult Register()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Register(RegisterViewModel model)
	{
		if (ModelState.IsValid)
		{
			if (await _context.Users.AnyAsync(u => u.Email == model.Email))
			{
				ModelState.AddModelError(string.Empty, "Email đã được sử dụng.");
				return View(model);
			}

			string imageUrl = null;
			if (model.Image != null)
			{
				imageUrl = await _imageService.UploadImageAsync(model.Image);

				if (imageUrl == null)
				{
					ModelState.AddModelError(string.Empty, "Không thể tải hình ảnh lên.");
					return View(model);
				}
			}

			var user = new User
			{
				UserName = model.UserName,
				Email = model.Email,
				PasswordHash = HashPassword(model.Password),
				IsActive = true,
				CreatedAt = DateTime.Now,
				ImageUrl = imageUrl // Save the image URL
			};

			_context.Add(user);
			await _context.SaveChangesAsync();

			await _emailService.SendRegistrationConfirmationEmail(user.Email, user.UserName, model.Password);

			TempData["SuccessMessage"] = "Bạn đã đăng ký thành công.";
			TempData["EmailMessage"] = "Email xác nhận đã được gửi.";

			return RedirectToAction("RegisterSuccess");
		}

		return View(model);
	}

	[HttpGet]
	public IActionResult RegisterSuccess()
	{
		return View();
	}

	[HttpGet]
	public IActionResult Login()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Login(LoginViewModel model)
	{
		if (ModelState.IsValid)
		{
			var user = await _context.Users
				.Include(u => u.UserRoles)
				.ThenInclude(ur => ur.Role)
				.SingleOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == HashPassword(model.Password));

			if (user != null)
			{
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim("ImageUrl", user.ImageUrl ?? string.Empty)
				};

				var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
				foreach (var role in roles)
				{
					claims.Add(new Claim(ClaimTypes.Role, role));
				}

				var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
				var authProperties = new AuthenticationProperties
				{
					IsPersistent = model.RememberMe
				};

				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

				if (roles.Contains("Admin"))
				{
					return RedirectToAction("Index", "Admin");
				}
				else
				{
					return RedirectToAction("Index", "User");
				}
			}

			ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
		}

		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		HttpContext.Session.Clear();
		return RedirectToAction("Login");
	}

	private string HashPassword(string password)
	{
		using (var sha256 = SHA256.Create())
		{
			var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
			var builder = new StringBuilder();
			foreach (var t in bytes)
			{
				builder.Append(t.ToString("x2"));
			}
			return builder.ToString();
		}
	}
}
