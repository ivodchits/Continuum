using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Continuum.Models
{
    public class BookMetadata
    {
        // Basic metadata properties
        public string Shelf { get; set; } = "None"; // Default shelf
        
        // In the future, we can add more metadata properties here, such as:
        // public int LastReadPage { get; set; }
        // public DateTime LastReadDate { get; set; }
        // public List<Bookmark> Bookmarks { get; set; }
        // public Dictionary<string, string> CustomMetadata { get; set; }
        
        // Get the metadata file path for a specific book file path
        public static string GetMetadataFilePath(string bookFilePath)
        {
            var fileName = Path.GetFileName(bookFilePath);
            return Path.Combine(
                Path.GetDirectoryName(bookFilePath) ?? string.Empty,
                $"{fileName}.metadata.json");
        }
        
        // Save metadata for a book
        public static async Task SaveAsync(string bookFilePath, BookMetadata metadata)
        {
            try
            {
                var metadataPath = GetMetadataFilePath(bookFilePath);
                var json = JsonSerializer.Serialize(metadata);
                await File.WriteAllTextAsync(metadataPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving book metadata: {ex.Message}");
            }
        }
        
        // Load metadata for a book, or create a new metadata object if none exists
        public static async Task<BookMetadata> LoadAsync(string bookFilePath)
        {
            try
            {
                var metadataPath = GetMetadataFilePath(bookFilePath);
                
                if (!File.Exists(metadataPath))
                {
                    return new BookMetadata();
                }
                
                var json = await File.ReadAllTextAsync(metadataPath);
                var metadata = JsonSerializer.Deserialize<BookMetadata>(json) ?? new BookMetadata();
                
                return metadata;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading book metadata: {ex.Message}");
                return new BookMetadata();
            }
        }
    }
}
