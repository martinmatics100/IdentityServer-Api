

using SchoolPilot_IdentityServer.Common.Attributes;
using SchoolPilot_IdentityServer.Data.Attributes;

namespace SchoolPilot_IdentityServer.Data.Interfaces
{
    public interface IAuditable
    {
        [NotAutoMapped]
        [AuditableCreationDate]
        DateTime? CreatedOn { get; set; }

        [NotAutoMapped]
        [AuditableModificationDate]
        DateTime? ModifiedOn { get; set; }
    }
}
