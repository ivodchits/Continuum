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
	
	// Event handler for the Add Book button
	private async void OnAddBookButtonClicked(object sender, EventArgs e)
	{
		if (BindingContext is ViewModels.LibraryViewModel viewModel)
		{
			await viewModel.AddBookFileAsync();
		}
	}
	
	// Event handler for the shelf picker selection changed
	private void OnShelfPickerSelectedIndexChanged(object sender, EventArgs e)
	{
		if (BindingContext is LibraryViewModel viewModel && sender is Picker picker)
		{
			// The book is bound to the picker's BindingContext
			if (picker.BindingContext is Book book)
			{
				// Update the shelf using our new simpler command
				viewModel.UpdateBookShelfSimple(book);
			}
		}
	}
}
