using Continuum.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Continuum.Services
{
    public class PdfReaderService : IBookReaderService
    {
        private ObservableCollection<string> _chapters = new();
        private bool _isBookLoaded;
        private readonly PagedContentService _pagedContentService;
        
        public PdfReaderService(PagedContentService pagedContentService)
        {
            _pagedContentService = pagedContentService ?? throw new System.ArgumentNullException(nameof(pagedContentService));
        }
        
        public bool CanHandleBook(Book book)
        {
            if (book == null || string.IsNullOrEmpty(book.FilePath))
                return false;
                
            string extension = Path.GetExtension(book.FilePath).ToLowerInvariant();
            return extension == ".pdf";
        }
        
        public async Task<bool> LoadBookAsync(Book book)
        {
            // PDF implementation will be added in the future
            _isBookLoaded = true;
            
            // Currently just creates a single chapter stub
            _chapters.Clear();
            _chapters.Add("pdf-content");
            
            return true;
        }
        
        public ObservableCollection<string> GetTableOfContents()
        {
            // PDF TOC implementation will be added in the future
            return new ObservableCollection<string>();
        }
        
        public ObservableCollection<string> GetChapters()
        {
            return _chapters;
        }
        
        public async Task<string> LoadChapterByIndexAsync(int index)
        {
            // PDF content loading will be added in the future
            string basicHtml = "<html><body><h1>PDF Reading</h1><p>PDF reading capabilities coming soon!</p></body></html>";
            
            // Apply pagination to ensure consistent reading experience
            return _pagedContentService.PreparePagedContent(basicHtml);
        }
        
        public async Task<string> NavigateToChapterAsync(string navigationItem)
        {
            // PDF chapter navigation will be added in the future
            return await LoadChapterByIndexAsync(0);
        }
        
        public bool HasPreviousChapter(int currentIndex)
        {
            return false; // Only one chapter in current implementation
        }
        
        public bool HasNextChapter(int currentIndex)
        {
            return false; // Only one chapter in current implementation
        }
        
        public int GetChapterCount()
        {
            return _chapters.Count;
        }
    }
}
