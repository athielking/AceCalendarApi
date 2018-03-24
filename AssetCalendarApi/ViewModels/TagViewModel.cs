using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class TagViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Icon { get; set; }

        public string Description { get; set; }

        public string Color { get; set; }

        public bool FromJobDay { get; set; }
    }
}
