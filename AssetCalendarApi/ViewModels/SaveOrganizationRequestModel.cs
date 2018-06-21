using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class SaveOrganizationRequestModel
    {
        [Required]
        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        public string AddressLine1 { get; set; }

        public string AddressLIne2 { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        [StringLength(2, MinimumLength = 2)]
        [RegularExpression(@"^(?:(A[KLRZ]|C[AOT]|D[CE]|FL|GA|HI|I[ADLN]|K[SY]|LA|M[ADEINOST]|N[CDEHJMVY]|O[HKR]|P[AR]|RI|S[CD]|T[NX]|UT|V[AIT]|W[AIVY]))$", ErrorMessage = "Invalid State")]
        public string State { get; set; }

        [Required]
        [StringLength(5, MinimumLength = 5)]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Invalid Zip")]
        public string Zip { get; set; }

        //public int NumberOfUsers { get; set; }
        //public string Stripe_PlanId { get; set; }
        //public StripeToken PaymentToken { get; set; }
    }
}
