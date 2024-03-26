using Dapper;
using SV20T1020136.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020136.DataLayers.SQLServer
{
    public class EmployeeDAL : _BaseDAL, ICommonDAL<Employee>
    {
        public EmployeeDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Employee data)
        {
            int id = 0;

            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from Employees where Email = @Email)
                                select -1
                            else
                                begin
                                    insert into Employees(FullName,BirthDate,Address,Phone,Email,Photo,IsWorking)
                                    values(@FullName,@BirthDate,@Address,@Phone,@Email,@Photo,@IsWorking);

                                    select @@identity;
                                end";
                var paramters = new
                {
                   FullName = data.FullName ?? "",
                   BirthDate = data.BirthDate,
                   Address = data.Address ?? "",
                   Phone = data.Phone ?? "",
                   Email = data.Email ?? "",
                   Photo = data.Photo ?? "",
                   IsWorking = data.IsWorking
                };

                id = connection.ExecuteScalar<int>(sql: sql, param: paramters, commandType: CommandType.Text);
                connection.Close();
            }

            return id;
        }

        public int Count(string searchValue = "")
        {
            int count = 0;
            if(!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using(var connection = OpenConnection())
            {
                var sql = @"select count(*) from Employees
                            where (@searchValue = N'') or (FullName like @searchValue)";
                var parameters = new
                {
                    searchValue = searchValue ?? ""
                };

                count = connection.ExecuteScalar<int>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return count;
        }

        public bool Delete(int id)
        {
            bool result = false;

            using( var connection = OpenConnection())
            {
                var sql = "delete from Employees where EmployeeID = @EmployeeID";
                var parameters = new
                {
                    EmployeeID = id,
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }

        public Employee? Get(int id)
        {
            Employee? employee = null;

            using( var connection = OpenConnection())
            {
                var sql = @"select * from Employees where EmployeeID = @EmployeeID";
                var parameters = new
                {
                    EmployeeID = id
                };

                employee = connection.QueryFirstOrDefault<Employee>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return employee;
        }

        public bool IsUsed(int id)
        {
            bool result = false;

            using( var connection = OpenConnection())
            {
                var sql = @"if exists(select EmployeeID from Orders where EmployeeID = @EmployeeID)
                                select 1
                            else select 0";
                var parameters = new
                {
                    EmployeeID = id
                };

                result = connection.ExecuteScalar<bool>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return result;
        }

        public IList<Employee> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Employee> list = new List<Employee>();
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using ( var connection = OpenConnection())
            {
                var sql = @"select *
                            from (
                                select *, ROW_NUMBER() over (order by FullName) as RowNumber
                                from Employees
                                where (@searchValue = N'') or (FullName like @searchValue) 
                            ) as t
                            where (@pageSize = 0) or (t.RowNumber between (@page - 1) * @pageSize + 1 and @page * @pageSize)
                            order by t.RowNumber";
                var parameters = new
                {
                    searchValue = searchValue ?? "",
                    page = page,
                    pageSize = pageSize,
                };

                list = connection.Query<Employee>(sql, parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }

            return list;
        }

        public bool Update(Employee data)
        {
            bool result = false;

            using(var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from Employees where EmployeeID <> @EmployeeID and Email = @Email)
                                begin
                                    update Employees
                                    set FullName = @FullName,
                                        BirthDate = @BirthDate,
                                        Address = @Address,
                                        Phone = @Phone,
                                        Email = @Email,
                                        Photo = @Photo,
                                        IsWorking = @IsWorking
                                    where EmployeeID = @EmployeeID
                                end";
                var parameters = new
                {
                    EmployeeID = data.EmployeeID,
                    Email = data.Email ?? "",
                    FullName = data.FullName ?? "",
                    BirthDate = data.BirthDate,
                    Address = data.Address ?? "",
                    Phone = data.Phone ?? "",
                    Photo = data.Photo ?? "",
                    IsWorking = data.IsWorking
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }
    }
}
