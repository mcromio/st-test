using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using st.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace st.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Index()
        {
            using (var db = new DatabaseContext())
            {
                ViewData["SalaryReport"] = null;
                if (User.Identity.IsAuthenticated)
                {

                    var logged_user = db.Employees.Where(q => q.UserName == User.Identity.Name).ToList();
                    if (logged_user.Count > 0)
                    {
                        ViewData["LoggedUser"] = logged_user[0];
                        if (TempData["SalaryReport"]!=null)
                            ViewData["SalaryReport"] = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SalaryEntry>>(TempData["SalaryReport"].ToString());

                    }
                }
            }
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
