using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Utilities;

namespace TheOneUnity.Platform.Operations
{
    class TheOneUniqueAddOperation : ITheOneFieldOperation
    {
        ReadOnlyCollection<object> Data { get; }

        public string __op { get { return "AddUnique"; } }

        public IEnumerable<object> objects => Data;
        public TheOneUniqueAddOperation(IEnumerable<object> objects) => Data = new ReadOnlyCollection<object>(objects.ToList());

        public ITheOneFieldOperation MergeWithPrevious(ITheOneFieldOperation previous) => previous switch
        {
            null => this,
            TheOneDeleteOperation { } => new TheOneSetOperation(Data.ToList()),
            TheOneSetOperation { } setOp => new TheOneSetOperation(Conversion.To<IList<object>>(setOp.Value).Concat(Data).ToList()),
            TheOneAddOperation { } addition => new TheOneAddOperation(addition.objects.Concat(Data)),
            _ => throw new InvalidOperationException("Operation is invalid after previous operation.")
        };

        public object Apply(object oldValue, string key)
        {
            if (oldValue == null)
            {
                return Data.ToList();
            }

            List<object> result = Conversion.To<IList<object>>(oldValue).ToList();
            IEqualityComparer<object> comparer = TheOneFieldOperations.TheOneObjectComparer;

            foreach (object target in Data)
            {
                if (target is TheOneObject)
                {
                    if (!(result.FirstOrDefault(reference => comparer.Equals(target, reference)) is { } matched))
                    {
                        result.Add(target);
                    }
                    else
                    {
                        result[result.IndexOf(matched)] = target;
                    }
                }
                else if (!result.Contains(target, comparer))
                {
                    result.Add(target);
                }
            }

            return result;
        }
    }
}
