namespace Continuum;

public partial class AppShell : Shell
{
    // Property to track if the menu is in compact mode
    private bool _isCompactMode = false;
    private const double EXPANDED_FLYOUT_WIDTH = 250;
    private const double COMPACT_FLYOUT_WIDTH = 60;

	public AppShell()
	{
		InitializeComponent();        // Register routes for navigation
        Routing.RegisterRoute(nameof(Views.LibraryView), typeof(Views.LibraryView));
        Routing.RegisterRoute(nameof(Views.ShelvesView), typeof(Views.ShelvesView));
        Routing.RegisterRoute(nameof(Views.BookDetailView), typeof(Views.BookDetailView));
        // Register other views if needed
        
        // Set FlyoutBehavior based on platform
        SetFlyoutBehaviorByPlatform();
    }

    private void SetFlyoutBehaviorByPlatform()
    {
#if WINDOWS
        // On Windows, make the flyout part of the UI layout and always visible
        AppShellInstance.FlyoutBehavior = FlyoutBehavior.Locked;
        AppShellInstance.FlyoutWidth = EXPANDED_FLYOUT_WIDTH;
#else
        // For other platforms, use default Flyout behavior
        AppShellInstance.FlyoutBehavior = FlyoutBehavior.Flyout;
#endif
    }
    
    // Toggle between compact and expanded menu modes
    private void OnToggleMenuClicked(object sender, EventArgs e)
    {
        _isCompactMode = !_isCompactMode;
        
        // Update the flyout width based on mode
        AppShellInstance.FlyoutWidth = _isCompactMode ? COMPACT_FLYOUT_WIDTH : EXPANDED_FLYOUT_WIDTH;
        
        // Update all FlyoutItems to show/hide text
        foreach (var item in Items)
        {
            if (item is FlyoutItem flyoutItem)
            {
                foreach (var shellContent in flyoutItem.Items)
                {
                    shellContent.FlyoutItemIsVisible = !_isCompactMode;
                }
            }
            else
            {
                item.FlyoutItemIsVisible = !_isCompactMode;
            }
        }
    }
}
