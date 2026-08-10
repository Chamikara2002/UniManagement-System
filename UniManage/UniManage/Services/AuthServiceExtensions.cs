using System.Web;
using Microsoft.Owin.Security;
using System.Security.Claims;

namespace UniManage.Services
{
    public static class AuthServiceExtensions
    {
        public static void SignInIdentity(this IAuthenticationManager authManager, ClaimsIdentity identity, bool isPersistent)
        {
            authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            authManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
        }
    }
}