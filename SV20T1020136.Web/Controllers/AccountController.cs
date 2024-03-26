using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020136.BusinessLayers;
using SV20T1020136.DomainModels;

namespace SV20T1020136.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// Giao diện đăng nhập
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        /// <summary>
        /// Tiến hành đăng nhập vào hệ thống với 2 thông tin là username và password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username = "", string password = "")
        {
            ViewBag.Username = username;
            // kiểm tra xem thông tin nhập có đủ không?
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin");
                return View();
            }

            // kiểm tra thông tin đăng nhập có hợp lệ không?
            UserAccount? userAccount = UserAccountService.Authorize(username, password);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại");
                return View();
            }

            WebUserData userData = new WebUserData()
            {
                UserId = userAccount.UserID,
                UserName = userAccount.UserName,
                DisplayName = userAccount.FullName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                ClientIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                SessionId = HttpContext.Session.Id,
                AdditionalData = "",
                Roles = userAccount.RoleNames.Split(',').ToList(),
            };

            // thiết lập phiên đăng nhập cho tài khoản
            await HttpContext.SignInAsync(userData.CreatePrincipal());

            return RedirectToAction("Index", "Home");
        }
        /// <summary>
        /// Đăng xuất khỏi hệ thống
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
        /// <summary>
        /// Đổi mật khẩu
        /// Nếu đổi mật khẩu thành công thì hệ thống sẽ chuyển người dùng về trang chủ
        /// </summary>
        /// <param name="oldPassword">Mật khẩu cũ</param>
        /// <param name="newPassword">Mật khẩu mới</param>
        /// <param name="confirmPassword">Xác nhận mật khẩu mới</param>
        /// <returns></returns>
        public IActionResult ChangePassword(string username = "", string oldPassword = "", string newPassword = "",
            string confirmPassword = "")
        {
            if (Request.Method != "POST") return View();


            // yêu cầu nhập đầy đủ các giá trị mật khẩu cũ, mật khẩu mới, xác nhận mật khẩu mới
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ các trường");
                return View();
            }
            var currentUser = UserAccountService.Authorize(username, oldPassword);

            // nếu currentUser == null => oldPassword sai
            if (currentUser == null)
            {
                ModelState.AddModelError(nameof(oldPassword), "Mật khẩu cũ không đúng, vui lòng nhập chính xác");
                return View();
            }

            if (newPassword == oldPassword)
            {
                ModelState.AddModelError(nameof(newPassword), "Vui lòng nhập mật khẩu mới khác mật khẩu cũ");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(nameof(confirmPassword), "Xác nhận mật khẩu mới không khớp, vui lòng nhập lại");
                return View();
            }

            bool result = UserAccountService.ChangePassword(username, oldPassword, newPassword);

            return RedirectToAction("Index", "Home");
        }
    }
}
