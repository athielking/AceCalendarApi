using AssetCalendarApi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public Guid OrganizationId { get; set; }

        [JsonIgnore]
        public ICollection<JobTags> JobTags { get; set; }

        [JsonIgnore]
        public ICollection<DayJobTag> DayJobTags { get; set; }

        public Organization Organization { get; set; }

        public TagViewModel GetViewModel(bool fromJobDay = false)
        {
            return new TagViewModel() {
                Id = this.Id,
                Color = this.Color,
                Description = this.Description,
                Icon = this.Icon,
                FromJobDay = fromJobDay
            };
        }
    }
}
