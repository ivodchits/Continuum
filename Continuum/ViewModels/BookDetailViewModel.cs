using Continuum.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VersOne.Epub;

namespace Continuum.ViewModels
{
    [QueryProperty(nameof(Book), "Book")]
    public partial class BookDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private Book _book;

        [ObservableProperty]
        private ObservableCollection<EpubNavigationItem> _tableOfContents = new();

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
        
        private EpubBook? _epubBook;

        public BookDetailViewModel()
        {
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

            // Determine file extension and load appropriate content
            string extension = System.IO.Path.GetExtension(Book.FilePath).ToLowerInvariant();
            
            switch (extension)
            {
                case ".epub":
                    await LoadEpubContentAsync();
                    break;
                    
                case ".pdf":
                    await LoadPdfContentAsync();
                    break;
                    
                case ".mp3":
                    await LoadAudiobookContentAsync();
                    break;
                    
                case ".mobi":
                    await LoadMobiContentAsync();
                    break;
                    
                default:
                    // Handle unsupported file type
                    break;
            }
        }
        
        private async Task LoadEpubContentAsync()
        {
            if (Book == null || string.IsNullOrEmpty(Book.FilePath) || !File.Exists(Book.FilePath))
                return;
            
            try
            {
                // Load the EPUB book
                _epubBook = await EpubReader.ReadBookAsync(Book.FilePath);
                
                if (_epubBook == null)
                    return;
                
                // Clear existing data
                Chapters.Clear();
                TableOfContents.Clear();
                IsChapterLoaded = false;
                
                // Get all chapters/reading items
                var readingOrder = _epubBook.ReadingOrder;
                
                // Add all chapters
                foreach (var chapter in readingOrder)
                {
                    Chapters.Add(chapter.FilePath);
                }
                
                // Load table of contents
                if (_epubBook.Navigation != null && _epubBook.Navigation.Count > 0)
                {
                    foreach (var item in _epubBook.Navigation)
                    {
                        TableOfContents.Add(item);
                    }
                }

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
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during EPUB loading
                CurrentChapterHtml = $"<html><body><h1>Error loading EPUB</h1><p>{ex.Message}</p></body></html>";
                IsChapterLoaded = true;
                System.Diagnostics.Debug.WriteLine($"Error loading EPUB: {ex.Message}");
            }
        }

        private async Task LoadPdfContentAsync()
        {
            // PDF reader implementation will go here
            await Task.CompletedTask;
        }

        private async Task LoadAudiobookContentAsync()
        {
            // Audiobook player implementation will go here
            await Task.CompletedTask;
        }

        private async Task LoadMobiContentAsync()
        {
            // MOBI reader implementation will go here
            await Task.CompletedTask;
        }

        private void UpdateNavigationState()
        {
            HasPreviousChapter = CurrentChapterIndex > 0;
            HasNextChapter = CurrentChapterIndex < Chapters.Count - 1;
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
        private async Task NavigateToChapter(EpubNavigationItem navigationItem)
        {
            if (_epubBook != null && navigationItem.Link != null)
            {
                // Find the chapter matching the href in our list
                int index = -1;
                for (int i = 0; i < Chapters.Count; i++)
                {
                    if (Chapters[i].Contains(navigationItem.HtmlContentFile.FilePath))
                    {
                        index = i;
                        break;
                    }
                }
                
                if (index >= 0)
                {
                    await LoadChapterByIndexAsync(index);
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
            if (_epubBook == null || index < 0 || index >= Chapters.Count)
                return;

            try
            {
                // Get the chapter file path
                var chapterPath = Chapters[index];
                
                // Get the chapter content
                var chapterContent = _epubBook.Content.Html.GetLocalFileByFilePath(chapterPath);
                if (chapterContent != null)
                {
                    // Get document content as HTML
                    if (chapterContent is EpubContentFile contentFile)
                    {
                        CurrentChapterHtml = chapterContent.Content;
                        IsChapterLoaded = !string.IsNullOrEmpty(CurrentChapterHtml);
                    }
                }
                
                // Update current index
                CurrentChapterIndex = index;
                
                // Update navigation buttons state
                UpdateNavigationState();
            }
            catch (Exception ex)
            {
                CurrentChapterHtml = $"<html><body><h1>Error loading chapter</h1><p>{ex.Message}</p></body></html>";
                IsChapterLoaded = true;
                System.Diagnostics.Debug.WriteLine($"Error loading chapter: {ex.Message}");
            }
        }
    }
}
