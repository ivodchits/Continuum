using Continuum.ViewModels;

namespace Continuum.Views;

public partial class ShelvesView : ContentPage
{
	public ShelvesView(LibraryViewModel libraryViewModel)
	{
		InitializeComponent();
        // BindingContext is set in XAML for this simple case
        // If you needed to inject dependencies into the ViewModel, you'd do it here:
        BindingContext = new ShelvesViewModel(libraryViewModel);
	}

    // Optional: Override OnAppearing if you need to refresh data when the page appears
    // protected override void OnAppearing()
    // {
    //     base.OnAppearing();
    //     if (BindingContext is ShelvesViewModel vm)
    //     {
    //         // vm.LoadShelves(); // Or some other refresh logic
    //     }
    // }
}
