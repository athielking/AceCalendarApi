using AssetCalendarApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class OrganizationViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<UserViewModel> Users { get; set; }
    }
}
