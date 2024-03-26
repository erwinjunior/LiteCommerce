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
    public class CategoryDAL : _BaseDAL, ICommonDAL<Category>
    {
        public CategoryDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Category data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"insert into Categories(CategoryName, Description) values (@CategoryName, @Description)
                            select @@identity";
                var parameters = new
                {
                    CategoryName = data.CategoryName,
                    Description = data.Description,
                    CategoryID = data.CategoryID
                };

                id = connection.ExecuteScalar<int>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return id;
        }

        public int Count(string searchValue = "")
        {
            int count = 0;
            if (!string.IsNullOrEmpty(searchValue)) searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"select count(*) from Categories
                            where (@searchValue = N'') or (CategoryName like @searchValue)";


                var parameters = new
                {
                    searchValue = searchValue ?? ""
                };

                count = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return count;
        }

        public bool Delete(int id)
        {
            bool result = false;

            using(var connection = OpenConnection())
            {
                var sql = "delete from Categories where CategoryID = @CategoryID";
                var parameters = new
                {
                    CategoryID = id
                };

                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }

        public Category? Get(int id)
        {
            Category? category = null;

            using (var connection = OpenConnection())
            {
                var sql = "select * from Categories where CategoryID = @CategoryID";
                var parameters = new
                {
                    CategoryID = id
                };

                category = connection.QueryFirstOrDefault<Category>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return category;
        }

        public bool IsUsed(int id)
        {
            bool result = false;

            using( var connection = OpenConnection())
            {
                var sql = @"if exists(select CategoryID from Products where CategoryID = @CategoryID)
                                select 1
                            else select 0";
                var parameters = new
                {
                    CategoryID = id
                };

                result = connection.ExecuteScalar<bool>(sql, parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return result;
        }

        public IList<Category> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Category> list = new List<Category>();

            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"select *
                        from (
	                        select *, ROW_NUMBER() over (order by CategoryName) as RowNumber
	                        from Categories
	                        where (@searchValue = N'') or (CategoryName like @searchValue)
	                        ) as t
                        where (@pageSize = 0) or (t.RowNumber between (@page - 1) * @pageSize + 1 and @page * @pageSize)
                        order by t.RowNumber";

                var parameters = new
                {
                    page = page,
                    pageSize = pageSize,
                    searchValue = searchValue ?? ""
                };

                list = connection.Query<Category>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }


            return list;
        }

        public bool Update(Category data)
        {
            bool result = false;

            using (var connection = OpenConnection())
            {
                var sql = @"update Categories
                            set CategoryName = @CategoryName,
                                Description = @Description
                            where CategoryID = @CategoryID";
                var parameters = new
                {
                    CategoryName = data.CategoryName ?? "",
                    Description = data.Description ?? "",
                    CategoryID = data.CategoryID
                };

                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }
    }
}
