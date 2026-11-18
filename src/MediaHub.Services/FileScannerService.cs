using MediaHub.Core.Factories;
using MediaHub.Data;
using Microsoft.EntityFrameworkCore;

namespace MediaHub.Services;

public class FileScannerService
{
   private readonly string _databasePath;

   public FileScannerService(string databasePath)
   {
      _databasePath = databasePath;
   }
// Скануємо папку й повертаємо кількість нових доданих файлів
   public async Task<int> ScanFolderAsync(string folderPath)
   {
      using var db = new MediaDbContext(_databasePath);
      await db.Database.EnsureCreatedAsync();

      var knownPaths = (await db.MediaFiles.Select(m => m.FilePath).ToListAsync()).ToHashSet();

      var options = new EnumerationOptions { RecurseSubdirectories = true };
      int added = 0;

      foreach (var path in Directory.EnumerateFiles(folderPath, "*.*", options))
      {
         if (!MediaFileFactory.IsSupported(path) || knownPaths.Contains(path))
            continue;

         // Шляхи, які вже є в базі, щоб не додавати дублікати
         var file = MediaFileFactory.Create(path);
         if (file == null)
            continue;

         try
         {
            file.ExtractMetadata();
         }
         catch (Exception)
         {
            continue; // пошкоджений файл пропускаємо
         }

         db.MediaFiles.Add(file);
         added++;
      }

      await db.SaveChangesAsync();
      return added;
   }
}
 
