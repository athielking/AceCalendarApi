using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using AssetCalendarApi.ViewModels;
using Microsoft.EntityFrameworkCore;
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

        public OrganizationViewModel GetOrganizationById(Guid id)
        {
            var org = _dbContext.Organizations
                .Include(o => o.CalendarUsers)
                .SingleOrDefault(organization => organization.Id == id);

            return new OrganizationViewModel() {
                Id = org.Id,
                Name = org.Name,
                Users = org.CalendarUsers.Select(u => AutoMapper.Mapper.Map<UserViewModel>(u))
            };
        }

        public Organization EditOrganization(Guid id, string name)
        {
            var organization = _dbContext.Organizations.FirstOrDefault( o => o.Id == id);
            _dbContext.Attach(organization);

            organization.Name = name;

            _dbContext.SaveChanges();

            return organization;
        }

        public void DeleteOrganization(Guid id)
        {
            var org = _dbContext.Organizations.FirstOrDefault(o => o.Id == id);

            _dbContext.Remove(org);
            _dbContext.SaveChanges();
        }

        public Organization GetOrganizationByName( string name )
        {
            return _dbContext.Organizations
                .SingleOrDefault(organization => organization.Name == name);
        }

        public IEnumerable<OrganizationViewModel> GetAllOrganizations()
        {
            return _dbContext.Organizations.Select(o => AutoMapper.Mapper.Map<OrganizationViewModel>(o));
        }
        #endregion
    }
}
