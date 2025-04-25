using Continuum.ViewModels;

namespace Continuum.Views;

public partial class BookDetailView : ContentPage
{
    // Using the null-forgiving operator since we know it will be set in the constructor
    private BookDetailViewModel? _viewModel;
    
    public BookDetailView(BookDetailViewModel bookDetailViewModel)
    {
        InitializeComponent();
        _viewModel = bookDetailViewModel;
        BindingContext = bookDetailViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BookDetailViewModel viewModel && viewModel.Book != null)
        {
            await viewModel.LoadContentBasedOnFileTypeAsync();
        }
    }
}
