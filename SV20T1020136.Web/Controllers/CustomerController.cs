using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020136.BusinessLayers;
using SV20T1020136.DomainModels;
using SV20T1020136.Web.Models;

namespace SV20T1020136.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Employee}")]
    public class CustomerController : Controller
    {
        // Số khách hàng trên 1 trang khi hiển thị danh sách khách hàng
        const int PAGE_SIZE = 20;
        // tên biến session dùng để lưu lại điều kiện tìm kiếm
        const string CUSTOMER_SEARCH = "customer_search"; 
        /// <summary>
        /// Giao diện tìm kiếm và hiển thị kết quả tìm kiếm khách hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            // kiểm tra session có lưu điều kiện tìm kiếm không?
            // nếu có thì sử dụng lại điều kiện tìm kiếm, ngược lại thì dùng điều kiện tìm kiếm mặc định
            PaginationSearchInput? input = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH);
            if(input == null)
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
        /// Tìm kiếm khách hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IActionResult Search(PaginationSearchInput input)
        {
            int rowCount = 0;
            var data = CommonDataService.ListOfCustomers(
                out rowCount, input.Page, input.PageSize, input.SearchValue ?? "");

            var model = new CustomerSearchResult
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };

            // Lưu lại điều kiện tìm kiếm
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH, input);
                
            return View(model);
        }
        /// <summary>
        /// Giao diện bổ sung 1 khách hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung khách hàng";

            var model = new Customer
            {
                CustomerID = 0
            };

            return View("Edit", model);
        }
        /// <summary>
        /// Giao diện cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns></returns>
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";

            var model = CommonDataService.GetCustomer(id);

            if (model == null) return RedirectToAction("Index");
            return View(model);
        }
        /// <summary>
        /// Bổ sung hoặc cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost] //Attribute => chỉ nhận dữ liệu dạng post
        public IActionResult Save(Customer model)
        {
            // Kiểm soát dữ liệu trong model xem có hợp lệ không?
            // Yêu cầu: Tên khách hàng, tên giao dịch, tỉnh thành không được để trống
            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError(nameof(model.CustomerName), "Tên khách hàng không được để trống");
            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError(nameof(model.ContactName), "Tên giao dịch không được để trống");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Email không được để trống");
            if (string.IsNullOrWhiteSpace(model.Province))
                ModelState.AddModelError(nameof(model.Province), "Vui lòng chọn tỉnh thành");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.CustomerID == 0 ? "Bổ sung khách hàng" : "Cập nhật thông tin khách hàng";
                return View("Edit", model);
            }


            if (model.CustomerID == 0)
            {
                int id = CommonDataService.AddCustomer(model);
                if (id <= 0)
                {
                    ModelState.AddModelError("Email", "Email bị trùng");
                    ViewBag.Title = "Bổ sung khách hàng";

                    return View("Edit", model);
                }
            }
            else
            {
                bool result = CommonDataService.UpdateCustomer(model);
                if(!result)
                {
                    ModelState.AddModelError("Error", "Cập nhật không thành công, có thể email bị trùng");
                    ViewBag.Title = "Cập nhật thông tin khách hàng";

                    return View("Edit", model);
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                bool result = CommonDataService.DeleteCustomer(id);
                return RedirectToAction("Index");
            }

            var model = CommonDataService.GetCustomer(id);

            if (model == null) return RedirectToAction("Index");
            return View(model);
        }
    }
}
