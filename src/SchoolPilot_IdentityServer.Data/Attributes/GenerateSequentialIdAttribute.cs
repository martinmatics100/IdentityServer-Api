

using Microsoft.EntityFrameworkCore;
using SchoolPilot_IdentityServer.Data.Helpers;

namespace SchoolPilot_IdentityServer.Data.Attributes
{
    public class GenerateSequentialIdAttribute : SaveActionAttribute
    {
        public override bool CanPerform(Type type)
        {
            return type == typeof(Guid);
        }

        public override object Perform(object obj, EntityState state, object originalValue, object parentEntity, string propertyName)
        {
            //not null check because you cant have a nullable primary key
            var shouldAct = (state == EntityState.Added || state == EntityState.Modified)
                            && obj != null && (Guid)obj == default(Guid);

            return shouldAct ? SequentialGuid.Create() : obj;
        }
    }
}
