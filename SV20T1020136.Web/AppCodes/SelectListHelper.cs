using Microsoft.AspNetCore.Mvc.Rendering;
using SV20T1020136.BusinessLayers;

namespace SV20T1020136.Web
{
    public static class SelectListHelper
    {
        public static List<SelectListItem> Provinces()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem
            {
                Value = "",
                Text = "-- Chọn tỉnh thành --",
            });
            foreach (var item in CommonDataService.ListOfProvinces())
            {
                list.Add(new SelectListItem
                {
                    Value = item.ProvinceName,
                    Text = item.ProvinceName,
                });
            }

            return list;
        }
        public static List<SelectListItem> Categories()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem
            {
                Value = "0",
                Text = "-- Chọn mặt hàng --",
            });

            int rowCount = 0;
            foreach (var item in CommonDataService.ListOfCategories(out rowCount))
            {
                list.Add(new SelectListItem
                {
                    Value = item.CategoryID.ToString(),
                    Text = item.CategoryName,
                });
            }

            return list;
        }
        public static List<SelectListItem> Suppliers()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem
            {
                Value = "0",
                Text = "-- Chọn nhà cung cấp --",
            });

            int rowCount = 0;
            foreach (var item in CommonDataService.ListOfSuppliers(out rowCount))
            {
                list.Add(new SelectListItem
                {
                    Value = item.SupplierID.ToString(),
                    Text = item.SupplierName,
                });
            }

            return list;
        }
        public static List<SelectListItem> OrderStatus()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem
            {
                Value = "0",
                Text = "-- Trạng thái --",
            });

            foreach (var item in OrderDataService.ListStatus())
            {
                list.Add(new SelectListItem
                {
                    Value = item.Status.ToString(),
                    Text = item.Description,
                });
            }

            return list;
        }
    }
}