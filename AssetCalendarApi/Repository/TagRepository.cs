using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetCalendarApi.ViewModels;

namespace AssetCalendarApi.Repository
{
    public class TagRepository
    {
        #region Private Methods

        private readonly AssetCalendarDbContext _dbContext;

        #endregion

        #region Constructor

        public TagRepository
        (
            AssetCalendarDbContext dbContext
        )
        {
            _dbContext = dbContext;
        }

        #endregion

        public IEnumerable<TagViewModel> GetAllTags(Guid organizationId)
        {
            return _dbContext.Tags.Select(t => t.GetViewModel(false));
        }

        public Tag GetTag( Guid id, Guid organizationId)
        {
            return _dbContext.Tags.FirstOrDefault(t => t.OrganizationId == organizationId && t.Id == id);
        }

        public Tag AddTag(TagViewModel tag, Guid organizationId)
        {
            var dbTag = new Tag()
            {
                Id = Guid.NewGuid(),
                Color = tag.Color,
                Description = tag.Description,
                Icon = tag.Icon,
                OrganizationId = organizationId
            };

            _dbContext.Tags.Add(dbTag);
            _dbContext.SaveChanges();

            return dbTag;
        }

        public void EditTag(Guid id, TagViewModel tag, Guid organizationId)
        {
            var dbTag = _dbContext.Tags.FirstOrDefault(t => t.OrganizationId == organizationId && t.Id == id);
            if (dbTag == null)
                throw new ApplicationException("Tag not Found");

            dbTag.Icon = tag.Icon;
            dbTag.Color = tag.Color;
            dbTag.Description = tag.Description;

            _dbContext.Tags.Update(dbTag);
            _dbContext.SaveChanges();
        }

        public void DeleteTag(Guid id, Guid organizationId)
        {
            var dbTag = _dbContext.Tags.FirstOrDefault(t => t.OrganizationId == organizationId && t.Id == id);
            if (dbTag == null)
                throw new ApplicationException("Tag not Found");

            _dbContext.Tags.Remove(dbTag);
            _dbContext.SaveChanges();
        }

        public void DeleteTagsFromJob( Guid jobId )
        {
            var dbTags = _dbContext.JobTags.Where(j => j.IdJob == jobId);
            foreach (var jobTag in dbTags)
                _dbContext.JobTags.Remove(jobTag);

            _dbContext.SaveChanges();
        }

        public void DeleteTagsFromJobDay( Guid jobId, DateTime date )
        {
            var dayJob = _dbContext.DaysJobs.FirstOrDefault(dj => dj.IdJob == jobId && dj.Date.Date == date.Date);
            var dbTags = _dbContext.DaysJobsTags.Where(dt => dt.IdDayJob == dayJob.Id);

            foreach (var dayJobTag in dbTags)
                _dbContext.DaysJobsTags.Remove(dayJobTag);

            _dbContext.SaveChanges();
        }

        public void UpdateTagsForJob(Guid jobId, IEnumerable<TagViewModel> tags)
        {
            var jobTags = _dbContext.JobTags.Where(t => t.IdJob == jobId);
            var jobDays = _dbContext.DaysJobs.Where(d => d.IdJob == jobId);
            var jobDaysTags = _dbContext.DaysJobsTags.Where(dt => jobDays.Any(j => j.Id == dt.IdDayJob));

            foreach (var deleted in jobTags)
            {
                if (tags.Any(t => t.Id == deleted.IdTag))
                    continue;

                _dbContext.JobTags.Remove(deleted);
            }

            foreach ( var tag in tags)
            {
                //Job tag overrules Day Job Tag
                var dayTags = jobDaysTags.Where(dt => dt.IdTag == tag.Id);
                foreach (var dTag in dayTags)
                    _dbContext.DaysJobsTags.Remove(dTag);

                if (jobTags.Any(t => t.IdTag == tag.Id))
                    continue;

                AddTagToJob(tag.Id, jobId, false);
            }

            _dbContext.SaveChanges();
        }

        public Dictionary<Guid, IEnumerable<TagViewModel>> GetTagsByJob(DateTime date, Guid organizationId)
        {
            return _dbContext.TagsByJobDate.Where(t => t.Date.Date == date.Date && t.OrganizationId == organizationId)
                .GroupBy(t => t.IdJob)
                .ToDictionary(group => group.Key, group => group.Select(t => AutoMapper.Mapper.Map<TagViewModel>(t)));
        }

        public void AddTagToJob(Guid idTag, Guid idJob, bool saveChanges = true)
        {

            if (!_dbContext.Tags.Any(t => t.Id == idTag))
                throw new ApplicationException("Unable to add Tag. Tag not found");

            if (_dbContext.JobTags.Any(j => j.IdTag == idTag && j.IdJob == idJob))
                return;

            var dbJobTag = new JobTags() {
                Id = Guid.NewGuid(),
                IdJob = idJob,
                IdTag = idTag
            };

            _dbContext.JobTags.Add(dbJobTag);

            if (saveChanges)
                _dbContext.SaveChanges();
        }

        public void AddTagToJobDay(Guid idTag, Guid idJob, DateTime date, bool saveChanges = true)
        {
            if (!_dbContext.Tags.Any(t => t.Id == idTag))
                throw new ApplicationException("Unable to add Tag. Tag not found");

            var jobDay = _dbContext.DaysJobs.FirstOrDefault(dj => dj.Date.Date == date.Date && dj.IdJob == idJob);

            if (_dbContext.DaysJobsTags.Any( d => d.IdDayJob == jobDay.Id && d.IdTag == idTag ))
                return;

            var dbDayJobTag = new DayJobTag()
            {
                Id = Guid.NewGuid(),
                IdDayJob = jobDay.Id,
                IdTag = idTag
            };

            _dbContext.DaysJobsTags.Add(dbDayJobTag);

            if(saveChanges)
                _dbContext.SaveChanges();
        }
    }
}
