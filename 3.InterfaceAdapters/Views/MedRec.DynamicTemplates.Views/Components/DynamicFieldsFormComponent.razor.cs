using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.DynamicTemplates.ViewModels.Orchestration.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace MedRec.DynamicTemplates.Views.Components;

public partial class DynamicFieldsFormComponent : IDisposable
{
    #region Parámetros

    [Parameter] public Guid VisitId { get; set; }
    [Parameter] public Guid SpecialtyId { get; set; }
    [Parameter] public bool IsReadOnly { get; set; } = false;

    /// <summary>
    /// Se dispara cuando los campos se guardan correctamente.
    /// </summary>
    [Parameter] public EventCallback OnSaved { get; set; }

    #endregion

    #region Inyección de dependencias

    [Inject] public IGetTemplateFieldsOrchestrator GetTemplateFieldsOrchestrator { get; set; } = default!;
    [Inject] public IGetDynamicFieldsOrchestrator GetDynamicFieldsOrchestrator { get; set; } = default!;
    [Inject] public ISaveDynamicFieldsOrchestrator SaveDynamicFieldsOrchestrator { get; set; } = default!;

    #endregion

    #region Estado interno

    private SaveDynamicFieldsModel _model = new();
    private bool isLoading;
    private bool isSaving;
    private string? loadErrorMessage;

    public List<TemplateFieldDefinitionModel> TemplateFields { get; private set; } = [];
    public Dictionary<string, List<string>> ValidationErrors { get; private set; } = [];

    private CancellationTokenSource? _cts;

    #endregion

    #region Ciclo de vida

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        Console.WriteLine($"[DynamicFields] VisitId={VisitId}, SpecialtyId={SpecialtyId}");

