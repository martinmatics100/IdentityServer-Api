

using SchoolPilot_IdentityServer.Data.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SchoolPilot_IdentityServer.Data.Interfaces
{
    public interface IEntity
    {
        [Key]
        [GenerateSequentialId]
        Guid Id { get; set; }
    }
}
