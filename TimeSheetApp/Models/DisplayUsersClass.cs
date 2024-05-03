using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace timesheetapps.Models
{
    public class DisplayUsersClass
    {
        [Key]
        public int UserID { get; set; }
        [Required(ErrorMessage = "User Last Name is Required")]
        public String LastName { get; set; }
        [Required(ErrorMessage = "User First Name is Required")]
        public String FirstName { get; set; }
        [MinLength(8, ErrorMessage = "Minimum Password must be 8 in charaters")]
        [Required(ErrorMessage = "Password is Required")]
        public String Password { get; set; }

        [RegularExpression("^([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$", ErrorMessage = "Please enter a valid Email ID")]
        [Required(ErrorMessage = "Email ID is Required")]
        public String EmailAddress { get; set; }
        public Boolean IsActive { get; set; } = true;
        [Required]
        public int RoleID { get; set; }
        public int CreateUserID { get; set; }
        public DateTime createDate { get; set; }
        public int? ModifyUserID { get; set; }
        public DateTime? ModifyDate { get; set; }

        public string RoleName { get; set; }
    }
}
