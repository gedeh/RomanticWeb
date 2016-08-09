using System;

namespace NullGuard
{
    public enum ValidationFlags
    {
        None,
        OutValues,
        All
    }

    public class NullGuardAttribute : Attribute
    {
        public NullGuardAttribute()
        {
        }

        public NullGuardAttribute(ValidationFlags validationFlags)
        {
        }
    }
}
