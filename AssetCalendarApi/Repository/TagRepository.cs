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
            return _dbContext.Tags.Select(t => t.GetViewModel());
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
                MatIcon = tag.Icon,
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

            dbTag.MatIcon = tag.Icon;
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

        public Dictionary<Guid, IEnumerable<Tag>> GetTagsByJob(DateTime date, Guid organizationId)
        {
            return _dbContext.TagsByJobDate.Where(t => t.Date.Date == date.Date && t.OrganizationId == organizationId)
                .GroupBy(t => t.IdJob)
                .ToDictionary(group => group.Key, group => group.Select(t => AutoMapper.Mapper.Map<Tag>(t)));
        }
    }
}
