using System.Web;
using System.Threading.Tasks;
using UniManage.ViewModels;

namespace UniManage.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterViewModel model, HttpContextBase httpContext);
        Task<AuthResult> LoginAsync(LoginViewModel model, HttpContextBase httpContext);
        void Logout(HttpContextBase httpContext);
    }
}