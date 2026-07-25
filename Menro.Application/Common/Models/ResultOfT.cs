namespace Menro.Application.Common.Models
{
    /// <summary>
    /// Generic counterpart to Result, for operations that need to return a value
    /// alongside success/failure. Add this as a new file next to Result.cs.
    ///
    /// ⚠️ VERIFY: this assumes Result has IsSuccess (bool) and Error (string?)
    /// properties with Success()/Failure(string) static factories. If your
    /// actual Result.cs differs (different property names, Succeeded instead
    /// of IsSuccess, etc.), adjust this file to match — share Result.cs and
    /// I'll align it exactly.
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public T? Value { get; }

        protected Result(bool isSuccess, T? value, string? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new(true, value, null);
        public static Result<T> Failure(string error) => new(false, default, error);
    }
}
