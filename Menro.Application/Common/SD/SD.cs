namespace Menro.Application.Common.SD
{
    static public class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_Owner = "Owner";
        public const string Role_Customer = "Customer";
        // blog related roles
        public const string Role_Contributor = "Contributor";
        public const string Role_Author = "Author";
        public const string Role_Editor = "Editor";

        // ترکیب نقش‌ها برای Authorize - از پایین‌ترین نقش مجاز به بالا
        public const string Roles_ContributorUp = $"{Role_Admin},{Role_Editor},{Role_Author},{Role_Contributor}";
        public const string Roles_AuthorUp = $"{Role_Admin},{Role_Editor},{Role_Author}";
        public const string Roles_EditorUp = $"{Role_Admin},{Role_Editor}";
    }
}
