using TheOneUnity.Platform.Abstractions;

namespace TheOneUnity.Platform.Operations
{
    public class TheOneDeleteOperation : ITheOneFieldOperation
    {
        public string __op { get { return "Delete";  } }
        internal static object Token { get; } = new object { };

        public static TheOneDeleteOperation Instance { get; } = new TheOneDeleteOperation { };

        private TheOneDeleteOperation() { }

        public ITheOneFieldOperation MergeWithPrevious(ITheOneFieldOperation previous) => this;

        public object Apply(object oldValue, string key) => Token;
    }
}
