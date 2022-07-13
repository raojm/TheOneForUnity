using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheOneUnity
{
    /// <summary>
    /// Provides a persistable object to hold TheOne related data.
    /// </summary>
    [Serializable]
    public class TheOneServerSettings : ScriptableObject
    {
        [FormerlySerializedAs("ServerUrl")]
        [FormerlySerializedAs("ServerUri")] 
        public string DappUrl;
        [FormerlySerializedAs("ApplicationId")] public string DappId;
        [FormerlySerializedAs("ApplicationName")] public string DappName;
        [FormerlySerializedAs("ApplicationDescription")] public string DappDescription;
        [FormerlySerializedAs("ApplicationVersion")] public string DappVersion;
        [FormerlySerializedAs("DappIconUri")] [FormerlySerializedAs("ApplicationIconUri")] public string DappIconUrl;
        [FormerlySerializedAs("DappWebsite")] [FormerlySerializedAs("ApplicationUrl")] public string DappWebsiteUrl;
        public bool DisableAutoOpenWizard = false;

        public TheOneServerSettings()
        {
            DappUrl = String.Empty;
            DappId = String.Empty;
            DappName = "Your dapp name";
            DappDescription = "Your dapp description";
            DappVersion = "0.0.1";
            DappWebsiteUrl = "https://moralis.io";
            DappIconUrl = "https://moralis.io/wp-content/uploads/2022/05/symbol_for_light_bckg.svg";
        }
    }
}