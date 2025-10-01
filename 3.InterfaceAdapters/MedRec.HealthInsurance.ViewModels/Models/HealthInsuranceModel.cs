using System.ComponentModel.DataAnnotations;

namespace MedRec.HealthInsurance.ViewModels.Models;
public class HealthInsuranceModel
{
    private Guid _id;
    private string _name;
    private string _acronym;

    public Guid Id
    {
        get => _id;
        set => _id = value;
    }
    [Required(ErrorMessage = "Es obligatorio proporcionar el nombre el la Obra Social.")]
    [MaxLength(200, ErrorMessage = "El nombre no puede tener más de {1} caracteres")]
    public string Name
    {
        get => _name;
        set => _name = value;
    }
    [MaxLength(19, ErrorMessage = "El campo no debe tener más de {1} caracteres.")]
    public string Acronym
    {
        get => _acronym;
        set => _acronym = value;
    }
}
