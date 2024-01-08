

using Gs3PLv9MOBAPI.Models.Entity;
using Gs3PLv9MOBAPI.Models.ViewModel;
using Gs3PLv9MOBAPI.Services;
using Gs3PLv9MOBAPI.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Gs3PLv9MOBAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        ILoginService objLoginService;
        public LoginController()
        {
            objLoginService = new LoginService();
        }

        [HttpPost]
        public LoginResult Login([FromBody] LoginObject objLogin)
        {
            LoginResult _result;
            Login obj = new Login();
            obj.Password = objLogin.Password;
            obj.Email = objLogin.UserName;
            Login objLoginData = objLoginService.LoginAuthentication(obj);
            if (objLoginData != null)
            {
               
                _result = new LoginResult()
                {
                    CompanyId = objLoginData.dflt_cust_id.Trim(),
                    UserId = objLoginData.user_id,
                    IsMobUser = objLoginData.ismob_user,
                    IsLogin = true
                };
            }
            else
            {
                _result = new LoginResult()
                {
                    IsLogin = false
                };
            }
            return _result;

        }

     
    }
}
