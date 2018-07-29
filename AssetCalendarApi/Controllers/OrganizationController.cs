using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
using Microsoft.AspNetCore.Identity;
using AssetCalendarApi.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Stripe;
using AutoMapper;
using AssetCalendarApi.Tools;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Organization Admin")]
    public class OrganizationController : ApiBaseController
    {
        #region Data Members

        private readonly OrganizationRepository _organizationRepository;
        private readonly SignalRService _signalRService;
        private readonly IConfiguration _config;

        #endregion

        #region Constructor

        public OrganizationController
        (
            IConfiguration configuration,
            OrganizationRepository organizationRepository,
            SignalRService signalRService,
            UserManager<AceUser> userManager
        ) : base(userManager)
        {
            _config = configuration;
            _organizationRepository = organizationRepository;
            _signalRService = signalRService;
        }

        #endregion

        #region Public Methods

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Get()
        {
            try
            {
                var orgs = _organizationRepository
                    .GetAllOrganizations()
                    .Select(organization => Mapper.Map<OrganizationViewModel>(organization))
                    .ToList();

                return SuccessResult(orgs);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Organizations"));
            }
        }

        //[HttpGet("{id}")]       
        //public IActionResult Get(Guid id)
        //{
        //    try
        //    {
        //        if (!UserIsAdmin() && CalendarUser.OrganizationId != id)
        //            return BadRequest(GetErrorMessageObject($"User '{CalendarUser.UserName}' does not have access to this organization."));

        //        var org = _organizationRepository.GetOrganizationById(id);

        //        return SuccessResult(org);
        //    }
        //    catch
        //    {
        //        return BadRequest(GetErrorMessageObject("Failed to Get Organization"));
        //    }
        //}

        [HttpGet]
        [Route("getOrganizationDetails")]
        public IActionResult GetOrganizationDetails(Guid id)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                return SuccessResult(_organizationRepository.GetOrganizationDetails(id));
            }
            catch
            {
                return BadRequest("Unable to Get Organization Details");
            }
        }

        [HttpGet]
        [Route("getSubscriptionDetails")]
        public IActionResult GetSubscriptionDetails(Guid id)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                _signalRService.CheckSubscriptionAsync(id);
                return SuccessResult(_organizationRepository.GetSubscriptionDetails(id));
            }
            catch
            {
                return BadRequest("Unable to Get Subscription Details");
            }
        }

        [HttpGet]
        [Route("getDefaultPaymentSource")]
        public IActionResult GetDefaultPaymentSource(Guid id)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                var defaultPaymentSource = _organizationRepository.GetDefaultPaymentSource(id);

                return SuccessResult(defaultPaymentSource);
            }
            catch
            {
                return BadRequest("Unable to Get Default Payment Source");
            }
        }

        //Have this one authorized to admin since it will return admin users as well.
        //Create a new one for mapping organization users to calendars.
        [HttpGet]
        [Authorize(Roles = "Admin, Organization Admin")]     
        [Route("getOrganizationUsers")]
        public IActionResult GetOrganizationUsers(Guid id)
        {
            try
            {
                
                var organizationUsers = _organizationRepository.GetOrganizationUsers(id).Select(user =>
                {
                    return new UserGridModel()
                    {
                        Id = user.Id,
                        Role = user.Role,
                        Username = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    };
                }).ToList();

                if (User.IsInRole("Organization Admin"))
                    organizationUsers = organizationUsers.Where(u => u.Role != "Admin").ToList();

                return SuccessResult(organizationUsers);
            }
            catch
            {
                return BadRequest("Unable to Get Organization Users");
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody]SaveOrganizationRequestModel organization)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var addedOrg = _organizationRepository.AddOrganization(organization);

                return Ok(addedOrg);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Organization"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody]SaveOrganizationRequestModel organization)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                _organizationRepository.EditOrganization(id, organization);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Update Organization"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                _organizationRepository.DeleteOrganization(id);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Delete Organization"));
            }
        }

        [HttpPost("addCard/{id}")]
        public IActionResult AddCard(Guid id, [FromBody]StripeToken token)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                _organizationRepository.AddCardToOrganization(id, token, false);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Payment Source"));
            }
        }

        [HttpPost("updateDefaultPaymentSource/{id}")]
        public IActionResult UpdateDefaultPaymentSource(Guid id, [FromBody]StripeToken token)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                _organizationRepository.DeleteDefaultPaymentSource(id);

                _organizationRepository.AddCardToOrganization(id, token, true);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Update Default Payment Source"));
            }
        }

        [HttpGet("deleteCard/{id}/{sourceId}")]
        public IActionResult DeleteCard(Guid id, string sourceId)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                var deleted = _organizationRepository.DeleteCard(id, sourceId);

                if (deleted.Deleted)
                    return Ok();
            }
            catch
            {
            }

            return BadRequest(GetErrorMessageObject("Failed to Delete Payment Source"));
        }

        [HttpGet("deleteDefaultPaymentSource/{id}")]
        public IActionResult DeleteDefaultPaymentSource(Guid id)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                _organizationRepository.DeleteDefaultPaymentSource(id);
                
                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Delete Default Payment Source"));
            }         
        }

        [HttpGet("cancelSubscription/{id}")]
        public IActionResult CancelSubscription(Guid id)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                _organizationRepository.CancelSubscription(id);
                _signalRService.CheckSubscriptionAsync(id);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Cancel Subscription"));
            }
        }

        [HttpGet("setDefaultSource/{id}/{sourceId}")]
        public IActionResult SetDefaultSource(Guid id, string sourceId)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                var cust = _organizationRepository.SetOrganizationDefaultPaymentSource(id, sourceId);

                if (cust != null)
                    return Ok();
            }
            catch
            {
            }

            return BadRequest(GetErrorMessageObject("Failed to Set Default Payment Source"));
        }

        [HttpGet("productPlans")]
        public IActionResult GetProductPlans()
        {
            try
            {
                return SuccessResult(_organizationRepository.GetProductPlans().ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(GetErrorMessageObject(ex.Message));
            }
        }

        [HttpPost("productPlans/{id}")]
        public IActionResult SetProductPlan(Guid id, [FromBody]SetProductPlanRequest request)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                var subscription = _organizationRepository.SetProductPlan(id, request);

                return Accepted();
            }
            catch (Exception ex)
            {
                return BadRequest(GetErrorMessageObject(ex.Message));
            }
        }

        [HttpGet("startTrial/{id}")]
        public IActionResult StartTrial(Guid id)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                var subscription = _organizationRepository.GetSubscriptionDetails(id);

                if (subscription != null)
                    return BadRequest(GetErrorMessageObject($"A Subscription already exists for this Organization."));

                _organizationRepository.StartTrial(id);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Start Trial"));
            }
        }

        [HttpPost("activateSubscription/{id}")]
        public IActionResult ActivateSubscription(Guid id, [FromBody]SetProductPlanRequest setProductPlanRequest)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                _organizationRepository.ActivateSubscription(id, setProductPlanRequest);
                _signalRService.CheckSubscriptionAsync(id);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Activate Subscription"));
            }
        }

        [HttpGet("organizationHadTrial/{id}")]
        public IActionResult OrganizationHadTrial(Guid id)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                return SuccessResult(_organizationRepository.OrganizationHadTrial(id));
            }
            catch
            {
                return SuccessResult(true);
            }
        }

        [HttpGet("getSubscriptionLicenseDetails/{id}")]
        public IActionResult GetSubscriptionLicenseDetails(Guid id)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                var subscriptionLicenseDetails = _organizationRepository.GetSubscriptionLicenseDetails(id);

                return SuccessResult(subscriptionLicenseDetails);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Subscription License Details"));
            }
        }

        [HttpGet("validate/{id}")]
        public IActionResult ValidateSubscription(Guid id)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                var validation = _organizationRepository.GetSubscriptionValidation(id);

                return SuccessResult(validation);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to get Subscription Validation"));
            }
        }

        #endregion
    }
}
