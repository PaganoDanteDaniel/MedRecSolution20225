namespace MedRec.WPF.UI.Shared;
public partial class NavMenu
{
    private bool _isCollapsed = true;

    private string NavClass => _isCollapsed ? "nav-collapsed" : "nav-expanded";

    private void ToggleNav()
    {
        _isCollapsed = !_isCollapsed;
    }
}