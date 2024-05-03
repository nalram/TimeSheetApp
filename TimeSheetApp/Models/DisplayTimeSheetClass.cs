using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace timesheetapps.Models
{
    public class DisplayTimeSheetClass
    {
        [Key]
        public int RecordID { get; set; }

        [Required(ErrorMessage = "User Name is Required")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Account is Required")]
        public int AccountID { get; set; }

        [Required(ErrorMessage = "Start Date is Required")]
        [DataType(DataType.Date, ErrorMessage = "Not a valid date")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime StartDate { set; get; }

        [Required(ErrorMessage = "Hour is Required")]
        [RegularExpression(@"^\d+\.\d{2,2}$", ErrorMessage = "Enter only decimal number [0.00]")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(0.00, 999, ErrorMessage = "Not a valid Hour")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Hours { get; set; }

        [RegularExpression(@"^\d+\.\d{2,2}$", ErrorMessage = "Enter only decimal number [0.00]")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(0.00, 99999, ErrorMessage = "Not a valid Rate")]
        [Column(TypeName = "decimal(18,4)")]        
        public decimal? Rate { get; set; }

        public Boolean Chargeable { get; set; } = true;

        public int RecordStatus { get; set; } = 1;

        [DataType(DataType.MultilineText)]
        [StringLength(250, ErrorMessage = "Description length can't be more than 250.")]
        [Required(ErrorMessage = "Description is Required")]
        public string Description { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(250, ErrorMessage = "Non-Chargeable Reason length can't be more than 250.")]
        public string NonChargeReason { get; set; }

        public string ID { get; set; }
        public string Job { get; set; }

        public int CreateUserID { get; set; }
        public DateTime createDate { get; set; }
        public int? ModifyUserID { get; set; }
        public DateTime? ModifyDate { get; set; }
        public String LastName { get; set; }
        public String FirstName { get; set; }
        public String FullName { get; set; }
        public String AccountName { get; set; }
    }
}
