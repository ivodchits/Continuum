using Continuum.Models;
using Continuum.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Continuum.ViewModels
{
    [QueryProperty(nameof(Book), "Book")]
    public partial class BookDetailViewModel : ObservableObject
    {
        private readonly BookReaderServiceFactory _readerServiceFactory;
        private IBookReaderService? _currentReaderService;

        [ObservableProperty]
        private Book _book;

        [ObservableProperty]
        private ObservableCollection<string> _tableOfContents = new();

        [ObservableProperty]
        private ObservableCollection<string> _chapters = new();
        
        [ObservableProperty]
        private string _currentChapterHtml = string.Empty;
        
        [ObservableProperty]
        private int _currentChapterIndex = 0;
        
        [ObservableProperty]
        private bool _hasNextChapter;
        
        [ObservableProperty]
        private bool _hasPreviousChapter;
        
        [ObservableProperty]
        private bool _showTableOfContents = false;
    
        [ObservableProperty]
        private bool _isChapterLoaded = false;

        public BookDetailViewModel(BookReaderServiceFactory readerServiceFactory)
        {
            _readerServiceFactory = readerServiceFactory;
        }
        
        [RelayCommand]
        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task LoadContentBasedOnFileTypeAsync()
        {
            if (Book == null)
                return;

            try
            {
                // Get the appropriate reader service for this book
                _currentReaderService = _readerServiceFactory.GetReaderService(Book);
                
                // Clear UI state before loading
                IsChapterLoaded = false;
                TableOfContents.Clear();
                Chapters.Clear();
                
                // Load book content
                var successful = await _currentReaderService.LoadBookAsync(Book);
                
                if (successful)
                {
                    // Get table of contents and chapters
                    TableOfContents = _currentReaderService.GetTableOfContents();
                    Chapters = _currentReaderService.GetChapters();
                    
                    // Set current chapter index if we have chapters
                    CurrentChapterIndex = Chapters.Count > 0 ? 0 : -1;
                    
                    // Update navigation state
                    UpdateNavigationState();
                    
                    // Load the first chapter if available
                    if (Chapters.Count > 0)
                    {
                        await LoadChapterByIndexAsync(0);
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Handle any exceptions that might occur during loading
                CurrentChapterHtml = $"<html><body><h1>Error loading book</h1><p>{ex.Message}</p></body></html>";
                IsChapterLoaded = true;
                System.Diagnostics.Debug.WriteLine($"Error loading book: {ex.Message}");
            }
        }        private void UpdateNavigationState()
        {
            if (_currentReaderService != null)
            {
                HasPreviousChapter = _currentReaderService.HasPreviousChapter(CurrentChapterIndex);
                HasNextChapter = _currentReaderService.HasNextChapter(CurrentChapterIndex);
            }
            else
            {
                HasPreviousChapter = false;
                HasNextChapter = false;
            }
        }

        [RelayCommand]
        private void NavigateToPreviousChapter()
        {
            if (HasPreviousChapter && CurrentChapterIndex > 0)
            {
                LoadChapterByIndexAsync(CurrentChapterIndex - 1);
            }
        }

        [RelayCommand]
        private void NavigateToNextChapter()
        {
            if (HasNextChapter && CurrentChapterIndex < Chapters.Count - 1)
            {
                LoadChapterByIndexAsync(CurrentChapterIndex + 1);
            }
        }
        
        [RelayCommand]
        private async Task NavigateToChapter(string navigationItem)
        {
            if (_currentReaderService != null)
            {
                string html = await _currentReaderService.NavigateToChapterAsync(navigationItem);
                
                if (!string.IsNullOrEmpty(html))
                {
                    CurrentChapterHtml = html;
                    IsChapterLoaded = true;
                    
                    // Find the chapter index
                    for (int i = 0; i < Chapters.Count; i++)
                    {
                        // This is a simplification - we'd need a better way to find the index
                        // based on the TOC item in a real implementation
                        if (html.Contains(Chapters[i]))
                        {
                            CurrentChapterIndex = i;
                            break;
                        }
                    }
                    
                    UpdateNavigationState();
                }
            }
        }

        [RelayCommand]
        private void ToggleTableOfContents()
        {
            ShowTableOfContents = !ShowTableOfContents;
        }
        
        private async Task LoadChapterByIndexAsync(int index)
        {
            if (_currentReaderService == null || index < 0 || index >= Chapters.Count)
                return;

            try
            {
                var html = await _currentReaderService.LoadChapterByIndexAsync(index);
                
                if (!string.IsNullOrEmpty(html))
                {
                    CurrentChapterHtml = html;
                    IsChapterLoaded = true;
                }
                
                // Update current index
                CurrentChapterIndex = index;
                
                // Update navigation buttons state
                UpdateNavigationState();
            }
            catch (System.Exception ex)
            {
                CurrentChapterHtml = $"<html><body><h1>Error loading chapter</h1><p>{ex.Message}</p></body></html>";
                IsChapterLoaded = true;
                System.Diagnostics.Debug.WriteLine($"Error loading chapter: {ex.Message}");
            }
        }
    }
}
