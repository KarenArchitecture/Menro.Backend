namespace Menro.Application.Common.Exceptions
{
    /// <summary>
    /// پرتاب می‌شه وقتی کاربر (Contributor/Author) سعی می‌کنه پستی رو تغییر بده
    /// (Update/Publish/Delete/Content) که مالکش نیست و نقش Elevated (Editor/Admin) هم نداره.
    /// کنترلر این exception رو می‌گیره و 403 برمی‌گردونه.
    /// </summary>
    public class BlogPostAccessDeniedException : Exception
    {
        public BlogPostAccessDeniedException()
            : base("شما اجازه‌ی تغییر این پست را ندارید.")
        {
        }
    }
}