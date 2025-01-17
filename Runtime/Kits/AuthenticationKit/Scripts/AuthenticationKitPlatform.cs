﻿using UnityEngine;

namespace TheOneUnity.Kits.AuthenticationKit
{
    /// <summary>
    /// List of possible platforms of the <see cref="AuthenticationKit"/>.
    /// </summary>
    public enum AuthenticationKitPlatform
    {
        None,
        Android,
        iOS,
        WalletConnect,
        WebGL
    }
}