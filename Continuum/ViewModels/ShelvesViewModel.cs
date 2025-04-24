using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Continuum.Models; // Assuming Book model is here
using Microsoft.Maui.Controls;

namespace Continuum.ViewModels
{
    public partial class ShelvesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<string> _shelves;

        [ObservableProperty]
        private string _newShelfName;

        // Assuming you have a way to access all books, perhaps from another service or static list
        // For demonstration, let's use the LibraryViewModel's source if possible, or create a dummy list
        // In a real app, you'd likely inject a data service.
        private readonly LibraryViewModel _libraryViewModel; // Or inject a data service

        public ICommand AddShelfCommand { get; }
        public ICommand GoToShelfCommand { get; }        // Option 1: Inject LibraryViewModel (if it holds the master book list)
        public ShelvesViewModel(LibraryViewModel libraryViewModel)
        {
            _libraryViewModel = libraryViewModel;
            _shelves = new ObservableCollection<string>(); // Initialize with empty collection
            AddShelfCommand = new AsyncRelayCommand(AddShelfAsync);
            GoToShelfCommand = new AsyncRelayCommand<string>(GoToShelf);
            
            // Load shelves after initialization
            _ = LoadShelvesAsync();
        }
        
        private async Task LoadShelvesAsync()
        {
            // Load shelves from the shelf collection file
            var shelfCollection = await ShelfCollection.LoadAsync();
            Shelves = new ObservableCollection<string>(shelfCollection.Shelves.OrderBy(s => s));
        }
          private async Task AddShelfAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewShelfName))
            {
                var trimmedName = NewShelfName.Trim();
                
                if (!Shelves.Contains(trimmedName))
                {
                    // Add to UI collection
                    Shelves.Add(trimmedName);
                    
                    // Persist to storage
                    var shelfCollection = await ShelfCollection.LoadAsync();
                    shelfCollection.AddShelf(trimmedName);
                    await shelfCollection.SaveAsync();
                    
                    // Clear input field
                    NewShelfName = string.Empty;
                    OnPropertyChanged(nameof(NewShelfName));
                }
            }
        }

         private async Task GoToShelf(string shelfName)
        {
            if (string.IsNullOrEmpty(shelfName)) return;

            // Navigate to LibraryView, passing the shelf name as a parameter
            await Shell.Current.GoToAsync($"{nameof(Views.LibraryView)}?ShelfName={Uri.EscapeDataString(shelfName)}");
        }
    }
}
