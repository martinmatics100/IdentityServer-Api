

using Microsoft.AspNetCore.Identity;
using SchoolPilot_IdentityServer.Common.Enums;
using SchoolPilot_IdentityServer.Data.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace SchoolPilot_IdentityServer.Data.Entities
{
    public class ApplicationUser : IdentityUser<Guid>, IEntity, IAuditable, IArchivable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsDeprecated { get; set; }

        [ForeignKey("Account")]
        public Guid? AccountId { get; set; }
        public virtual Account Account { get; set; }
        public UserRole Role { get; set; }
    }
}
