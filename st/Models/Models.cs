using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace st.Models
{
    public class Position
    {
        public int Id { get; set; }
        [Display(Name = "Наименование должности")]
        [Required(ErrorMessage = "Наименование должности является обязательным полем")]
        public string CodeName { get; set; }
        [Display(Name = "Базовая ставка")]
        [Required(ErrorMessage = "Базовая ставка является обязательным полем")]
        public decimal BaseRate { get; set; }
        [Display(Name = "Бонус за стаж")]
        [Required(ErrorMessage = "Бонус за стаж является обязательным полем")]
        public decimal YearlyBonus { get; set; }
        [Display(Name = "Максимальный бонус за стаж")]
        [Required(ErrorMessage = "Максимальный бонус за стаж является обязательным полем")]
        public decimal MaxYearlyBonus { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        [Display(Name = "Бонус за подчиненного")]
        [Required(ErrorMessage = "Бонус за подчиненного является обязательным полем")]
        [RegularExpression(@"^[0-9]*[,]?[0-9]+$", ErrorMessage = "Требуется ввести актуальное дробное число")]
        public decimal ReferalBonus { get; set; }
        [Display(Name = "Имеет подчиненных?")]
        public bool haveSubordinates { get; set; }
        [Display(Name = "Бонус только за подчиненных первого уровня?")]
        public bool isFirstLevelBonus { get; set; }
    }
    public class Employee:IdentityUser
    {
        [Display(Name = "Имя сотрудника")]
        public string Name { get; set; }
        [Display(Name = "Дата приема на работу")]
        public DateTime AdmissionDate { get; set; }
        public virtual List<Employee> SubEmployee { get; set; }
        public string BossId { get; set; }
        [Display(Name = "Должность")]
        public virtual Position Position { get; set; }

        
        public int PosId { get; set; }

        [Display(Name = "Пароль")]
        public string Password { get; set; }
        [Display(Name = "Администратор")]
        public bool isAdmin { get; set; }
    }
    public class SalaryQuery
    {
        public DateTime DtStart { get; set; }
        public DateTime DtFinish { get; set; }
    }
    public class SalaryEntry
    {
        public string Month { get; set; }
        public decimal BaseRate { get; set; }
        public decimal PeriodBonus { get; set; }
        public decimal ReferalBonus { get; set; }
        public decimal Total { get; set; }
    }
}
