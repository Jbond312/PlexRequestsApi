using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRequests.ApiRequests
{
    public class ValidationContext<T> where T : class
    {
        public bool IsSuccessful => ValidationErrors.Count == 0;
        public T Data { get; set; }
        public List<ValidationResult> ValidationErrors { get; set; }

        public ValidationContext()
        {
            ValidationErrors = new List<ValidationResult>();
        }

        public ValidationContext(string message, string description)
        {
            ValidationErrors = new List<ValidationResult>
            {
                new ValidationResult(message, description)
            };
        }

        public ValidationContext(string message) : this(message, string.Empty) {}

        public bool AddErrorIf(Func<bool> expression, string message, string description)
        {
            var shouldAddError = expression();
            if (shouldAddError)
            {
                ValidationErrors.Add(new ValidationResult(message, description));
            }

            return shouldAddError;
        }

        public async Task<bool> AddErrorIf(Func<Task<bool>> expression, string message, string description)
        {
            var shouldAddError = await expression();
            if (shouldAddError)
            {
                ValidationErrors.Add(new ValidationResult(message, description));
            }

            return shouldAddError;
        }

        public void AddError(string message, string description)
        {
            ValidationErrors.Add(new ValidationResult(message, description));
        }

        public void AddError(string message)
        {
            ValidationErrors.Add(new ValidationResult(message));
        }
    }

    public class ValidationContext
    {
        public ValidationContext()
        {
            ValidationErrors = new List<ValidationResult>();
        }

        public bool IsSuccessful => ValidationErrors.Count == 0;

        public List<ValidationResult> ValidationErrors { get; set; }

        public bool AddErrorIf(Func<bool> expression, string message, string description)
        {
            var shouldAddError = expression();
            if (shouldAddError)
            {
                ValidationErrors.Add(new ValidationResult(message, description));
            }

            return shouldAddError;
        }

        public async Task<bool> AddErrorIf(Func<Task<bool>> expression, string message, string description)
        {
            var shouldAddError = await expression();
            if (shouldAddError)
            {
                ValidationErrors.Add(new ValidationResult(message, description));
            }

            return shouldAddError;
        }

        public void AddError(string message, string description)
        {
            ValidationErrors.Add(new ValidationResult(message, description));
        }

        public void AddError(string message)
        {
            ValidationErrors.Add(new ValidationResult(message));
        }
    }

    public class ValidationResult
    {
        public ValidationResult(string message, string description)
        {
            Message = message;
            Description = description;
        }

        public ValidationResult(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
        public string Description { get; set; }
    }
}
