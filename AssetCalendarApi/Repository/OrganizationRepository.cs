using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using AssetCalendarApi.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
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
        private readonly IConfiguration _configuration;
        private readonly StripeRepository _stripeRepository;

        #endregion

        #region  Constructor

        public OrganizationRepository(IConfiguration configuration, AssetCalendarDbContext dbContext, StripeRepository stripeRepository)
        {
            _dbContext = dbContext;
            _configuration = configuration;

            _stripeRepository = stripeRepository;
        }

        #endregion

        #region Public Methods

        public Organization AddOrganization(SaveOrganizationRequestModel model)
        {
            var organization = new Organization()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Email = model.Email,
            };

            _dbContext.Organizations.Add(organization);

            var customer = _stripeRepository.CreateCustomer(model);

            organization.Stripe_CustomerId = customer.Id;

            _dbContext.SaveChanges();

            //if (!String.IsNullOrEmpty(model.Stripe_PlanId) && model.PaymentToken != null)
            //{
            //    _stripeRepository.CreateSubscription(organization.Stripe_CustomerId, model.Stripe_PlanId, model.NumberOfUsers, model.PaymentToken.Id);
            //}

            return organization;
        }

        public DefaultPaymentSourceViewModel GetDefaultPaymentSource(Guid id)
        {
            var organization = _dbContext.Organizations.SingleOrDefault(org => org.Id == id);

            if (organization == null)
                throw new ApplicationException($"Unable to locate Organization with Id:'{id}'");

            if (String.IsNullOrEmpty(organization.Stripe_CustomerId))
                return null;

            var customer = _stripeRepository.GetCustomer(organization.Stripe_CustomerId);

            var defaultSource = customer.Sources.SingleOrDefault(source => source.Id == customer.DefaultSourceId);

            if (defaultSource == null)
                return null;

            if (defaultSource.Card == null)
                throw new ApplicationException("Credit card not used as default source");

            return new DefaultPaymentSourceViewModel()
            {
                Brand = defaultSource.Card.Brand,
                Last4 = defaultSource.Card.Last4,
                ExpirationMonth = defaultSource.Card.ExpirationMonth,
                ExpirationYear = defaultSource.Card.ExpirationYear
            };
        }

        public OrganizationDetailsViewModel GetOrganizationDetails(Guid id)
        {
            var organization = _dbContext.Organizations.SingleOrDefault(org => org.Id == id);

            if (organization == null)
                throw new ApplicationException($"Unable to locate Organization with Id:'{id}'");

            var organizationDetailsViewModel = new OrganizationDetailsViewModel()
            {
                OrganizationName = organization.Name,
                Email = organization.Email
            };

            if (String.IsNullOrEmpty(organization.Stripe_CustomerId))
                return organizationDetailsViewModel;

            var billingInformation = _stripeRepository.GetCustomerBillingInformation(organization.Stripe_CustomerId);

            if (billingInformation == null)
                return organizationDetailsViewModel;

            organizationDetailsViewModel.AddressLine1 = billingInformation.AddressLine1;
            organizationDetailsViewModel.AddressLine2 = billingInformation.AddressLine2;
            organizationDetailsViewModel.City = billingInformation.City;
            organizationDetailsViewModel.State = billingInformation.State;
            organizationDetailsViewModel.Zip = billingInformation.Zip;
            organizationDetailsViewModel.CityStateZip = $"{billingInformation.City}, {billingInformation.State}, {billingInformation.Zip}";

            return organizationDetailsViewModel;
        }

        public SubscriptionViewDetailsModel GetSubscriptionDetails(Guid organizationId)
        {
            var organization = _dbContext.Organizations.SingleOrDefault(org => org.Id == organizationId);

            if (organization == null)
                throw new ApplicationException($"Unable to locate Organization with Id:'{organizationId}'");

            if (String.IsNullOrEmpty(organization.Stripe_CustomerId))
                return null;

            return _stripeRepository.GetSubscription(organization.Stripe_CustomerId);
        }

        public Organization GetOrganizationWithUsers(Guid organizationId)
        {
            return _dbContext.Organizations
                .Include(o => o.AceUsers)
                .SingleOrDefault(org => org.Id == organizationId);
        }

        //public OrganizationViewModel GetOrganizationById(Guid id)
        //{
        //    var org = _dbContext.Organizations
        //        .Include(o => o.CalendarUsers)
        //        .SingleOrDefault(organization => organization.Id == id);

        //    var rolesByUser = _dbContext.Roles.Join(
        //        _dbContext.UserRoles,
        //        r => r.Id,
        //        u => u.RoleId,
        //        (role, userRole) =>
        //            new { userId = userRole.UserId, role = role.Name }).GroupBy(x => x.userId).ToDictionary(group => group.Key, group => group.Select(x => x.role));

        //    var orgVM = new OrganizationViewModel() { Id = org.Id, Name = org.Name };
        //    var users = new List<UserViewModel>();
        //    foreach (var user in org.CalendarUsers)
        //    {
        //        var model = AutoMapper.Mapper.Map<UserViewModel>(user);

        //        if (rolesByUser.ContainsKey(user.Id))
        //            model.Role = rolesByUser[user.Id].First(); //May need to support multiple roles, but for now we only have singular roles 

        //        users.Add(model);
        //    }

        //    orgVM.Users = users;

        //    if (!String.IsNullOrEmpty(org.Stripe_CustomerId))
        //    {
        //        var customer = _stripeRepository.GetCustomer(org.Stripe_CustomerId);

        //        orgVM.Stripe_DefaultSourceId = customer.DefaultSourceId;
        //        orgVM.PaymentSources = customer.Sources.Where(s => s.Type == SourceType.BankAccount || s.Type == SourceType.Card).Select(s => s);
        //        orgVM.Subscription = _stripeRepository.GetSubscription(org.Stripe_CustomerId);
        //        orgVM.BillingInformation = _stripeRepository.GetCustomerBillingInformation(org.Stripe_CustomerId);
        //    }

        //    return orgVM;
        //}

        public IEnumerable<UserViewModel> GetOrganizationUsers(Guid id)
        {
            var org = _dbContext.Organizations
                .Include(o => o.AceUsers)
                .SingleOrDefault(organization => organization.Id == id);

            var rolesByUser = _dbContext.Roles.Join(
                _dbContext.UserRoles,
                r => r.Id,
                u => u.RoleId,
                (role, userRole) =>
                    new { userId = userRole.UserId, role = role.Name }).GroupBy(x => x.userId).ToDictionary(group => group.Key, group => group.Select(x => x.role));

            var users = new List<UserViewModel>();
            foreach (var user in org.AceUsers)
            {
                var model = AutoMapper.Mapper.Map<UserViewModel>(user);

                if (rolesByUser.ContainsKey(user.Id))
                    model.Role = rolesByUser[user.Id].First(); //May need to support multiple roles, but for now we only have singular roles 

                users.Add(model);
            }

            return users;
        }

        public Calendar AddCalendar(Guid organizationId, string calendarName)
        {
            var calendar = _dbContext.Calendars.FirstOrDefault(c => c.OrganizationId == organizationId && c.CalendarName == calendarName);
            if (calendar != null)
                throw new InvalidOperationException($"Calendar {calendarName} already exists on organization");

            calendar = new Calendar()
            {
                Id = Guid.NewGuid(),
                CalendarName = calendarName,
                OrganizationId = organizationId
            };

            _dbContext.Calendars.Add(calendar);
            _dbContext.SaveChanges();

            return calendar;
        }

        public IEnumerable<Calendar> GetOrganizationCalendars(Guid organizationId)
        {
            return _dbContext.Calendars.Where(calendar => calendar.OrganizationId == organizationId);
        }

        public Organization EditOrganization(Guid id, SaveOrganizationRequestModel saveOrganizationRequestModel)
        {
            var organization = _dbContext.Organizations.FirstOrDefault(o => o.Id == id);

            _dbContext.Attach(organization);

            organization.Email = saveOrganizationRequestModel.Email;

            if (string.IsNullOrWhiteSpace(organization.Stripe_CustomerId))
            {
                var customer = _stripeRepository.CreateCustomer(saveOrganizationRequestModel);

                organization.Stripe_CustomerId = customer.Id;
            }
            else
            {
                _stripeRepository.UpdateCustomer(organization.Stripe_CustomerId, saveOrganizationRequestModel);
            }

            _dbContext.SaveChanges();

            return organization;
        }

        public void DeleteOrganization(Guid id)
        {
            var org = _dbContext.Organizations.FirstOrDefault(o => o.Id == id);

            //If has subscription punt. Possibly change this later. Be safe for now.
            if (!String.IsNullOrWhiteSpace(org.Stripe_CustomerId) && _stripeRepository.GetSubscription(org.Stripe_CustomerId) != null)
                throw new ApplicationException("Attempting to delete organization with subscription");

            //Remove Stripe Customer Record
            if (!String.IsNullOrWhiteSpace(org.Stripe_CustomerId))
            {
                var deleted = _stripeRepository.DeleteCustomer(org.Stripe_CustomerId);
                //log if not deleted but don't throw exception.
            }

            _dbContext.Remove(org);
            _dbContext.SaveChanges();
        }

        public Organization GetOrganizationByName(string name)
        {
            return _dbContext.Organizations
                .SingleOrDefault(organization => organization.Name == name);
        }

        public IEnumerable<Organization> GetAllOrganizations()
        {
            return _dbContext.Organizations.Select( organization => organization );
        }

        public StripeDeleted DeleteCard(Guid id, string sourceId)
        {
            var org = _dbContext.Organizations.FirstOrDefault(o => o.Id == id);
            return _stripeRepository.DeleteCard(org.Stripe_CustomerId, sourceId);
        }

        public void DeleteDefaultPaymentSource(Guid organizationId)
        {
            var organization = _dbContext.Organizations.FirstOrDefault(o => o.Id == organizationId);

            if (organization == null)
                throw new ApplicationException($"Unable to locate Organization with Id:'{organizationId}'");

            if (String.IsNullOrEmpty(organization.Stripe_CustomerId))
                return;

            var customer = _stripeRepository.GetCustomer(organization.Stripe_CustomerId);

            var defaultSource = customer.Sources.SingleOrDefault(source => source.Id == customer.DefaultSourceId);

            if (defaultSource == null)
                return;
          
            var stripeDeleted = _stripeRepository.DeleteCard(organization.Stripe_CustomerId, defaultSource.Id);

            if (!stripeDeleted.Deleted)
                throw new ApplicationException("Unable to delete default payment source");
        }

        public void CancelSubscription(Guid organizationId)
        {
            var organization = _dbContext.Organizations.SingleOrDefault(org => org.Id == organizationId);

            if (organization == null)
                throw new ApplicationException($"Unable to locate Organization with Id:'{organizationId}'");

            if (String.IsNullOrEmpty(organization.Stripe_CustomerId))
                throw new ApplicationException("A Stripe Customer has not been setup for this Organization");

            _stripeRepository.CancelSubscription(organization.Stripe_CustomerId);
        }

        public void StartTrial(Guid organizationId)
        {
            var organization = _dbContext.Organizations.SingleOrDefault(org => org.Id == organizationId);

            if (organization == null)
                throw new ApplicationException($"Unable to locate Organization with Id:'{organizationId}'");

            if (String.IsNullOrEmpty(organization.Stripe_CustomerId))
                throw new ApplicationException("A Stripe Customer has not been setup for this Organization"); 

            var subscription = _stripeRepository.GetSubscription(organization.Stripe_CustomerId);

            if( subscription != null )
                throw new ApplicationException("A Subscription already exist for this Organization");

            _stripeRepository.StartTrial(organization.Stripe_CustomerId);
        }

        public bool OrganizationHadTrial(Guid organizationId)
        {
            var organization = _dbContext.Organizations.SingleOrDefault(org => org.Id == organizationId);

            if (organization == null)
                return false;

            if (String.IsNullOrEmpty(organization.Stripe_CustomerId))
                return false;

            var customer = _stripeRepository.GetCustomer(organization.Stripe_CustomerId);

            if (customer.Metadata == null)
                return false;

            return customer.Metadata.ContainsKey(StripeRepository.HadTrialMetadataKey);
        }

        public SubscriptionLicenseDetailsViewModel GetSubscriptionLicenseDetails(Guid organizationId)
        {
            var organization = _dbContext.Organizations.SingleOrDefault(org => org.Id == organizationId);

            if (organization == null)
                throw new ApplicationException($"Unable to locate Organization with Id:'{organizationId}'");

            if (String.IsNullOrEmpty(organization.Stripe_CustomerId))
                throw new ApplicationException("A Stripe Customer has not been setup for this Organization");

            return _stripeRepository.GetSubscriptionLicenseDetailsViewModel(organization.Stripe_CustomerId);
        }

        public void ActivateSubscription(Guid organizationId, SetProductPlanRequest setProductPlanRequest)
        {
            var organization = _dbContext.Organizations.SingleOrDefault(org => org.Id == organizationId);

            if (organization == null)
                throw new ApplicationException($"Unable to locate Organization with Id:'{organizationId}'");

            if (String.IsNullOrEmpty(organization.Stripe_CustomerId))
                throw new ApplicationException("A Stripe Customer has not been setup for this Organization");

            _stripeRepository.ActivateSubscription(organization.Stripe_CustomerId, setProductPlanRequest);
        }

        public StripeCard AddCardToOrganization(Guid id, StripeToken token, bool setAsDefault = true )
        {
            var org = _dbContext.Organizations.FirstOrDefault(o => o.Id == id);

            if (string.IsNullOrWhiteSpace(org.Stripe_CustomerId))
                throw new ApplicationException(string.Format("{0} does not have a Stripe Customer Id", org.Name));

            return _stripeRepository.AddCard(org.Stripe_CustomerId, token, setAsDefault);
        }

        public StripeCustomer SetOrganizationDefaultPaymentSource(Guid id, string sourceId)
        {
            var org = _dbContext.Organizations.FirstOrDefault(o => o.Id == id);

            return _stripeRepository.SetDefaultPaymentSource(org.Stripe_CustomerId, sourceId);
        }

        public IEnumerable<ProductPlanViewModel> GetProductPlans()
        {
            return _stripeRepository.GetPlans().Select(p => new ProductPlanViewModel()
            {
                Amount = p.Amount ?? 0,
                Id = p.Id,
                Name = p.Nickname,
                BillingScheme = p.IntervalCount > 1 ?
                   $"{p.IntervalCount} {p.Interval}s" :
                   p.Interval
            });
        }

        public StripeSubscription SetProductPlan(Guid id, SetProductPlanRequest request)
        {
            var org = _dbContext.Organizations.FirstOrDefault(o => o.Id == id);

            return _stripeRepository.SetProductPlan(org.Stripe_CustomerId, request);
        }

        public bool HasActiveSubscription(Guid organizationId)
        {
            var organization = _dbContext.Organizations.SingleOrDefault(org => org.Id == organizationId);

            if (organization == null)
                throw new ApplicationException($"Unable to locate Organization with Id:'{organizationId}'");

            if (String.IsNullOrEmpty(organization.Stripe_CustomerId))
                return false;

            var subscription = _stripeRepository.GetSubscription(organization.Stripe_CustomerId);

            return subscription != null && ( subscription.IsActive || subscription.IsTrial );
        }

        public ValidSubscriptionModel GetSubscriptionValidation(Guid organizationId)
        {
            var org = GetOrganizationWithUsers(organizationId);
            var calendars = GetOrganizationCalendars(organizationId).Where(c => !c.Inactive);
            var users = GetOrganizationUsers(organizationId);

            var licenseInfo = _stripeRepository.GetSubscriptionLicenseDetailsViewModel(org.Stripe_CustomerId);
            var subInfo = _stripeRepository.GetSubscription(org.Stripe_CustomerId);

            var model = new ValidSubscriptionModel();

            if (subInfo == null || (!subInfo.IsActive && !subInfo.IsTrial))
            {
                model.IsValid = false;
                model.AddMessage("Subscription is not active.  All Data is Read-Only until subscription is activated");

                return model;
            }

            if (licenseInfo.Calendars < calendars.Count())
            {
                model.IsValid = false;
                model.AllowCalendarEdit = true;
                model.AddMessage($"You have more active calendars than your subscription allows. Data is Read-Only until {calendars.Count() - licenseInfo.Calendars} are made inactive.");

                return model;
            }

            var editorCount = users.Where(u => u.IsEditor()).Count();
            if (licenseInfo.EditingUsers < editorCount)
            {
                model.IsValid = false;
                model.AddMessage($"You have more active editing users than your subscription allows. Data is Read-Only until {editorCount - licenseInfo.EditingUsers} are made read-only");

                return model;
            }

            if (subInfo.IsTrial && subInfo.DaysLeft <= 7)
            {
                model.IsValid = true;
                model.AddMessage($"Your trial ends in {subInfo.DaysLeft}. Please upgrade to the full version to avoid any interruption in service.");

                return model;
            }

            return model;
        }

        #endregion
    }
}
