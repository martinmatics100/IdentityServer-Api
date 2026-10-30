

using SchoolPilot_IdentityServer.Common.Attributes;

namespace SchoolPilot_IdentityServer.Data.Interfaces
{
    public interface IArchivable
    {
        [NotAutoMapped]
        bool IsDeprecated { get; set; }
    }
}
