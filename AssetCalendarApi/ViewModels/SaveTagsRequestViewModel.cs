using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class SaveTagsRequestViewModel
    {
       public Guid IdJob { get; set; }
       public IEnumerable<TagViewModel> Tags { get; set; }
       public DateTime? Date { get; set; }
    }
}
