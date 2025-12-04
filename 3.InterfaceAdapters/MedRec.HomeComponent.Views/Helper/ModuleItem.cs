using Microsoft.AspNetCore.Components;

namespace MedRec.HomeComponent.Views.Helper;
public class ModuleItem
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; } // clase CSS o nombre de icono
    public string Color { get; set; } // color del icono
    public string BgColor { get; set; } // fondo del icono
    public Func<Task> Action { get; set; }
}

public class MenuModule
{
    public string Title { get; set; }
    public string Description { get; set; }
    public RenderFragment IconSvg { get; set; }
    public string CardClass { get; set; }   // card-green, card-blue...
    public string TitleClass { get; set; }  // title-green, etc
    public string IconClass { get; set; }   // icon-green...
    public EventCallback Action { get; set; }
}
