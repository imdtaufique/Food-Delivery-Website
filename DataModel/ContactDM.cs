using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DataModel
{
    public class ContactDM
    { 
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        [MaxLength(30)]
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public int GenderValue { get; set; }
        public string GenderText { get; set; }
        public int ContactTypeValue { get; set; }
        public string ContactTypeText { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Street { get; set; }

        public string City { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        [Required(ErrorMessage ="Password is required")]
        [MaxLength(15)]
        public string Password { get; set; }
        public bool ForgetPassword { get; set; }
    }
}
