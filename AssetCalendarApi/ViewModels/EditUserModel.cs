using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class EditUserModel
    {
        #region Properties

        [Required]
        public Guid Id
        {
            get;
            set;
        }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email
        {
            get;
            set;
        }

        [Required]
        public string FirstName
        {
            get;
            set;
        }

        [Required]
        public string LastName
        {
            get;
            set;
        }

        [Required]
        public string Role
        {
            get;
            set;
        }

        #endregion
    }
}
