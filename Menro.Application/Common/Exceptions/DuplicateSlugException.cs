namespace Menro.Application.Common.Exceptions
{
    public class DuplicateSlugException : Exception
    {
        public DuplicateSlugException()
            : base("این آدرس (اسلاگ) قبلاً برای پست دیگری استفاده شده است.")
        {
        }
    }
}