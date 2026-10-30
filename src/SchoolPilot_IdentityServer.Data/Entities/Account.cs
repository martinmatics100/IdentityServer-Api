

using SchoolPilot_IdentityServer.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SchoolPilot_IdentityServer.Data.Entities
{
    public class Account : IEntity, IAuditable
    {
        public Guid Id { get; set; }

        [StringLength(256)]
        [Required]
        public string Name { get; set; }

        public bool Disabled { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public AccountMiscFlags Flags { get; set; }

        /// <summary>
        /// Future date to trigger the disabling of an account.
        /// </summary>
        public DateTime? SuspendDate { get; set; }

        /// <summary>
        /// UTC DateTime for when an account is disabled
        /// </summary>
        public DateTime? DisabledOn { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
    }

    [Flags]
    public enum AccountMiscFlags
    {
        None = 0,
        IsPersonalAccount = 1,
        IsTestingAgency = 1 << 1,
        IsDisabled = 1 << 2,
    }
}
