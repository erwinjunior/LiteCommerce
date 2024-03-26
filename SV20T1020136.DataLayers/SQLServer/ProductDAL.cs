using Dapper;
using SV20T1020136.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020136.DataLayers.SQLServer
{
    public class ProductDAL : _BaseDAL, IProductDAL
    {
        public ProductDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Product data)
        {
            int productId = 0;

            using (var connection = OpenConnection())
            {
                var sql = @"insert into Products(ProductName, ProductDescription, 
                                SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                            values (@productName, @productDescription, @supplierID, 
                                @categoryID, @unit, @price, @photo, @isSelling)
                            select @@identity";
                var parameters = new
                {
                    productName = data.ProductName ?? "",
                    productDescription = data.ProductDescription ?? "",
                    supplierID = data.SupplierID,
                    categoryID = data.CategoryID,
                    unit = data.Unit ?? "",
                    price = data.Price,
                    photo = data.Photo ?? "",
                    isSelling = data.IsSelling
                };

                productId = connection.ExecuteScalar<int>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return productId;
        }

        public long AddAttribute(ProductAttribute data)
        {
            long attributeID = 0;

            using(var connection = OpenConnection())
            {
                var sql = @"if exists(select DisplayOrder from ProductAttributes where AttributeID <> @attributeID 
                                and DisplayOrder = @displayOrder and productID = @productID)
	                            select 0
                            else
                                begin
                                    insert into ProductAttributes(ProductID, AttributeName, AttributeValue, DisplayOrder)
                                        values (@productID, @attributeName, @attributeValue, @displayOrder)
                                    select @@identity
                                end";
                var parameters = new
                {
                    attributeID = data.AttributeID,
                    productID = data.ProductID,
                    attributeName = data.AttributeName ?? "",
                    attributeValue = data.AttributeValue ?? "",
                    displayOrder = data.DisplayOrder,
                };

                attributeID = connection.ExecuteScalar<int>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return attributeID;
        }

        public long AddPhoto(ProductPhoto data)
        {
            long photoID = 0;

            using(var connection = OpenConnection())
            {
                var sql = @"if exists(select DisplayOrder from ProductPhotos where PhotoID <> @photoID 
                                and DisplayOrder = @displayOrder and productID = @productID)
	                            select 0
                            else
                                begin
                                    insert into ProductPhotos(ProductID, Photo, Description, DisplayOrder, IsHidden)
                                        values (@productID, @photo, @description, @displayOrder, @isHidden)
                                    select @@identity
                                end";
                var parameters = new
                {
                    photoID = data.PhotoID,
                    productId = data.ProductID,
                    photo = data.Photo ?? "",
                    description = data.Description ?? "",
                    displayOrder = data.DisplayOrder,
                    isHidden = data.IsHidden,
                };

                photoID = connection.ExecuteScalar<int>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return photoID;
        }

        public int Count(string searchValue = "", int categoryID = 0, int supplierID = 0)
        {
            int count = 0;

            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"select count(*)
                            from Products
                            where (@searchValue = N'' or ProductName like @searchValue)
                                    and (@categoryID = 0 or CategoryID = @categoryID)
                                    and (@supplierID = 0 or SupplierID = @supplierID)";
                var parmeters = new
                {
                    searchValue = searchValue ?? "",
                    categoryID = categoryID,
                    supplierID = supplierID,
                };

                count = connection.ExecuteScalar<int>(sql, parmeters, commandType: CommandType.Text);
                connection.Close();
            }

            return count;
        }

        public bool Delete(int productID)
        {
            bool result = false;

            using (var connection = OpenConnection())
            {
                var sql = "delete from Products where ProductID = @productID";
                var parameters = new
                {
                    productID = productID,
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }

        public bool DeleteAttribute(long attributeID)
        {
            bool result = false;

            using (var connection = OpenConnection())
            {
                var sql = "delete from ProductAttributes where AttributeID = @attributeID";
                var parameters = new
                {
                    attributeID = attributeID
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }

        public bool DeletPhoto(long photoID)
        {
            bool result = false;

            using(var connection = OpenConnection())
            {
                var sql = @"delete from ProductPhotos where PhotoId = @photoID";
                var parameters = new
                {
                    photoID = photoID
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }

        public Product? Get(int productID)
        {
            Product? product = null;

            using (var connection = OpenConnection())
            {
                var sql = "select * from Products where ProductID = @productID";
                var parameters = new
                {
                    productID = productID
                };

                product = connection.QueryFirstOrDefault<Product>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return product;
        }

        public ProductAttribute? GetAttribute(long attributeID)
        {
            ProductAttribute? productAttribute = null;

            using( var connection = OpenConnection())
            {
                var sql = "select * from ProductAttributes where AttributeID = @attributeID";
                var parameters = new
                {
                    attributeID = attributeID
                };

                productAttribute = connection.QueryFirstOrDefault<ProductAttribute>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return productAttribute;
        }

        public ProductPhoto? GetPhoto(long photoID)
        {
            ProductPhoto? productPhoto = null;

            using(var connection = OpenConnection())
            {
                var sql = @"select * from ProductPhotos where PhotoID = @photoID";
                var parameters = new
                {
                    photoID = photoID
                };

                productPhoto = connection.QueryFirstOrDefault<ProductPhoto>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return productPhoto;
        }

        public bool InUsed(int productID)
        {
            bool result = false;

            using(var connection = OpenConnection())
            {
                var sql = @"if exists(select * from OrderDetails where ProductID = @productID)
	                            select 1
                            else select 0";
                var parameters = new
                {
                    productID = productID
                };

                result = connection.ExecuteScalar<bool>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return result;
        }

        public IList<Product> List(int page = 1, int pageSize = 0, string searchValue = "",
            int categoryID = 0, int supplierID = 0)
        {
            List<Product> list = new List<Product>();

            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"select *
                            from (
	                            select *, ROW_NUMBER() over (order by ProductName) as RowNumber
	                            from Products
	                            where (@searchValue = N'' or ProductName like @searchValue)
                                    and (@categoryID = 0 or CategoryID = @categoryID)
                                    and (@supplierID = 0 or SupplierID = @supplierID)
                            ) as t
                            where (@pageSize = 0) 
                                or (t.RowNumber between (@page - 1) * @pageSize + 1 and @page * @pageSize)";
                var parameters = new
                {
                    searchValue = searchValue ?? "",
                    categoryID = categoryID,
                    supplierID = supplierID,
                    page = page,
                    pageSize = pageSize,
                };

                list = connection.Query<Product>(sql, parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }

            return list;
        }

        public IList<ProductAttribute> ListAttributes(int productID)
        {
            List<ProductAttribute> list = new List<ProductAttribute>();

            using (var connection = OpenConnection())
            {
                var sql = @"select * from ProductAttributes where ProductID = @productID
                            order by DisplayOrder asc";
                var parameters = new
                {
                    productID = productID
                };

                list = connection.Query<ProductAttribute>(sql, parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }

            return list;
        }

        public IList<ProductPhoto> ListPhotos(int productID)
        {
            List<ProductPhoto> list = new List<ProductPhoto>();

            using(var connection = OpenConnection())
            {
                var sql = @"select * from ProductPhotos where ProductID = @productID
                            order by DisplayOrder asc";
                var parameters = new
                {
                    productID = productID
                };

                list = connection.Query<ProductPhoto>(sql, parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }

            return list;
        }

        public bool Update(Product data)
        {
            bool result = false;

            using (var connection = OpenConnection())
            {
                var sql = @"update Products
                            set ProductName = @productName,
	                            ProductDescription = @productDescription,
	                            Unit = @unit,
	                            Price = @price,
	                            Photo = @photo,
	                            IsSelling = @isSelling
                            where ProductID = @productID";
                var parameters = new
                {
                    productName = data.ProductName ?? "",
                    productDescription = data.ProductDescription ?? "",
                    unit = data.Unit,
                    price = data.Price,
                    photo = data.Photo ?? "",
                    isSelling = data.IsSelling,
                    productID = data.ProductID
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }

        public bool UpdateAttribute(ProductAttribute data)
        {
            bool result = false;

            using(var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from ProductAttributes where AttributeID <> @attributeID
                                and ProductID = @productID and DisplayOrder = @displayOrder)    
                                begin
                                    update ProductAttributes
                                    set AttributeName = @attributeName,
	                                    AttributeValue = @attributeValue,
	                                    DisplayOrder = @displayOrder
                                    where AttributeID = @attributeID
                                end";
                var parameters = new
                {
                    attributeName = data.AttributeName ?? "",
                    attributeValue = data.AttributeValue ?? "",
                    displayOrder = data.DisplayOrder,
                    attributeID = data.AttributeID,
                    productID = data.ProductID
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }

        public bool UpdatePhoto(ProductPhoto data)
        {
            bool result = false;

            using(var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from ProductPhotos where PhotoID <> @photoID
                                and ProductID = @productID and DisplayOrder = @displayOrder)    
                                begin
                                    update ProductPhotos
                                    set Photo = @photo,
	                                    Description = @description,
	                                    DisplayOrder = @displayOrder,
	                                    IsHidden = @isHidden
                                    where PhotoID = @photoID
                                end";
                var parameters = new
                {
                    photo = data.Photo,
                    description = data.Description,
                    displayOrder = data.DisplayOrder,
                    isHidden = data.IsHidden,
                    photoID = data.PhotoID,
                    productID = data.ProductID
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }
    }
}
