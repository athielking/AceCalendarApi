using AssetCalendarApi.Hubs;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
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
        private readonly JobRepository _jobRepository;
        private readonly TagRepository _tagRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SignalRService(IHubContext<CalendarHub> hubContext,
            IServiceScopeFactory serviceScopeFactory,
            CalendarRepository calendarRepository,
            JobRepository jobRepository,
            TagRepository tagRepository)
        {
            _hubContext = hubContext;
            _calendarRepository = calendarRepository;
            _serviceScopeFactory = serviceScopeFactory;
            _jobRepository = jobRepository;
            _tagRepository = tagRepository;
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

        public async void SendDataUpdated(DateTime startDate, Guid organizationId, DateTime? endDate = null, Guid? idWorker = null)
        {
            var data = _calendarRepository.GetDataForRange(startDate, organizationId, endDate, idWorker);
            await _hubContext.Clients.Groups(organizationId.ToString()).SendAsync("DataUpdated", data);
        }

        public Task SendDataUpdatedAsync(IEnumerable<DateTime> dates, Guid organizationId, Guid? idWorker = null)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<CalendarRepository>();
                    Dictionary<DateTime, DayViewModel> dictionary = null;

                    foreach (var d in dates)
                    {
                        if (dictionary == null)
                            dictionary = repo.GetDataForRange(d, organizationId, null, idWorker);
                        else
                            dictionary.Add(d, repo.GetDataForRange(d, organizationId, null, idWorker)[d]);
                    }

                    _hubContext.Clients.Groups(organizationId.ToString()).SendAsync("DataUpdated", dictionary);
                }
            });
        }

        public Task SendJobUpdatedAsync(Guid idJob, Guid organizationId)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var jobRepo = scope.ServiceProvider.GetRequiredService<JobRepository>();
                    var calendarRepo = scope.ServiceProvider.GetRequiredService<CalendarRepository>();

                    var jobDays = jobRepo.GetJobDaysForJob(idJob, organizationId);
                    foreach (var jd in jobDays)
                    {
                        var data = _calendarRepository.GetDataForRange(jd.Date, organizationId);
                        _hubContext.Clients.Groups(organizationId.ToString()).SendAsync("DataUpdated", data);
                    }
                }
            });
        }

        public async void SendJobUpdated(Guid idJob, Guid organizationId)
        {
            var jobDays = _jobRepository.GetJobDaysForJob(idJob, organizationId);
            foreach( var jd in jobDays )
            {
                var data = _calendarRepository.GetDataForRange(jd.Date, organizationId);
                await _hubContext.Clients.Groups(organizationId.ToString()).SendAsync("DataUpdated", data);
            }
        }
    }
}
