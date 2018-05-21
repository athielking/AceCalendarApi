using AssetCalendarApi.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class WorkerViewModel
    {
        #region Properties

        [Required]
        public Guid Id
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

        [JsonIgnore]
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }

        [Required]
        public string LastName
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

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}", ErrorMessage = "Invalid Phone Number")]
        public string Phone
        {
            get;
            set;
        }

        public List<TagViewModel> Tags
        {
            get;
            set;
        }

        #endregion
    }
}
