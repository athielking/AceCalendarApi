using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.Repository
{
    public class OrganizationRepository
    {
        #region Data Members

        private readonly AssetCalendarDbContext _dbContext;

        #endregion

        #region  Constructor

        public OrganizationRepository(AssetCalendarDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        #region Public Methods

        public Organization AddOrganization(string name)
        {
            var organization = new Organization()
            {
                Id = Guid.NewGuid(),
                Name = name
            };

            _dbContext.Organizations.Add(organization);
            _dbContext.SaveChanges();

            return organization;
        }

        public Organization GetOrganizationById(Guid id)
        {
            return _dbContext.Organizations
                .SingleOrDefault(organization => organization.Id == id);
        }

        public Organization GetOrganizationByName( string name )
        {
            return _dbContext.Organizations
                .SingleOrDefault(organization => organization.Name == name);
        }

        #endregion
    }
}
