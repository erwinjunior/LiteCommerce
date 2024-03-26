using Dapper;
using SV20T1020136.DomainModels;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020136.DataLayers.SQLServer
{
    public class OrderDAL : _BaseDAL, IOrderDAL
    {
        public OrderDAL(string connectionString) : base(connectionString)
        {
        }
        public int Add(Order data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"insert into Orders(CustomerId, OrderTime,
                            DeliveryProvince, DeliveryAddress,
                            EmployeeID, Status)
                            values(@CustomerID, getdate(),
                            @DeliveryProvince, @DeliveryAddress,
                            @EmployeeID, @Status);

                            select @@identity";
                var parameters = new
                {
                    CustomerId = data.CustomerID,
                    DeliveryProvince = data.DeliveryProvince ?? "",
                    DeliveryAddress = data.DeliveryAddress ?? "",
                    EmployeeID = data.EmployeeID,
                    Status = data.Status
                };

                id = connection.ExecuteScalar<int>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return id;
        }
        public int Count(int status = 0, DateTime? fromTime = null,
        DateTime? toTime = null, string searchValue = "")
        {
            int count = 0;
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";
            using (var connection = OpenConnection())
            {
                var sql = @"select count(*)
                            from Orders as o
                            left join Customers as c on o.CustomerID = c.CustomerID
                            left join Employees as e on o.EmployeeID = e.EmployeeID
                            left join Shippers as s on o.ShipperID = s.ShipperID
                            where (@Status = 0 or o.Status = @Status)
                            and (@FromTime is null or o.OrderTime >= @FromTime)
                            and (@ToTime is null or o.OrderTime <= @ToTime)
                            and (@SearchValue = N''
                            or c.CustomerName like @SearchValue
                            or e.FullName like @SearchValue
                            or s.ShipperName like @SearchValue)";
                var parameters = new
                {
                    Status = status,
                    FromTime = fromTime,
                    ToTime = toTime,
                    SearchValue = searchValue ?? ""
                };
                count = connection.ExecuteScalar<int>(sql, parameters, commandType: CommandType.Text);

                connection.Close();
            }
            return count;
        }
        public bool Delete(int orderID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"delete from Orders where OrderID = @OrderID";
                var parameters = new
                {
                    OrderId = orderID,
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        public bool DeleteDetail(int orderID, int productID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"delete from OrderDetails
                            where OrderID = @OrderID and ProductID = @ProductID";
                var parameters = new
                {
                    OrderId = orderID,
                    ProductId = productID,
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        public Order? Get(int orderID)
        {
            Order? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"select o.*,
                            c.CustomerName,
                            c.ContactName as CustomerContactName,
                            c.Address as CustomerAddress,
                            c.Phone as CustomerPhone,
                            c.Email as CustomerEmail,
                            e.FullName as EmployeeName,
                            s.ShipperName,
                            s.Phone as ShipperPhone
                            from Orders as o
                            left join Customers as c on o.CustomerID = c.CustomerID
                            left join Employees as e on o.EmployeeID = e.EmployeeID
                            left join Shippers as s on o.ShipperID = s.ShipperID
                            where o.OrderID = @OrderID";
                var parameters = new
                {
                    OrderId = orderID
                };

                data = connection.QueryFirstOrDefault<Order>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return data;
        }
        public OrderDetail? GetDetail(int orderID, int productID)
        {
            OrderDetail? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"select od.*, p.ProductName, p.Photo, p.Unit
                            from OrderDetails as od
                            join Products as p on od.ProductID = p.ProductID
                            where od.OrderID = @OrderID and od.ProductID = @ProductID";
                var parameters = new
                {
                    OrderID = orderID,
                    ProductId = productID
                };

                data = connection.QueryFirstOrDefault<OrderDetail>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return data;
        }
        public IList<Order> List(int page = 1, int pageSize = 0,
            int status = 0, DateTime? fromTime = null, DateTime? toTime = null,
            string searchValue = "")

        {
            List<Order> list = new List<Order>();
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";
            using (var connection = OpenConnection())
            {
                var sql = @"with cte as
                            (
                                select row_number() over(order by o.OrderTime desc) as RowNumber,
                                o.*,
                                c.CustomerName,
                                c.ContactName as CustomerContactName,
                                c.Address as CustomerAddress,
                                c.Phone as CustomerPhone,
                                c.Email as CustomerEmail,
                                e.FullName as EmployeeName,
                                s.ShipperName,
                                s.Phone as ShipperPhone
                                from Orders as o
                                left join Customers as c on o.CustomerID = c.CustomerID
                                left join Employees as e on o.EmployeeID = e.EmployeeID
                                left join Shippers as s on o.ShipperID = s.ShipperID
                                where (@Status = 0 or o.Status = @Status)
                                and (@FromTime is null or o.OrderTime >= @FromTime)
                                and (@ToTime is null or o.OrderTime <= @ToTime)
                                and (@SearchValue = N''
                                or c.CustomerName like @SearchValue
                                or e.FullName like @SearchValue
                                or s.ShipperName like @SearchValue)
                            )
                            select * from cte
                            where (@PageSize = 0)
                            or (RowNumber between (@Page - 1) * @PageSize + 1 and @Page * @PageSize)
                            order by RowNumber";
                var parameters = new
                {
                    Status = status,
                    FromTime = fromTime,
                    ToTime = toTime,
                    SearchValue = searchValue ?? "",
                    Page = page,
                    PageSize = pageSize,
                };

                list = connection.Query<Order>(sql, parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return list;
        }


        public IList<OrderDetail> ListDetails(int orderID)
        {
            List<OrderDetail> list = new List<OrderDetail>();
            using (var connection = OpenConnection())
            {
                var sql = @"select od.*, p.ProductName, p.Photo, p.Unit
                            from OrderDetails as od
                            join Products as p on od.ProductID = p.ProductID
                            where od.OrderID = @OrderID";
                var parameters = new
                {
                    OrderId = orderID,
                };

                list = connection.Query<OrderDetail>(sql, parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return list;
        }

        public IList<OrderStatus> ListStatus()
        {
            List<OrderStatus> list = new List<OrderStatus>();
            using(var connection = OpenConnection())
            {
                var sql = "select * from OrderStatus";

                list = connection.Query<OrderStatus>(sql, commandType: CommandType.Text).ToList();
                connection.Close();
            }

            return list;
        }

        public bool SaveDetail(int orderID, int productID, int quantity, decimal salePrice)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from OrderDetails 
                                where OrderID = @OrderID and ProductID = @ProductID)
                                begin
                                    update OrderDetails
                                    set Quantity = @Quantity,
                                    SalePrice = @SalePrice
                                    where OrderID = @OrderID and ProductID = @ProductID                                
                                end
                            else
                                insert into OrderDetails(OrderID, ProductID, Quantity, SalePrice)
                                values(@OrderID, @ProductID, @Quantity, @SalePrice)";
                var parameters = new
                {
                    OrderID = orderID,
                    ProductID = productID,
                    Quantity = quantity,
                    SalePrice = salePrice
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        public bool Update(Order data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"update Orders
                            set CustomerID = @CustomerID,
                            OrderTime = @OrderTime,
                            DeliveryProvince = @DeliveryProvince,
                            DeliveryAddress = @DeliveryAddress,
                            EmployeeID = @EmployeeID,
                            AcceptTime = @AcceptTime,
                            ShipperID = @ShipperID,
                            ShippedTime = @ShippedTime,
                            FinishedTime = @FinishedTime,
                            Status = @Status
                            where OrderID = @OrderID";
                var parameters = new
                {
                    OrderTime = data.OrderTime,
                    DeliveryProvince = data.DeliveryProvince ?? "",
                    DeliveryAddress = data.DeliveryAddress ?? "",
                    EmployeeID = data.EmployeeID,
                    AcceptTime = data.AcceptTime,
                    ShipperID = data.ShipperID,
                    ShippedTime = data.ShippedTime,
                    FinishedTime = data.FinishedTime,
                    Status = data.Status,
                    OrderID = data.OrderID,
                    CustomerID = data.CustomerID
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
    }
}
