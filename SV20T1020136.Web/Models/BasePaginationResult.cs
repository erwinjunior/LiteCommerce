using SV20T1020136.DomainModels;

namespace SV20T1020136.Web.Models
{
    public abstract class BasePaginationResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchValue { get; set; } = string.Empty;
        public int RowCount { get; set; }
        public int PageCount
        {
            get
            {
                if (PageSize == 0) return 1;

                int count = RowCount / PageSize;
                if (RowCount % PageSize > 0) ++count;

                return count;
            }
        }

    }
    public class CustomerSearchResult : BasePaginationResult
    {
        public List<Customer> Data { get; set; } = new List<Customer>();
    }

    public class ShipperSearchResult : BasePaginationResult
    {
        public List<Shipper> Data { get; set; } = new List<Shipper>();
    }

    public class CategorySearchResult : BasePaginationResult
    {
        public List<Category> Data { get; set; } = new List<Category>();
    }

    public class EmployeeSearchResult : BasePaginationResult
    {
        public List<Employee> Data { get; set; } = new List<Employee>();
    }

    public class SupplierSearchResult : BasePaginationResult
    {
        public List<Supplier> Data { get; set; } = new List<Supplier>();
    }

    public class ProductSeachResult : BasePaginationResult
    {
        public List<Product> Data { get; set; } = new List<Product>();
    }
}