        // Para nueva visita, VisitId puede ser Guid.Empty. Solo necesitamos SpecialtyId.
        if (SpecialtyId != Guid.Empty)
        {
            await LoadAsync();
        }
    }

    #endregion

    #region Carga de datos

    private async Task LoadAsync()
    {
        isLoading = true;
        loadErrorMessage = null;
        TemplateFields.Clear();
        _model = new SaveDynamicFieldsModel { VisitId = VisitId, Fields = [] };
        ValidationErrors.Clear();

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            // 1. Obtener definiciones de campos por SpecialtyId
            var fieldsResult = await GetTemplateFieldsOrchestrator.ExecuteAsync(SpecialtyId, _cts.Token);

            if (!fieldsResult.Success)
            {
                if (fieldsResult.NotFound)
                {
                    loadErrorMessage = fieldsResult.ErrorMessage ?? "No hay campos definidos para esta especialidad.";
                }
                else
                {
                    loadErrorMessage = fieldsResult.ErrorMessage ?? "Error al cargar campos dinámicos.";
                }

                isLoading = false;
                return;
            }

            TemplateFields = fieldsResult.Fields ?? [];

            // 2. Obtener valores existentes solo si ya hay VisitId
            if (VisitId != Guid.Empty)
            {
                var valuesResult = await GetDynamicFieldsOrchestrator.ExecuteAsync(VisitId, _cts.Token);

                if (valuesResult.Success && valuesResult.Fields is not null && valuesResult.Fields.Any())
                {
                    _model.Fields = valuesResult.Fields;
                }
                else
                {
                    _model.Fields = TemplateFields
                        .Select(f => new DynamicFieldValueModel
                        {
                            FieldDefinitionId = f.Id,
                            FieldValue = f.DefaultValue
                        })
                        .ToList();
                }
            }
            else
            {
                // Nueva visita: inicializar solo con valores por defecto
                _model.Fields = TemplateFields
                    .Select(f => new DynamicFieldValueModel
                    {
                        FieldDefinitionId = f.Id,
                        FieldValue = f.DefaultValue
                    })
                    .ToList();
            }
        }
        catch (OperationCanceledException)
        {
            // ignorar cancelación
        }
        catch (Exception ex)
        {
            loadErrorMessage = $"Error inesperado al cargar campos dinámicos: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SaveAsync()
    {
        if (IsReadOnly)
            return;

        isSaving = true;
        ValidationErrors.Clear();

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            _model.VisitId = VisitId;

            var result = await SaveDynamicFieldsOrchestrator.ExecuteAsync(_model, _cts.Token);

            if (result.ValidationErrors is not null && result.ValidationErrors.Any())
            {
                ValidationErrors = result.ValidationErrors;
                return;
            }

            if (!result.Success)
            {
                loadErrorMessage = result.ErrorMessage ?? "Error al guardar los campos dinámicos.";
                return;
            }

            ValidationErrors.Clear();
            if (OnSaved.HasDelegate)
                await OnSaved.InvokeAsync();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            loadErrorMessage = $"Error inesperado al guardar campos dinámicos: {ex.Message}";
        }
        finally
        {
            isSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    #endregion

    #region Bind helpers

    private DynamicFieldValueModel EnsureField(Guid fieldId)
    {
        var field = _model.Fields.FirstOrDefault(f => f.FieldDefinitionId == fieldId);
        if (field is null)
        {
            field = new DynamicFieldValueModel
            {
                FieldDefinitionId = fieldId
            };
            _model.Fields.Add(field);
        }

        return field;
    }

    private string? GetTextValue(Guid fieldId)
    {
        return EnsureField(fieldId).FieldValue;
    }

    private void SetTextValue(Guid fieldId, string? value)
    {
        EnsureField(fieldId).FieldValue = value;
    }

    private decimal GetNumericValue(Guid fieldId)
    {
        return EnsureField(fieldId).NumericValue ?? 0m;
    }

    private void SetNumericValue(Guid fieldId, decimal? value)
    {
        var field = EnsureField(fieldId);
        field.NumericValue = value;
        field.FieldValue = value?.ToString();
    }

    private DateTime? GetDateValue(Guid fieldId)
    {
        return EnsureField(fieldId).DateValue ?? DateTime.Today;
    }

    private void SetDateValue(Guid fieldId, DateTime? value)
    {
        var field = EnsureField(fieldId);
        field.DateValue = value;
        field.FieldValue = value?.ToString("yyyy-MM-dd");
    }

    private bool GetBoolValue(Guid fieldId)
    {
        return EnsureField(fieldId).BooleanValue ?? false;
    }

    private void SetBoolValue(Guid fieldId, bool value)
    {
        var field = EnsureField(fieldId);
        field.BooleanValue = value;
        field.FieldValue = value.ToString();
    }

    // Métodos para @bind-Value (get/set)
    private decimal? GetNumericValue(Guid fieldId, decimal? _ = null) => GetNumericValue(fieldId);
    private void SetNumericValue(Guid fieldId, decimal? value, decimal? _ = null) => SetNumericValue(fieldId, value);

    private DateTime? GetDateValue(Guid fieldId, DateTime? _ = null) => GetDateValue(fieldId);
    private void SetDateValue(Guid fieldId, DateTime? value, DateTime? _ = null) => SetDateValue(fieldId, value);

    private bool GetBoolValue(Guid fieldId, bool _ = false) => GetBoolValue(fieldId);
    private void SetBoolValue(Guid fieldId, bool value, bool _ = false) => SetBoolValue(fieldId, value);

    #endregion

    #region Validación (hook para ModelValidator)

    // Por ahora devolvemos lista vacía: la validación fuerte ya la hace el caso de uso (SaveDynamicFieldsDto)
    // Aquí podrías agregar validaciones de UI si lo necesitas.
    private IReadOnlyList<MedRec.Validator.ValueObjects.ValidationError> SaveDynamicFieldsValidationRules(SaveDynamicFieldsModel model)
        => Array.Empty<MedRec.Validator.ValueObjects.ValidationError>();

    #endregion

    private decimal ParseDecimal(object? value) =>
        decimal.TryParse(Convert.ToString(value), NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;

    private bool ParseBool(object? value) =>
    value switch
    {
        bool b => b,
        string s when bool.TryParse(s, out var result) => result,
        _ => false
    };

    private DateTime? ParseDate(object? value) =>
        DateTime.TryParse(Convert.ToString(value), out var result) ? result : null;

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}