using System;
using System.Collections.Generic;
using System.Text;
using TheOneUnity.Platform.Queries;

namespace TheOneUnity.Platform.Abstractions
{
    public interface IObjectState : IEnumerable<KeyValuePair<string, object>>
    {
        bool IsNew { get; }
        object this[string key] { get; }

        bool ContainsKey(string key);

        IObjectState MutatedClone(Action<MutableObjectState> func);
    }
}
