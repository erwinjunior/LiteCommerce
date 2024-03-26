using Dapper;
using SV20T1020136.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020136.DataLayers.SQLServer
{
    public class SupplierDAL : _BaseDAL, ICommonDAL<Supplier>
    {
        public SupplierDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Supplier data)
        {
            int id = 0;

            using(var connection = OpenConnection())
            {
                var sql = @"insert into Suppliers(SupplierName, ContactName, Province, Address, Phone, Email, DateOfIncorporation) 
                            values (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email, @DateOfIncorporation)
                            select @@identity";
                var parameters = new
                {
                    SupplierName = data.SupplierName ?? "",
                    ContactName = data.ContactName ?? "",
                    Province = data.Province ?? "",
                    Address = data.Address ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? "",
                    DateOfIncorporation = data.DateOfIncorporation
                };

                id = connection.ExecuteScalar<int>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return id;
        }

        public int Count(string searchValue = "")
        {
            int count = 0;
            if(!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"select count(*) from Suppliers 
                            where (@searchValue = N'') or (SupplierName like @searchValue)";
                var parameters = new
                {
                    searchValue = searchValue ?? "",
                };

                count = connection.ExecuteScalar<int>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return count;
        }

        public bool Delete(int id)
        {
            bool result = false;

            using(var connection = OpenConnection())
            {
                var sql = "delete from Suppliers where SupplierID = @SupplierID";
                var parameters = new
                {
                    SupplierID = id,
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }

        public Supplier? Get(int id)
        {
            Supplier? supplier = null;
            
            using( var connection = OpenConnection())
            {
                var sql = "select * from Suppliers where SupplierID = @SupplierID";
                var parameters = new
                {
                    SupplierID = id
                };

                supplier = connection.QueryFirstOrDefault<Supplier>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return supplier;
        }

        public bool IsUsed(int id)
        {
            bool result = false;

            using(var connection = OpenConnection())
            {
                var sql = @"if exists(select SupplierID from Products where SupplierID = @SupplierID)
                                select 1
                            else select 0";
                var parameters = new
                {
                    SupplierID = id
                };

                result = connection.ExecuteScalar<bool>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return result;
        }

        public IList<Supplier> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Supplier> list = new List<Supplier>();
            if(!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using( var connection = OpenConnection())
            {
                var sql = @"select *
                        from (
	                        select *, ROW_NUMBER() over (order by SupplierName) as RowNumber
	                        from Suppliers
	                        where (@searchValue = N'') or (SupplierName like @searchValue)
	                        ) as t
                        where (@pageSize = 0) or (t.RowNumber between (@page - 1) * @pageSize + 1 and @page * @pageSize)
                        order by t.RowNumber";
                var parameters = new
                {
                    page = page,
                    pageSize = pageSize,
                    searchValue = searchValue
                };

                list = connection.Query<Supplier>(sql, parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }

            return list;
        }

        public bool Update(Supplier data)
        {
            bool result = false;

            using (var connection = OpenConnection())
            {
                var sql = @"update Suppliers
                            set SupplierName = @SupplierName,
                                ContactName = @ContactName,
                                Province = @Province,
                                Address = @Address,
                                Phone = @Phone,
                                Email = @Email,
                                DateOfIncorporation = @DateOfIncorporation
                            where SupplierID = @SupplierID";
                var parameters = new
                {
                    SupplierName = data.SupplierName ?? "",
                    ContactName = data.ContactName ?? "",
                    Province = data.Province ?? "",
                    Address = data.Address ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? "",
                    SupplierID = data.SupplierID,
                    DateOfIncorporation = data.DateOfIncorporation
                };

                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }
    }
}
