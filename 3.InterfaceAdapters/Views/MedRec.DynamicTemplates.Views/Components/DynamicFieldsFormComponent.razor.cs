using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.DynamicTemplates.ViewModels.Orchestration.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace MedRec.DynamicTemplates.Views.Components;

public partial class DynamicFieldsFormComponent : IDisposable
{
    [Parameter] public Guid VisitId { get; set; }
    [Parameter] public Guid SpecialtyId { get; set; }
    [Parameter] public bool IsReadOnly { get; set; }
    [Parameter] public IList<DynamicFieldValueModel>? Fields { get; set; }
    [Parameter] public EventCallback OnFieldsChanged { get; set; }
    [Parameter] public IEnumerable<TemplateFieldDefinitionModel>? TemplateDefinitions { get; set; }

    [Inject] public IGetTemplateFieldsOrchestrator GetTemplateFieldsOrchestrator { get; set; } = default!;
    [Inject] public IGetDynamicFieldsOrchestrator GetDynamicFieldsOrchestrator { get; set; } = default!;

    private bool isLoading;
    private string? loadErrorMessage;
    public List<TemplateFieldDefinitionModel> TemplateFields { get; private set; } = [];
    public Dictionary<string, List<string>> ValidationErrors { get; private set; } = [];
    private CancellationTokenSource? _cts;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        Fields ??= [];

        var hasExternalDefinitions = TemplateDefinitions?.ToList() is { Count: > 0 };

        if (!hasExternalDefinitions && SpecialtyId == Guid.Empty)
        {
            TemplateFields.Clear();
            return;
        }

