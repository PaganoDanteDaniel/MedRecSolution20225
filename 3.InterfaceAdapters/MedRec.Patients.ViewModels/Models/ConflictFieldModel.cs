using MedRec.Shared.DTOs;
using MedRec.Shared.Enums;

namespace MedRec.Patients.ViewModels.Models;

using MedRec.Entity.Enums;
using MedRec.Entity.Extensions;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ConflictFieldModel : INotifyPropertyChanged
{
    public string Label { get; }
    public string? DbValueDisplay { get; }
    public string? UserValueDisplay { get; }
    public object? DbValue { get; }
    public object? UserValue { get; }

    private ResolutionChoice _resolution = ResolutionChoice.Review;
    public ResolutionChoice Resolution
    {
        get => _resolution;
        set
        {
            if (_resolution != value)
            {
                _resolution = value;
                OnPropertyChanged();
            }
        }
    }

    public ConflictFieldModel(ConcurrencyConflictDto dto)
    {
        Label = dto.PropertyName;
        DbValue = dto.DataBaseValue;
        UserValue = dto.UserValue;
        DbValueDisplay = FormatValue(dto.DataBaseValue);
        UserValueDisplay = FormatValue(dto.UserValue);
    }

    // Formateo simple y seguro, sin abstracciones innecesarias
    private static string? FormatValue(object? value)
    {
        return value switch
        {
            null => "(vacío)",
            DateTime dt => dt.ToString("dd/MM/yyyy HH:mm"),
            DateOnly d => d.ToString("dd/MM/yyyy"),
            bool b => b ? "Sí" : "No",
            BiologicalSex bs => bs.GetDescription(),
            _ => value.ToString()
        };
    }

    // INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
