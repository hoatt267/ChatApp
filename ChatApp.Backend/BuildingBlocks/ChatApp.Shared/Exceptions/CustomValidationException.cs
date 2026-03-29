namespace ChatApp.Shared.Exceptions
{
    public class CustomValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public CustomValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation failures have occurred.")
        {
            Errors = errors;
        }
    }
}