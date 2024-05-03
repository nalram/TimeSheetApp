using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace timesheetapps.Models
{
    public class AccountClass
    {
        [Key]
        public int AccountID { get; set; }
        [Required(ErrorMessage = "Account Name is Required")]
        public String AccountName { get; set; }
        public Boolean IsActive { get; set; } = true;
        public int CreateUserID { get; set; }
        public DateTime createDate { get; set; }
        public int? ModifyUserID { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
