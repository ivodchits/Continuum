using System;
using System.IO;

namespace Continuum.Models
{
    public class Book
    {
        public required string Title { get; set; }
        public required string Author { get; set; }
        public string? CoverImage { get; set; }
        public string Shelf { get; set; } = "None"; // Default shelf
        public bool IsAudiobook { get; set; } = false;
        public required string FilePath { get; set; } // Path to the book file
        public required string FileExtension { get; set; } // File extension
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public long FileSize { get; set; } // File size in bytes

        // Helper method to determine if this is an audiobook based on extension
        public static bool IsAudiobookExtension(string extension)
        {
            return extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase);
        }

        // Helper method to check if the extension is a supported e-book format
        public static bool IsEbookExtension(string extension)
        {
            return extension.Equals(".epub", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".mobi", StringComparison.OrdinalIgnoreCase);
        }
        
        // Helper method to check if the extension is supported
        public static bool IsSupportedExtension(string extension)
        {
            return IsEbookExtension(extension) || IsAudiobookExtension(extension);
        }
    }
}
