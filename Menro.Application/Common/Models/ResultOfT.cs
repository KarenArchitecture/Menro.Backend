namespace Menro.Application.Common.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public ErrorCode ErrorCode { get; }
        public T? Value { get; }

        protected Result(bool isSuccess, T? value, string? error, ErrorCode errorCode)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            ErrorCode = errorCode;
        }

        public static Result<T> Success(T value) => new(true, value, null, ErrorCode.None);

        public static Result<T> Failure(string error, ErrorCode code = ErrorCode.Failure) =>
            new(false, default, error, code);
    }
}