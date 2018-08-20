using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class RegisterUserViewModel
    {
        public AddUserModel UserModel { get; set; }

        public SaveOrganizationRequestModel OrganizationModel { get; set; }
    }
}
