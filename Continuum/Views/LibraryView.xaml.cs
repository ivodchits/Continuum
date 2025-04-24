using Continuum.ViewModels; // Make sure this namespace is correct
using Continuum.Models;

namespace Continuum.Views;

public partial class LibraryView : ContentPage
{
	// No changes needed here if using QueryProperty in ViewModel
	public LibraryView()
	{
		InitializeComponent();
		// BindingContext is often set in XAML or AppShell/DI container
	}

	protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Get a reference to the model and force refresh
        if (BindingContext is LibraryViewModel viewModel)
        {
            // Force UI refresh
            await Task.Delay(100); // Small delay to allow UI to update
            RefreshPickerBindings(viewModel);
        }
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
