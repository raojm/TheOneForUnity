using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Queries
{
    public static class QueryServiceExtensions
    {
        public static TheOneQuery<T> GetQuery<T, TUser>(this IServiceHub<TUser> serviceHub) where T : TheOneObject where TUser : TheOneUser => new TheOneQuery<T>(serviceHub.QueryService, serviceHub.InstallationService, serviceHub.ServerConnectionData, serviceHub.JsonSerializer, serviceHub.CurrentUserService.CurrentUser.sessionToken);

        /// <summary>
        /// Constructs a query that is the and of the given queries.
        /// </summary>
        /// <typeparam name="T">The type of TheOneObject being queried.</typeparam>
        /// <param name="serviceHub"></param>
        /// <param name="source">An initial query to 'and' with additional queries.</param>
        /// <param name="queries">The list of TheOneQueries to 'and' together.</param>
        /// <returns>A query that is the and of the given queries.</returns>
        public static TheOneQuery<T> ConstructAndQuery<T, TUser>(this IServiceHub<TUser> serviceHub, TheOneQuery<T> source, params TheOneQuery<T>[] queries) where T : TheOneObject where TUser : TheOneUser => serviceHub.ConstructAndQuery(queries.Concat(new[] { source }));

        // ALTERNATE NAME: BuildOrQuery

        /// <summary>
        /// Constructs a query that is the or of the given queries.
        /// </summary>
        /// <typeparam name="T">The type of TheOneObject being queried.</typeparam>
        /// <param name="source">An initial query to 'or' with additional queries.</param>
        /// <param name="queries">The list of TheOneQueries to 'or' together.</param>
        /// <returns>A query that is the or of the given queries.</returns>
        public static TheOneQuery<T> ConstructOrQuery<T, TUser>(this IServiceHub<TUser> serviceHub, TheOneQuery<T> source, params TheOneQuery<T>[] queries) where T : TheOneObject where TUser : TheOneUser => serviceHub.ConstructOrQuery(queries.Concat(new[] { source }));
        public static TheOneQuery<T> ConstructNorQuery<T, TUser>(this IServiceHub<TUser> serviceHub, TheOneQuery<T> source, params TheOneQuery<T>[] queries) where T : TheOneObject where TUser : TheOneUser => serviceHub.ConstructNorQuery(queries.Concat(new[] { source }));

        /// <summary>
        /// Construct a query that is the and of two or more queries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceHub"></param>
        /// <param name="queries">The list of TheOneQueries to 'and' together.</param>
        /// <returns>A TheOneQquery that is the 'and' of the passed in queries.</returns>
        public static TheOneQuery<T> ConstructAndQuery<T, TUser>(this IServiceHub<TUser> serviceHub, IEnumerable<TheOneQuery<T>> queries) where T : TheOneObject where TUser : TheOneUser
        {
            string className = default;
            List<IDictionary<string, object>> andValue = new List<IDictionary<string, object>> { };

            // We need to cast it to non-generic IEnumerable because of AOT-limitation

            IEnumerable nonGenericQueries = queries;
            foreach (object obj in nonGenericQueries)
            {
                TheOneQuery<T> query = obj as TheOneQuery<T>;

                if (className is { } && query.ClassName != className)
                {
                    throw new ArgumentException("All of the queries in an and query must be on the same class.");
                }

                className = query.ClassName;
                IDictionary<string, object> parameters = query.BuildParameters();

                if (parameters.Count == 0)
                {
                    continue;
                }

                if (!parameters.TryGetValue("where", out object where) || parameters.Count > 1)
                {
                    throw new ArgumentException("None of the queries in an and query can have non-filtering clauses");
                }

                //orValue.Add(where as IDictionary<string, object>);

                andValue.Add(serviceHub.JsonSerializer.Deserialize<IDictionary<string, object>>(where.ToString()));
                //andValue.Add(JsonConvert.DeserializeObject<IDictionary<string, object>>(where.ToString()));
            }

            return new TheOneQuery<T>(new TheOneQuery<T>(serviceHub.QueryService, serviceHub.InstallationService, serviceHub.ServerConnectionData, serviceHub.JsonSerializer, serviceHub.CurrentUserService.CurrentUser?.sessionToken, className), where: new Dictionary<string, object> { ["$and"] = andValue });
        }

        /// <summary>
        /// Constructs a query that is the or of the given queries.
        /// </summary>
        /// <param name="queries">The list of TheOneQueries to 'or' together.</param>
        /// <returns>A TheOneQquery that is the 'or' of the passed in queries.</returns>
        public static TheOneQuery<T> ConstructOrQuery<T, TUser>(this IServiceHub<TUser> serviceHub, IEnumerable<TheOneQuery<T>> queries) where T : TheOneObject where TUser : TheOneUser
        {
            string className = default;
            List<IDictionary<string, object>> orValue = new List<IDictionary<string, object>> { };

            // We need to cast it to non-generic IEnumerable because of AOT-limitation

            IEnumerable nonGenericQueries = queries;
            foreach (object obj in nonGenericQueries)
            {
                TheOneQuery<T> query = obj as TheOneQuery<T>;

                if (className is { } && query.ClassName != className)
                {
                    throw new ArgumentException("All of the queries in an or query must be on the same class.");
                }

                className = query.ClassName;
                IDictionary<string, object> parameters = query.BuildParameters();

                if (parameters.Count == 0)
                {
                    continue;
                }

                if (!parameters.TryGetValue("where", out object where) || parameters.Count > 1)
                {
                    throw new ArgumentException("None of the queries in an or query can have non-filtering clauses");
                }

                //orValue.Add(where as IDictionary<string, object>);
                orValue.Add(serviceHub.JsonSerializer.Deserialize<IDictionary<string, object>>(where.ToString()));
                //orValue.Add(JsonConvert.DeserializeObject<IDictionary<string, object>>(where.ToString()));
            }

            return new TheOneQuery<T>(new TheOneQuery<T>(serviceHub.QueryService, serviceHub.InstallationService, serviceHub.ServerConnectionData, serviceHub.JsonSerializer, serviceHub.CurrentUserService.CurrentUser.sessionToken, className), where: new Dictionary<string, object> { ["$or"] = orValue });
        }

        /// <summary>
        /// Construct a query that is the nor of two queries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceHub"></param>
        /// <param name="queries">The list of TheOneQueries to 'nor' together.</param>
        /// <returns>A TheOneQquery that is the 'nor' of the passed in queries.</returns>
        public static TheOneQuery<T> ConstructNorQuery<T, TUser>(this IServiceHub<TUser> serviceHub, IEnumerable<TheOneQuery<T>> queries) where T : TheOneObject where TUser : TheOneUser
        {
            string className = default;
            List<IDictionary<string, object>> norValue = new List<IDictionary<string, object>> { };

            // We need to cast it to non-generic IEnumerable because of AOT-limitation

            IEnumerable nonGenericQueries = queries;
            foreach (object obj in nonGenericQueries)
            {
                TheOneQuery<T> query = obj as TheOneQuery<T>;

                if (className is { } && query.ClassName != className)
                {
                    throw new ArgumentException("All of the queries in an nor query must be on the same class.");
                }

                className = query.ClassName;
                IDictionary<string, object> parameters = query.BuildParameters();

                if (parameters.Count == 0)
                {
                    continue;
                }

                if (!parameters.TryGetValue("where", out object where) || parameters.Count > 1)
                {
                    throw new ArgumentException("None of the queries in an or query can have non-filtering clauses");
                }

                norValue.Add(serviceHub.JsonSerializer.Deserialize<IDictionary<string, object>>(where.ToString()));
            }

            return new TheOneQuery<T>(new TheOneQuery<T>(serviceHub.QueryService, serviceHub.InstallationService, serviceHub.ServerConnectionData, serviceHub.JsonSerializer, serviceHub.CurrentUserService.CurrentUser.sessionToken, className), where: new Dictionary<string, object> { ["$nor"] = norValue });
        }
    }

}
