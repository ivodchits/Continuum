using Continuum.ViewModels; // Make sure this namespace is correct
using Continuum.Models;
using Microsoft.Maui.Controls;
using System.Runtime.InteropServices;
using Microsoft.Maui.Platform;

namespace Continuum.Views;

public partial class LibraryView : ContentPage
{	// No changes needed here if using QueryProperty in ViewModel
	public LibraryView()
	{
		InitializeComponent();
		// BindingContext is often set in XAML or AppShell/DI container
        
        // Subscribe to property changed events to detect when UI needs to be refreshed
        this.Loaded += (s, e) => OnViewLoaded();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is LibraryViewModel viewModel)
		{
			// Refresh the picker bindings when the view appears
			await Task.Delay(100); // Small delay to ensure UI is ready
			RefreshPickerBindings(viewModel);
		}
	}

    private void OnViewLoaded()
    {
        // Subscribe to view model events once the page is loaded
        if (BindingContext is LibraryViewModel viewModel)
        {
            // Remove previous subscription if any to avoid duplicates
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Set initial handlers
            AttachRightClickHandlers();
        }
    }
    
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // When books collection or view type changes, reattach right-click handlers
        if (e.PropertyName == nameof(LibraryViewModel.Books) || 
            e.PropertyName == nameof(LibraryViewModel.IsGridView) ||
            e.PropertyName == nameof(LibraryViewModel.IsListView))
        {
            // Small delay to ensure UI has updated
            Dispatcher.Dispatch(async () => 
            {
                await Task.Delay(50); 
                AttachRightClickHandlers();
            });
        }
    }

    // New method to handle book item tapped (left click)
    private async void OnBookItemTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Book book)
        {
            // Create navigation parameters
            var navigationParameter = new Dictionary<string, object>
            {
                { "Book", book }
            };
            
            // Navigate to the book detail page with the selected book
            await Shell.Current.GoToAsync(nameof(BookDetailView), navigationParameter);
        }
    }
    
    private void AttachRightClickHandlers()
    {
        // Find all book containers in list and grid views
        var allBorders = GetAllBookBorders();
        
        foreach (var border in allBorders)
        {
            // Add tap gesture recognizer for right-click (or contextual menu)
            var tapGesture = new TapGestureRecognizer
            {
                Buttons = ButtonsMask.Secondary, // This is for right-click
                NumberOfTapsRequired = 1,
                CommandParameter = border.BindingContext
            };
            
            tapGesture.Tapped += OnBookRightClicked;
            border.GestureRecognizers.Add(tapGesture);
            
            // For touch devices, add a long-press (implemented as a custom tap gesture)
            var longPressTapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                CommandParameter = border.BindingContext
            };
            
            // Using a standard tap gesture with a press-and-hold behavior
            longPressTapGesture.Tapped += (s, e) => 
            {
                if (s is Element element && element.BindingContext is Book book && BindingContext is LibraryViewModel viewModel)
                {
                    var point = e.GetPosition(this);
                    if (point.HasValue)
                    {
                        viewModel.ContextMenuPosition = new Thickness(point.Value.X, point.Value.Y, 0, 0);
                        viewModel.ShowContextMenu(book);
                    }
                }
            };
            
            // Don't add this gesture for now since we can't differentiate long press
            // We'll address this in future updates if needed
            // border.GestureRecognizers.Add(longPressTapGesture);
        }
    }
    
    private void OnBookRightClicked(object? sender, TappedEventArgs e)
    {
        if (sender is Element element && element.BindingContext is Book book && BindingContext is LibraryViewModel viewModel)
        {
            // Calculate position for context menu
            var position = e.GetPosition(this);
            if (position.HasValue)
            {
                // Set context menu position
                viewModel.ContextMenuPosition = new Thickness(position.Value.X, position.Value.Y, 0, 0);
                
                // Show context menu with this book
                viewModel.ShowContextMenu(book);
            }
        }
    }
    
    private List<Border> GetAllBookBorders()
    {
        var borders = new List<Border>();
        
        // Find all Border elements that contain book items
        foreach (var element in this.GetVisualTreeDescendants())
        {
            if (element is Border border && border.BindingContext is Book)
            {
                borders.Add(border);
            }
        }
        
        return borders;
    }
    
    private void RefreshPickerBindings(LibraryViewModel viewModel)
    {
        // Find all pickers in the list view and grid view
        var allPickers = GetAllPickers();
        
        foreach (var picker in allPickers)
        {
            if (picker.BindingContext is Book book)
            {
				var currentShelf = viewModel.GetShelf(book);
                picker.SelectedItem = null;
                picker.SelectedItem = currentShelf;
            }
        }
    }
    
    private List<Picker> GetAllPickers()
    {
        var pickers = new List<Picker>();
        
        // This is a simplified way - you may need to navigate your visual tree more specifically
        foreach (var element in this.GetVisualTreeDescendants())
        {
            if (element is Picker picker)
            {
                pickers.Add(picker);
            }
        }
        
        return pickers;
    }
	
	// Event handler for the Add Book button
	private async void OnAddBookButtonClicked(object sender, EventArgs e)
	{
		if (BindingContext is ViewModels.LibraryViewModel viewModel)
		{
			await viewModel.AddBookFileAsync();
		}
	}
	
	// Event handler for the shelf picker selection changed
	private async void OnShelfPickerSelectedIndexChanged(object sender, EventArgs e)
	{
		if (BindingContext is LibraryViewModel viewModel && sender is Picker picker)
		{
			// The book is bound to the picker's BindingContext
			if (picker.BindingContext is Book book && !string.IsNullOrEmpty(book.Shelf))
			{
				// Update the shelf using our new simpler command
				await viewModel.UpdateBookShelfSimple(book);
			}
		}
	}
}
