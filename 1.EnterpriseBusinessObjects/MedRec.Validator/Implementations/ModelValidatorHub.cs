using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;

namespace MedRec.Validator.Implementations;

/// <summary>
/// Implementación concreta de <see cref="IModelValidatorHub{TModel}"/>.
/// Proporciona funcionalidad para validar un modelo <typeparamref name="TModel"/>
/// y almacenar los errores de validación ocurridos durante la última operación.
/// </summary>
/// <typeparam name="TModel">El tipo del modelo que se va a validar.</typeparam>
internal class ModelValidatorHub<TModel> : IModelValidatorHub<TModel>
{
    /// <summary>
    /// Almacena los errores de la última validación ejecutada.
    /// </summary>
    private List<ValidationError> _errors = new();

    /// <summary>
    /// Obtiene una colección de errores de validación producidos durante la última llamada a <see cref="Validate"/>.
    /// </summary>
    public IEnumerable<ValidationError> Errors => _errors;

    /// <summary>
    /// Valida una instancia del modelo <typeparamref name="TModel"/> usando las reglas de validación proporcionadas.
    /// </summary>
    /// <param name="model">La instancia del modelo a validar.</param>
    /// <param name="rules">
    /// Una función que define las reglas de validación para el modelo.
    /// Recibe una instancia de <typeparamref name="TModel"/> y devuelve una lista de errores de validación.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica.
    /// Retorna <c>true</c> si la validación fue exitosa (sin errores), <c>false</c> en caso contrario.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Se lanza si <paramref name="rules"/> es <c>null</c>.
    /// </exception>
    public async Task<bool> Validate(TModel model, Func<TModel, IReadOnlyList<ValidationError>> rules)
    {
        // Validar que las reglas no sean nulas
        if (rules == null)
            throw new ArgumentNullException(nameof(rules));

        // Ejecutar las reglas de validación y almacenar los errores
        _errors = new List<ValidationError>(rules(model));

        // Retornar true si no hay errores (simulando operación asincrónica)
        return await Task.FromResult(_errors.Count == 0);
    }
}