using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020136.BusinessLayers;
using SV20T1020136.DomainModels;
using SV20T1020136.Web.Models;
using System.Buffers;

namespace SV20T1020136.Web.Controllers
{
    public class EmployeeController : Controller
    {
        // Số nhân viên trên 1 trang khi hiển thị danh sách nhân viên
        const int PAGE_SIZE = 10;
        // Biến session lưu lại điều kiện tìm kiếm nhân viên
        const string EMPLOYEE_SEARCH = "employee_search";
        /// <summary>
        /// Giao diện tìm kiếm và hiển thị kết quả tìm kiếm nhân viên
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = $"{WebUserRoles.Employee},{WebUserRoles.Administrator}")]
        public IActionResult Index()
        {
            PaginationSearchInput? input = ApplicationContext.GetSessionData<PaginationSearchInput>(EMPLOYEE_SEARCH);
            if (input == null)
            {
                input = new PaginationSearchInput
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            }

            return View(input);
        }
        /// <summary>
        /// Tìm kiếm nhân viên
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(Roles = $"{WebUserRoles.Employee},{WebUserRoles.Administrator}")]
        public IActionResult Search(PaginationSearchInput input)
        {
            int rowCount = 0;

            var data = CommonDataService.ListOfEmployees(out rowCount, input.Page,
                input.PageSize, input.SearchValue ?? "");
            var model = new EmployeeSearchResult
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                Data = data,
            };
            // lưu lại điều kiện tìm kiếm
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH, input);

            return View(model);
        }
        /// <summary>
        /// Giao diện bổ sung 1 nhân viên
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = $"{WebUserRoles.Administrator}")]
        public IActionResult Create()
        {
            var model = new Employee
            {
                EmployeeID = 0,
                BirthDate = new DateTime(1990, 1, 1),
                Photo = "nophoto.png"

            };

            return View("Edit", model);
        }
        /// <summary>
        /// Giao diện cập nhật thông tin 1 nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns></returns>
        [Authorize(Roles = $"{WebUserRoles.Employee},{WebUserRoles.Administrator}")]
        public IActionResult Edit(int id = 0)
        {
            var user = User.GetUserData();
            if (user == null) return RedirectToAction("Index");

            // Nếu tài khoản đăng nhập có role là admin thì có quyền cập nhật thông tin toàn bộ nhân viên
            // Nếu tài khoản đăng nhập không có role admin mà chỉ có role employee thì chỉ được phép
            // cập nhật thông tin cá nhân của chính bản thân
            var condition = user.Roles!.Contains(WebUserRoles.Administrator) ||
                (user.Roles.Contains(WebUserRoles.Employee) && user.UserId == id.ToString());
            if (!condition)
                return RedirectToAction("AccessDenied", "Home");

            ViewBag.Title = "Cập nhật thông tin nhân viên";

            var model = CommonDataService.GetEmployee(id);
            if (model == null) return RedirectToAction("Index");

            if (string.IsNullOrEmpty(model.Photo))
                model.Photo = "nophoto.png";

            return View(model);
        }
        /// <summary>
        /// Xóa 1 nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns></returns>
        [Authorize(Roles = $"{WebUserRoles.Administrator}")]
        public IActionResult Delete(int id = 0)
        {
            ViewBag.Title = "Xóa nhân viên";
            if (Request.Method == "POST")
            {
                bool result = CommonDataService.DeleteEmployee(id);
                return RedirectToAction("Index");
            }
            var model = CommonDataService.GetEmployee(id);

            return View(model);
        }
        /// <summary>
        /// Bổ sung hoặc cập nhật thông tin 1 nhân viên
        /// </summary>
        /// <param name="model"></param>
        /// <param name="birthDayInput"></param>
        /// <param name="uploadPhoto"></param>
        /// <returns></returns>
        [Authorize(Roles = $"{WebUserRoles.Employee},{WebUserRoles.Administrator}")]
        public async Task<IActionResult> Save(Employee model, string birthDayInput = "",
            IFormFile? uploadPhoto = null)
        {
            var user = User.GetUserData();
            if (user == null) return RedirectToAction("Index");

            // Nếu tài khoản đăng nhập có role là admin thì có quyền cập nhật thông tin toàn bộ nhân viên
            // Nếu tài khoản đăng nhập không có role admin mà chỉ có role employee thì chỉ được phép
            // cập nhật thông tin cá nhân của chính bản thân
            var condition = user.Roles!.Contains(WebUserRoles.Administrator) ||
                (user.Roles.Contains(WebUserRoles.Employee) && user.UserId == model.EmployeeID.ToString());
            if (!condition) return RedirectToAction("AccessDenied", "Home");

            // Xử lý ngày sinh
            DateTime? d = birthDayInput.ToDateTime();
            if (d.HasValue) model.BirthDate = d.Value;

            // Xử lý ảnh upload: Nếu có ảnh được upload lên thì lưu ảnh mới, còn không thì giữ lại ảnh cũ
            if (uploadPhoto != null)
            {
                // Tên file lưu trên server
                string fileName = $"{DateTime.Now:yyyy-MM-dd}_{uploadPhoto.FileName}";

                // Đường dẫn vật lý lưu trên server
                string filePath = Path.Combine(ApplicationContext.HostEnviroment!.WebRootPath,
                    @"images\employees", fileName);

                // Lưu file lên server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }

                // Gán tên ảnh cho model
                model.Photo = fileName;
            }

            // Kiểm soát dữ liệu trong model xem có hợp lệ không?
            // Yêu cầu: Tên nhân viên, địa chỉ, điện thoại, email không được để trống
            if (string.IsNullOrWhiteSpace(model.FullName))
                ModelState.AddModelError(nameof(model.FullName), "Tên nhân viên không được để trống");
            if (string.IsNullOrWhiteSpace(model.Address))
                ModelState.AddModelError(nameof(model.Address), "Địa chỉ không được để trống");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError(nameof(model.Phone), "Địa chỉ không được để trống");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Email không được để trống");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";

                return View("Edit", model);
            }

            if (model.EmployeeID == 0)
            {
                int id = CommonDataService.AddEmployee(model);
                if (id <= 0)
                {
                    ModelState.AddModelError("Email", "Email bị trùng");
                    ViewBag.Title = "Bổ sung nhân viên";

                    return View("Edit", model);
                }
            }
            else
            {
                bool result = CommonDataService.UpdateEmployee(model);
                if (!result)
                {
                    ModelState.AddModelError("Error", "Cập nhật không thành công, có thể email bị trùng");
                    ViewBag.Title = "Cập nhật thông tin nhân viên";

                    return View("Edit", model);
                }

                // Cập nhật lại thông tin user lưu trong phiên đăng nhập nếu tự cập nhật thông tin chính mình
                if (user.UserId == model.EmployeeID.ToString())
                {
                    user.DisplayName = model.FullName;
                    user.Email = model.Email;
                    user.Photo = model.Photo;

                    await HttpContext.SignInAsync(user.CreatePrincipal());
                }
            }

            return RedirectToAction("Index");
        }
    }
}
