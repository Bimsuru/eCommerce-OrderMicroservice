
using FluentValidation;
using FluentValidation.Results;

namespace BusinessLogicLayer.Services;

public class ValidationService
{

    /// <summary>
    /// Validates a single object and throws ArgumentException if validation fails
    /// </summary>
    /// <typeparam name="T">Type of object to validate</typeparam>
    /// <param name="requestModel">requestModel to validate</param>
    /// <param name="validator">FluentValidation validator</param>
    /// <param name="parameterName">Parameter name for null check exception</param>
    public async Task ModelValidationAsync<T>(IValidator<T> validator, T requestModel, string parameterName = null)
    {
        //Check for null parameter
        if (requestModel == null)
        {
            throw new ArgumentNullException(parameterName ?? typeof(T).Name);
        }

        //Validate OrderAddRequest using Fluent Validations
        ValidationResult orderAddRequestValidationResult = await validator.ValidateAsync(requestModel);
        if (!orderAddRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }
    }

    /// <summary>
    /// Validates a collection of objects and throws ArgumentException if any validation fails
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="validator"></param>
    /// <param name="requestModelCollection"></param>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    public async Task CollectionModelVaidationAsync<T>(IValidator<T> validator, IEnumerable<T> requestModelCollection, string parameterName = null)
    {

        foreach (T requestModel in requestModelCollection)
        {
            await ModelValidationAsync(validator, requestModel, parameterName);
        }
    }


}
