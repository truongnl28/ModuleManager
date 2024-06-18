using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuleManager.SqlServer.Data;
using ModuleManager.SqlServer.Models;
using ModuleManager.SqlServer.Services;
using System.Security.Claims;

namespace ModuleManager.SqlServer.Controllers
{
	[Authorize]
	public class UserController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly ImageService _imageService;

		public UserController(ApplicationDbContext context, ImageService imageService)
		{
			_context = context;
			_imageService = imageService;
		}

		public async Task<IActionResult> Index()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _context.Users.FindAsync(int.Parse(userId));

			if (user == null)
			{
				return NotFound();
			}

			var model = new EditUserViewModel
			{
				UserId = user.UserId,
				UserName = user.UserName,
				Email = user.Email,
				ImageUrl = user.ImageUrl
			};

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Edit()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _context.Users.FindAsync(int.Parse(userId));

			if (user == null)
			{
				return NotFound();
			}

			var model = new EditUserViewModel
			{
				UserId = user.UserId,
				UserName = user.UserName,
				Email = user.Email,
				ImageUrl = user.ImageUrl
			};

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(EditUserViewModel model)
		{
			if (ModelState.IsValid)
			{
				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var user = await _context.Users.FindAsync(int.Parse(userId));

				if (user == null)
				{
					return NotFound();
				}

				user.UserName = model.UserName;

				// Check if a new image file is provided
				if (model.ImageFile != null && model.ImageFile.Length > 0)
				{
					var uploadResult = await _imageService.UploadImageAsync(model.ImageFile);
					if (uploadResult != null)
					{
						// Update user's image URL only if a new image is uploaded
						user.ImageUrl = uploadResult;
					}
				}

				_context.Update(user);
				await _context.SaveChangesAsync();

				// Update Claims
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(ClaimTypes.Name));
				claimsIdentity.RemoveClaim(claimsIdentity.FindFirst("ImageUrl"));
				claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
				claimsIdentity.AddClaim(new Claim("ImageUrl", user.ImageUrl ?? string.Empty));

				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

				TempData["SuccessMessage"] = "Thông tin đã được cập nhật thành công.";
				return RedirectToAction("Index");
			}

			// Truyền lại thông tin Email và ImageUrl khi validation thất bại
			var originalUser = await _context.Users.FindAsync(model.UserId);
			if (originalUser != null)
			{
				model.Email = originalUser.Email;
				model.ImageUrl = originalUser.ImageUrl;
			}

			return View(model);
		}
	}
}