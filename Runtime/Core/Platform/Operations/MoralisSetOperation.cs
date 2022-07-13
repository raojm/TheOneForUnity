using TheOneUnity.Platform.Abstractions;

namespace TheOneUnity.Platform.Operations
{
    class TheOneSetOperation : ITheOneFieldOperation
    {
        public object Value { get; private set; }

        public TheOneSetOperation(object value) => Value = value;

        public ITheOneFieldOperation MergeWithPrevious(ITheOneFieldOperation previous) => this;

        public object Apply(object oldValue, string key) => Value;


    }
}
