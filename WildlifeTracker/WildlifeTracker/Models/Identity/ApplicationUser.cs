using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WildlifeTracker.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        // Населено място, за което доброволецът подава данни (служителите null)
        public int? SettlementId { get; set; }
        public Settlement? Settlement { get; set; }
    }
}