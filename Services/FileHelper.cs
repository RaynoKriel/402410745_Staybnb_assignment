namespace _402410745_Staybnb_assignment.Services;

// Checks and saves uploaded files (property photos and guest documents).
public static class FileHelper
{
    public static readonly string[] ImageTypes = { ".jpg", ".jpeg", ".png", ".webp" };
    public static readonly string[] DocumentTypes = { ".jpg", ".jpeg", ".png", ".pdf" };
    public const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    public static bool IsValid(IFormFile file, string[] allowedTypes)
    {
        if (file == null || file.Length == 0 || file.Length > MaxBytes) return false;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedTypes.Contains(extension);
    }

    // Saves the file with a random name and returns that name.
    public static async Task<string> SaveAsync(IFormFile file, string folder)
    {
        Directory.CreateDirectory(folder);
        var name = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
        using var stream = new FileStream(Path.Combine(folder, name), FileMode.Create);
        await file.CopyToAsync(stream);
        return name;
    }
}
