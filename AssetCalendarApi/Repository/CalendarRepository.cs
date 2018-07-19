using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using AssetCalendarApi.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Repository
{
    public class CalendarRepository
    {
        #region Data Members

        private readonly AssetCalendarDbContext _dbContext;
        private readonly WorkerRepository _workerRepository;
        private readonly JobRepository _jobRepository;
        private readonly TagRepository _tagRepository;

        #endregion

        #region Constructor

        public CalendarRepository(AssetCalendarDbContext dbContext, WorkerRepository workerRepository, JobRepository jobRepository, TagRepository tagRepository)
        {
            _dbContext = dbContext;
            _workerRepository = workerRepository;
            _jobRepository = jobRepository;
            _tagRepository = tagRepository;
        }

        #endregion
        
        #region Public Methods

        public Dictionary<DateTime, DayViewModel> GetDataForRange(DateTime date, Guid calendarId, DateTime? endDate = null, Guid? idWorker = null)
        {
            var availableByDate = _workerRepository.GetAvailableWorkersForDates(calendarId, date, endDate);
            var offByDate = _workerRepository.GetOffWorkersForDates(calendarId, date, endDate);

            var jobsByDate = _jobRepository.GetJobsForRange(calendarId, date, endDate, idWorker);
            var tagsByWorker = _tagRepository.GetTagsByWorker(calendarId);

            var end = endDate.HasValue ? endDate.Value : date;

            Dictionary<DateTime, DayViewModel> result = new Dictionary<DateTime, DayViewModel>();
            for (DateTime d = date.Date; d <= end.Date; d = d.AddDays(1))
            {
                DayViewModel vm = new DayViewModel()
                {
                    Date = d,
                    AvailableWorkers = availableByDate.ContainsKey(d) ? availableByDate[d] : Enumerable.Empty<Worker>(),
                    TimeOffWorkers = offByDate.ContainsKey(d) ? offByDate[d] : Enumerable.Empty<Worker>(),
                    Jobs = jobsByDate.ContainsKey(d) ? jobsByDate[d].OrderBy(j => j.Name ) : Enumerable.Empty<Job>()
                };
                vm.WorkersByJob = _workerRepository.GetWorkersByJob(d, calendarId);
                vm.TagsByJob = _tagRepository.GetTagsByJob(d, calendarId);
                vm.TagsByWorker = tagsByWorker;

                result.Add(d, vm);
            }

            return result;
        }

        public Task<Dictionary<DateTime, DayViewModel>> GetDataForRangeAsync(DateTime date, Guid calendarId, DateTime? endDate, Guid? idWorker)
        {
            return Task<Dictionary<DateTime, DayViewModel>>.Run(() =>
            {
                return GetDataForRange(date, calendarId, endDate, idWorker);
            });
        }

        public IEnumerable<CalendarViewModel> GetCalendarsForOrganization(Guid organizationId)
        {
            return _dbContext.Calendars.Where(c => c.OrganizationId == organizationId)
                .Select(c => AutoMapper.Mapper.Map<CalendarViewModel>(c));
        }

        public IEnumerable<CalendarViewModel> GetCalendarsAndUsersForOrganization(Guid organizationId)
        {
            return _dbContext.Calendars.Include(c => c.CalendarUsers).Where(c => c.OrganizationId == organizationId)
                .Select(c => AutoMapper.Mapper.Map<CalendarViewModel>(c));
        }

        public IEnumerable<CalendarViewModel> GetCalendarsForUser(string userId, Guid organizationId)
        {
            return _dbContext.CalendarUsers
                .Include(u => u.Calendar)
                .Where(u => u.UserId == userId)
                .Select(u => AutoMapper.Mapper.Map<CalendarViewModel>(u.Calendar))
                .Where(c => c.OrganizationId == organizationId);
        }

        public CalendarViewModel GetCalendar(Guid id)
        {
            return AutoMapper.Mapper.Map<CalendarViewModel>(_dbContext.Calendars.FirstOrDefault(c => c.Id == id));
        }

        public IEnumerable<CalendarViewModel> DeleteCalendar(Guid id)
        {
            var cal = _dbContext.Calendars.FirstOrDefault(c => c.Id == id);
            var orgId = cal.OrganizationId;

            _dbContext.Remove(cal);
            _dbContext.SaveChanges();

            return GetCalendarsForOrganization(orgId);
        }

        public IEnumerable<CalendarUser> GetCalendarUsers(Guid id)
        {
            return _dbContext.Calendars.Include(c => c.CalendarUsers).Where(c => c.Id == id).SelectMany(c => c.CalendarUsers);
        }

        public IEnumerable<CalendarUser> AssignCalendarUsers(Guid id, List<string> users)
        {
            foreach (var uId in users)
            {
                if (_dbContext.CalendarUsers.Any(c => c.CalendarId == id && c.UserId == uId))
                    continue;

                _dbContext.CalendarUsers.Add(new CalendarUser()
                {
                    Id = Guid.NewGuid(),
                    UserId = uId,
                    CalendarId = id
                });
            }

            _dbContext.SaveChanges();

            return GetCalendarUsers(id);
        }

        public IEnumerable<CalendarUser> DeleteUserFromCalendar(Guid id, string userId)
        {
            var calUser = _dbContext.CalendarUsers.FirstOrDefault(c => c.CalendarId == id && c.UserId == userId);
            _dbContext.CalendarUsers.Remove(calUser);
            _dbContext.SaveChanges();

            return GetCalendarUsers(id);
        }

        public CalendarViewModel AddCalendarToOrganization(AddCalendarModel model)
        {
            //TODO:
            //Subscription Check here? Do they have enough calendars left to add one?

            var existing = _dbContext.Calendars.Where(c => c.OrganizationId == new Guid(model.OrganizationId));
            if (existing.Any(c => c.CalendarName == model.CalendarName))
                throw new InvalidOperationException($"Cannot add duplicate calendar {model.CalendarName} to organiztion {model.OrganizationId}");

            var calendar = new Calendar()
            {
                Id = Guid.NewGuid(),
                CalendarName = model.CalendarName,
                OrganizationId = new Guid(model.OrganizationId)
            };

            _dbContext.Calendars.Add(calendar);

            //foreach( var userId in model.UserIds )
            //{
            //    var cu = new CalendarUser()
            //    {
            //        Id = Guid.NewGuid(),
            //        CalendarId = calendar.Id,
            //        UserId = userId
            //    };

            //    _dbContext.CalendarUsers.Add(cu);
            //}

            _dbContext.SaveChanges();

            return AutoMapper.Mapper.Map<CalendarViewModel>(calendar);
        }

        public void EditCalendar(Guid id, EditCalendarModel editCalendarModel)
        {
            var calendar = _dbContext.Calendars.FirstOrDefault(c => c.Id == id);

            if (calendar == null)
                throw new ApplicationException("Calendar not Found");

            calendar.CalendarName = editCalendarModel.CalendarName;

            _dbContext.Calendars.Update(calendar);
            _dbContext.SaveChanges();
        } 

        #endregion
    }
}
