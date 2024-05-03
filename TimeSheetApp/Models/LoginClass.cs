using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace timesheetapps.Models
{
    public class LoginClass
    {
        [RegularExpression("^([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$", ErrorMessage = "Please enter a valid Email ID")]
        [Required(ErrorMessage = "Email ID is Required")]
        public String EmailAddress { get; set; }
        [MinLength(8, ErrorMessage = "Minimum Password must be 8 in charaters")]
        [Required(ErrorMessage = "Password is Required")]
        public String Password { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
    }
}
