﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class LoginViewModel
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

        public bool RememberMe
        {
            get;
            set;
        }

        #endregion
    }
}
