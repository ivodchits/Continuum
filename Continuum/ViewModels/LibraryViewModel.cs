using System.Collections.ObjectModel;
using Continuum.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Windows.Input;

namespace Continuum.ViewModels
{
    [QueryProperty(nameof(ShelfName), "ShelfName")]    public partial class LibraryViewModel : ObservableObject
    {
        private List<Book> _allBooks = new();
        private readonly string _localStoragePath;

        [ObservableProperty]
        private ObservableCollection<Book> _books = new();

        [ObservableProperty]
        private string _shelfName = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsListView))]
        private bool _isGridView;

        [ObservableProperty]
        private string _searchText = string.Empty;
        
        [ObservableProperty]
        private bool _showOnlyAudiobooks = false;
        
        [ObservableProperty]
        private bool _isFilterDropdownOpen = false;
        
        [ObservableProperty]
        private List<string> _availableShelves = new();
        
        [ObservableProperty]
        private string _selectedShelfFilter = "All Books";
        
        [ObservableProperty]
        private bool _isLoading = false;
          [ObservableProperty]
        private Book? _selectedBook;
        
        [ObservableProperty]
        private bool _isContextMenuVisible = false;
        
        [ObservableProperty]
        private Thickness _contextMenuPosition = new Thickness(0);

        public bool IsListView => !IsGridView;

        public LibraryViewModel()
        {
            _isGridView = false; // Default to list view
            _localStoragePath = Path.Combine(FileSystem.AppDataDirectory, "Books");
                        
            // Ensure storage directory exists
            if (!Directory.Exists(_localStoragePath))
            {
                Directory.CreateDirectory(_localStoragePath);
            }
            
            LoadBooksAsync();
        }

        private async Task LoadBooksAsync()
        {
            IsLoading = true;
            
            try
            {
                // Ensure we're getting the latest book data with shelf assignments
                _allBooks = await GetBooksFromStorageAsync();
                
                ApplyFilters();
                LoadShelves();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading books: {ex.Message}");
                // In a real app, you would want to display an error message to the user
            }
            finally
            {
                IsLoading = false;
            }
        }
          private async Task LoadShelvesAsync()
        {
            var appData = await AppData.LoadAsync();
            
            // Get any shelves from books that aren't in our stored collection
            var bookShelves = _allBooks
                .Where(b => !string.IsNullOrEmpty(b.Shelf))
                .Select(b => b.Shelf!)
                .Distinct()
                .ToList(); // Create a fixed list to avoid modification during iteration
            
            // Create a list of shelves to add (instead of modifying during iteration)
            var shelvesToAdd = bookShelves
                .Where(shelf => shelf != "None" && !appData.Shelves.Contains(shelf))
                .ToList();
                
            // Add missing shelves all at once
            if (shelvesToAdd.Count > 0)
            {
                appData.Shelves.AddRange(shelvesToAdd);
                await appData.SaveAsync();
            }
            
            // Sort the shelves alphabetically (create a new list instead of modifying)
            var shelves = appData.Shelves.OrderBy(s => s).ToList();
            
            // Insert None option at the beginning for shelf selection
            shelves.Insert(0, "None");
            AvailableShelves = shelves;
            
            // Apply the saved view preference
            IsGridView = appData.IsGridView;
        }
         
         // Fire-and-forget wrapper that doesn't block
        private void LoadShelves()
        {
            // Instead of .Wait(), we use fire-and-forget pattern with ConfigureAwait(false)
            // to avoid blocking the UI thread
            _ = LoadShelvesAsync().ConfigureAwait(false);
        }

        public async Task<List<Book>> GetBooksFromStorageAsync()
        {
            var books = new List<Book>();
            
            try
            {
                // Check if storage directory exists
                if (!Directory.Exists(_localStoragePath))
                {
                    return books; // Return empty list if no storage directory
                }
                
                // Get all supported book files in the app's storage
                var bookFiles = Directory.GetFiles(_localStoragePath)
                    .Select(filePath => new FileInfo(filePath))
                    .Where(file => Book.IsSupportedExtension(file.Extension))
                    .ToList();
                
                // Create Book objects for each file
                foreach (var file in bookFiles)
                {
                    // For a real app, you would extract book metadata (title, author) 
                    // from the file itself. For now, we'll use the filename.
                    var fileName = Path.GetFileNameWithoutExtension(file.Name);
                    
                    // Load metadata for this book
                    var metadata = await BookMetadata.LoadAsync(file.FullName);
                    
                    // Ensure we're setting the shelf value correctly from metadata
                    string shelfValue = !string.IsNullOrEmpty(metadata.Shelf) ? metadata.Shelf : "None";
                    
                    books.Add(BookFactory.CreateBook(file));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading books from storage: {ex.Message}");
            }
            
            return books;
        }

        [RelayCommand]
        public async Task AddBookFileAsync()
        {
            try
            {
                var options = new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.epub", "com.adobe.pdf", "public.mp3", "public.mobi" } },
                        { DevicePlatform.Android, new[] { "application/epub+zip", "application/pdf", "audio/mpeg", "application/x-mobipocket-ebook" } },
                        { DevicePlatform.WinUI, new[] { ".epub", ".pdf", ".mp3", ".mobi" } },
                        { DevicePlatform.macOS, new[] { "epub", "pdf", "mp3", "mobi" } }
                    }),
                    PickerTitle = "Select a book file"
                };
                
                var result = await FilePicker.Default.PickMultipleAsync(options);
                
                if (result != null && result.Any())
                {
                    foreach (var file in result)
                    {
                        await ProcessSelectedBookFileAsync(file);
                    }
                    
                    // Refresh the book list after adding new files
                    await LoadBooksAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error picking file: {ex.Message}");
                // In a real app, you would want to display an error message to the user
            }
        }
        
        private async Task ProcessSelectedBookFileAsync(FileResult file)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                
                // Verify this is a supported file type
                if (!Book.IsSupportedExtension(extension))
                {
                    Console.WriteLine($"Unsupported file type: {extension}");
                    return;
                }
                
                // Create a unique filename to avoid conflicts
                var destinationFileName = file.FileName;
                var destinationPath = Path.Combine(_localStoragePath, destinationFileName);
                
                // Copy the file to our app's storage
                using (var sourceStream = await file.OpenReadAsync())
                using (var destinationStream = File.Create(destinationPath))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
                
                // Create initial metadata for this book
                var metadata = new BookMetadata();
                await BookMetadata.SaveAsync(destinationPath, metadata);
                
                Console.WriteLine($"Added book: {file.FileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file: {ex.Message}");
            }
        }

        partial void OnShelfNameChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }
        
        partial void OnShowOnlyAudiobooksChanged(bool value)
        {
            ApplyFilters();
        }
        
        partial void OnSelectedShelfFilterChanged(string value)
        {
            ApplyFilters();
        }
        
        private void ApplyFilters()
        {
            var filteredBooks = new List<Book>(_allBooks);

            // Apply shelf filter from navigation
            if (!string.IsNullOrEmpty(ShelfName) && ShelfName != "All Books")
            {
                filteredBooks = filteredBooks.Where(b => b.Shelf == ShelfName).ToList();
            }
            
            // Apply shelf filter from dropdown
            if (!string.IsNullOrEmpty(SelectedShelfFilter) && SelectedShelfFilter != "All Books")
            {
                filteredBooks = filteredBooks.Where(b => b.Shelf == SelectedShelfFilter).ToList();
            }

            // Apply audiobook filter
            if (ShowOnlyAudiobooks)
            {
                filteredBooks = filteredBooks.Where(b => b.IsAudiobook).ToList();
            }
            
            // Apply search filter
            if (!string.IsNullOrEmpty(SearchText))
            {
                string search = SearchText.ToLower();
                filteredBooks = filteredBooks.Where(b => 
                    b.Title.ToLower().Contains(search) || 
                    b.Author.ToLower().Contains(search))
                    .ToList();
            }
            
            if (filteredBooks.Count != Books.Count && filteredBooks.Any(b => !Books.Contains(b)))
            {
                // Use dispatcher to defer UI updates until after current UI operation completes
                // This prevents collection modification during measure/arrange                if (Microsoft.Maui.Controls.Application.Current != null)
                {
                    Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() =>
                    {
                        // Replace the collection instead of modifying it in-place
                        Books = new ObservableCollection<Book>(filteredBooks);
                    });
                }
            }            
        }
        
        [RelayCommand]
        public void ToggleFilterDropdown()
        {
            IsFilterDropdownOpen = !IsFilterDropdownOpen;
        }
        
        [RelayCommand]
        public void ClearFilters()
        {
            SelectedShelfFilter = "All Books";
            ShowOnlyAudiobooks = false;
            SearchText = string.Empty;
        }
        
        [RelayCommand]
        public async Task RefreshBooksAsync()
        {
            await LoadBooksAsync();
        }

        [RelayCommand]
        public async Task UpdateBookShelfSimple(Book book)
        {
            if (book == null)
                return;
                
            // Get the current shelf from the book
            string newShelf = book.Shelf ?? "None";
              // Update the book's shelf (use empty string instead of null to avoid null reference issues)
            book.Shelf = newShelf;
            
            // Find the corresponding book in _allBooks and update it
            var bookInAllBooks = _allBooks.FirstOrDefault(b => b.FilePath == book.FilePath);
            if (bookInAllBooks != null)
            {
                bookInAllBooks.Shelf = book.Shelf;
            }
            
            // Save the shelf to book metadata file
            var metadata = await BookMetadata.LoadAsync(book.FilePath);
            metadata.Shelf = book.Shelf;
            await BookMetadata.SaveAsync(book.FilePath, metadata);
            
            // Only apply filters if necessary and we're on a filtered view
            if (SelectedShelfFilter != "All Books" || !string.IsNullOrEmpty(ShelfName))
            {
                ApplyFilters();
            }
        }

        public string GetShelf(Book book)
        {
            return Books.FirstOrDefault(b => b.FilePath == book.FilePath)?.Shelf ?? "None";
        }
        
        // Update to save the view preference when it changes
        partial void OnIsGridViewChanged(bool value)
        {
            // Save the view preference
            SaveViewPreferenceAsync(value).ConfigureAwait(false);
        }
        
        private async Task SaveViewPreferenceAsync(bool isGridView)
        {
            try
            {
                var appData = await AppData.LoadAsync();
                appData.IsGridView = isGridView;
                await appData.SaveAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving view preference: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task DeleteBookAsync(Book book)
        {
            if (book == null)
                return;
                
            try
            {
                // Remove the book from the collections
                _allBooks.Remove(book);
                Books.Remove(book);
                
                // Delete the file from storage
                if (File.Exists(book.FilePath))
                {
                    File.Delete(book.FilePath);
                    
                    // Also delete the metadata file if it exists
                    string metadataPath = BookMetadata.GetMetadataFilePath(book.FilePath);
                    if (File.Exists(metadataPath))
                    {
                        File.Delete(metadataPath);
                    }
                }
                
                // Hide the context menu
                IsContextMenuVisible = false;
                
                // Refresh the UI
                await LoadBooksAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting book: {ex.Message}");
                // In a real app, you would want to display an error message to the user
            }
        }
        
        [RelayCommand]
        public async void ShowContextMenu(Book book)
        {
            SelectedBook = book;
            await Task.Delay(100); // Delay to ensure the book is set before showing the menu
            IsContextMenuVisible = true;
        }
        
        [RelayCommand]
        public void HideContextMenu()
        {
            if (IsContextMenuVisible)
            {
                IsContextMenuVisible = false;
            }
        }
    }
}
