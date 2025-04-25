using Continuum.ViewModels;
using Microsoft.Maui.Controls;

namespace Continuum.Views
{
    public partial class BookDetailView : ContentPage
    {
        private readonly BookDetailViewModel _viewModel;

        public BookDetailView()
        {
            InitializeComponent();
            _viewModel = BindingContext as BookDetailViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Load content based on the book type
            if (_viewModel?.Book != null)
            {
                await _viewModel.LoadContentBasedOnFileTypeAsync();
            }
        }
    }
}
