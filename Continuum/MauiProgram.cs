using Continuum.ViewModels;
using Continuum.Views;
using Microsoft.Extensions.Logging;

namespace Continuum;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // Register ViewModels
        builder.Services.AddTransient<LibraryViewModel>();

        // Register Views
        builder.Services.AddTransient<LibraryView>();

		return builder.Build();
	}
}