        await LoadAsync(hasExternalDefinitions ? TemplateDefinitions : null);
    }

    private async Task LoadAsync(IEnumerable<TemplateFieldDefinitionModel>? externalDefinitions = null)
    {
        isLoading = true;
        loadErrorMessage = null;
        TemplateFields.Clear();
        ValidationErrors.Clear();

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            if (externalDefinitions?.ToList() is { Count: > 0 })
            {
                TemplateFields = externalDefinitions
                    .OrderBy(f => f.DisplayOrder)
                    .Select(CloneDefinition)
                    .ToList();
            }
            else
            {
                var fieldsResult = await GetTemplateFieldsOrchestrator.ExecuteAsync(SpecialtyId, _cts.Token);

                if (!fieldsResult.Success)
                {
                    loadErrorMessage = fieldsResult.ErrorMessage ??
                                       (fieldsResult.NotFound
                                           ? "No hay campos definidos para esta especialidad."
                                           : "Error al cargar campos dinámicos.");
                    return;
                }

                TemplateFields = (fieldsResult.Fields ?? [])
                    .OrderBy(f => f.DisplayOrder)
                    .Select(CloneDefinition)
                    .ToList();
            }

            if (VisitId != Guid.Empty && Fields!.Count == 0)
            {
                var valuesResult = await GetDynamicFieldsOrchestrator.ExecuteAsync(VisitId, _cts.Token);

                if (valuesResult.Success && valuesResult.Fields is not null && valuesResult.Fields.Any())
                {
                    Fields.Clear();
                    foreach (var field in valuesResult.Fields)
                    {
                        Fields.Add(new DynamicFieldValueModel
                        {
                            FieldDefinitionId = field.FieldDefinitionId,
                            FieldValue = field.FieldValue,
                            NumericValue = field.NumericValue,
                            DateValue = field.DateValue,
                            BooleanValue = field.BooleanValue
                        });
                    }

                    NotifyFieldsChanged();
                }
                else if (!valuesResult.Success && !valuesResult.NotFound)
                {
                    loadErrorMessage = valuesResult.ErrorMessage ?? "Error al obtener los valores dinámicos.";
                    return;
                }
            }

            if (EnsureFieldsAlignedWithDefinitions())
            {
                NotifyFieldsChanged();
            }
        }
        catch (OperationCanceledException)
        {
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

    private bool EnsureFieldsAlignedWithDefinitions()
    {
        if (Fields is null)
            return false;

        var added = false;

        foreach (var definition in TemplateFields)
        {
            var existing = Fields.FirstOrDefault(f => f.FieldDefinitionId == definition.Id);
            if (existing is null)
            {
                existing = new DynamicFieldValueModel { FieldDefinitionId = definition.Id };
                ApplyDefaultValue(existing, definition);
                Fields.Add(existing);
                added = true;
            }
        }

        return added;
    }

    private void ApplyDefaultValue(DynamicFieldValueModel target, TemplateFieldDefinitionModel definition)
    {
        if (string.IsNullOrWhiteSpace(definition.DefaultValue))
            return;

        target.FieldValue = definition.DefaultValue;

        switch (definition.FieldType?.ToLowerInvariant())
        {
            case "number":
            case "decimal":
                target.NumericValue = ParseDecimal(definition.DefaultValue);
                break;
            case "date":
                target.DateValue = ParseDate(definition.DefaultValue);
                break;
            case "boolean":
            case "checkbox":
                target.BooleanValue = ParseBool(definition.DefaultValue);
                break;
        }
    }

    private DynamicFieldValueModel EnsureField(Guid fieldId)
    {
        Fields ??= [];

        var field = Fields.FirstOrDefault(f => f.FieldDefinitionId == fieldId);
        if (field is null)
        {
            field = new DynamicFieldValueModel
            {
                FieldDefinitionId = fieldId
            };
            Fields.Add(field);
            NotifyFieldsChanged();
        }

        return field;
    }

    private string? GetTextValue(Guid fieldId) =>
        EnsureField(fieldId).FieldValue;

    private void SetTextValue(Guid fieldId, string? value)
    {
        EnsureField(fieldId).FieldValue = value;
        NotifyFieldsChanged();
    }

    private decimal GetNumericValue(Guid fieldId) =>
        EnsureField(fieldId).NumericValue ?? 0m;

    private void SetNumericValue(Guid fieldId, decimal? value)
    {
        var field = EnsureField(fieldId);
        field.NumericValue = value;
        field.FieldValue = value?.ToString(CultureInfo.InvariantCulture);
        NotifyFieldsChanged();
    }

    private DateTime? GetDateValue(Guid fieldId) =>
        EnsureField(fieldId).DateValue ?? DateTime.Today;

    private void SetDateValue(Guid fieldId, DateTime? value)
    {
        var field = EnsureField(fieldId);
        field.DateValue = value;
        field.FieldValue = value?.ToString("yyyy-MM-dd");
        NotifyFieldsChanged();
    }

    private bool GetBoolValue(Guid fieldId) =>
        EnsureField(fieldId).BooleanValue ?? false;

    private void SetBoolValue(Guid fieldId, bool value)
    {
        var field = EnsureField(fieldId);
        field.BooleanValue = value;
        field.FieldValue = value.ToString();
        NotifyFieldsChanged();
    }

    private decimal? GetNumericValue(Guid fieldId, decimal? _ = null) => GetNumericValue(fieldId);
    private void SetNumericValue(Guid fieldId, decimal? value, decimal? _ = null) => SetNumericValue(fieldId, value);

    private DateTime? GetDateValue(Guid fieldId, DateTime? _ = null) => GetDateValue(fieldId);
    private void SetDateValue(Guid fieldId, DateTime? value, DateTime? _ = null) => SetDateValue(fieldId, value);

    private bool GetBoolValue(Guid fieldId, bool _ = false) => GetBoolValue(fieldId);
    private void SetBoolValue(Guid fieldId, bool value, bool _ = false) => SetBoolValue(fieldId, value);

    private string GetInputCssClass(Guid fieldId) =>
        HasFieldErrors(fieldId) ? "dynamic-input input-invalid" : "dynamic-input";

    private string? GetMinValue(TemplateFieldDefinitionModel field) =>
        field.MinimumValue?.ToString(CultureInfo.InvariantCulture);

    private string? GetMaxValue(TemplateFieldDefinitionModel field) =>
        field.MaximumValue?.ToString(CultureInfo.InvariantCulture);

    private string? GetStepValue(TemplateFieldDefinitionModel field) =>
        field.FieldType?.Equals("decimal", StringComparison.OrdinalIgnoreCase) == true
            ? "0.01"
            : field.FieldType?.Equals("number", StringComparison.OrdinalIgnoreCase) == true ? "1" : null;

    private bool HasFieldErrors(Guid fieldId) =>
        ValidationErrors.TryGetValue(fieldId.ToString(), out var errors) && errors.Count > 0;

    private IEnumerable<string> GetFieldErrors(Guid fieldId) =>
        ValidationErrors.TryGetValue(fieldId.ToString(), out var errors) ? errors : Array.Empty<string>();

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

    private static TemplateFieldDefinitionModel CloneDefinition(TemplateFieldDefinitionModel source) => new()
    {
        Id = source.Id,
        SpecialtyId = source.SpecialtyId,
        FieldName = source.FieldName,
        FieldLabel = source.FieldLabel,
        FieldType = source.FieldType,
        Category = source.Category,
        IsRequired = source.IsRequired,
        DisplayOrder = source.DisplayOrder,
        SelectOptions = source.SelectOptions,
        DefaultValue = source.DefaultValue,
        Unit = source.Unit,
        MinimumValue = source.MinimumValue,
        MaximumValue = source.MaximumValue,
        HelpText = source.HelpText
    };

    private void NotifyFieldsChanged()
    {
        if (OnFieldsChanged.HasDelegate)
        {
            _ = OnFieldsChanged.InvokeAsync();
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}