using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020136.BusinessLayers;
using SV20T1020136.DomainModels;
using SV20T1020136.Web.Models;

namespace SV20T1020136.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Employee},{WebUserRoles.Administrator}")]
    public class OrderController : Controller
    {
        // Số dòng trên 1 trang khi hiển thị danh sách đơn hàng
        private const int ORDER_PAGE_SIZE = 10;
        // Số dòng trên 1 trang khi hiển thị danh sách mặt hàng cần tìm kiếm khi lập đơn hàng
        private const int PRODUCT_PAGE_SIZE = 5;
        // Tên biến session để lưu điều kiện tìm kiếm đơn hàng
        private const string ORDER_SEARCH = "order_search";
        // Tên biến session để lưu điều kiện tìm kiếm mặt hàng khi lập đơn hàng
        private const string PRODUCT_SEARCH = "product_search_for_sale";
        // Tên biến session dùng để lưu giỏ hàng
        private const string SHOPPING_CART = "shopping_cart";
        /// <summary>
        /// Giao diện tìm kiếm và hiển thị kết quả tìm kiếm đơn hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            OrderSearchInput? input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH);
            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = ORDER_PAGE_SIZE,
                    SearchValue = "",
                    Status = 0,
                    DateRange = string.Format("{0:dd/MM/yyyy} - {1:dd/MM/yyyy}",
                                                DateTime.Today.AddMonths(-6), DateTime.Today)
                };
            }

            return View(input);
        }
        /// <summary>
        /// Thực hiện chức năng tìm kiếm đơn hàng
        /// </summary>
        /// <param name="input"></param>
        /// <param name="orderTime"></param>
        /// <returns></returns>
        public IActionResult Search(OrderSearchInput input, string orderTime = "")
        {
            int rowCount = 0;
            input.DateRange = orderTime;
            var data = OrderDataService.ListOrders(out rowCount, input.Page, input.PageSize,
                input.Status, input.FromTime, input.ToTime, input.SearchValue ?? "");
            var model = new OrderSearchResult
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                Status = input.Status,
                TimeRange = input.DateRange,
                RowCount = rowCount,
                Data = data
            };
            ApplicationContext.SetSessionData(ORDER_SEARCH, input);

            return View(model);
        }
        /// <summary>
        /// Hiển thị thông tin chi tiết của 1 đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Details(int id = 0)
        {
            var order = OrderDataService.GetOrder(id);
            if (order == null)
                return RedirectToAction("Index");

            var details = OrderDataService.ListOrderDetails(id);
            var model = new OrderDetailModel
            {
                Order = order,
                Details = details
            };

            return View(model);
        }
        /// <summary>
        /// Giao diện lập đơn hàng mới
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);
            if (input == null)
            {
                input = new ProductSearchInput
                {
                    Page = 1,
                    PageSize = PRODUCT_PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(input);
        }
        /// <summary>
        /// Giao diện cập nhật thông tin đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Update(int id = 0)
        {
            var model = OrderDataService.GetOrder(id);
            return View(model);
        }
        /// <summary>
        /// Cập nhật thông tin đơn hàng với dữ liệu mới
        /// Những thông tin có thể cập nhật sẽ phụ thuộc vào trạng thái đơn hàng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Save(Order model)
        {
            if (string.IsNullOrWhiteSpace(model.DeliveryAddress) && model.Status != Constants.ORDER_SHIPPING)
                ModelState.AddModelError(nameof(model.DeliveryAddress), "Vui lòng nhập địa chỉ nhận hàng");

            if (!ModelState.IsValid)
                return View("Update", model);

            bool result = OrderDataService.UpdateOrder(model);

            return RedirectToAction("Details", new { id = model.OrderID });
        }
        /// <summary>
        /// Giao diện chọn người giao hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Shipping(int id = 0)
        {
            ViewBag.OrderId = id;
            return View();
        }
        /// <summary>
        /// Ghi nhận người giao hàng và chuyển trạng thái đơn hàng sang đang giao
        /// Trả về chuỗi khác rỗng nếu có lỗi hoặc đầu vào không hợp lệ
        /// Trả về chuỗi rỗng nếu thành công
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <param name="shipperID">Mã người giao hàng</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Shipping(int id = 0, int shipperID = 0)
        {
            if (shipperID <= 0)
                return Json("Vui lòng chọn người giao hàng");

            bool result = OrderDataService.ShipOrder(id, shipperID);
            if (!result)
                return Json("Đơn hàng không cho phép chuyển cho người giao hàng");

            return Json("");

        }
        /// <summary>
        /// Chuyển đơn hàng sang trạng thái đã duyệt
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Accept(int id = 0)
        {
            bool result = OrderDataService.AcceptOrder(id);
            if (!result)
                ViewBag.Message = "Không thể duyệt đơn hàng này";

            return RedirectToAction("Details", new { id });
        }
        /// <summary>
        /// Chuyển đơn hàng sang trạng thái đã kết thúc
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Finish(int id = 0)
        {
            bool result = OrderDataService.FinishOrder(id);
            if (!result)
                ViewBag.Message = "Không thể ghi nhận trạng thái kết thúc cho đơn hàng này";

            return RedirectToAction("Details", new { id });
        }
        /// <summary>
        /// Chuyển đơn hàng sang trạng thái từ chối
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Reject(int id = 0)
        {
            bool result = OrderDataService.RejectOrder(id);
            if (!result)
                ViewBag.Message = "Không thể thực hiện thao tác từ chối với đơn hàng này";

            return RedirectToAction("Details", new { id });
        }
        /// <summary>
        /// Xóa 1 đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Delete(int id = 0)
        {
            bool result = OrderDataService.DeleteOrder(id);
            if (!result)
            {
                ViewBag.Message = "Không thể xóa đơn hàng này";
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction("Index");
        }
        /// <summary>
        /// Chuyển đơn hàng sang trạng thái bị hủy
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Cancel(int id = 0)
        {
            bool result = OrderDataService.CancelOrder(id);
            if (!result)
                ViewBag.Message = "Không thể thực hiện thao tác từ chối với đơn hàng này";

            return RedirectToAction("Details", new { id });
        }
        /// <summary>
        /// Xóa mặt hàng ra khỏi đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <param name="productID">Mã mặt hàng cần xóa</param>
        /// <returns></returns>
        public IActionResult DeleteDetail(int id = 0, int productID = 0)
        {
            bool result = OrderDataService.DeleteOrderDetail(id, productID);
            if (!result)
                TempData["Message"] = "Không thể xóa mặt hàng ra khỏi đơn hàng";

            return RedirectToAction("Details", new { id });
        }
        /// <summary>
        /// Giao diện sửa đổi thông tin mặt hàng trong đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <param name="productID">Mã mặt hàng</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult EditDetail(int id = 0, int productID = 0)
        {
            var model = OrderDataService.GetOrderDetail(id, productID);
            return View(model);
        }
        /// <summary>
        /// Cập nhật giá bán và số lượng 1 mặt hàng được bán trong đơn hàng
        /// Trả về chuỗi khác rỗng thông báo lỗi nếu đầu vào không hợp lệ hoặc lỗi
        /// Trả về chuỗi rỗng nếu thành công
        /// </summary>
        /// <param name="orderID">Mã đơn hàng</param>
        /// <param name="productID">Mã mặt hàng</param>
        /// <param name="quantity">Số lượng bán</param>
        /// <param name="salePrice">Giá bán</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateDetail(int orderID = 0, int productID = 0, int quantity = 0,
            decimal salePrice = 0)
        {

            if (quantity <= 0)
                return Json("Số lượng phải lớn hơn 0");
            if (salePrice <= 0)
                return Json("Giá phải lớn hơn 0");

            bool result = OrderDataService.SaveOrderDetail(orderID, productID, quantity, salePrice);
            if (!result)
                return Json("Không thể cập nhật chi tiết đơn hàng này");

            return Json("");
        }
        /// <summary>
        /// Tìm kiếm mặt hàng và đưa vào giỏ hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IActionResult SearchProduct(ProductSearchInput input)
        {
            int rowCount = 0;
            // Chỉ hiển thị những sản phẩm đang bán
            var data = ProductDataService.ListOfProducts(out rowCount, input.Page,
                input.PageSize, input.SearchValue ?? "").Where(item => item.IsSelling == true).ToList();

            var model = new ProductSeachResult
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);

            return View(model);
        }
        /// <summary>
        /// Lấy giỏ hàng đang lưu trong session
        /// </summary>
        /// <returns></returns>
        private List<OrderDetail> GetShoppingCart()
        {
            // giỏ hàng là danh sách các mặt hàng (order detail) được chọn để bán trong đơn hàng
            // và được lưu trong session

            var shopppingCart = ApplicationContext.GetSessionData<List<OrderDetail>>(SHOPPING_CART);
            if (shopppingCart == null)
            {
                shopppingCart = new List<OrderDetail>();
                ApplicationContext.SetSessionData(SHOPPING_CART, shopppingCart);
            }

            return shopppingCart;
        }
        /// <summary>
        /// Trang hiển thị danh sách các mặt hàng đang có trong giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult ShowShoppingCart()
        {
            var model = GetShoppingCart();
            return View(model);
        }
        /// <summary>
        /// Bổ sung 1 mặt hàng vào giỏ hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IActionResult AddToCart(OrderDetail data)
        {
            if (data.SalePrice <= 0 || data.Quantity <= 0)
                return Json("Giá bán hoặc số lượng không hợp lệ");

            var shoppingCart = GetShoppingCart();
            var existsProduct = shoppingCart.FirstOrDefault(p => p.ProductID == data.ProductID);
            if (existsProduct == null) // nếu mặt hàng chưa có trong giỏ thì thêm vào giỏ
                shoppingCart.Add(data);
            else
            {
                // Nếu mặt hàng đã có trong giỏ thì tăng số lượng bán và giá bán
                existsProduct.Quantity += data.Quantity;
                existsProduct.SalePrice = data.SalePrice;
            }
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);

            return Json("");
        }
        /// <summary>
        /// Xóa 1 mặt hàng ra khỏi giỏ hàng
        /// </summary>
        /// <param name="id">Mã mặt hàng cần xóa khỏi giỏ hàng</param>
        /// <returns></returns>
        public IActionResult RemoveFromCart(int id = 0)
        {
            var shoppingCart = GetShoppingCart();
            int index = shoppingCart.FindIndex(m => m.ProductID == id);
            if (index >= 0)
                shoppingCart.RemoveAt(index);
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);

            return Json("");
        }
        /// <summary>
        /// Xóa tất cả mặt hàng trong giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult ClearCart()
        {
            var shoppingCart = GetShoppingCart();
            shoppingCart.Clear();

            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);

            return Json("");
        }
        /// <summary>
        /// Khởi tạo đơn hàng (lập đơn hàng mới)
        /// Trả về chuỗi khác rỗng thông báo lỗi nếu đầu vào không hợp lệ hoặc tạo đơn hàng không thành công
        /// Trả về mã đơn hàng được tạo thành công (là 1 giá trị số)
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="deliveryProvince"></param>
        /// <param name="deliveryAddress"></param>
        /// <returns></returns>
        public IActionResult Init(int customerID = 0, string deliveryProvince = "",
            string deliveryAddress = "")
        {
            var shoppingCart = GetShoppingCart();
            if (shoppingCart.Count == 0)
                return Json("Giỏ hàng trống, không thể lập đơn hàng");

            if (customerID <= 0 || string.IsNullOrWhiteSpace(deliveryAddress) ||
                string.IsNullOrWhiteSpace(deliveryProvince))
                return Json("Vui lòng điền đầy đủ thông tin");

            int employeeID = Convert.ToInt32(User.GetUserData()?.UserId);
            int orderID = OrderDataService.InitOrder(employeeID, customerID, deliveryProvince,
                deliveryAddress, shoppingCart);
            ClearCart();

            return Json(orderID);
        }
    }
}
