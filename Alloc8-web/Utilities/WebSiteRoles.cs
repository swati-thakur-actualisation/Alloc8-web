namespace Alloc8_web.Utilities
{
    public class WebSiteRoles
    {
        public const string Website_Admin = "Admin";
        public const string Website_Manager = "Manager";
        public const string Website_User = "User";
        public static List<string> getRoles()
        {
            var roles = new List<string>();
            roles.Add(Website_Admin);
            roles.Add(Website_User);
            roles.Add(Website_Manager);
            return roles;
        }
    }
}
