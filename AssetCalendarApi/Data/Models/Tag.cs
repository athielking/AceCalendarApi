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
        public string MatIcon { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }

        [JsonIgnore]
        public ICollection<JobTags> JobTags { get; set; }
    }
}
