using CommunityToolkit.Maui;
using Continuum.Services;
using Continuum.ViewModels;
using Continuum.Views;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Continuum;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		// Register Services
        builder.Services.AddSingleton<Services.EpubReaderService>();
        builder.Services.AddSingleton<Services.PdfReaderService>();
        builder.Services.AddSingleton<Services.BookReaderServiceFactory>(provider => 
        {
            var readers = new List<Services.IBookReaderService> 
            {
                provider.GetRequiredService<Services.EpubReaderService>(),
                provider.GetRequiredService<Services.PdfReaderService>()
            };
            return new Services.BookReaderServiceFactory(readers);
        });
        
        // Register ViewModels
        builder.Services.AddTransient<LibraryViewModel>();
        builder.Services.AddTransient<BookDetailViewModel>();

        // Register Views
        builder.Services.AddTransient<LibraryView>();
        builder.Services.AddTransient<BookDetailView>();

		return builder.Build();
	}
}
