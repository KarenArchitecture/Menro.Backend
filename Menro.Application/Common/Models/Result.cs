namespace Menro.Application.Common.Models
{
    public enum ErrorCode
    {
        None,
        Invalid,
        NotFound,
        Duplicate,
        Failure
    }

    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public ErrorCode ErrorCode { get; set; } = ErrorCode.None;

        public static Result Success() => new Result { IsSuccess = true };

        public static Result Failure(string error, ErrorCode code = ErrorCode.Failure) =>
            new Result { IsSuccess = false, Error = error, ErrorCode = code };
    }
}