using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CarsTradeAPI.Entities
{
    /// <summary>
    /// Класс описывающий сущность работника
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Свойство указывает на ID работника
        /// </summary>
        [Key]
        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Свойство указывает на имя работника 
        /// </summary>
        [Column("employee_name")]
        public string EmployeeName { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на фамилию сотрудника
        /// </summary>
        [Column("employee_surname")]
        public string EmployeeSurname { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на отчество сотрудника
        /// </summary>
        [Column("employee_middlename")]
        public string EmployeeMiddlename { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на возраст сотрудника
        /// </summary>
        [Column("employee_age")]
        public uint EmployeeAge { get; set; }

        /// <summary>
        /// Свойство указывает на количество продаж сотрудника
        /// </summary>
        [Column("employee_sellcounter")]
        public uint? EmployeeSellCounter { get; set; }

        /// <summary>
        /// Свойство указывает на логин сотрудника
        /// </summary>
        [Column("employee_login")]
        public string EmployeeLogin { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на пароль сотрудника
        /// </summary>
        [Column("employee_password")]
        [MaxLength(260)]
        public  string EmployeePassword { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на роль сотрудника, User или Admin 
        /// </summary>
        [Column("employee_role")]
        public string EmployeeRole { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на коллекцию заказов, которые выполнил сотрудник
        /// </summary>
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}