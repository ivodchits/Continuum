using Continuum.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VersOne.Epub;

namespace Continuum.Services
{
    public class EpubReaderService : IBookReaderService
    {
        private EpubBook? _epubBook;
        private ObservableCollection<string> _tableOfContents = new();
        private ObservableCollection<string> _chapters = new();
        private Dictionary<string, string> _cssCache = new();
        private Dictionary<string, string> _imageCache = new();
        private bool _isBookLoaded;
        private readonly PagedContentService _pagedContentService;
        
        public EpubReaderService(PagedContentService pagedContentService)
        {
            _pagedContentService = pagedContentService ?? throw new ArgumentNullException(nameof(pagedContentService));
        }
        
        public bool CanHandleBook(Book book)
        {
            if (book == null || string.IsNullOrEmpty(book.FilePath))
                return false;
                
            string extension = Path.GetExtension(book.FilePath).ToLowerInvariant();
            return extension == ".epub";
        }
        
        public async Task<bool> LoadBookAsync(Book book)
        {
            if (book == null || string.IsNullOrEmpty(book.FilePath) || !File.Exists(book.FilePath))
                return false;
                
            try
            {
                // Load the EPUB book
                _epubBook = await EpubReader.ReadBookAsync(book.FilePath);
                
                if (_epubBook == null)
                    return false;
                
                // Clear existing data
                _chapters.Clear();
                _tableOfContents.Clear();
                _cssCache.Clear();
                _imageCache.Clear();
                
                // Add all chapters from reading order
                foreach (var chapter in _epubBook.ReadingOrder)
                {
                    _chapters.Add(chapter.FilePath);
                }
                
                // Load table of contents
                if (_epubBook.Navigation != null)
                {
                    foreach (var item in _epubBook.Navigation)
                    {
                        _tableOfContents.Add(item.Title);
                    }
                }
                
                // Pre-cache CSS and image files for better performance
                await Task.Run(() => CacheContentFiles());
                
                _isBookLoaded = true;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading EPUB: {ex.Message}");
                _isBookLoaded = false;
                return false;
            }
        }
        
        public ObservableCollection<string> GetTableOfContents()
        {
            return _tableOfContents;
        }
        
        public ObservableCollection<string> GetChapters()
        {
            return _chapters;
        }
        
        public async Task<string> LoadChapterByIndexAsync(int index)
        {
            if (_epubBook == null || !_isBookLoaded || index < 0 || index >= _chapters.Count)
                return GenerateErrorHtml("Invalid chapter index");

            try
            {
                // Get the chapter from reading order
                var chapter = _epubBook.ReadingOrder[index];
                
                // Get the HTML content and process it
                string html = await Task.Run(() => ProcessChapterHtml(chapter.Content, chapter.FilePath));
                
                // Apply pagination to the processed HTML content
                return _pagedContentService.PreparePagedContent(html);
            }
            catch (Exception ex)
            {
                return GenerateErrorHtml($"Error loading chapter: {ex.Message}");
            }
        }
        
        public async Task<string> NavigateToChapterAsync(string navigationItem)
        {
            if (_epubBook == null || !_isBookLoaded)
                return GenerateErrorHtml("Invalid navigation item");

            try
            {
                if (string.IsNullOrEmpty(navigationItem))
                    return GenerateErrorHtml("Empty navigation item");
                
                // Find the navigation item by title
                var navItem = _epubBook.Navigation?.FirstOrDefault(n => n.Title == navigationItem);
                if (navItem == null || navItem.HtmlContentFile == null)
                    return GenerateErrorHtml("Navigation item not found");
                
                // Find the chapter in reading order
                int index = -1;
                for (int i = 0; i < _epubBook.ReadingOrder.Count; i++)
                {
                    if (_epubBook.ReadingOrder[i].FilePath == navItem.HtmlContentFile.FilePath)
                    {
                        index = i;
                        break;
                    }
                }
                
                if (index >= 0)
                {
                    return await LoadChapterByIndexAsync(index);
                }
                
                // If not found in reading order, try to load directly
                string html = await Task.Run(() => ProcessChapterHtml(navItem.HtmlContentFile.Content, navItem.HtmlContentFile.FilePath));
                
                // Apply pagination to the processed HTML content
                return _pagedContentService.PreparePagedContent(html);
            }
            catch (Exception ex)
            {
                return GenerateErrorHtml($"Error navigating to chapter: {ex.Message}");
            }
        }
        
