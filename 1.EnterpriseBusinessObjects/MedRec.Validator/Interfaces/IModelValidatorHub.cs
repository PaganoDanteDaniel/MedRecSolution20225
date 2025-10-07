using MedRec.Validator.ValueObjects;

namespace MedRec.Validator.Interfaces;
/// <summary>
/// Representa un centro de validación genérico para un modelo de tipo <typeparamref name="TModel"/>.
/// Permite validar un objeto de tipo <typeparamref name="TModel"/> usando reglas personalizadas
/// y obtener una colección de errores si la validación falla.
/// </summary>
/// <typeparam name="TModel">El tipo del modelo que se va a validar.</typeparam>
public interface IModelValidatorHub<TModel>
{
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
    Task<bool> Validate(TModel model, Func<TModel, IReadOnlyList<ValidationError>> rules);

    /// <summary>
    /// Obtiene una colección de errores de validación producidos durante la última llamada a <see cref="Validate"/>.
    /// </summary>
    /// <value>
    /// Colección de <see cref="ValidationError"/>. Vacía si no hubo errores en la última validación.
    /// </value>
    IEnumerable<ValidationError> Errors { get; }
}
