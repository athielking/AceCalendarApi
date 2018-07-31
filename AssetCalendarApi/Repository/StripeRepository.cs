using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using AssetCalendarApi.ViewModels;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Repository
{
    public class StripeRepository
    {
        #region Data Members

        private readonly IConfiguration _config;

        private readonly AssetCalendarDbContext _context;

        public const string HadTrialMetadataKey = "HadTrial";

        #endregion

        #region Properties

        private string StripeSK
        {
            get => _config["Stripe_SK"];
        }

        private string AceCalendarProductId
        {
            get => _config["Stripe_AceCalendarProductId"];
        }

        #endregion

        #region Constructor

        public StripeRepository(IConfiguration config, AssetCalendarDbContext dbContext)
        {
            _config = config;
            _context = dbContext;
        }

        #endregion

        #region Private Methods

        private StripeSubscription CreateSubscription(string customerId, bool includeTrial, SetProductPlanRequest setProductPlanRequest = null)
        {
            var planService = new StripePlanService(StripeSK);

            var plans = planService.List(new StripePlanListOptions()
            {
                ProductId = AceCalendarProductId
            });

            var subscriptionService = new StripeSubscriptionService(StripeSK);

            return subscriptionService.Create(customerId, new StripeSubscriptionCreateOptions()
            {
                Items = new List<StripeSubscriptionItemOption>()
                {
                    new StripeSubscriptionItemOption()
                    {
                        PlanId = setProductPlanRequest != null ? setProductPlanRequest.PlanId : plans.OrderBy(p => p.Amount).First().Id,
                        Quantity = 1
                    }
                },
                TrialPeriodDays = includeTrial ? 30 : 0
            });
        }

        #endregion

        #region Public Methods

        public IEnumerable<StripePlan> GetPlans()
        {
            var service = new StripePlanService(StripeSK);
            return service.List(new StripePlanListOptions()
            {
                ProductId = AceCalendarProductId
            });
        }

        public SubscriptionViewDetailsModel GetSubscription(string customerId)
        {
            var customerService = new StripeCustomerService(StripeSK);
            var productService = new StripeProductService(StripeSK);

            if (string.IsNullOrEmpty(customerId))
                return null;

            var customer = customerService.Get(customerId);

            if (customer.Subscriptions == null)
                return null;

            if (!customer.Subscriptions.Any())
                return null;

            var subscription = customer.Subscriptions.FirstOrDefault();
            var product = productService.Get(subscription.StripePlan.ProductId);

            var daysLeftInCurrentPeriod = ( subscription.CurrentPeriodEnd.Value - DateTime.Now ).Days;

            var defaultSource = customer.Sources.SingleOrDefault(source => source.Id == customer.DefaultSourceId);

            return new SubscriptionViewDetailsModel()
            {
                SubscriptionId = subscription.Id,
                ProductName = subscription.StripePlan.Nickname,
                IsActive = subscription.Status == "active",
                DaysLeft = daysLeftInCurrentPeriod > 0 ? daysLeftInCurrentPeriod : 0,
                IsTrial = subscription.Status == "trialing",
                CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                HasDefaultPaymentMethod = defaultSource != null,
                DefaultPaymentMethod = defaultSource != null ? $"{defaultSource.Card.Brand} {defaultSource.Card.Last4}" : string.Empty,
                Calendars = subscription.StripePlan.Metadata.ContainsKey("Calendars") ? Int32.Parse(subscription.StripePlan.Metadata["Calendars"]) : 0,
                Users = subscription.StripePlan.Metadata.ContainsKey("Users") ? Int32.Parse(subscription.StripePlan.Metadata["Users"]) : 0
            };
        }

        public SubscriptionLicenseDetailsViewModel GetSubscriptionLicenseDetailsViewModel(string customerId)
        {
            var customerService = new StripeCustomerService(StripeSK);

            if (String.IsNullOrEmpty(customerId))
                return new SubscriptionLicenseDetailsViewModel();

            var customer = customerService.Get(customerId);

            if (!customer.Subscriptions.Any())
                return new SubscriptionLicenseDetailsViewModel();

            var subscription = customer.Subscriptions.FirstOrDefault();

            return new SubscriptionLicenseDetailsViewModel()
            {
                Calendars = subscription.StripePlan.Metadata.ContainsKey("Calendars") ? Int32.Parse(subscription.StripePlan.Metadata["Calendars"]) : 0,
                EditingUsers = subscription.StripePlan.Metadata.ContainsKey("Users") ? Int32.Parse(subscription.StripePlan.Metadata["Users"]) : 0
            };
        }

        public BillingInformationViewModel GetCustomerBillingInformation(string customerId)
        {
            var customer = GetCustomer(customerId);

            return new BillingInformationViewModel()
            {
                AddressLine1 = customer.Shipping.Address.Line1,
                AddressLine2 = customer.Shipping.Address.Line2,
                City = customer.Shipping.Address.City,
                State = customer.Shipping.Address.State,
                Zip = customer.Shipping.Address.PostalCode,
                Phone = customer.Shipping.Phone
            };
        }

        public StripeCustomer GetCustomer(string customerId)
        {
            var service = new StripeCustomerService(StripeSK);

            return service.Get(customerId);
        }

        public StripeDeleted DeleteCustomer(string customerId)
        {
            var customerService = new StripeCustomerService(StripeSK);

            return customerService.Delete(customerId);
        }

        public StripeCustomer CreateCustomer(SaveOrganizationRequestModel org)
        {
            var service = new StripeCustomerService(StripeSK);
            return service.Create(new StripeCustomerCreateOptions()
            {
                Description = org.Name,
                Email = org.Email,
                Shipping = new StripeShippingOptions()
                {
                    Line1 = org.AddressLine1,
                    Line2 = org.AddressLIne2,
                    CityOrTown = org.City,
                    State = org.State,
                    PostalCode = org.Zip,
                    Name = org.Name
                }
            });
        }

        public StripeCustomer UpdateCustomer(string customerId, SaveOrganizationRequestModel saveOrganizationRequestModel)
        {
            var customerService = new StripeCustomerService(StripeSK);

            return customerService.Update(customerId, new StripeCustomerUpdateOptions()
            {
                Email = saveOrganizationRequestModel.Email,
                Shipping = new StripeShippingOptions()
                {
                    Line1 = saveOrganizationRequestModel.AddressLine1,
                    Line2 = saveOrganizationRequestModel.AddressLIne2,
                    CityOrTown = saveOrganizationRequestModel.City,
                    State = saveOrganizationRequestModel.State,
                    PostalCode = saveOrganizationRequestModel.Zip,
                    Name = saveOrganizationRequestModel.Name
                }
            });
        }

        public StripeSubscription CreateSubscription(string customerId, string planId, int quantity, string sourceId)
        {
            var subService = new StripeSubscriptionService(StripeSK);
            var subscription = subService.Create(customerId, new StripeSubscriptionCreateOptions()
            {
                Source = sourceId,
                Items = new List<StripeSubscriptionItemOption>()
                {
                    new StripeSubscriptionItemOption()
                    {
                        PlanId = planId,
                        Quantity = quantity > 0 ? quantity : 1
                    }
                }
            });

            return subscription;
        }

        public StripeCustomer SetDefaultPaymentSource(string customerId, string sourceId)
        {
            var customer = new StripeCustomerService(StripeSK);
            return customer.Update(customerId, new StripeCustomerUpdateOptions()
            {
                DefaultSource = sourceId
            });
        }

        public StripeCard AddCard(string customerId, StripeToken token, bool setAsDefault = true)
        {
            var cardService = new StripeCardService(StripeSK);

            var options = new StripeCardCreateOptions()
            {
                SourceToken = token.Id
            };

            var stripeCard = cardService.Create(customerId, options);

            if (setAsDefault)
                SetDefaultPaymentSource(customerId, stripeCard.Id);

            return stripeCard;
        }

        public StripeDeleted DeleteCard(string customerId, string sourceId)
        {
            var service = new StripeCardService(StripeSK);
            return service.Delete(customerId, sourceId);
        }

        public StripeSubscription SetProductPlan(string customerId, SetProductPlanRequest request)
        {
            var customerService = new StripeCustomerService(StripeSK);
            var productService = new StripeProductService(StripeSK);

            var aceCalendar = productService.Get(AceCalendarProductId);

            var customer = customerService.Get(customerId);

            var subscription = customer.Subscriptions.FirstOrDefault(s => s.Items.Any(i => i.Plan?.ProductId == aceCalendar.Id));
            var subItem = subscription.Items.FirstOrDefault(i => i.Plan.ProductId == aceCalendar.Id);

            var itemService = new StripeSubscriptionItemService(StripeSK);
            itemService.Update(subItem.Id, new StripeSubscriptionItemUpdateOptions()
            {
                PlanId = request.PlanId,
                Quantity = 1,
                Prorate = true
            });

            customer = customerService.Get(customerId);
            return customer.Subscriptions.First();
        }

        public StripeSubscription CancelSubscription(string customerId)
        {
            var subscriptionService = new StripeSubscriptionService(StripeSK);

            var subscription = this.GetSubscription(customerId);

            return subscriptionService.Cancel(subscription.SubscriptionId, true);
        }

        public void StartTrial(string customerId)
        {
            var subscriptionService = new StripeSubscriptionService(StripeSK);

            var subscription = CreateSubscription(customerId, true);

            //Setup the trial to not renew automatically
            subscriptionService.Cancel(subscription.Id, true);

            var customerService = new StripeCustomerService(StripeSK);

            //Set in metadata that the customer has had a trial
            var metadataDictionary = new Dictionary<string, string>();

            metadataDictionary[HadTrialMetadataKey] = "True";

            customerService.Update(customerId, new StripeCustomerUpdateOptions()
            {
                Metadata = metadataDictionary
            });
        }

        public void ActivateSubscription(string customerId, SetProductPlanRequest setProductPlanRequest)
        {
            var subscription = this.GetSubscription(customerId);

            if (subscription == null)
            {
                CreateSubscription(customerId, false, setProductPlanRequest);
                return;
            }

            var subscriptionService = new StripeSubscriptionService(StripeSK);

            var stripeSubscription = subscriptionService.Get(subscription.SubscriptionId);

            subscriptionService.Update(subscription.SubscriptionId, new StripeSubscriptionUpdateOptions()
            {
                CancelAtPeriodEnd = false,
                Items = new List<StripeSubscriptionItemUpdateOption>()
                {
                    new StripeSubscriptionItemUpdateOption()
                    {
                        Id = stripeSubscription.Items.Data[0].Id,
                        PlanId = setProductPlanRequest.PlanId,
                    }
                }
            });
        }

        #endregion
    }
}
