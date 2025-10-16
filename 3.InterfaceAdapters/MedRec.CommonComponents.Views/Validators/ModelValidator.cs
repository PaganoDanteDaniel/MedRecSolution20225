using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace MedRec.CommonComponents.Views.Validators;
public class ModelValidator<T> : ComponentBase
{
    [CascadingParameter] EditContext EditContext { get; set; }

    [Parameter] public IModelValidatorHub<T> Validator { get; set; }

    // Nuevo parámetro: regla de validación
    [Parameter] public Func<T, IReadOnlyList<ValidationError>> ValidationRules { get; set; }

    ValidationMessageStore ValidationMessageStore;

    FieldIdentifier GetFieldIdentifier(object model, string propertyName)
    {
        char[] PropertyNameSeparators = ['.', '['];
        object NewModel = model;

        string PropertyPath = propertyName;

        int SeparatorIndex;
        string Token = null;

        do
        {
            SeparatorIndex =
                PropertyPath.IndexOfAny(PropertyNameSeparators);

            if (SeparatorIndex >= 0)
            {
                Token = PropertyPath.Substring(0, SeparatorIndex);
                PropertyPath = PropertyPath.Substring(SeparatorIndex + 1);

                if (Token.EndsWith("]"))
                {
                    Token = Token.Substring(0, Token.Length - 1);
                    var PropertyInfo =
                        NewModel.GetType().GetProperty("Item");

                    var IndexerType =
                        PropertyInfo.GetIndexParameters()[0]
                        .ParameterType;

                    var IndexValue =
                        Convert.ChangeType(Token, IndexerType);

                    NewModel = PropertyInfo.GetValue(NewModel,
                        new object[] { IndexValue });
                }
                else
                {
                    var PropertyInfo = NewModel.GetType().GetProperty(Token);
                    if (PropertyInfo != null)
                    {
                        NewModel = PropertyInfo.GetValue(NewModel);
                    }
                    else
                    {
                        // Manejar el caso donde PropertyInfo es null
                        throw new Exception($"Propiedad '{Token}' no encontrada en el tipo '{NewModel.GetType().Name}'");
                    }
                }
                Token = null;
            }
        } while (SeparatorIndex >= 0);

        return new FieldIdentifier(NewModel,
            Token ?? PropertyPath);
    }

    public void AddErrors(IEnumerable<ValidationError> errors)
    {
        ValidationMessageStore.Clear();
        foreach (var Error in errors)
        {
            var FieldIdentifier =
                GetFieldIdentifier(EditContext.Model, Error.PropertyName);
            ValidationMessageStore.Add(FieldIdentifier, Error.ErrorMessage);
        }
        EditContext.NotifyValidationStateChanged();
    }

    async void ValidationRequested(object sender, ValidationRequestedEventArgs args)
    {
        // Ahora pasas las reglas al validar
        bool IsValid = await Validator.Validate((T)EditContext.Model, ValidationRules);
        if (IsValid)
        {
            ValidationMessageStore.Clear();
            EditContext.NotifyValidationStateChanged();
        }
        else
        {
            AddErrors(Validator.Errors);
        }
    }

    async void FieldChanged(object sender, FieldChangedEventArgs args)
    {
        // Limpiar solo los errores del campo que cambió
        ValidationMessageStore.Clear(args.FieldIdentifier);

        // Revalidar todo el modelo
        bool IsValid = await Validator.Validate((T)EditContext.Model, ValidationRules);

        if (!IsValid)
        {
            // Filtrar solo los errores del campo que cambió
            foreach (var error in Validator.Errors)
            {
                var fieldIdentifier = GetFieldIdentifier(EditContext.Model, error.PropertyName);

                // Solo agregar errores del campo que cambió
                if (fieldIdentifier.Equals(args.FieldIdentifier))
                {
                    ValidationMessageStore.Add(fieldIdentifier, error.ErrorMessage);
                }
            }
        }

        EditContext.NotifyValidationStateChanged();
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        EditContext PreviousEditContext = EditContext;
        await base.SetParametersAsync(parameters);

        if (EditContext != PreviousEditContext)
        {
            ValidationMessageStore = new ValidationMessageStore(EditContext);
            EditContext.OnValidationRequested += ValidationRequested;
            EditContext.OnFieldChanged += FieldChanged;
        }
    }
}
