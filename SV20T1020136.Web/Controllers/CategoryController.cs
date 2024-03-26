using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020136.BusinessLayers;
using SV20T1020136.DomainModels;
using SV20T1020136.Web.Models;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace SV20T1020136.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Employee}")]
    public class CategoryController : Controller
    {
        // Số loại hàng trên 1 trang khi hiển thị danh sách loại hàng
        const int PAGE_SIZE = 5;
        // Biến session dùng để lưu điều kiện tìm kiếm loại hàng
        const string CATEGORY_SEARCH = "category_search";
        /// <summary>
        /// Giao diện tìm kiếm và hiển thị danh sách loại hàng
        /// </summary>
        /// <param name="page"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public IActionResult Index(int page = 1, string searchValue = "")
        {
            // kiểm tra session có lưu điều kiện tìm kiếm không?
            // nếu có thì sử dụng lại điều kiện tìm kiếm, ngược lại thì dùng điều kiện tìm kiếm mặc định
            PaginationSearchInput? input = ApplicationContext.GetSessionData<PaginationSearchInput>(CATEGORY_SEARCH);
            if(input == null)
            {
                input = new PaginationSearchInput
                {
                    Page = page,
                    PageSize = PAGE_SIZE,
                    SearchValue = searchValue ?? ""
                };
            }

            return View(input);
        }
        /// <summary>
        /// Thực hiện chức năng tìm kiếm loại hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IActionResult Search(PaginationSearchInput input)
        {
            int rowCount = 0;

            var data = CommonDataService.ListOfCategories(out rowCount, input.Page, PAGE_SIZE, 
                input.SearchValue ?? "");
            var model = new CategorySearchResult
            {
                Data = data,
                Page = input.Page,
                PageSize = PAGE_SIZE,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount
            };

            // Lưu lại điều kiện tìm kiếm
            ApplicationContext.SetSessionData(CATEGORY_SEARCH, input);

            return View(model);
        }
        /// <summary>
        /// Giao diện bổ sung 1 loại hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung loại hàng";
            var model = new Category
            {
                CategoryID = 0,
            };
            return View("Edit", model);
        }
        /// <summary>
        /// Giao diện cập nhật thông tin 1 loại hàng
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns></returns>
        public IActionResult Edit(int id = 0)
        {
            var model = CommonDataService.GetCategory(id);
            if (model == null) return RedirectToAction("Index");

            ViewBag.Title = "Cập nhật thông tin loại hàng";
            return View(model);
        }
        /// <summary>
        /// Xóa 1 loại hàng
        /// Nếu method là post thì tiến hành xóa
        /// Ngược lại thì đến giao diện xóa 
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns></returns>
        public IActionResult Delete(int id = 0)
        {
            if(Request.Method == "POST")
            {
                bool result = CommonDataService.DeleteCategory(id);
                return RedirectToAction("Index");
            }

            var model = CommonDataService.GetCategory(id);
            return View(model);
        }
        /// <summary>
        /// Bổ sung mặt hàng hoặc cập nhật thông tin mặt hàng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Save(Category model)
        {
            // Kiểm soát dữ liệu trong model xem có hợp lệ không?
            // Yêu cầu: Tên loại hàng không được để trống
            if(string.IsNullOrWhiteSpace(model.CategoryName))
                ModelState.AddModelError(nameof(model.CategoryName), "Tên mặt hàng không được để trống");

            if(!ModelState.IsValid)
            {
                ViewBag.Title = model.CategoryID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";
                return View("Edit", model);
            }

            if (model.CategoryID == 0)
            {
                int id = CommonDataService.AddCategory(model);
            }
            else
            {
                bool result = CommonDataService.UpdateCategory(model);
            }
            return RedirectToAction("Index");
        }
    }
}