        public bool HasPreviousChapter(int currentIndex)
        {
            return currentIndex > 0;
        }
        
        public bool HasNextChapter(int currentIndex)
        {
            return currentIndex < _chapters.Count - 1;
        }
        
        public int GetChapterCount()
        {
            return _chapters.Count;
        }
        
        private string GenerateErrorHtml(string errorMessage)
        {
            return $"<html><body><h1>Error</h1><p>{errorMessage}</p></body></html>";
        }
        
        private void CacheContentFiles()
        {
            if (_epubBook == null) return;
            
            // Cache CSS files
            try
            {
                foreach (var item in _epubBook.Content.Css.Local)
                {
                    _cssCache[item.FilePath] = item.Content;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error caching CSS: {ex.Message}");
            }
            
            // Cache image files
            try
            {
                foreach (var item in _epubBook.Content.Images.Local)
                {
                    string mimeType = GetMimeTypeFromFilePath(item.FilePath);
                    string base64 = Convert.ToBase64String(item.Content);
                    _imageCache[item.FilePath] = $"data:{mimeType};base64,{base64}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error caching images: {ex.Message}");
            }
        }
        
        private string GetMimeTypeFromFilePath(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
        
        private string ProcessChapterHtml(string html, string filePath)
        {
            if (string.IsNullOrEmpty(html)) return html;
            
            try
            {
                string baseDirectory = Path.GetDirectoryName(filePath)?.Replace('\\', '/') ?? string.Empty;
                
                // Process CSS links - replace with inline styles
                html = ProcessCssLinks(html, baseDirectory);
                
                // Process images - replace with base64
                html = ProcessImages(html, baseDirectory);
                
                return html;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing HTML: {ex.Message}");
                return html;
            }
        }
        
        private string ProcessCssLinks(string html, string baseDirectory)
        {
            // Find all CSS link tags using regex
            var cssLinkRegex = new Regex(@"<link[^>]*rel=[""']stylesheet[""'][^>]*href=[""']([^""']+)[""'][^>]*>", RegexOptions.IgnoreCase);
            var matches = cssLinkRegex.Matches(html);
            
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string href = match.Groups[1].Value;
                    string cssPath = ResolvePath(href, baseDirectory);
                    
                    if (_cssCache.TryGetValue(cssPath, out string? cssContent) && cssContent != null)
                    {
                        // Replace the link tag with inline style
                        string linkTag = match.Value;
                        string styleTag = $"<style type=\"text/css\">{cssContent}</style>";
                        html = html.Replace(linkTag, styleTag);
                    }
                }
            }
            
            return html;
        }
        
        private string ProcessImages(string html, string baseDirectory)
        {
            // Find all image tags using regex
            var imgRegex = new Regex(@"<img[^>]*src=[""']([^""']+)[""'][^>]*>", RegexOptions.IgnoreCase);
            var matches = imgRegex.Matches(html);
            
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string src = match.Groups[1].Value;
                    
                    // Skip if already a data URL
                    if (src.StartsWith("data:"))
                        continue;
                    
                    string imagePath = ResolvePath(src, baseDirectory);
                    
                    if (_imageCache.TryGetValue(imagePath, out string? base64Image) && base64Image != null)
                    {
                        // Replace the src with base64 data URL
                        html = html.Replace($"src=\"{src}\"", $"src=\"{base64Image}\"");
                        html = html.Replace($"src='{src}'", $"src='{base64Image}'");
                    }
                }
            }
            
            return html;
        }
        
        private string ResolvePath(string relativePath, string baseDirectory)
        {
            // Remove fragments
            string path = relativePath.Split('#')[0];
            
            // Already an absolute path
            if (path.StartsWith("/"))
            {
                return path.TrimStart('/');
            }
            
            // Combine with base directory for relative paths
            if (!string.IsNullOrEmpty(baseDirectory))
            {
                return Path.Combine(baseDirectory, path).Replace('\\', '/');
            }
            
            return path;
        }
    }
}
