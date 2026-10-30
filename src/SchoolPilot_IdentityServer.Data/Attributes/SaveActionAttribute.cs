

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace SchoolPilot_IdentityServer.Data.Attributes
{
    public abstract class SaveActionAttribute : Attribute
    {
        public abstract bool CanPerform(Type type);

        public abstract object Perform([CanBeNull] object value, EntityState state, [CanBeNull] object originalValue, object parentEntity, string propertyName);
    }
}
