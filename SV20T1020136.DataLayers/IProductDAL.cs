using SV20T1020136.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020136.DataLayers
{
    public interface IProductDAL
    {
        IList<Product> List(int page = 1, int pageSize = 0, string searchValue = "", int categoryID = 0,
            int supplierID = 0);
        int Count(string searchValue = "", int categoryID = 0, int supplierID = 0);
        Product? Get(int productID);
        int Add(Product data);
        bool Update(Product data);
        bool Delete(int productID);
        bool InUsed(int productID);
        IList<ProductPhoto> ListPhotos(int productID);
        ProductPhoto? GetPhoto(long photoID);
        long AddPhoto(ProductPhoto data);
        bool UpdatePhoto(ProductPhoto data);
        bool DeletPhoto(long photoID);
        IList<ProductAttribute> ListAttributes(int productID);
        ProductAttribute? GetAttribute(long attributeID);
        long AddAttribute(ProductAttribute data);
        bool UpdateAttribute(ProductAttribute data);
        bool DeleteAttribute(long attributeID);
    }
}
