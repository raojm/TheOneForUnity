
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheOneUnity.Platform.Utilities;

namespace TheOneUnity.Platform.Services.Models
{
    /// <summary>
    /// ParseCommand is an <see cref="WebRequest"/> with pre-populated
    /// headers.
    /// </summary>
    public class TheOneCommand : WebRequest
    {
        //public IDictionary<string, object> DataObject { get; private set; }
        public string DataObject { get; private set; }

        public override Stream Data
        {
            //get => base.Data ??= DataObject is { } ? new MemoryStream(Encoding.UTF8.GetBytes(JsonUtilities.Encode(DataObject))) : default;
            get => base.Data ??= DataObject is { } ? new MemoryStream(Encoding.UTF8.GetBytes(DataObject)) : default;
            set => base.Data = value;
        }

        //public TheOneCommand(string relativeUri, string method, string sessionToken = null, IList<KeyValuePair<string, string>> headers = null, IDictionary<string, object> data = null) : this(relativeUri: relativeUri, method: method, sessionToken: sessionToken, headers: headers, stream: null, contentType: data != null ? "application/json" : null) => DataObject = data;
        public TheOneCommand(string relativeUri, string method, string sessionToken = null, IList<KeyValuePair<string, string>> headers = null, string data = null) : this(relativeUri: relativeUri, method: method, sessionToken: sessionToken, headers: headers, stream: null, contentType: data != null ? "application/json" : null) => DataObject = data;

        public TheOneCommand(string relativeUri, string method, string sessionToken = null, IList<KeyValuePair<string, string>> headers = null, Stream stream = null, string contentType = null)
        {
            Path = relativeUri;
            Method = method;
            Data = stream;
            Headers = new List<KeyValuePair<string, string>>(headers ?? Enumerable.Empty<KeyValuePair<string, string>>());

            if (!String.IsNullOrEmpty(sessionToken))
            {
                Headers.Add(new KeyValuePair<string, string>("X-Parse-Session-Token", sessionToken));
            }

            if (!String.IsNullOrEmpty(contentType))
            {
                Headers.Add(new KeyValuePair<string, string>("Content-Type", contentType));
            }
        }

        public TheOneCommand(TheOneCommand other)
        {
            Resource = other.Resource;
            Path = other.Path;
            Method = other.Method;
            DataObject = other.DataObject;
            Headers = new List<KeyValuePair<string, string>>(other.Headers);
            Data = other.Data;
        }
    }
}
