using AssetCalendarApi.Data.Models;
using AssetCalendarApi.Hubs;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
using Microsoft.AspNetCore.Identity;
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

        public Task SendDataUpdatedAsync(DateTime startDate, Guid calendarId, DateTime? endDate = null, Guid? idWorker = null)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<CalendarRepository>();
                    var data = repo.GetDataForRange(startDate, calendarId, endDate, idWorker);
                    _hubContext.Clients.Groups(calendarId.ToString()).SendAsync("DataUpdated", data);
                }
            });
        }

        public async void SendDataUpdated(DateTime startDate, Guid calendarId, DateTime? endDate = null, Guid? idWorker = null)
        {
            var data = _calendarRepository.GetDataForRange(startDate, calendarId, endDate, idWorker);
            await _hubContext.Clients.Groups(calendarId.ToString()).SendAsync("DataUpdated", data);
        }

        public Task SendDataUpdatedAsync(IEnumerable<DateTime> dates, Guid calendarId, Guid? idWorker = null)
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
                            dictionary = repo.GetDataForRange(d, calendarId, null, idWorker);
                        else
                            dictionary.Add(d, repo.GetDataForRange(d, calendarId, null, idWorker)[d]);
                    }

                    _hubContext.Clients.Groups(calendarId.ToString()).SendAsync("DataUpdated", dictionary);
                }
            });
        }

        public Task SendJobUpdatedAsync(Guid idJob, Guid calendarId)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var jobRepo = scope.ServiceProvider.GetRequiredService<JobRepository>();
                    var calendarRepo = scope.ServiceProvider.GetRequiredService<CalendarRepository>();

                    var jobDays = jobRepo.GetJobDaysForJob(idJob, calendarId);
                    foreach (var jd in jobDays)
                    {
                        var data = _calendarRepository.GetDataForRange(jd.Date, calendarId);
                        _hubContext.Clients.Groups(calendarId.ToString()).SendAsync("DataUpdated", data);
                    }
                }
            });
        }

        public async void SendJobUpdated(Guid idJob, Guid calendarId)
        {
            var jobDays = _jobRepository.GetJobDaysForJob(idJob, calendarId);
            foreach( var jd in jobDays )
            {
                var data = _calendarRepository.GetDataForRange(jd.Date, calendarId);
                await _hubContext.Clients.Groups(calendarId.ToString()).SendAsync("DataUpdated", data);
            }
        }

        public Task SendUserDataUpdatedAsync( string userId)
        {
            return Task.Factory.StartNew( async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AceUser>>();
                    var calendarRepo = scope.ServiceProvider.GetRequiredService<CalendarRepository>();

                    var aceUser = await userManager.FindByIdAsync(userId);

                    var calendars = calendarRepo.GetCalendarsForUser(aceUser.Id, aceUser.OrganizationId);
                    _hubContext.Clients.Groups(aceUser.UserName).SendAsync("UserDataUpdated", calendars);
                }
            });
        }

        public Task CheckSubscriptionAsync(Guid organizationId)
        {
            return Task.Factory.StartNew( () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var orgRepo = scope.ServiceProvider.GetRequiredService<OrganizationRepository>();
                    _hubContext.Clients.Groups(organizationId.ToString()).SendAsync("SubscriptionChecked", orgRepo.GetSubscriptionValidation(organizationId));
                }
            });
        }

        public Task RefreshTokenAsync(AceUser user)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var jwtHelper = scope.ServiceProvider.GetRequiredService<JwtTokenService>();
                    _hubContext.Clients.Group(user.UserName).SendAsync("TokenUpdated", jwtHelper.GenerateJwtForUser(user.UserName));
                }
            });
        }
    }
}
