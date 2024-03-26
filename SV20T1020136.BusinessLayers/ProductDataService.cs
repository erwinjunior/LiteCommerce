using SV20T1020136.DataLayers;
using SV20T1020136.DataLayers.SQLServer;
using SV20T1020136.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020136.BusinessLayers
{
    public static class ProductDataService
    {
        private static readonly IProductDAL productDB;

        static ProductDataService()
        {
            string connectionString = Configuration.ConnectionString;
            productDB = new ProductDAL(connectionString);
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng (không phân trang)
        /// </summary>
        /// <returns></returns>
        public static List<Product> ListOfProducts(string searchValue = "")
        {
            return productDB.List(searchValue: searchValue).ToList();
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng (có phân trang)
        /// </summary>
        /// <param name="rowCount"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <param name="categoryID"></param>
        /// <param name="supplierID"></param>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <returns></returns>
        public static List<Product> ListOfProducts(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "",
            int categoryID = 0, int supplierID = 0)
        {
            rowCount = productDB.Count(searchValue, categoryID, supplierID);
            return productDB.List(page, pageSize, searchValue, categoryID, supplierID).ToList();
        }
        /// <summary>
        /// Lấy thông tin 1 mặt hàng
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static Product? GetProduct(int productID)
        {
            return productDB.Get(productID);
        }
        /// <summary>
        /// Thêm mới 1 mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int AddProduct(Product data)
        {
            return productDB.Add(data);
        }
        /// <summary>
        /// Cập nhật thông tin 1 mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool UpdateProduct(Product data)
        {
            return productDB.Update(data);
        }
        /// <summary>
        /// Xóa 1 mặt hàng (nếu không có dữ liệu liên quan)
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static bool DeleteProduct(int productID)
        {
            if (productDB.InUsed(productID)) return false;
            return productDB.Delete(productID);
        }
        /// <summary>
        /// Kiểm tra 1 mặt hàng có dữ liệu liên quan hay không
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static bool InUsedProduct(int productID)
        {
            return productDB.InUsed(productID);
        }

        /// <summary>
        /// Lấy danh sách ảnh của 1 mặt hàng
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static List<ProductPhoto> ListOfProductPhotos(int productID)
        {
            return productDB.ListPhotos(productID).ToList();
        }
        /// <summary>
        /// Lấy thông tin 1 ảnh của 1 mặt hàng
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static ProductPhoto? GetProductPhoto(long photoID)
        {
            return productDB.GetPhoto(photoID);
        }
        /// <summary>
        /// Thêm mới 1 ảnh cho 1 mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static long AddProductPhoto(ProductPhoto data)
        {
            return productDB.AddPhoto(data);
        }
        /// <summary>
        /// Cập nhật 1 ảnh cho 1 mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool UpdateProductPhoto(ProductPhoto data)
        {
            return productDB.UpdatePhoto(data);
        }
        /// <summary>
        /// Xóa 1 ảnh cho 1 mặt hàng
        /// </summary>
        /// <param name="photoID"></param>
        /// <returns></returns>
        public static bool DeleteProductPhoto(long photoID)
        {
            return productDB.DeletPhoto(photoID);
        }

        /// <summary>
        /// Lấy danh sách thuộc tính cho 1 mặt hàng
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static List<ProductAttribute> ListOfProductAttributes(int productID)
        {
            return productDB.ListAttributes(productID).ToList();
        }
        /// <summary>
        /// Lấy thông tin 1 thuộc tính cho 1 mặt hàng
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static ProductAttribute? GetProductAttribute(long attributeID)
        {
            return productDB.GetAttribute(attributeID);
        }
        /// <summary>
        /// Thêm mới 1 thuộc tính cho 1 mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static long AddProductAttribute(ProductAttribute data)
        {
            return productDB.AddAttribute(data);
        }
        /// <summary>
        /// Cập nhật thông tin 1 thuộc tính cho 1 mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool UpdateProductAttribute(ProductAttribute data)
        {
            return productDB.UpdateAttribute(data);
        }
        /// <summary>
        /// Xóa 1 thuộc tính cho 1 mặt hàng
        /// </summary>
        /// <param name="attributeID"></param>
        /// <returns></returns>
        public static bool DeleteProductAttribute(long attributeID)
        {
            return productDB.DeleteAttribute(attributeID);
        }
    }
}
