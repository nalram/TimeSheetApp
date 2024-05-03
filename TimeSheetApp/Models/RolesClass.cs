using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace timesheetapps.Models
{
    public class RolesClass
    {
        [Key]
        public int RoleID { get; set; }
        [Required]
        public String RoleName { get; set; }
    }
}
