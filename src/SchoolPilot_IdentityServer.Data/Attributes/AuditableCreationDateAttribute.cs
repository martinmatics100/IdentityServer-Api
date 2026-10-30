
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace SchoolPilot_IdentityServer.Data.Attributes
{
    public class AuditableCreationDateAttribute : SaveActionAttribute
    {
        public override bool CanPerform(Type type)
        {
            return type == typeof(DateTime?);
        }

        public override object Perform(object value, EntityState state, object originalValue, object parentEntity, string propertyName)
        {
            var shouldAct = state == EntityState.Added && (value == null || (DateTime?)value == DateTime.MinValue);
            return shouldAct ? DateTime.UtcNow : value;
        }

    }

    public class TimeZonedDateAttribute : SaveActionAttribute
    {
        public override bool CanPerform(Type type)
        {
            return type == typeof(ZonedDateTime);
        }

        public override object Perform(object value, EntityState state, object originalValue, object parentEntity, string propertyName)
        {
            var zonedDateTime = value as ZonedDateTime? ?? new ZonedDateTime();
            var originalDate = originalValue as ZonedDateTime? ?? new ZonedDateTime();
            if (zonedDateTime != originalDate)
            {

            }



            var shouldAct = state == EntityState.Added && (value == null || (DateTime?)value == DateTime.MinValue);
            return shouldAct ? DateTime.UtcNow : value;
        }
    }
}
