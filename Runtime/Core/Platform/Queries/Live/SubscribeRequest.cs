using System.Collections.Generic;
using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Queries.Live
{
    public class SubscribeRequest<T> : QueryEventMessage where T : TheOneObject
    {
        private TheOneQuery<T> _query;

        /// <summary>
        /// REQUIRED: TheOne Application Id
        /// </summary>
        public string applicationId { get; set; }

        /// <summary>
        /// REQUIRED: Client generated 
        /// </summary>
        public int requestId { get; set; }

        /// <summary>
        /// OPTIONAL: TheOne current user session token.
        /// </summary>
        public string sessionToken { get; set; }

        /// <summary>
        /// Query parameter values sent to 
        /// </summary>
        public IDictionary<string, object> query { get; private set; }

        internal TheOneQuery<T> OriginalQuery
        { 
            get { return _query; }
            set
            {
                _query = value;

                query = _query.BuildParameters(true);
            }
        }

        public SubscribeRequest() => op = OperationTypes.subscribe.ToString();

        public SubscribeRequest(TheOneQuery<T> targetQuery, string applicationId, int requestId, string sessionToken = null) => 
            (this.OriginalQuery, this.applicationId, this.requestId, op, this.sessionToken) = 
            (targetQuery, applicationId, requestId, OperationTypes.subscribe.ToString(), sessionToken);
        
    }
}
