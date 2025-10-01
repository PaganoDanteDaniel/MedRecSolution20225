using Microsoft.AspNetCore.Components;

namespace MedRec.CommonComponents.Views.Components;
public partial class FooterComponent
{
    [Parameter]
    public string Message { get; set; } = "MedRec - Medical Record System";
}