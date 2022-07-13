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
            TheOneObject moralisObj1 = p1 as TheOneObject;
            TheOneObject moralisObj2 = p2 as TheOneObject;
            if (moralisObj1 != null && moralisObj1 != null)
            {
                return Equals(moralisObj1.objectId, moralisObj1.objectId);
            }
            return Equals(p1, p2);
        }

        public int GetHashCode(object p)
        {
            TheOneObject moralisObject = p as TheOneObject;
            if (moralisObject != null)
            {
                return moralisObject.objectId.GetHashCode();
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
