using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class Student
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name ="Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(50)]
        [Column("FirstName")]
        [Display(Name = "First Name")]
        public string FirstMidName { get; set; }
        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                return LastName + ", "+FirstMidName;
            }

        }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString ="{0:yyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name ="Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}