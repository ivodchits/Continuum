using Continuum.ViewModels;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace Continuum.Views;

public partial class BookDetailView : ContentPage
{
    // Using the null-forgiving operator since we know it will be set in the constructor
    private BookDetailViewModel? _viewModel;
    private IDispatcherTimer? _resizeTimer;
    
    public BookDetailView(BookDetailViewModel bookDetailViewModel)
    {
        InitializeComponent();
        _viewModel = bookDetailViewModel;
        BindingContext = bookDetailViewModel;
        
        // Register WebView event handlers
        BookContentWebView.Navigated += OnWebViewNavigated;
        SizeChanged += OnPageSizeChanged;
        
        // Initialize the timer
        _resizeTimer = Dispatcher.CreateTimer();
        _resizeTimer.Interval = TimeSpan.FromMilliseconds(300);
        _resizeTimer.Tick += (s, e) => {
            _resizeTimer?.Stop();
            if (_viewModel != null)
            {
                BookContentWebView.Reload();
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BookDetailViewModel viewModel && viewModel.Book != null)
        {
            await viewModel.LoadContentBasedOnFileTypeAsync();
        }
    }
    
    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (e.Result == WebNavigationResult.Success)
        {
            // The HTML has been successfully loaded - resize handling is now done in the JavaScript
        }
    }
    
    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        // When page size changes, we need to reload the content to update pagination
        if (_viewModel?.IsChapterLoaded == true)
        {
            // Throttle resize events
            _resizeTimer?.Stop();
            _resizeTimer?.Start();
        }
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Clean up event handlers
        BookContentWebView.Navigated -= OnWebViewNavigated;
        SizeChanged -= OnPageSizeChanged;
        
        // Clean up timer
        if (_resizeTimer != null)
        {
            _resizeTimer.Stop();
        }
    }
}
