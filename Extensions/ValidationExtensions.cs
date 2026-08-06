using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.Extensions
{
        public static class ValidationExtensions
        {
            public static IEnumerable<string>? ValidateModel<T>(this T model)
            {
                var context = new ValidationContext(model!);
                var results = new List<ValidationResult>();

                var valid = Validator.TryValidateObject(
                    model!,
                    context,
                    results,
                    true);

                return valid
                    ? null
                    : results.Select(r => r.ErrorMessage!);
            }
        }
 }
