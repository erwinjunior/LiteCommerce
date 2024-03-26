using Microsoft.AspNetCore.Mvc;
using SV20T1020136.Web.Models;

namespace SV20T1020136.Web.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Create()
        {
            var model = new Person
            {
                Name = "Bảo",
                Birthday = new DateTime(1999, 10, 28),
                Salary = 500.25m
            };
            return View(model);
        }

        public IActionResult Save(Person model) 
        {
            return Json(model);
        }
    }
}
