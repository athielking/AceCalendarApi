using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public Guid OrganizationId { get; set; }

        public string Role { get; set; }
        public List<string> Claims { get; set; }

        public UserViewModel()
        {
            Claims = new List<string>();
        }

        public bool IsEditor()
        {
            return UserViewModel.RoleIsEditor(this.Role);
        }

        public static bool RoleIsEditor(string role )
        {
            return role == "Admin" || role == "Organization Admin" || role == "User";
        }
    }
}
