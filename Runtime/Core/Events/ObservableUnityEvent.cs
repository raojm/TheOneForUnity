using UnityEngine.Events;
using TheOneUnity.Sdk.Data;
using System;

namespace TheOneUnity.Sdk.Events
{
    /// <summary>
    /// The main event for <see cref="Observable{t}"/>.
    /// </summary>
    public class ObservableUnityEvent<T> : UnityEvent<T> where T : struct, IConvertible
    {
    }
}