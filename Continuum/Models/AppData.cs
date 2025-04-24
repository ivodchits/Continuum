using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Continuum.Models
{
    public class AppData
    {
        // Original shelves data
        public List<string> Shelves { get; set; } = new List<string>();
        
        // New UI preferences
        public bool IsGridView { get; set; } = false;
        
        // File configuration
        private static readonly string FileName = "data.json";
        private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, FileName);
        
        public static async Task<AppData> LoadAsync()
        {
            try
            {
                // Try to load from the new data.json file first
                if (File.Exists(FilePath))
                {
                    var json = await File.ReadAllTextAsync(FilePath);
                    var appData = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
                    return appData;
                }
                
                // If no data.json exists, try to migrate from the old shelves.json
                var oldFilePath = Path.Combine(FileSystem.AppDataDirectory, "shelves.json");
                if (File.Exists(oldFilePath))
                {
                    var oldJson = await File.ReadAllTextAsync(oldFilePath);
                    var shelfCollection = JsonSerializer.Deserialize<ShelfCollection>(oldJson);
                    
                    if (shelfCollection != null)
                    {
                        // Migrate data to new format
                        var appData = new AppData
                        {
                            Shelves = shelfCollection.Shelves,
                            IsGridView = false // Default to list view during migration
                        };
                        
                        // Save in new format
                        await appData.SaveAsync();
                        return appData;
                    }
                }
                
                // If neither file exists, return a new instance
                return new AppData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading app data: {ex.Message}");
                return new AppData();
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
                Console.WriteLine($"Error saving app data: {ex.Message}");
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
