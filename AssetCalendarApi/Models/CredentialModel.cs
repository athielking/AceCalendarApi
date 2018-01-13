using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.Models
{
    public class CredentialModel
    {
        #region Properties

        [Required]
        public string UserName
        {
            get;
            set;
        }

        [Required]
        public string Password
        {
            get;
            set;
        }

        #endregion
    }
}
