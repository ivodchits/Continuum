using Continuum.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Continuum.Services
{
    public interface IBookReaderService
    {
        /// <summary>
        /// Checks if this reader can handle the given book based on its file extension
        /// </summary>
        bool CanHandleBook(Book book);
        
        /// <summary>
        /// Loads the book content
        /// </summary>
        Task<bool> LoadBookAsync(Book book);
        
        /// <summary>
        /// Gets the table of contents as a collection
        /// </summary>
        ObservableCollection<string> GetTableOfContents();
        
        /// <summary>
        /// Gets all chapters as a collection of strings
        /// </summary>
        ObservableCollection<string> GetChapters();
        
        /// <summary>
        /// Navigates to a specific chapter by index
        /// </summary>
        Task<string> LoadChapterByIndexAsync(int index);
        
        /// <summary>
        /// Navigates to a specific chapter by table of contents item
        /// </summary>
        Task<string> NavigateToChapterAsync(string navigationItem);
        
        /// <summary>
        /// Checks if there is a previous chapter available
        /// </summary>
        bool HasPreviousChapter(int currentIndex);
        
        /// <summary>
        /// Checks if there is a next chapter available
        /// </summary>
        bool HasNextChapter(int currentIndex);
        
        /// <summary>
        /// Gets the total number of chapters
        /// </summary>
        int GetChapterCount();
    }
}
