using Continuum.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VersOne.Epub;

namespace Continuum.Services
{
    public class EpubReaderService : IBookReaderService
    {
        private EpubBook? _epubBook;
        private ObservableCollection<EpubNavigationItem> _tableOfContents = new();
        private ObservableCollection<string> _chapters = new();
        private bool _isBookLoaded;
        
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
                
                // Get all chapters/reading items
                var readingOrder = _epubBook.ReadingOrder;
                
                // Add all chapters
                foreach (var chapter in readingOrder)
                {
                    _chapters.Add(chapter.FilePath);
                }
                
                // Load table of contents
                if (_epubBook.Navigation != null && _epubBook.Navigation.Count > 0)
                {
                    foreach (var item in _epubBook.Navigation)
                    {
                        _tableOfContents.Add(item);
                    }
                }
                
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
            return new ObservableCollection<string>(_tableOfContents.Select(item => item.Title).ToList());       }
        
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
                // Get the chapter file path
                var chapterPath = _chapters[index];
                
                // Get the chapter content
                var chapterContent = _epubBook.Content.Html.GetLocalFileByFilePath(chapterPath);
                if (chapterContent != null && chapterContent is EpubContentFile contentFile)
                {
                    return chapterContent.Content;
                }
                
                return string.Empty;
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

            if (!string.IsNullOrEmpty(navigationItem))
            {
                // Find the chapter matching the href in our list
                int index = -1;
                var path = _epubBook.Navigation.Find(n => n.Title == navigationItem)?.HtmlContentFile.FilePath;
                for (int i = 0; i < _chapters.Count; i++)
                {
                    if (_chapters[i] == path)
                    {
                        index = i;
                        break;
                    }
                }
                
                if (index >= 0)
                {
                    return await LoadChapterByIndexAsync(index);
                }
            }
            
            return GenerateErrorHtml("Chapter not found");
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
    }
}
