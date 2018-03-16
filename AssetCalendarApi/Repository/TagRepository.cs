using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public IEnumerable<Tag> GetAllTags()
        {
            return _dbContext.Tags.AsEnumerable();
        }

        public Dictionary<Guid, IEnumerable<Tag>> GetTagsByJob( DateTime date, Guid organizationId)
        {
            return _dbContext.TagsByJobDate.Where(t => t.Date.Date == date.Date && t.OrganizationId == organizationId)
                .GroupBy(t => t.IdJob)
                .ToDictionary(group => group.Key, group => group.Select(t => AutoMapper.Mapper.Map<Tag>(t)));
        }
    }
}
