using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ModuleManager.SqlServer.Services
{
	public class ImageService
	{
		private readonly Cloudinary _cloudinary;

		public ImageService(Cloudinary cloudinary)
		{
			_cloudinary = cloudinary;
		}

		public async Task<string> UploadImageAsync(IFormFile file)
		{
			if (file.Length > 0)
			{
				using var stream = file.OpenReadStream();
				var uploadParams = new ImageUploadParams()
				{
					File = new FileDescription(file.FileName, stream)
				};

				var uploadResult = await _cloudinary.UploadAsync(uploadParams);

				if (uploadResult?.SecureUrl != null)
				{
					return uploadResult.SecureUrl.AbsoluteUri;
				}
				else
				{
					// Log the error message for debugging
					Console.WriteLine("Image upload failed or SecureUrl is null.");
					return null;
				}
			}

			return null;
		}
	}
}
