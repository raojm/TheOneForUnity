﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Net;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Exceptions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Services.Models;
using TheOneUnity.Platform.Utilities;
using UnityEngine;
using TheOneUnity.Core.Exceptions;

namespace TheOneUnity.Platform.Services.ClientServices
{
    public class TheOneObjectService : IObjectService
    {
        ITheOneCommandRunner CommandRunner { get; }

        IServerConnectionData ServerConnectionData { get; }

        IJsonSerializer JsonSerializer { get; }

        public TheOneObjectService(ITheOneCommandRunner commandRunner, IServerConnectionData serverConnectionData, IJsonSerializer jsonSerializer)
        {
            CommandRunner = commandRunner;
            ServerConnectionData = serverConnectionData;
            JsonSerializer = jsonSerializer;
        }

        public async UniTask<T> FetchAsync<T>(T item, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject
        {
            TheOneCommand command = new TheOneCommand($"server/classes/{Uri.EscapeDataString(item.ClassName)}/{Uri.EscapeDataString(item.objectId)}", method: "GET", sessionToken: sessionToken, data: default);
            Tuple<HttpStatusCode, string> cmdResp = await CommandRunner.RunCommandAsync(command, cancellationToken: cancellationToken);
         
            T resp = default;

            if ((int)cmdResp.Item1 < 400)
            {
                resp = (T)JsonSerializer.Deserialize<T>(cmdResp.Item2);
                    
            }

            return resp;
        }

        public async UniTask<string> SaveAsync(TheOneObject item, IDictionary<string, ITheOneFieldOperation> operations, string sessionToken, CancellationToken cancellationToken = default)
        {
            TheOneCommand command = new TheOneCommand(item.objectId == null ? $"server/classes/{Uri.EscapeDataString(item.ClassName)}" : $"server/classes/{Uri.EscapeDataString(item.ClassName)}/{item.objectId}", method: item.objectId is null ? "POST" : "PUT", sessionToken: sessionToken, data: operations is { } && operations.Count > 0 ? JsonSerializer.Serialize(operations, JsonSerializer.DefaultOptions).JsonInsertParseDate() : JsonSerializer.Serialize(item, JsonSerializer.DefaultOptions).JsonInsertParseDate());
            Tuple<HttpStatusCode, string> cmdResp = await CommandRunner.RunCommandAsync(command, cancellationToken: cancellationToken);
            
            string resp = default;

            if ((int)cmdResp.Item1 < 400)
            {
                resp = cmdResp.Item2;
            }
            else
            {
                throw new TheOneSaveException(cmdResp.Item2);
            }

            return resp;
        }

        public async UniTask DeleteAsync(TheOneObject item, string sessionToken, CancellationToken cancellationToken = default)
        {
            Tuple<HttpStatusCode, string> cmdResp = await CommandRunner.RunCommandAsync(new TheOneCommand($"server/classes/{item.ClassName}/{item.objectId}", method: "DELETE", sessionToken: sessionToken, data: null), cancellationToken: cancellationToken);
            
            if ((int)cmdResp.Item1 >= 400)
            {
                Debug.LogError($"SaveAsync failed: {cmdResp.Item2}");
            }
        }
               
        int MaximumBatchSize { get; } = 50;

        async UniTask<IList<UniTask<IDictionary<string, object>>>> ExecuteBatchRequestAsync<T>(IList<TheOneCommand> requests, string sessionToken, CancellationToken cancellationToken = default)
        {
            int batchSize = requests.Count;

            List<UniTask<IDictionary<string, object>>> tasks = new List<UniTask<IDictionary<string, object>>> { };
            List<UniTaskCompletionSource<IDictionary<string, object>>> completionSources = new List<UniTaskCompletionSource<IDictionary<string, object>>> { };

            for (int i = 0; i < batchSize; ++i)
            {
                UniTaskCompletionSource<IDictionary<string, object>> tcs = new UniTaskCompletionSource<IDictionary<string, object>>();

                completionSources.Add(tcs);
                tasks.Add(tcs.Task);
            }

            List<object> encodedRequests = requests.Select(request =>
            {
                Dictionary<string, object> results = new Dictionary<string, object>
                {
                    ["method"] = request.Method,
                    ["path"] = request is { Path: { }, Resource: { } } ? request.Target.AbsolutePath : new Uri(new Uri(ServerConnectionData.ServerURI), request.Path).AbsolutePath,
                };

                if (request.DataObject != null)
                    results["body"] = request.DataObject;

                return results;
            }).Cast<object>().ToList();

            TheOneCommand command = new TheOneCommand("batch", method: "POST", sessionToken: sessionToken, data: JsonSerializer.Serialize(new Dictionary<string, object> { [nameof(requests)] = encodedRequests }, JsonSerializer.DefaultOptions));

            Tuple<HttpStatusCode, string> cmdResp = await CommandRunner.RunCommandAsync(command, cancellationToken: cancellationToken);

            if ((int)cmdResp.Item1 < 400)
            {
                foreach (UniTaskCompletionSource<IDictionary<string, object>> tcs in completionSources)
                {
                    if (UniTaskStatus.Faulted.Equals(tcs.Task.Status))
                        tcs.TrySetException(new Exception("Update failed."));
                    else if (UniTaskStatus.Canceled.Equals(tcs.Task.Status))
                        tcs.TrySetCanceled();
                }
                

                IList<object> resultsArray = Conversion.As<IList<object>>(cmdResp.Item2);
                int resultLength = resultsArray.Count;

                if (resultLength != batchSize)
                {
                    foreach (UniTaskCompletionSource<IDictionary<string, object>> completionSource in completionSources)
                        completionSource.TrySetException(new InvalidOperationException($"Batch command result count expected: {batchSize} but was: {resultLength}."));
                }
                else
                {
                    for (int i = 0; i < batchSize; ++i)
                    {
                        Dictionary<string, object> result = resultsArray[i] as Dictionary<string, object>;
                        UniTaskCompletionSource<IDictionary<string, object>> target = completionSources[i];

                        if (result.ContainsKey("success"))
                            target.TrySetResult(result["success"] as IDictionary<string, object>);
                        else if (result.ContainsKey("error"))
                        {
                            IDictionary<string, object> error = result["error"] as IDictionary<string, object>;
                            target.TrySetException(new TheOneFailureException((TheOneFailureException.ErrorCode)(long)error["code"], error[nameof(error)] as string));
                        }
                        else
                            target.TrySetException(new InvalidOperationException("Invalid batch command response."));
                    }
                }
            }

            return tasks;
        }
    }
}
