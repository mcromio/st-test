using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using st.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace st.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<Employee> _userManager;
        private readonly SignInManager<Employee> _signInManager;
        public bool  CheckPrivileges()
        {
            using (var db = new DatabaseContext())
            {
                if (User.Identity.IsAuthenticated)
                {
                    var emps = db.Employees.Where(q => q.UserName == User.Identity.Name).ToList();
                    if (emps.Count > 0)
                        if (emps[0].isAdmin)
                            return true;
                }
            }
            return false;
        }
        public UserController(UserManager<Employee> userManager, SignInManager<Employee> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        //получить список сотрудников
        [HttpGet]
        public ActionResult EmployeeList()
        {
            if (CheckPrivileges())
            {
                using (var db = new DatabaseContext())
                {
                    var employees = db.Employees.ToList();
                    foreach (var emp in employees)
                        if (emp.SubEmployee == null)
                            emp.SubEmployee = new List<Employee>();
                    ViewData["Employees"] = employees;
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        //загрузить страничку создания сотрудника
        [HttpGet]
        public ActionResult EmployeeCreate()
        {
            if (CheckPrivileges())
            {
                using (var db = new DatabaseContext())
                {
                    var positions = db.Positions.ToList();
                    var bosses = db.Employees.Where(q => q.Position.CodeName != "Employee").ToList();
                    ViewData["Positions"] = positions;
                    ViewData["Bosses"] = bosses;
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        //загрузить страничку логина пользователя
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        //добавить сотрудника и заредиректиться на список сотрудников
        [HttpPost]
        public async Task<IActionResult> AddEmployee(Employee model)
        {

            using (var db = new DatabaseContext())
            {
                Employee user = new Employee { UserName = model.UserName };
                user.AdmissionDate = model.AdmissionDate;
                user.isAdmin = model.isAdmin;
                user.Name = model.Name;
                user.PosId = model.PosId;
                user.BossId = model.BossId;
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    db.Attach(user);
                    user.Position = db.Positions.Find(model.PosId);
                    db.SaveChanges();
                    var boss = db.Employees.Find(model.BossId);
                    if (boss != null)
                    {
                        db.Attach(boss);
                        if (boss.SubEmployee == null)
                            boss.SubEmployee = new List<Employee>();
                        boss.SubEmployee.Add(user);
                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("EmployeeList");
        }
        //залогиниться
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(Employee employee)
        {
            var result = await _signInManager.PasswordSignInAsync(employee.UserName, employee.Password, false, false);
            if (!result.Succeeded)
            {
                TempData["LoginError"] = "Ошибка при авторизации";
                return View();
            }
            else
                return RedirectToAction("Index", "Home");
        }
        //разлогиниться
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        //удалить пользователя
        [HttpGet]
        public ActionResult EmployeeDelete(string id)
        {
            if (CheckPrivileges())
            {
                using (var db = new DatabaseContext())
                {
                    var employee = db.Employees.Find(id);
                    if (employee != null)
                    {
                        db.Employees.Remove(employee);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("EmployeeList");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        //запросить список должностей
        [HttpGet]
        public ActionResult PositionList()
        {
            if (CheckPrivileges())
            {
                using (var db = new DatabaseContext())
                {
                    var positions = db.Positions.ToList();
                    ViewData["Positions"] = positions;
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        public ActionResult SubList()
        {
            using (var db = new DatabaseContext())
            {
                if (User.Identity.IsAuthenticated)
                {
                    var ff = db.Employees.ToList();
                    var emps = db.Employees.Where(q => q.UserName == User.Identity.Name).ToList();
                    if (emps.Count > 0)
                    {
                        var subs = ReturnEmployees(emps[0]);
                        ViewData["Subs"] = subs;
                    }
                    return View();
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
        }
        //удалить должность
        [HttpGet]
        public ActionResult PositionDelete(string id)
        {
            if (CheckPrivileges())
            {
                using (var db = new DatabaseContext())
                {
                    int num_id = 0;
                    if (Int32.TryParse(id, out num_id))
                    {
                        var position = db.Positions.Find(num_id);
                        if (position != null)
                        {
                            db.Positions.Remove(position);
                            db.SaveChanges();
                        }
                    }
                }
                return RedirectToAction("PositionList");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        //добавить должность
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PositionList(Position position)
        {
            using (var db = new DatabaseContext())
            {
                position.MaxYearlyBonus = position.MaxYearlyBonus / 100;
                position.ReferalBonus = position.ReferalBonus / 100;
                position.YearlyBonus = position.YearlyBonus / 100;
                db.Positions.Add(position);
                db.SaveChanges();
                ViewData["Positions"] = db.Positions.ToList();
                return View(position);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CalculateSalary(SalaryQuery query)
        {
            using (var db = new DatabaseContext())
            {
                List<SalaryEntry> result = new List<SalaryEntry>();
                var ff = db.Employees.ToList();
                var emps = db.Employees.Where(q => q.UserName == User.Identity.Name).ToList();
                if (emps.Count > 0)
                {
                    TempData["SalaryError"] = null;
                    TempData["SalaryReport"] = null;
                    if (query.DtStart >= query.DtFinish)
                        TempData["SalaryError"] = "Дата начала периода должна быть меньше даты конца периода.";
                    else if (emps[0].AdmissionDate>query.DtFinish)
                        TempData["SalaryError"] = "Работник был принят на работу позднее указанного периода.";
                    else
                    {
                        DateTime Dt;
                        if (query.DtStart < emps[0].AdmissionDate)
                            Dt = emps[0].AdmissionDate;
                        else
                            Dt = query.DtStart;
                        while (Dt < query.DtFinish)
                        {
                            var entry = ReturnSalary(Dt, emps[0]);
                            result.Add(entry);
                            Dt = Dt.AddMonths(1);
                        }
                        if (result.Count>0)
                            TempData["SalaryReport"] = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                        else
                            TempData["SalaryError"] = "За выбранный период отсутствует информация о заработной плате работника.";
                    }                   
                }               
            }
            return RedirectToAction("Index", "Home");
        }

        public SalaryEntry ReturnSalary (DateTime dt,Employee emp)
        {
            var result = new SalaryEntry();
            using (var db = new DatabaseContext())
            {                
                emp.Position = db.Positions.Find(emp.PosId);
                result.Month = dt.ToString("MMMM yyyy");
                result.BaseRate = emp.Position.BaseRate;
                var bonus = 0;
                var admDt = emp.AdmissionDate;
                while (admDt.AddYears(1).AddDays(-1) < dt)
                {
                    admDt = admDt.AddYears(1);
                    bonus++;
                }
                result.PeriodBonus = result.BaseRate * emp.Position.YearlyBonus * bonus;
                if (result.PeriodBonus > emp.Position.MaxYearlyBonus * result.BaseRate)
                    result.PeriodBonus = emp.Position.MaxYearlyBonus * result.BaseRate;
                result.ReferalBonus = ReturnReferalBonus(dt, emp);
                result.Total = result.BaseRate + result.PeriodBonus + result.ReferalBonus;
            }
            return result;
        }
        public decimal ReturnReferalBonus(DateTime dt, Employee emp)
        {
            decimal refbonus = 0;
            using (var db = new DatabaseContext())
            {
                emp.Position = db.Positions.Find(emp.PosId);
                List<Employee> subs = new List<Employee>();
                List<Employee> init = new List<Employee>();
                if (emp.Position.isFirstLevelBonus)
                    subs = emp.SubEmployee.Where(q => q.AdmissionDate <= dt).ToList();
                else
                {
                    subs.AddRange(ReturnEmployees(emp));
                }
                foreach (var sub in subs)
                {
                    sub.Position = db.Positions.Find(sub.PosId);
                    refbonus += sub.Position.BaseRate * emp.Position.ReferalBonus;
                }
            }
            return refbonus;            
        }
        public List<Employee> ReturnEmployees(Employee emp)
        {
            List<Employee> result = new List<Employee>();
            var lst = new List<Employee>();
            if (emp.SubEmployee!=null)
                lst=emp.SubEmployee;
            result.AddRange(lst);
            foreach (var ent in lst)
                result.AddRange(ReturnEmployees(ent));
            return result;
        }
        [HttpGet]
        public ActionResult EmployeeSalary(string id)
        {
            using (var db=new DatabaseContext())
            {
                ViewData["SelectedUser"] = db.Employees.Find(id);
            }
            ViewData["SalaryReport"] = null;
            if (TempData["SalaryReport"] != null)
               ViewData["SalaryReport"] = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SalaryEntry>>(TempData["SalaryReport"].ToString());

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmployeeSalary(SalaryQuery query)
        {
            using (var db = new DatabaseContext())
            {
                List<SalaryEntry> result = new List<SalaryEntry>();
                var ff = db.Employees.ToList();
                var emp = db.Employees.Find(query.UserGuid);
                if (emp!= null)
                {
                    ViewData["SelectedUser"] = emp;
                    //ViewData["SalaryError"] = null;
                    //ViewData["SalaryReport"] = null;
                    if (query.DtStart >= query.DtFinish)
                        TempData["SalaryError"] = "Дата начала периода должна быть меньше даты конца периода.";
                    else if (emp.AdmissionDate > query.DtFinish)
                        TempData["SalaryError"] = "Работник был принят на работу позднее указанного периода.";
                    else
                    {
                        DateTime Dt;
                        if (query.DtStart < emp.AdmissionDate)
                            Dt = emp.AdmissionDate;
                        else
                            Dt = query.DtStart;
                        while (Dt < query.DtFinish)
                        {
                            var entry = ReturnSalary(Dt, emp);
                            result.Add(entry);
                            Dt = Dt.AddMonths(1);
                        }
                        if (result.Count > 0)
                            TempData["SalaryReport"] = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                        else
                            TempData["SalaryError"] = "За выбранный период отсутствует информация о заработной плате работника.";
                    }
                }
            }
            return RedirectToAction("EmployeeSalary", "User");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
