using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020136.BusinessLayers;
using SV20T1020136.DomainModels;
using SV20T1020136.Web.Models;
using System.Buffers;
using System.Reflection;

namespace SV20T1020136.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Employee}")]
    public class ProductController : Controller
    {
        // Số mặt hàng trên 1 trang khi hiển thị danh sách mặt hàng
        const int PAGE_SIZE = 20;
        // Biến session lưu lại điều kiện tìm kiếm mặt hàng
        const string PRODUCT_SEARCH = "product_search";
        /// <summary>
        /// Giao diện tìm kiếm và hiển thị kết quả tìm kiếm mặt hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ProductSearchInput? input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);
            if (input == null)
            {
                input = new ProductSearchInput
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0
                };
            }
            
            return View(input);
        }
        /// <summary>
        /// Tìm kiếm mặt hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IActionResult Search(ProductSearchInput input)
        {
            int rowCount = 0;
            var data = ProductDataService.ListOfProducts(out rowCount, input.Page, input.PageSize,
                input.SearchValue ?? "", input.CategoryID, input.SupplierID);

            var model = new ProductSeachResult
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                Data = data,
            };
            // lưu lại điều kiện tìm kiếm
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);

            return View(model);
        }
        /// <summary>
        /// Giao diện bổ sung 1 mặt hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.IsEdit = false;
            ViewBag.Title = "Thêm mặt hàng";

            var model = new Product
            {
                ProductID = 0,
                Photo = "nophoto.png"
            };

            return View("Edit", model);
        }
        /// <summary>
        /// Giao diện cập nhật thông tin 1 mặt hàng
        /// </summary>
        /// <param name="id">Mã mặt hàng</param>
        /// <returns></returns>
        public IActionResult Edit(int id = 0)
        {
            ViewBag.IsEdit = true;
            ViewBag.Title = "Cập nhật thông tin mặt hàng";

            var model = ProductDataService.GetProduct(id);
            if (model == null) return RedirectToAction("Index");

            return View(model);
        }
        /// <summary>
        /// Xóa 1 mặt hàng
        /// </summary>
        /// <param name="id">Mã mặt hàng</param>
        /// <returns></returns>
        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                bool result = ProductDataService.DeleteProduct(id);
                return RedirectToAction("Index");
            }

            var model = ProductDataService.GetProduct(id);
            if (model == null) return RedirectToAction("Index");

            return View(model);
        }
        /// <summary>
        /// Bổ sung hoặc cập nhật thông tin 1 mặt hàng
        /// </summary>
        /// <param name="model"></param>
        /// <param name="uploadPhoto"></param>
        /// <returns></returns>
        public IActionResult Save(Product model, IFormFile? uploadPhoto = null)
        {
            // Kiểm soát dữ liệu trong model xem có hợp lệ không?
            if (string.IsNullOrWhiteSpace(model.ProductName))
                ModelState.AddModelError(nameof(model.ProductName), "Tên mặt hàng không được trống");
            if (model.CategoryID <= 0)
                ModelState.AddModelError(nameof(model.CategoryID), "Vui lòng chọn loại hàng");
            if (model.SupplierID <= 0)
                ModelState.AddModelError(nameof(model.SupplierID), "Vui lòng chọn nhà cung cấp");
            if (string.IsNullOrWhiteSpace(model.Unit))
                ModelState.AddModelError(nameof(model.Unit), "Đơn vị tính không được để trống");
            if (model.Price <= 0)
                ModelState.AddModelError(nameof(model.Price), "Giá tiền phải là 1 giá trị dương");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";
                ViewBag.IsEdit = model.ProductID != 0;
                return View("Edit", model);
            }

            // Xử lý ảnh upload: Nếu có ảnh được upload lên thì lưu ảnh mới, còn không thì giữ lại ảnh cũ
            if (uploadPhoto != null)
            {
                string fileName = $"{DateTime.Now:yyyy-MM-dd}_{uploadPhoto.FileName}";

                // Đường dẫn vật lý lưu trên server
                string filePath = Path.Combine(ApplicationContext.HostEnviroment!.WebRootPath,
                    @"images\products", fileName);

                // Lưu file lên server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }

                // Gán tên ảnh cho model
                model.Photo = fileName;
            }

            if (model.ProductID == 0)
            {
                int id = ProductDataService.AddProduct(model);
            }
            else
            {
                bool result = ProductDataService.UpdateProduct(model);
            }

            return RedirectToAction("Index");
            //return Json(model);
        }

        public IActionResult Photo(ProductPhoto model, int id = 0, int photoID = 0,
            string method = "", IFormFile? uploadPhoto = null)
        {

            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh mặt hàng";
                    var addModel = new ProductPhoto
                    {
                        PhotoID = 0,
                        Photo = "nophoto.png",
                        ProductID = id,
                    };

                    return View(addModel);

                case "edit":
                    ViewBag.Title = "Cập nhật ảnh mặt hàng";

                    var editModel = ProductDataService.GetProductPhoto(photoID);
                    return View(editModel);

                case "delete":
                    // TODO: xóa trực tiếp ảnh có mã là photoId (không cần xác nhận)
                    ViewBag.Title = "Xóa ảnh mặt hàng";

                    bool result = ProductDataService.DeleteProductPhoto(photoID);
                    return RedirectToAction("Edit", new { id });

                case "save":
                    // ràng buộc dữ liệu
                    if (string.IsNullOrWhiteSpace(model.Photo))
                        ModelState.AddModelError(nameof(model.Photo), "Ảnh không được để trống");

                    if (model.DisplayOrder <= 0)
                        ModelState.AddModelError(nameof(model.DisplayOrder), "Thứ tự hiển thị phải là số dương");

                    if (!ModelState.IsValid)
                    {
                        ViewBag.Title = model.PhotoID == 0 ? "Bổ sung ảnh mặt hàng" : "Cập nhật ảnh mặt hàng";
                        return View(model);
                    }
                    // Xử lý ảnh upload: Nếu có ảnh được upload lên thì lưu ảnh mới, còn không thì giữ lại ảnh cũ
                    if (uploadPhoto != null)
                    {
                        string fileName = $"{DateTime.Now:yyyy-MM-dd}_{uploadPhoto.FileName}";

                        // Đường dẫn vật lý lưu trên server
                        string filePath = Path.Combine(ApplicationContext.HostEnviroment!.WebRootPath,
                            @"images\products", fileName);

                        // Lưu file lên server
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            uploadPhoto.CopyTo(stream);
                        }

                        // Gán tên ảnh cho model
                        model.Photo = fileName;
                    }

                    if (model.PhotoID == 0)
                    {
                        long newPhotoID = ProductDataService.AddProductPhoto(model);
                        if(newPhotoID == 0)
                        {
                            ViewBag.Title = "Bổ sung ảnh mặt hàng";
                            ModelState.AddModelError("SaveError", "Có lỗi, không thể bổ sung");
                            return View(model);
                        }
                    }
                    else
                    {
                        bool updateResult = ProductDataService.UpdateProductPhoto(model);
                        if(!updateResult)
                        {
                            ViewBag.Title = "Cập nhật ảnh mặt hàng";
                            ModelState.AddModelError("SaveError", "Có lỗi, không thể cập nhật");
                            return View(model);
                        }
                    }
                    return RedirectToAction("Edit", new { id });

                default:
                    return RedirectToAction("Index");
            }
        }

        public IActionResult Attribute(ProductAttribute model, int id = 0, int attributeID = 0,
            string method = "", IFormFile? uploadPhoto = null)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính mặt hàng";
                    var addModel = new ProductAttribute
                    {
                        AttributeID = 0,
                        ProductID = id
                    };

                    return View(addModel);

                case "edit":
                    ViewBag.Title = "Cập nhật thuộc tính mặt hàng";
                    var editModel = ProductDataService.GetProductAttribute(attributeID);

                    return View(editModel);

                case "delete":
                    ViewBag.Title = "Xóa thuộc tính mặt hàng";

                    bool deleteResult = ProductDataService.DeleteProductAttribute(attributeID);

                    return RedirectToAction("Edit", new { id });

                case "save":
                    // ràng buộc dữ liệu
                    if (string.IsNullOrEmpty(model.AttributeValue))
                        ModelState.AddModelError(nameof(model.AttributeValue), "Giá trị thuộc tính không được để trống");

                    if (string.IsNullOrEmpty(model.AttributeName))
                        ModelState.AddModelError(nameof(model.AttributeName), "Tên thuộc tính không được để trống");

                    if (model.DisplayOrder <= 0)
                        ModelState.AddModelError(nameof(model.DisplayOrder), "Thứ tự hiển thị phải là số dương");

                    if (!ModelState.IsValid)
                    {
                        ViewBag.Title = model.AttributeID == 0 ? "Bổ sung thuộc tính mặt hàng" : "Cập nhật thuộc tính mặt hàng";
                        return View(model);
                    }
                    if (model.AttributeID == 0)
                    {
                        long newAttributeID = ProductDataService.AddProductAttribute(model);
                        if (newAttributeID == 0)
                        {
                            ViewBag.Title = "Bổ sung thuộc tính mặt hàng";
                            ModelState.AddModelError("SaveError", "Có lỗi, không thể bổ sung");
                            return View(model);
                        }
                    }
                    else
                    {
                        bool updateResult = ProductDataService.UpdateProductAttribute(model);
                        if (!updateResult)
                        {
                            ViewBag.Title = "Cập nhật thuộc tính mặt hàng";
                            ModelState.AddModelError("SaveError", "Có lỗi, không thể cập nhật");
                            return View(model);
                        }
                    }

                    return RedirectToAction("Edit", new { id });

                default:
                    return RedirectToAction("Index");
            }
        }
    }
}
