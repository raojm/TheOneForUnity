using System;
using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Utilities
{
    public class TheOneFileExtensions
    {
        public static TheOneFile Create(string name, Uri uri, string mimeType = null) => new TheOneFile(name, uri, mimeType);
    }
}
