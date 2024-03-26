using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020136.BusinessLayers;
using SV20T1020136.DomainModels;
using SV20T1020136.Web.Models;
using System.Buffers;

namespace SV20T1020136.Web.Controllers
{
    [Authorize]
    [Authorize(Roles = $"{WebUserRoles.Employee},{WebUserRoles.Administrator}")]
    public class SupplierController : Controller
    {
        // Số nhà cung cấp trên 1 trang khi hiển thị danh sách nhà cung cấp
        const int PAGE_SIZE = 5;
        // Biến session lưu lại điều kiện tìm kiếm nhà cung cấp
        const string SUPPLIER_SEARCH = "supplier_search";
        /// <summary>
        /// Giao diện tìm kiếm và hiển thị kết quả tìm kiếm nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            PaginationSearchInput? input = ApplicationContext.GetSessionData<PaginationSearchInput>(SUPPLIER_SEARCH);
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
        /// Tìm kiếm nhà cung cấp
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IActionResult Search(PaginationSearchInput input)
        {
            int rowCount = 0;
            var data = CommonDataService.ListOfSuppliers(out rowCount, input.Page,
                input.PageSize, input.SearchValue ?? "");
            var model = new SupplierSearchResult
            {
                Data = data,
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount
            };

            // lưu lại kết quả tìm kiếm
            ApplicationContext.SetSessionData(SUPPLIER_SEARCH, input);

            return View(model);
        }
        /// <summary>
        /// Giao diện bổ sung 1 nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";

            var model = new Supplier
            {
                SupplierID = 0,
                DateOfIncorporation = new DateTime(1990, 1, 1),
            };

            return View("Edit", model);
        }
        /// <summary>
        /// Giao diện cập nhật thông tin 1 nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhà cung cấp";

            var model = CommonDataService.GetSupplier(id);

            return View(model);
        }
        /// <summary>
        /// Xóa 1 nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(int id = 0)
        {
            ViewBag.Title = "Xóa nhà cung cấp";
            if (Request.Method == "POST")
            {
                bool result = CommonDataService.DeleteSupplier(id);
                return RedirectToAction("Index");
            }

            var model = CommonDataService.GetSupplier(id);

            return View(model);
        }
        /// <summary>
        /// Bổ sung hoặc cập nhật thông tin 1 nhà cung cấp
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Save(Supplier model, string dateOfIncorporationInput = "")
        {
            if (string.IsNullOrWhiteSpace(model.SupplierName))
                ModelState.AddModelError(nameof(model.SupplierName), "Tên nhà cung cấp không được trống");
            if (string.IsNullOrWhiteSpace(model.ContactName))
                ModelState.AddModelError(nameof(model.ContactName), "Tên giao dịch không được trống");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError(nameof(model.Phone), "Số điện thoại không được trống");
            if (string.IsNullOrWhiteSpace(model.Province))
                ModelState.AddModelError(nameof(model.Province), "Vui lòng chọn tỉnh thành");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.SupplierID == 0 ? "Bổ sung nhà cung cấp" : "Cập nhật thông tin nhà cung cấp";
                return View("Edit", model);
            }

            DateTime? d = dateOfIncorporationInput.ToDateTime();
            if (d.HasValue) model.DateOfIncorporation = d.Value;

            if (model.SupplierID == 0)
            {
                int id = CommonDataService.AddSupplier(model);
            }
            else
            {
                bool result = CommonDataService.UpdateSupplier(model);
            }
            return RedirectToAction("Index");
        }
    }
}
