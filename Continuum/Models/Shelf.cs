using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Continuum.Models
{
    public class ShelfCollection
    {
        public List<string> Shelves { get; set; } = new List<string>();
        
        private static readonly string FileName = "shelves.json";
        private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, FileName);
        
        public static async Task<ShelfCollection> LoadAsync()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return new ShelfCollection();
                }
                
                var json = await File.ReadAllTextAsync(FilePath);
                var shelfCollection = JsonSerializer.Deserialize<ShelfCollection>(json) ?? new ShelfCollection();
                
                return shelfCollection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shelves: {ex.Message}");
                return new ShelfCollection();
            }
        }
        
        public async Task SaveAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(this);
                await File.WriteAllTextAsync(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving shelves: {ex.Message}");
            }
        }
        
        public bool AddShelf(string shelfName)
        {
            if (string.IsNullOrWhiteSpace(shelfName))
                return false;
                
            var trimmedName = shelfName.Trim();
            
            if (Shelves.Contains(trimmedName))
                return false;
                
            Shelves.Add(trimmedName);
            return true;
        }
        
        public bool RemoveShelf(string shelfName)
        {
            return Shelves.Remove(shelfName);
        }
    }
}
