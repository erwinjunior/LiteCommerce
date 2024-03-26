using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020136.BusinessLayers;
using SV20T1020136.DomainModels;
using SV20T1020136.Web.Models;
using System.Buffers;

namespace SV20T1020136.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Employee},{WebUserRoles.Administrator}")]
    public class ShipperController : Controller
    {
        // Số người giao hàng trên 1 trang khi hiển thị danh sách người giao hàng
        const int PAGE_SIZE = 5;
        // Biến session lưu lại điều kiện tìm kiếm người giao hàng
        const string SHIPPER_SEARCH = "shipper_search";
        /// <summary>
        /// Giao diện tìm kiếm và hiển thị kết quả tìm kiếm người giao hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            PaginationSearchInput? input = ApplicationContext.GetSessionData<PaginationSearchInput>(SHIPPER_SEARCH);
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
        /// Tìm kiếm người giao hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IActionResult Search(PaginationSearchInput input)
        {
            int rowCount = 0;

            var data = CommonDataService.ListOfShippers(out rowCount, input.Page, 
                PAGE_SIZE, input.SearchValue ?? "");
            var model = new ShipperSearchResult
            {
                Data = data,
                Page = input.Page,
                PageSize = PAGE_SIZE,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount
            };

            // lưu lại điều kiện tìm kiếm
            ApplicationContext.SetSessionData(SHIPPER_SEARCH, input);

            return View(model);
        }
        /// <summary>
        /// Giao diện bổ sung 1 người giao hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung người giao hàng";

            var model = new Shipper
            {
                ShipperID = 0
            };

            return View("Edit", model);
        }
        /// <summary>
        /// Giao diện cập nhật thông tin 1 người giao hàng
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns></returns>
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin người giao hàng";

            var data = CommonDataService.GetShipper(id);

            return View(data);
        }
        /// <summary>
        /// Xóa 1 người giao hàng
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns></returns>
        public IActionResult Delete(int id = 0)
        {
            ViewBag.Title = "Xóa người giao hàng";
            if (Request.Method == "POST")
            {
                bool result = CommonDataService.DeleteShipper(id);
                return RedirectToAction("Index");
            }

            var model = CommonDataService.GetShipper(id);

            return View(model);
        }
        /// <summary>
        /// Bổ sung hoặc cập nhật thông tin 1 người giao hàng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Save(Shipper model)
        {
            // Tên người giao hàng và điện thoại không được trống
            if (string.IsNullOrWhiteSpace(model.ShipperName))
                ModelState.AddModelError(nameof(model.ShipperName), "Tên người giao hàng không được trống");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError(nameof(model.Phone), "Số điện thoại không được trống");

            if(!ModelState.IsValid)
            {
                ViewBag.Title = model.ShipperID == 0 ? "Bổ sung người giao hàng" : "Cập nhật thông tin người giao hàng";

                return View("Edit", model);
            }

            if (model.ShipperID == 0)
            {
                int id = CommonDataService.AddShipper(model);
            }
            else
            {
                bool result = CommonDataService.UpdateShipper(model);
            }
            return RedirectToAction("Index");
        }
    }
}
