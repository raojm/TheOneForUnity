using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Utilities;

namespace TheOneUnity.Platform.Operations
{
    class TheOneAddOperation : ITheOneFieldOperation
    {
        ReadOnlyCollection<object> Data { get; }

        public string __op { get { return "Add"; } }

        public IEnumerable<object> objects => Data;

        public TheOneAddOperation(IEnumerable<object> objects) => Data = new ReadOnlyCollection<object>(objects.ToList());

        public ITheOneFieldOperation MergeWithPrevious(ITheOneFieldOperation previous) => previous switch
        {
            null => this,
            TheOneDeleteOperation { } => new TheOneSetOperation(Data.ToList()),
            TheOneSetOperation { } setOp => new TheOneSetOperation(Conversion.To<IList<object>>(setOp.Value).Concat(Data).ToList()),
            TheOneAddOperation { } addition => new TheOneAddOperation(addition.objects.Concat(Data)),
            _ => throw new InvalidOperationException("Operation is invalid after previous operation.")
        };

        public object Apply(object oldValue, string key) => oldValue is { } ? Conversion.To<IList<object>>(oldValue).Concat(Data).ToList() : Data.ToList();

        
    }
}
