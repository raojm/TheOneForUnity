using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TheOneUnity.Platform.Utilities;
using Cysharp.Threading.Tasks;
using System.Net;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Queries;
using TheOneUnity.Platform.Services.ClientServices;
using TheOneUnity.Platform.Services.Models;

namespace TheOneUnity.Platform.Services.Infrastructure
{
    internal class TheOneQueryService : IQueryService
    {

        ITheOneCommandRunner CommandRunner { get; }

        IJsonSerializer JsonSerializer { get; }
        string SessionToken { get; }
        //ITheOneDataDecoder Decoder { get; }

        public IObjectService ObjectService { get; }

        public TheOneQueryService(ITheOneCommandRunner commandRunner, string sessionToken, IJsonSerializer jsonSerializer, IObjectService objectService) => (CommandRunner, SessionToken, JsonSerializer, ObjectService) = (commandRunner, sessionToken, jsonSerializer, objectService);

        public async UniTask<IEnumerable<T>> FindAsync<T>(TheOneQuery<T> query, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject
        {
            string result = await FindAsync(query.ClassName, query.BuildParameters(), sessionToken, cancellationToken);
            
            return JsonSerializer.Deserialize<List<T>>(result) as IEnumerable<T>;
        }

        public async UniTask<IEnumerable<T>> AggregateAsync<T>(TheOneQuery<T> query, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject
        {
            string aggResp = await AggregateAsync(query.ClassName, query.BuildParameters(), sessionToken, cancellationToken);
            return JsonSerializer.Deserialize<List<T>>(aggResp) as IEnumerable<T>;
        }

        public async UniTask<int> CountAsync<T>(TheOneQuery<T> query, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject
        {
            IDictionary<string, object> parameters = query.BuildParameters();
            parameters["limit"] = 0;
            parameters["count"] = 1;

            string findResp = await FindAsync(query.ClassName, parameters, sessionToken, cancellationToken);
            
            CountQueryResult result = JsonSerializer.Deserialize<CountQueryResult>(findResp); 
            
            return result.count; 
        }

        public async UniTask<IEnumerable<T>> DistinctAsync<T>(TheOneQuery<T> query, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject
        {
            IDictionary<string, object> parameters = query.BuildParameters();
            parameters["distinct"] = ""; // key
            parameters["where"] = ""; // where ?
            parameters["hint"] = ""; // hint

            string aggResp = await AggregateAsync(query.ClassName, parameters, sessionToken, cancellationToken);
            
            return JsonSerializer.Deserialize<List<T>>(aggResp) as IEnumerable<T>;
        }

        public async UniTask<T> FirstAsync<T>(TheOneQuery<T> query, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject
        {
            IDictionary<string, object> parameters = query.BuildParameters();
            parameters["limit"] = 1;

            string findResp = await FindAsync(query.ClassName, parameters, sessionToken, cancellationToken);
                
            IList<T> l = JsonSerializer.Deserialize<List<T>>(findResp);

            return l.FirstOrDefault();
        }

        async UniTask<string> FindAsync(string className, IDictionary<string, object> parameters, string sessionToken, CancellationToken cancellationToken = default)
        {
            Tuple<HttpStatusCode, string> cmdResult = await CommandRunner.RunCommandAsync(new TheOneCommand($"server/classes/{Uri.EscapeDataString(className)}?{TheOneService<TheOneUser>.BuildQueryString(parameters)}", method: "GET", sessionToken: sessionToken, data: null), cancellationToken: cancellationToken);

            return cmdResult.Item2;
        }

        async UniTask<string> AggregateAsync(string className, IDictionary<string, object> parameters, string sessionToken, CancellationToken cancellationToken = default)
        {
            Tuple<HttpStatusCode, string> cmdResult = await CommandRunner.RunCommandAsync(new TheOneCommand($"server/aggregate/{Uri.EscapeDataString(className)}?{TheOneService<TheOneUser>.BuildQueryString(parameters)}", method: "GET", sessionToken: sessionToken, data: null), cancellationToken: cancellationToken);

            return cmdResult.Item2;
        }

    }
}
