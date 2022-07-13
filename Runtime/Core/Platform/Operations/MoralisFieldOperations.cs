using System;
using System.Collections.Generic;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Operations
{
    public class TheOneObjectIdComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object p1, object p2)
        {
            TheOneObject theoneObj1 = p1 as TheOneObject;
            TheOneObject theoneObj2 = p2 as TheOneObject;
            if (theoneObj1 != null && theoneObj1 != null)
            {
                return Equals(theoneObj1.objectId, theoneObj1.objectId);
            }
            return Equals(p1, p2);
        }

        public int GetHashCode(object p)
        {
            TheOneObject theoneObject = p as TheOneObject;
            if (theoneObject != null)
            {
                return theoneObject.objectId.GetHashCode();
            }
            return p.GetHashCode();
        }
    }

    static class TheOneFieldOperations
    {
        private static TheOneObjectIdComparer comparer;

        public static ITheOneFieldOperation Decode(IDictionary<string, object> json) => throw new NotImplementedException();

        public static IEqualityComparer<object> TheOneObjectComparer
        {
            get
            {
                if (comparer == null)
                {
                    comparer = new TheOneObjectIdComparer();
                }
                return comparer;
            }
        }
    }
}
