using AssetCalendarApi.Hubs;
using AssetCalendarApi.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Tools
{
    public class SignalRService
    {
        private readonly IHubContext<CalendarHub> _hubContext;
        private readonly CalendarRepository _calendarRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SignalRService(IHubContext<CalendarHub> hubContext,
            IServiceScopeFactory serviceScopeFactory,
            CalendarRepository calendarRepository)
        {
            _hubContext = hubContext;
            _calendarRepository = calendarRepository;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task SendDataUpdatedAsync(DateTime startDate, Guid organizationId, DateTime? endDate = null, Guid? idWorker = null)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<CalendarRepository>();
                    var data = repo.GetDataForRange(startDate, organizationId, endDate, idWorker);
                    _hubContext.Clients.Groups(organizationId.ToString()).SendAsync("DataUpdated", data);
                }
            });
        }

        public void SendDataUpdated(DateTime startDate, Guid organizationId, DateTime? endDate = null, Guid? idWorker = null)
        {
            var data = _calendarRepository.GetDataForRange(startDate, organizationId, endDate, idWorker);
            _hubContext.Clients.Groups(organizationId.ToString()).SendAsync("DataUpdated", data);
        }

        public void SendJobUpdated(Guid idJob)
        {

        }
    }
}
