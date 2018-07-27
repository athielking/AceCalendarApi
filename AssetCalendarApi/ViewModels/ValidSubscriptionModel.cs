using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class ValidSubscriptionModel
    {
        private List<string> _messages;

        public bool IsValid { get; set; }
        public bool AllowCalendarEdit { get; set; }

        public IEnumerable<string> Messages { get => _messages; }

        public ValidSubscriptionModel()
        {
            IsValid = true;
            AllowCalendarEdit = false;
            _messages = new List<string>();
        }
        
        public void AddMessage( string message)
        {
            if (_messages.Any(m => m.Equals(message, StringComparison.InvariantCultureIgnoreCase)))
                return;

            _messages.Add(message);
        }
    }
}
