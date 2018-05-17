﻿using System;
using System.IO;
using System.Web;
using System.Net;
using System.Collections.Generic;
using System.Reflection;
using Semantria.Com.Mapping.Configuration;
using Semantria.Com.Mapping.Configuration.Stub;
using Semantria.Com.Mapping.Output;
using Semantria.Com.Mapping.Output.Stub;
using Semantria.Com.Mapping;
using Semantria.Com.Mapping.Stub;
using Semantria.Com.Serializers;

namespace Semantria.Com
{
    public sealed class Session : IDisposable
    {
        #region Constructor
        
        private Session()
        {
        }
        
        private Session(string consumerKey, string consumerSecret, string appName) :
            this(consumerKey, consumerSecret, new JsonSerializer(), appName)
        {
        }

        private Session(string consumerKey, string consumerSecret, ISerializer serializer, string appName, bool usedAPIkeys = true)
        {
            if (usedAPIkeys)
            {
                _consumerKey = consumerKey;
                _consumerSecret = consumerSecret;
            }
            else
            {
                _username = consumerKey;
                _password = consumerSecret;
            }

            _serializer = serializer;
            _format = serializer.Type();
            _appName = appName;
        }

        private string GetAppName()
        {
            string appName = _appName;
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            if (!String.IsNullOrEmpty(appName))
            {
                appName = String.Format("{0}/{1}/{2}/{3}", appName, WRAPPER_NAME, version, _format.ToUpper());
            } 
            else 
            {
                appName = String.Format("{0}/{1}/{2}", WRAPPER_NAME, version, _format.ToUpper());
            }
            return appName;
        }

        #endregion Constructor

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region Private variables

        private string _consumerKey = string.Empty;
        private string _consumerSecret = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _appName = string.Empty;
        private ISerializer _serializer = null;
        private string _format = "json";
		private string _host = "https://api.semantria.com";
        private string _authHost = "https://semantria.com/auth";
        private string _authAppKey = "cd954253-acaf-4dfa-a417-0a8cfb701f12";
        private bool _useCompression = false;
        private string _apiVersion = "4.2";
        private const string WRAPPER_NAME = "dotNet";

        #endregion

        #region Properties

        /// <summary>
        /// API host URL (https://api.semantria.com used by default).
        /// </summary>
        public string Host
        {
            get { return _host; }
            set { _host = value; } 
        }

        /// <summary>
        /// Indicates whether to use HTTP compression or not.
        /// </summary>
        public bool UseCompression  
        {
            get { return _useCompression; }
            set { _useCompression = value; }
        }

        /// <summary>
        /// Forces Session to use specific API version (4.2 used by default).
        /// </summary>
        public string APIversion
        {
            get { return _apiVersion; }
            set { _apiVersion = value; }
        }

        internal bool ReUseSession
        {
            get;
            set;
        }

        #endregion

        #region Events

        public delegate void RequestHandler(object sender, RequestEventArgs ea);
        /// <summary>
        /// Occurs when request executed.
        /// </summary>
        public event RequestHandler Request;
        internal void OnRequest(object sender, RequestEventArgs ea)
        {
            if (Request != null)
            {
                Request(sender, ea);
            }
        }

        public delegate void ResponseHandler(object sender, ResponseEventArgs ea);  
        /// <summary>
        /// Occurs when response received.
        /// </summary>
        public event ResponseHandler Response;  
        internal void OnResponse(object sender, ResponseEventArgs ea)  
        {  
            if (Response != null)  
            {
                Response(sender, ea);  
            }  
        }

        public delegate void ErrorHandler(object sender, ResponseErrorEventArgs ea);
        /// <summary>
        /// Occurs when server-side error reported.
        /// </summary>
        public event ErrorHandler Error;
        internal void OnError(object sender, ResponseErrorEventArgs ea)
        {
            if (Error != null)
            {
                Error(sender, ea);
            }
            else
            {
                throw new System.Web.HttpException((int)ea.Status, ea.Message);
            }
        }

        public delegate void DocsAutoResponseHandler(object sender, DocsAutoResponseEventArgs ea);
        /// <summary>
        /// Occurs when the server return documents analysis data in the response of data queuing requests (auto-response feature).
        /// </summary>
        public event DocsAutoResponseHandler DocsAutoResponse;
        internal void OnDocsAutoResponse(object sender, DocsAutoResponseEventArgs ea)
        {
            if (DocsAutoResponse != null)
            {
                DocsAutoResponse(sender, ea);
            }
        }

        public delegate void CollsAutoResponseHandler(object sender, CollsAutoResponseEventArgs ea);
        /// <summary>
        /// Occurs when the server return collections analysis data in the response of data queuing requests (auto-response feature).
        /// </summary>
        public event CollsAutoResponseHandler CollsAutoResponse;
        internal void OnCollsAutoResponse(object sender, CollsAutoResponseEventArgs ea)
        {
            if (CollsAutoResponse != null)
            {
                CollsAutoResponse(sender, ea);
            }
        } 

        #endregion

        #region Public Static

        /// <summary>
        /// Creates new Semantria session using API keys.
        /// </summary>
        /// <param name="consumerKey">API key</param>
        /// <param name="consumerSecret">API secret</param>
        /// <returns>An instance of Session object.</returns>
        public static Session CreateSession(string consumerKey, string consumerSecret)
        {
            return CreateSession(consumerKey, consumerSecret, string.Empty);
        }

        /// <summary>
        /// Creates new Semantria session using API keys.
        /// </summary>
        /// <param name="consumerKey">API key</param>
        /// <param name="consumerSecret">API secret</param>
        /// <param name="appName">Name of the current application to be reported to the server.</param>
        /// <returns>An instance of Session object.</returns>
        public static Session CreateSession(string consumerKey, string consumerSecret, string appName)
        {
            return new Session(consumerKey, consumerSecret, appName);
        }

        /// <summary>
        /// Creates new Semantria session using API keys.
        /// </summary>
        /// <param name="consumerKey">API key</param>
        /// <param name="consumerSecret">API secret</param>
        /// <param name="serializer">Either XML or JSON serializer to use the specific data format while access the API.</param>
        /// <returns>An instance of Session object.</returns>
        public static Session CreateSession(string consumerKey, string consumerSecret, ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            return CreateSession(consumerKey, consumerSecret, serializer, string.Empty);
        }

        /// <summary>
        /// Creates new Semantria session using API keys
        /// </summary>
        /// <param name="consumerKey">API key</param>
        /// <param name="consumerSecret">API secret</param>
        /// <param name="serializer">Either XML or JSON serializer to use the specific data format while access the API.</param>
        /// <param name="appName">Name of the current application to be reported to the server.</param>
        /// <returns>An instance of Session object.</returns>
        public static Session CreateSession(string consumerKey, string consumerSecret, ISerializer serializer, string appName)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            return new Session(consumerKey, consumerSecret, serializer, appName);
        }

        /// <summary>
        /// Creates Semantria session using user's credentials.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="reUseSession">Indicates whether to re-use previously created session or create a new one.</param>
        /// <returns>An instance of Session object.</returns>
        public static Session CreateUserSession(string username, string password, bool reUseSession = true)
        {
            return new Session(username, password, new JsonSerializer(), null, false) { ReUseSession = reUseSession };
        }

        /// <summary>
        /// Creates Semantria session using user's credentials.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="appName">Name of the current application to be reported to the server.</param>
        /// <param name="reUseSession">Indicates whether to re-use previously created session or create a new one.</param>
        /// <returns>An instance of Session object.</returns>
        public static Session CreateUserSession(string username, string password, string appName, bool reUseSession = true)
        {
            return new Session(username, password, new JsonSerializer(), appName, false) { ReUseSession = reUseSession };
        }

        /// <summary>
        /// Creates Semantria session using user's credentials.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="serializer">Either XML or JSON serializer to use the specific data format while access the API.</param>
        /// <param name="reUseSession">Indicates whether to re-use previously created session or create a new one.</param>
        /// <returns>An instance of Session object.</returns>
        public static Session CreateUserSession(string username, string password, ISerializer serializer, bool reUseSession = true)
        {
            return new Session(username, password, serializer, null, false) { ReUseSession = reUseSession };
        }

        /// <summary>
        /// Creates Semantria session using user's credentials.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="appName">Name of the current application to be reported to the server.</param>
        /// <param name="serializer">Either XML or JSON serializer to use the specific data format while access the API.</param>
        /// <param name="reUseSession">Indicates whether to re-use previously created session or create a new one.</param>
        /// <returns>An instance of Session object.</returns>
        public static Session CreateUserSession(string username, string password, string appName, ISerializer serializer, bool reUseSession = true)
        {
            return new Session(username, password, new JsonSerializer(), appName, false) { ReUseSession = reUseSession };
        }

        #endregion

        #region API Status

        /// <summary>
        /// Registers serializer for already instantiated session to use another data format if necessary.
        /// </summary>
        /// <param name="serializer">Either XML or JSON serializer to use the specific data format while access the API.</param>
        public void RegisterSerializer(ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            
            _serializer = serializer;
            _format = serializer.Type();
        }

        /// <summary>
        /// Retrieves Semantria API status.
        /// </summary>
        /// <returns>Status object with a bunch of fields related to the API status.</returns>
        public Status GetStatus()
        {
            //GET https://api.semantria.com/status.json
            string url = String.Format("{0}/status.{1}", _host, _format);
            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            return ProcessGetResponse<Status>(authResponse);
        }

        /// <summary>
        /// Retrieves Semantria Subscription.
        /// </summary>
        /// <returns>Subscription object with a bunch of subscription related fields.</returns>
        public Subscription GetSubscription()
        {
            //GET https://api.semantria.com/subscription.json
            string url = String.Format("{0}/subscription.{1}", _host, _format);
            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            return ProcessGetResponse<Subscription>(authResponse);
        }

        /// <summary>
        /// Retrieves usage statistics for the given interval.
        /// </summary>
        /// <param name="interval">Time interval.</param>
        /// <returns>Statistics object with a bunch of fields.</returns>
        public StatisticsOverall GetStatistics(StatsInterval interval)
        {
            //GET https://api.semantria.com/statistics.json
            string url = String.Format("{0}/statistics.{1}", _host, _format);

            List<string> query_params = new List<string>();
            if (interval != StatsInterval.undefined)
            {
                query_params.Add(String.Format("interval={0}", interval.ToString()));
            }

            if (query_params.Count != 0)
            {
                url += "?" + String.Join("&", query_params.ToArray());
            }
            
            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            List<StatisticsOverall> stats = ProcessGetResponse<List<StatisticsOverall>>(authResponse);
            if (stats != null && stats.Count > 0)
            {
                return stats[0];
            }

            return null;
        }

        /// <summary>
        /// Retrieves usage statistics for the given time interval.
        /// </summary>
        /// <param name="from">Start DateTime point to get usage statistics for.</param>
        /// <param name="to">End DateTime point to get usage statistics for.</param>
        /// <returns>Statistics object with a bunch of fields.</returns>
        public StatisticsOverall GetStatistics(DateTime from, DateTime to)
        {
            //GET https://api.semantria.com/statistics.json
            string url = String.Format("{0}/statistics.{1}", _host, _format);

            if ((from - to).TotalSeconds != 0)
            {
                string s_from = from.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                string s_to = to.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                url = String.Format("{0}/statistics.{1}?from={2}&to={3}", _host, _format, s_from, s_to);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Interval between from an to is null.");
            }

            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            List<StatisticsOverall> stats = ProcessGetResponse<List<StatisticsOverall>>(authResponse);
            if (stats != null && stats.Count > 0)
            {
                return stats[0];
            }

            return null;
        }

        /// <summary>
        /// Retrieves usage statistics for the given time interval grouped by the given criteria.
        /// </summary>
        /// <param name="interval">Time interval.</param>
        /// <param name="groupBy">config_id, config_name, user_id, user_email, language, app and time values or their combinations supported.
        /// If several parameters use, they should be separated by, like "language,user_name".
        /// Time value can't be less than one minute - 1m. Supported following time units: m - minutes, h - hours, d - days, w - weeks.
        /// Combining time values and grouping parameters allowed: "language,user_name,10m"</param>
        /// <returns>List of statistics objects with a bunch of fields.</returns>
        public List<StatisticsGrouped> GetStatistics(StatsInterval interval, string groupBy = "app")
        {
            if (String.IsNullOrEmpty(groupBy))
            {
                throw new ArgumentNullException("groupBy");
            }

            //GET https://api.semantria.com/statistics.json
            string url = String.Format("{0}/statistics.{1}", _host, _format);

            List<string> query_params = new List<string>();

            if (interval != StatsInterval.undefined)
            {
                query_params.Add(String.Format("interval={0}", interval.ToString()));
            }
            
            if (!String.IsNullOrEmpty(groupBy))
            {
                query_params.Add(String.Format("group={0}", groupBy));
            }

            if (query_params.Count != 0)
            {
                url += "?" + String.Join("&", query_params.ToArray());
            }

            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            return ProcessGetResponse<List<StatisticsGrouped>>(authResponse);
        }

        /// <summary>
        /// Retrieves usage statistics for the given time interval grouped by the given criteria.
        /// </summary>
        /// <param name="from">Start DateTime point to get usage statistics for.</param>
        /// <param name="to">End DateTime point to get usage statistics for.</param>
        /// <param name="groupBy">config_id, config_name, user_id, user_email, language, app and time values or their combinations supported.
        /// If several parameters use, they should be separated by, like "language,user_name".
        /// Time value can't be less than one minute - 1m. Supported following time units: m - minutes, h - hours, d - days, w - weeks.
        /// Combining time values and grouping parameters allowed: "language,user_name,10m"</param>
        /// <returns>List of statistics objects with a bunch of fields.</returns>
        public List<StatisticsGrouped> GetStatistics(DateTime from, DateTime to, string groupBy = "app")
        {
            //GET https://api.semantria.com/statistics.json
            string url = String.Format("{0}/statistics.{1}", _host, _format);

            List<string> query_params = new List<string>();

            if ((from - to).TotalSeconds != 0)
            {
                string s_from = from.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                string s_to = to.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                query_params.Add(String.Format("from={0}", s_from));
                query_params.Add(String.Format("to={0}", s_to));
            }
            else
            {
                throw new ArgumentOutOfRangeException("Interval between from an to is null.");
            }

            if (!String.IsNullOrEmpty(groupBy))
            {
                query_params.Add(String.Format("group={0}", groupBy));
            }

            if (query_params.Count != 0)
            {
                url += "?" + String.Join("&", query_params.ToArray());
            }

            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            return ProcessGetResponse<List<StatisticsGrouped>>(authResponse);
        }

        /// <summary>
        /// Retrieves the list of supported features grouped by languages.
        /// </summary>
        /// <param name="language">Language name to get supported features for the certain language.</param>
        /// <returns>The list of supported features for each language offered by Semantria.</returns>
        public IList<FeaturesSet> GetSupportedFeatures(string language = null)
        {
            //GET https://api.semantria.com/features.json
            string url = String.Format("{0}/features.{1}", _host, _format);

            if (!String.IsNullOrEmpty(language))
            {
                url = String.Format("{0}/features.{1}?language={2}", _host, _format, language);
            }

            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            List<FeaturesSet> obj = new List<FeaturesSet>();

            switch (_format)
            {
                case "json":
                    {
                        List<FeaturesSet> result = ProcessGetResponse<List<FeaturesSet>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        FeaturesSetList result = ProcessGetResponse<FeaturesSetList>(authResponse);
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Retrieves the list of configurations from the server.
        /// </summary>
        /// <returns>The list of configurations.</returns>
        public List<Configuration> GetConfigurations()
        {
            AuthResponse authResponse = Get<Configuration>();
            List<Configuration> obj = new List<Configuration>();

            switch (_format)
            {
                case "json":
                    {
                        List<Configuration> result = ProcessGetResponse<List<Configuration>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        Configurations result = ProcessGetResponse<Configurations>(authResponse);
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        /// <summary>
        /// Adds configurations to the server.
        /// </summary>
        /// <param name="configs">The list of configurations to be added to the server.</param>
        /// <returns>The list of just added configurations (with auto-generated IDs, modified timestamps, etc.)</returns>
        public List<Configuration> AddConfigurations(List<Configuration> configs)
        {
            return Add<Configuration>(QueryMethod.POST, configs);
        }

        /// <summary>
        /// Updates configurations on the server side.
        /// </summary>
        /// <param name="configs">The list of configurations to be updated on the server.</param>
        /// <returns>The list of just updated configurations.</returns>
        public List<Configuration> UpdateConfigurations(List<Configuration> configs)
        {
            return Update<Configuration>(QueryMethod.PUT, configs);
        }

        /// <summary>
        /// Removes configurations from the server.
        /// </summary>
        /// <param name="configs">The list of configuration IDs to be removed.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int RemoveConfigurations(List<string> configs)
        {
            return Delete<Configuration>(QueryMethod.DELETE, configs);
        }

        /// <summary>
        /// Clons configuration from the given template and entitles to the given name.
        /// </summary>
        /// <param name="name">New configuration name.</param>
        /// <param name="template">Configuration ID to be cloned and used as template.</param>
        /// <returns>New configuration just created using the given template.</returns>
        public Configuration CloneConfiguration(string name, string template)
        {
            var items = new List<Configuration>();
            items.Add(new Configuration() { Name = name, Template = template });

            List<Configuration> configs = Add<Configuration>(QueryMethod.POST, items);
            if (configs != null && configs.Count > 0)
                return configs[0];

            return null;
        }

        #endregion Configuration

        #region Blacklist

        /// <summary>
        /// Retrieves the list of blacklisted items from the server.
        /// </summary>
        /// <param name="configId">Optional configuration ID the blacklist is associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of Blacklisted items.</returns>
        public List<BlacklistedItem> GetBlacklist(string configId = null)
        {
            AuthResponse authResponse = Get<BlacklistedItem>(configId);
            List<BlacklistedItem> obj = new List<BlacklistedItem>();

            switch (_format)
            {
                case "json":
                    {
                        List<BlacklistedItem> result = ProcessGetResponse<List<BlacklistedItem>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        Blacklists result = ProcessGetResponse<Blacklists>(authResponse);
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        /// <summary>
        /// Adds blacklisted items to the server.
        /// </summary>
        /// <param name="blacklist">The list of blacklisted items to be added to the server.</param>
        /// <param name="configId">Optional configuration ID the blacklist is associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just added blacklisted items (with auto-generated IDs, modified timestamps, etc.)</returns>
        public List<BlacklistedItem> AddBlacklist(List<BlacklistedItem> blacklist, string configId = null)
        {
            return Add<BlacklistedItem>(QueryMethod.POST, blacklist, configId);
        }

        /// <summary>
        /// Updates blacklisted items on the server.
        /// </summary>
        /// <param name="blacklist">The list of blacklisted items to be updated on the server.</param>
        /// <param name="configId">Optional configuration ID the blacklist is associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just updated blacklisted items.</returns>
        public List<BlacklistedItem> UpdateBlacklist(List<BlacklistedItem> blacklist, string configId = null)
        {
            return Update<BlacklistedItem>(QueryMethod.PUT, blacklist, configId);
        }

        /// <summary>
        /// Removes blacklisted items from the server.
        /// </summary>
        /// <param name="blacklist">The list of blacklisted items to be removed from the server.</param>
        /// <param name="configId">Optional configuration ID the blacklist is associated with. If not provided, primary configuration will be used.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int RemoveBlacklist(List<string> blacklist, string configId = null)
        {
            return Delete<BlacklistedItem>(QueryMethod.DELETE, blacklist, configId);
        }

        #endregion Blacklist

        #region Category

        /// <summary>
        /// Retrieves categories from the server.
        /// </summary>
        /// <param name="configId">Optional configuration ID the categories are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of categories.</returns>
        public List<Category> GetCategories(string configId = null)
        {
            AuthResponse authResponse = Get<Category>(configId);
            List<Category> obj = new List<Category>();

            switch (_format)
            {
                case "json":
                    {
                        List<Category> result = ProcessGetResponse<List<Category>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        Categories result = ProcessGetResponse<Categories>(authResponse);
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        /// <summary>
        /// Adds categories to the server.
        /// </summary>
        /// <param name="categories">The list of categories to be added to the server.</param>
        /// <param name="configId">Optional configuration ID the categories are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just added categories (with auto-generated IDs, modified timestamps, etc.)</returns>
        public List<Category> AddCategories(List<Category> categories, string configId = null)
        {
            return Add<Category>(QueryMethod.POST, categories, configId);
        }

        /// <summary>
        /// Updates categories on the server.
        /// </summary>
        /// <param name="categories">The list of categories to be updated on the server.</param>
        /// <param name="configId">Optional configuration ID the categories are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just updated categories.</returns>
        public List<Category> UpdateCategories(List<Category> categories, string configId = null)
        {
            return Update<Category>(QueryMethod.PUT, categories, configId);
        }

        /// <summary>
        /// Removes categories from the server.
        /// </summary>
        /// <param name="categories">The list of categories to be removed from the server.</param>
        /// <param name="configId">Optional configuration ID the categories are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int RemoveCategories(List<string> categories, string configId = null)
        {
            return Delete<Category>(QueryMethod.DELETE, categories, configId);
        }

        #endregion Category

        #region Query

        /// <summary>
        /// Retrieves queries from the server.
        /// </summary>
        /// <param name="configId">Optional configuration ID the queries are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of queries.</returns>
        public List<Query> GetQueries(string configId = null)
        {
            AuthResponse authResponse = Get<Query>(configId);
            List<Query> obj = new List<Query>();

            switch (_format)
            {
                case "json":
                    {
                        List<Query> result = ProcessGetResponse<List<Query>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        Queries result = ProcessGetResponse<Queries>(authResponse);
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        /// <summary>
        /// Add queries to the server.
        /// </summary>
        /// <param name="queries">The list of queries to be added to the server.</param>
        /// <param name="configId">Optional configuration ID the queries are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just added queries (with auto-generated IDs, modified timestamps, etc.)</returns>
        public List<Query> AddQueries(List<Query> queries, string configId = null)
        {
            return Add<Query>(QueryMethod.POST, queries, configId);
        }

        /// <summary>
        /// Updates queries on the server.
        /// </summary>
        /// <param name="queries">The list of queries to be updated on the server.</param>
        /// <param name="configId">Optional configuration ID the queries are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just updated queries.</returns>
        public List<Query> UpdateQueries(List<Query> queries, string configId = null)
        {
            return Update<Query>(QueryMethod.PUT, queries, configId);
        }

        /// <summary>
        /// Removes queries from the server.
        /// </summary>
        /// <param name="queries">The list of queries to be removed from the server.</param>
        /// <param name="configId">Optional configuration ID the queries are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int RemoveQueries(List<string> queries, string configId = null)
        {
            return Delete<Query>(QueryMethod.DELETE, queries, configId);
        }

        #endregion Query

        #region Entity

        /// <summary>
        /// Retrieves entities from the server.
        /// </summary>
        /// <param name="configId">Optional configuration ID the entities are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of entities.</returns>
        public List<UserEntity> GetEntities(string configId = null)
        {
            AuthResponse authResponse = Get<UserEntity>(configId);
            List<UserEntity> obj = new List<UserEntity>();

            switch (_format)
            {
                case "json":
                    {
                        List<UserEntity> result = ProcessGetResponse<List<UserEntity>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        UserEntities result = ProcessGetResponse<UserEntities>(authResponse);
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        /// <summary>
        /// Adds entities to the server.
        /// </summary>
        /// <param name="entities">The list of entities to be added to the server.</param>
        /// <param name="configId">Optional configuration ID the entities are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just added entities (with auto-generated IDs, modified timestamps, etc.)</returns>
        public List<UserEntity> AddEntities(List<UserEntity> entities, string configId = null)
        {
            return Add<UserEntity>(QueryMethod.POST, entities, configId);
        }

        /// <summary>
        /// Updates entities on the server.
        /// </summary>
        /// <param name="entities">The list of entities to be updated on the server.</param>
        /// <param name="configId">Optional configuration ID the entities are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just updated entities.</returns>
        public List<UserEntity> UpdateEntities(List<UserEntity> entities, string configId = null)
        {
            return Update<UserEntity>(QueryMethod.PUT, entities, configId);
        }

        /// <summary>
        /// Removes entities from the server.
        /// </summary>
        /// <param name="entities">The list of entities to be removed from the server.</param>
        /// <param name="configId">Optional configuration ID the entities are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int RemoveEntities(List<string> entities, string configId = null)
        {
            return Delete<UserEntity>(QueryMethod.DELETE, entities, configId);
        }

        #endregion Entity

        #region SentimentPhrase

        /// <summary>
        /// Retrieves sentiment-bearing phrases from the server.
        /// </summary>
        /// <param name="configId">Optional configuration ID the phrases are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of sentiment-bearing phrases.</returns>
        public List<SentimentPhrase> GetSentimentPhrases(string configId = null)
        {
            AuthResponse authResponse = Get<SentimentPhrase>(configId);
            List<SentimentPhrase> obj = new List<SentimentPhrase>();

            switch (_format)
            {
                case "json":
                    {
                        List<SentimentPhrase> result = ProcessGetResponse<List<SentimentPhrase>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        SentimentPhrases result = ProcessGetResponse<SentimentPhrases>(authResponse);
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        /// <summary>
        /// Adds sentiment-bearing phrases to the server.
        /// </summary>
        /// <param name="phrases">The list of sentiment-bearing phrases to be added to the server.</param>
        /// <param name="configId">Optional configuration ID the sentiment-bearing phrases are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just added sentiment-bearing phrases (with auto-generated IDs, modified timestamps, etc.)</returns>
        public List<SentimentPhrase> AddSentimentPhrases(List<SentimentPhrase> phrases, string configId = null)
        {
            return Add<SentimentPhrase>(QueryMethod.POST, phrases, configId);
        }

        /// <summary>
        /// Updates sentiment-bearing phrases on the server.
        /// </summary>
        /// <param name="phrases">The list of sentiment-bearing phrases to be updated on the server.</param>
        /// <param name="configId">Optional configuration ID the sentiment-bearing phrases are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just updated sentiment-bearing phrases.</returns>
        public List<SentimentPhrase> UpdateSentimentPhrases(List<SentimentPhrase> phrases, string configId = null)
        {
            return Update<SentimentPhrase>(QueryMethod.PUT, phrases, configId);
        }

        /// <summary>
        /// Removes sentiment-bearing phrases from the server.
        /// </summary>
        /// <param name="phrases">The list of sentiment-bearing phrases to be removed from the server.</param>
        /// <param name="configId">Optional configuration ID the sentiment-bearing phrases are associated with. If not provided, primary configuration will be used.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int RemoveSentimentPhrases(List<string> phrases, string configId = null)
        {
            return Delete<SentimentPhrase>(QueryMethod.DELETE, phrases, configId);
        }

        #endregion SentimentPhrase

        #region Taxonomy

        /// <summary>
        /// Retrieves taxonomy from the server.
        /// </summary>
        /// <param name="configId">Optional configuration ID the taxonomy is associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of taxonomy nodes.</returns>
        public List<TaxonomyNode> GetTaxonomy(string configId = null)
        {
            AuthResponse authResponse = Get<TaxonomyNode>(configId);
            List<TaxonomyNode> obj = new List<TaxonomyNode>();

            switch (_format)
            {
                case "json":
                    {
                        List<TaxonomyNode> result = ProcessGetResponse<List<TaxonomyNode>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        Taxonomies result = ProcessGetResponse<Taxonomies>(authResponse);
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        /// <summary>
        /// Adds taxonomy nodes to the server.
        /// </summary>
        /// <param name="nodes">The list of taxonomy nodes to be added to the server.</param>
        /// <param name="configId">Optional configuration ID the taxonomy is associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just added taxonomy nodes (with auto-generated IDs, modified timestamps, etc.)</returns>
        public List<TaxonomyNode> AddTaxonomy(List<TaxonomyNode> nodes, string configId = null)
        {
            return Add<TaxonomyNode>(QueryMethod.POST, nodes, configId);
        }

        /// <summary>
        /// Updates taxonomy nodes on the server.
        /// </summary>
        /// <param name="nodes">The list of taxonomy nodes to be updated on the server.</param>
        /// <param name="configId">Optional configuration ID the taxonomy is associated with. If not provided, primary configuration will be used.</param>
        /// <returns>The list of just updated taxonomy nodes.</returns>
        public List<TaxonomyNode> UpdateTaxonomy(List<TaxonomyNode> nodes, string configId = null)
        {
            return Update<TaxonomyNode>(QueryMethod.PUT, nodes, configId);
        }

        /// <summary>
        /// Removes taxonomy nodes from the server.
        /// </summary>
        /// <param name="nodes">The list of taxonomy nodes to be removed from the server.</param>
        /// <param name="configId">Optional configuration ID the taxonomy is associated with. If not provided, primary configuration will be used.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int RemoveTaxonomy(List<string> nodes, string configId = null)
        {
            return Delete<TaxonomyNode>(QueryMethod.DELETE, nodes, configId);
        }

        #endregion Taxonomy

        #region Document

        /// <summary>
        /// Queues the document for analysis using given configuration.
        /// </summary>
        /// <param name="document">Document to be analyzed.</param>
        /// <param name="configId">Optional configuration ID. If not provided, primary configuration will be used for analysis.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int QueueDocument(Document document, string configId = null)
        {
            string url = String.Format("{0}/document.{1}", _host, _format);
            if (!String.IsNullOrEmpty(configId))
            {
                url = String.Format("{0}/document.{1}?config_id={2}", _host, _format, configId);
            }
            string data = _serializer.Serialize(document);
            IList<DocAnalyticData> result = PostAnalyticData<DocAnalyticData>(url, data);

            if (result != null)
            {
                if (result.Count > 0)
                {
                    OnDocsAutoResponse(this, new DocsAutoResponseEventArgs(result));
                    return 200;
                }
                else
                {
                    return 202;
                }
            }
            return -1;
        }

        /// <summary>
        /// Queues a batch of documents for analysis using given given configuration.
        /// </summary>
        /// <param name="batch">Batch of documents to be analyzed.</param>
        /// <param name="configId">Optional configuration ID. If not provided, primary configuration will be used for analysis.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int QueueBatchOfDocuments(IList<Document> batch, string configId = null)
        {
            string url = String.Format("{0}/document/batch.{1}", _host, _format);
            if (!String.IsNullOrEmpty(configId))
            {
                url = String.Format("{0}/document/batch.{1}?config_id={2}", _host, _format, configId);
            }

            List<Document> list = new List<Document>();
            list.AddRange(batch);

            IList<DocAnalyticData> result = PostQueueBatch(url, list);

            if (result != null)
            {
                if (result.Count > 0)
                {
                    OnDocsAutoResponse(this, new DocsAutoResponseEventArgs(result));
                    return 200;
                }
                else
                {
                    return 202;
                }
            }
            return -1;
        }

        /// <summary>
        /// Retrieves docuemnt analysis results by the certain document/configuration ID from the server.
        /// </summary>
        /// <param name="id">Document ID to retrieve the anlaysis results.</param>
        /// <param name="configId">Optional configuration ID. If not provided, primary configuration will be used.</param>
        /// <returns>Docuemnt analysis results object with a bunch of result fields.</returns>
        public DocAnalyticData GetDocument(string id, string configId = null)
        {
            string encodedId = HttpUtility.UrlEncode(id);
            string url = String.Format("{0}/document/{1}.{2}", _host, encodedId, _format);
            if (!String.IsNullOrEmpty(configId))
            {
                url = String.Format("{0}/document/{1}.{2}?config_id={3}", _host, encodedId, _format, configId);
            }
            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            return ProcessGetResponse<DocAnalyticData>(authResponse);
        }

        /// <summary>
        /// Cancels specific document on the server side.
        /// </summary>
        /// <param name="id">Document ID to be canceld.</param>
        /// <param name="configId">Optional configuration ID. If not provided, primary configuration will be used.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int CancelDocument(string id, string configId = null)
        {
            string encodedId = HttpUtility.UrlEncode(id);
            string url = String.Format("{0}/document/{1}.{2}", _host, encodedId, _format);
            if (!String.IsNullOrEmpty(configId))
            {
                url = String.Format("{0}/document/{1}.{2}?config_id={3}", _host, encodedId, _format, configId);
            }
            AuthResponse authResponse = RunRequest(QueryMethod.DELETE, url, null);
            return ProcessPostResponse<Document>(authResponse);
        }

        /// <summary>
        /// Retrieves document analysis results from the server by the given configuration.
        /// </summary>
        /// <param name="configId">Optional configuration ID. If not provided, primary configuration will be used.</param>
        /// <returns>The list of document analysis results retrieved from the server for recently queued documents.</returns>
        public IList<DocAnalyticData> GetProcessedDocuments(string configId = null)
        {
            string url = String.Format("{0}/document/processed.{1}", _host, _format);
            if (!String.IsNullOrEmpty(configId))
            {
                url = String.Format("{0}/document/processed.{1}?config_id={2}", _host, _format, configId);
            }
            IList<DocAnalyticData> result = RequestProcessedDocuments(url);
            return result;
        }

        /// <summary>
        /// Retrieves document analysis results by the given Job identifier.
        /// </summary>
        /// <param name="jobId">Unique Job identifier used while documents queuing.</param>
        /// <returns>The list of document analysis results retrieved from the server for the given Job ID.</returns>
        public IList<DocAnalyticData> GetProcessedDocumentsByJobId(string jobId)
        {
            if (String.IsNullOrEmpty(jobId))
            {
                throw new ArgumentNullException("jobId", "jobId parameter can not be null or string empty.");
            }

            string url = String.Format("{0}/document/processed.{1}?job_id={2}", _host, _format, jobId);
            IList<DocAnalyticData> result = RequestProcessedDocuments(url);
            return result;
        }

        private List<DocAnalyticData> RequestProcessedDocuments(string url)
        {
            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            List<DocAnalyticData> obj = new List<DocAnalyticData>();

            switch (_format)
            {
                case "json":
                    {
                        List<DocAnalyticData> result = ProcessGetResponse<List<DocAnalyticData>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        DocAnalyticsData result = ProcessGetResponse<DocAnalyticsData>(authResponse);
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        #endregion Document

        #region Collection

        /// <summary>
        /// Queues the collection for analysis using given configuration.
        /// </summary>
        /// <param name="collection">Collection to be analyzed.</param>
        /// <param name="configId">Optional configuration ID. If not provided, primary configuration will be used for analysis.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int QueueCollection(Collection collection, string configId = null)
        {
            string url = String.Format("{0}/collection.{1}", _host, _format);
            if (!String.IsNullOrEmpty(configId))
            {
                url = String.Format("{0}/collection.{1}?config_id={2}", _host, _format, configId);
            }

            string data = _serializer.Serialize(collection);
            IList<CollAnalyticData> result = PostAnalyticData<CollAnalyticData>(url, data);

            if (result != null)
            {
                if (result.Count > 0)
                {
                    OnCollsAutoResponse(this, new CollsAutoResponseEventArgs(result));
                    return 200;
                }
                else
                {
                    return 202;
                }
            }
            return -1;
        }

        /// <summary>
        /// Retrieves collection analysis results by the certain collection/configuration ID from the server.
        /// </summary>
        /// <param name="id">Document ID to retrieve the anlaysis results.</param>
        /// <param name="configId">Optional configuration ID. If not provided, primary configuration will be used for analysis.</param>
        /// <returns>Collection analysis results object with a bunch of result fields.</returns>
        public CollAnalyticData GetCollection(string id, string configId = null)
        {
            string encodedId = HttpUtility.UrlEncode(id);
            string url = String.Format("{0}/collection/{1}.{2}", _host, encodedId, _format);
            if (!String.IsNullOrEmpty(configId))
            {
                url = String.Format("{0}/collection/{1}.{2}?config_id={3}", _host, encodedId, _format, configId);
            }
            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            return ProcessGetResponse<CollAnalyticData>(authResponse);
        }

        /// <summary>
        /// Cancels specific collection on the server side.
        /// </summary>
        /// <param name="id">Collection ID to be canceled.</param>
        /// <param name="configId">Optional configuration ID. If not provided, primary configuration will be used for analysis.</param>
        /// <returns>Operation execution result, actually HTTP status code.</returns>
        public int CancelCollection(string id, string configId = null)
        {
            string encodedId = HttpUtility.UrlEncode(id);
            string url = String.Format("{0}/collection/{1}.{2}", _host, encodedId, _format);
            if (!String.IsNullOrEmpty(configId))
            {
                url = String.Format("{0}/collection/{1}.{2}?config_id={3}", _host, encodedId, _format, configId);
            }
            AuthResponse authResponse = RunRequest(QueryMethod.DELETE, url, null);
            return ProcessPostResponse<Collection>(authResponse);
        }

        /// <summary>
        /// Retrieves collection analysis results from the server by the given configuration.
        /// </summary>
        /// <param name="configId">Optional configuration ID. If not provided, primary configuration will be used.</param>
        /// <returns>The list of collection analysis results retrieved from the server for recently queued collections.</returns>
        public IList<CollAnalyticData> GetProcessedCollections(string configId = null)
        {
            string url = String.Format("{0}/collection/processed.{1}", _host, _format);
            if (!String.IsNullOrEmpty(configId))
            {
                url = String.Format("{0}/collection/processed.{1}?config_id={2}", _host, _format, configId);
            }
            IList<CollAnalyticData> result = RequestProcessedCollections(url);
            return result;
        }

        /// <summary>
        /// Retrieves collection analysis results by the given Job identifier.
        /// </summary>
        /// <param name="jobId">Unique Job identifier used while collections queuing.</param>
        /// <returns>The list of collection analysis results retrieved from the server for the given Job ID.</returns>
        public IList<CollAnalyticData> GetProcessedCollectionsByJobId(string jobId)
        {
            if (String.IsNullOrEmpty(jobId))
            {
                throw new ArgumentNullException("jobId", "jobId parameter can not be null or string empty.");
            }

            string url = String.Format("{0}/collection/processed.{1}?job_id={2}", _host, _format, jobId);
            IList<CollAnalyticData> result = RequestProcessedCollections(url);
            return result;
        }

        private List<CollAnalyticData> RequestProcessedCollections(string url)
        {
            AuthResponse authResponse = RunRequest(QueryMethod.GET, url, null);
            List<CollAnalyticData> obj = new List<CollAnalyticData>();

            switch (_format)
            {
                case "json":
                    {
                        List<CollAnalyticData> result = ProcessGetResponse<List<CollAnalyticData>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        CollAnalyticsData result = ProcessGetResponse<CollAnalyticsData>(authResponse);
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        #endregion Collection

        #region Salience user directory

        public static readonly string ZIP_FORMAT = "zip";
        public static readonly string TAR_FORMAT = "tar";
        public static readonly string TARGZ_FORMAT = "tar.gz";
        public static readonly List<string> archiveFormats = new List<string>() {
                ZIP_FORMAT, TAR_FORMAT, TARGZ_FORMAT, 
        };

        private string getArchiveFormat(string path) {
            foreach (string format in archiveFormats) {
                if (path.EndsWith("." + format)) {
                    return format;
                }
            }
            return ZIP_FORMAT;
        }

        public byte[] GetUserDirectory(string configId) {
            return GetUserDirectory(configId, ZIP_FORMAT);
        }

        public byte[] GetUserDirectory(string configId, string format) {
            AuthResponse authResponse = GetBinary("salience/user-directory." + format, configId);
            return authResponse.BinaryData;
        }

        public void WriteUserDirectoryToFile(string configId, string path) {
             byte[] bytes = GetUserDirectory(configId, getArchiveFormat(path));
             if (bytes == null) {
                 throw new SystemException(String.Format(
                     "Can't get user directory for config {0}. Check error handler for specific error message",
                     configId));
             }
            WriteByteArrayToFile(path, bytes);
        }

        private static void WriteByteArrayToFile(string fileName, byte[] byteArray)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
            }
        }

        #endregion Salience user directory

        #region Private request and response support

        private string makeUrl(string tag, string configId, bool omitFormat=false) {
            string url = String.Format("{0}/{1}", _host, tag);
            if (!omitFormat)
            {
                url += "." + _format;
            }
            if (!String.IsNullOrEmpty(configId))
            {
                url += String.Format("?config_id={0}", configId);
            }
            return url;
        }


        private AuthResponse Get<T>(string configId = null)
        {
            string url = makeUrl(GetTag<T>(), configId);
            return RunRequest(QueryMethod.GET, url, null);
        }

        private AuthResponse GetBinary(string tag, string configId = null)
        {
            string url = makeUrl(tag, configId, true);
            return RunRequest(QueryMethod.GET, url, null, true);
        }

        private List<T> Add<T>(QueryMethod method, List<T> items, string configId = null)
        {
            string url = makeUrl(GetTag<T>(), configId);
            string data = null;
            object stub = items;

            if (_format == "xml")
            {
                stub = GetStub<T>(items);
            }

            data = _serializer.Serialize(stub);

            AuthResponse authResponse = RunRequest(method, url, data);
            List<T> obj = new List<T>();

            switch (_format)
            {
                case "json":
                    {
                        List<T> result = ProcessGetResponse<List<T>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        IStub<T> result = null;
                        
                        if (typeof(T).Equals(typeof(Configuration)))
                            result = (IStub<T>)ProcessGetResponse<Configurations > (authResponse);
                        else if (typeof(T).Equals(typeof(Category)))
                            result = (IStub<T>)ProcessGetResponse<Categories>(authResponse);
                        else if (typeof(T).Equals(typeof(BlacklistedItem)))
                            result = (IStub<T>)ProcessGetResponse<Blacklists>(authResponse);
                        else if (typeof(T).Equals(typeof(Query)))
                            result = (IStub<T>)ProcessGetResponse<Queries>(authResponse);
                        else if (typeof(T).Equals(typeof(UserEntity)))
                            result = (IStub<T>)ProcessGetResponse<UserEntities>(authResponse);
                        else if (typeof(T).Equals(typeof(SentimentPhrase)))
                            result = (IStub<T>)ProcessGetResponse<SentimentPhrases>(authResponse);
                        else if (typeof(T).Equals(typeof(TaxonomyNode)))
                            result = (IStub<T>)ProcessGetResponse<Taxonomies>(authResponse);
                        else
                            throw new ArgumentOutOfRangeException();
                        
                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        private List<T> Update<T>(QueryMethod method, List<T> items, string configId = null)
        {
            string url = makeUrl(GetTag<T>(), configId);
            object stub = items;

            if (_format == "xml")
            {
                stub = GetStub<T>(items);
            }

            string data = _serializer.Serialize(stub);
            
            AuthResponse authResponse = RunRequest(method, url, data);
            List<T> obj = new List<T>();

            switch (_format)
            {
                case "json":
                    {
                        List<T> result = ProcessGetResponse<List<T>>(authResponse);
                        if (result != null) obj = result;
                    }
                    break;
                case "xml":
                    {
                        IStub<T> result = null;

                        if (typeof(T).Equals(typeof(Configuration)))
                            result = (IStub<T>)ProcessGetResponse<Configurations>(authResponse);
                        else if (typeof(T).Equals(typeof(Category)))
                            result = (IStub<T>)ProcessGetResponse<Categories>(authResponse);
                        else if (typeof(T).Equals(typeof(BlacklistedItem)))
                            result = (IStub<T>)ProcessGetResponse<Blacklists>(authResponse);
                        else if (typeof(T).Equals(typeof(Query)))
                            result = (IStub<T>)ProcessGetResponse<Queries>(authResponse);
                        else if (typeof(T).Equals(typeof(UserEntity)))
                            result = (IStub<T>)ProcessGetResponse<UserEntities>(authResponse);
                        else if (typeof(T).Equals(typeof(SentimentPhrase)))
                            result = (IStub<T>)ProcessGetResponse<SentimentPhrases>(authResponse);
                        else if (typeof(T).Equals(typeof(TaxonomyNode)))
                            result = (IStub<T>)ProcessGetResponse<Taxonomies>(authResponse);
                        else
                            throw new ArgumentOutOfRangeException();

                        if (result != null) obj = result.Data;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return obj;
        }

        private int Delete<T>(QueryMethod method, List<string> items, string configId = null)
        {
            string url = makeUrl(GetTag<T>(), configId);
            object stub = items;
            List<T> list = new List<T>();
            object obj = default(T);

            foreach (string item in items)
            {
                if (typeof(T).Equals(typeof(Configuration)))
                    obj = new Configuration() { Id = item };
                else if (typeof(T).Equals(typeof(Category)))
                    obj = new Category() { Id = item };
                else if (typeof(T).Equals(typeof(BlacklistedItem)))
                    obj = new BlacklistedItem() { Id = item };
                else if (typeof(T).Equals(typeof(Query)))
                    obj = new Query() { Id = item };
                else if (typeof(T).Equals(typeof(UserEntity)))
                    obj = new UserEntity() { Id = item };
                else if (typeof(T).Equals(typeof(SentimentPhrase)))
                    obj = new SentimentPhrase() { Id = item };
                else if (typeof(T).Equals(typeof(TaxonomyNode)))
                    obj = new TaxonomyNode() { Id = item };
                else
                    throw new ArgumentOutOfRangeException();
                
                list.Add((T)obj);
            }


            IStub<T> xstub = GetStub<T>(list) as IStub<T>;
            xstub.ToKeys();
            
            string data = string.Empty;

            if (_format == "xml")
            {
                data = _serializer.Serialize(xstub);
                data = xstub.ConvertTag(data);
            }
            else
            {
                data = _serializer.Serialize(xstub.Keys);
            }

            AuthResponse authResponse = RunRequest(method, url, data);
            return ProcessPostResponse<string>(authResponse);
        }

        private string GetTag<T>()
        {
            if (typeof(T).Equals(typeof(Configuration)))
                return "configurations";
            else if (typeof(T).Equals(typeof(Category)))
                return "categories";
            else if (typeof(T).Equals(typeof(BlacklistedItem)))
                return "blacklist";
            else if (typeof(T).Equals(typeof(Query)))
                return "queries";
            else if (typeof(T).Equals(typeof(UserEntity)))
                return "entities";
            else if (typeof(T).Equals(typeof(SentimentPhrase)))
                return "phrases";
            else if (typeof(T).Equals(typeof(TaxonomyNode)))
                return "taxonomy";
            else
                throw new ArgumentOutOfRangeException();
        }

        private object GetStub<T>(List<T> items)
        {
            object data = null;
            
            if (typeof(T).Equals(typeof(Configuration)))
                data = new Configurations(items as List<Configuration>);
            else if (typeof(T).Equals(typeof(Category)))
                data = new Categories(items as List<Category>);
            else if (typeof(T).Equals(typeof(BlacklistedItem)))
                data = new Blacklists(items as List<BlacklistedItem>);
            else if (typeof(T).Equals(typeof(Query)))
                data = new Queries(items as List<Query>);
            else if (typeof(T).Equals(typeof(UserEntity)))
                data = new UserEntities(items as List<UserEntity>);
            else if (typeof(T).Equals(typeof(SentimentPhrase)))
                data = new SentimentPhrases(items as List<SentimentPhrase>);
            else if (typeof(T).Equals(typeof(TaxonomyNode)))
                data = new Taxonomies(items as List<TaxonomyNode>);
            else
                throw new ArgumentOutOfRangeException();
 
            return data;
        }

        private WebResponse RunRegularRequest(QueryMethod method, string url, string data)
        {
            WebRequest request = WebRequest.Create(url);
            request.Proxy = WebRequest.DefaultWebProxy;
            request.ContentType = "application/json";
            request.Method = method.ToString();

            if (method == QueryMethod.POST)
            {
                using (StreamWriter writter = new StreamWriter(request.GetRequestStream()))
                {
                    writter.Write(data);
                }
            }

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    using (HttpWebResponse exResponse = (HttpWebResponse)e.Response)
                    {
                        OnError(this, new ResponseErrorEventArgs(null, exResponse.StatusCode, exResponse.StatusDescription));
                    }
                }
                else
                {
                    OnError(this, new ResponseErrorEventArgs(null, HttpStatusCode.BadRequest, e.Message));
                }
            }

            return response as WebResponse;
        }

        private AuthSession GetAuthServiceResponseData(WebResponse response)
        {
            if (response != null)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string data = reader.ReadToEnd();
                    ISerializer serializer = new JsonSerializer();

                    return serializer.Deserialize<AuthSession>(data);
                }
            }

            return null;
        }

        private bool GetTemporaryKeys()
        {
            string requestUrl = string.Format("{0}/session.json?appkey={1}", _authHost, _authAppKey);
            ISerializer serializer = new JsonSerializer();
            AuthCredentials credentials = new AuthCredentials()
            {
                Username = _username,
                Password = _password
            };

            string requestData = serializer.Serialize<AuthCredentials>(credentials);
            WebResponse resp = RunRegularRequest(QueryMethod.POST, requestUrl, requestData);
            AuthSession respObj = GetAuthServiceResponseData(resp);

            if (respObj == null)
            {
                return false;
            }

            string sessionId = respObj.Id;
            WriteSessionFile(sessionId);

            _consumerKey = respObj.Keys.Key;
            _consumerSecret = respObj.Keys.Secret;

            return true;
        }

        private bool ValidateSession(string session)
        {
            string requestUrl = string.Format("{0}/session/{1}.json?appkey={2}", _authHost, session, _authAppKey);
            WebResponse resp = RunRegularRequest(QueryMethod.GET, requestUrl, null);
            AuthSession respObj = GetAuthServiceResponseData(resp);

            if (respObj == null)
            {
                return false;
            }

            string sessionId = respObj.Id;
            WriteSessionFile(sessionId);

            _consumerKey = respObj.Keys.Key;
            _consumerSecret = respObj.Keys.Secret;

            return true;
        }

        private void WriteSessionFile(string sessionId)
        {
            string sessionFile = Path.Combine(Path.GetTempPath(), "semantria-session.dat");
            using (StreamWriter writer = new StreamWriter(sessionFile))
            {
                writer.Write(sessionId);
            }
        }

        private string ReadSessionFile()
        {
            string sessionFile = Path.Combine(Path.GetTempPath(), "semantria-session.dat");
            if (File.Exists(sessionFile))
            {
                using (StreamReader reader = new StreamReader(sessionFile))
                {
                    string sessionId = reader.ReadToEnd();
                    return sessionId;
                }
            }

            return null;
        }

        private AuthResponse RunRequest(QueryMethod method, string url, string data, bool isBinary=false)
        {
            if (string.IsNullOrEmpty(_consumerKey) && string.IsNullOrEmpty(_consumerSecret))
            {
                string sessionId = ReadSessionFile();
                if (!string.IsNullOrEmpty(sessionId) && ReUseSession)
                {
                    if (!ValidateSession(sessionId))
                    {
                        if (!GetTemporaryKeys())
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    if (!GetTemporaryKeys())
                    {
                        return null;
                    }
                }
            }

            return ExecuteRequest(method, url, data, isBinary);
        }

        private AuthResponse ExecuteRequest(QueryMethod method, string url, string data, bool isBinary)
        {
            string appName = GetAppName();
            AuthRequest authRequest = new AuthRequest(_consumerKey, _consumerSecret, appName, _useCompression, _apiVersion);
            using (authRequest)
            {
                OnRequest(this, new RequestEventArgs(method.ToString(), url, data));
                AuthResponse authResponse = authRequest.AuthWebRequest(method, url, data, isBinary);
                OnResponse(this, new ResponseEventArgs(authResponse.Status, authResponse.Data));
                return authResponse;
            }
        }

        private T ProcessGetResponse<T>(AuthResponse authResponse)
        {
            int status = (int)authResponse.Status;
            string message = authResponse.Data;
            T obj = default(T);

            switch (status)
            {
                case 200:
                    obj = ConvertResponse<T>(message);
                    return obj;
                case 202:
                    return obj;
                default:
                    ResolveError<T>(authResponse);
                    return obj;
            }
        }

        private int ProcessPostResponse<T>(AuthResponse authResponse)
        {
            int status = (int)authResponse.Status;
            string message = authResponse.Data;

            switch (status)
            {
                case 200:
                case 202:
                    return status;
                default:
                    ResolveError<T>(authResponse);
                    return status;
            }
        }

        private T ConvertResponse<T>(string data)
        {
            T obj = default(T);
            if (_format == "xml")
            {
                ISerializer serializer = new XmlSerializer();
                obj = serializer.Deserialize<T>(data);
            }
            else if (_format == "json")
            {
                ISerializer serializer = new JsonSerializer();
                obj = serializer.Deserialize<T>(data);
            }
            return obj;
        }

        private List<DocAnalyticData> PostQueueBatch(string url, List<Document> tasks)
        {
            string data = string.Empty;
            switch (_format)
            {
                case "json":
                    {
                        data = _serializer.Serialize(tasks);
                    }
                    break;
                case "xml":
                    {
                        Documents wrapper = new Documents();
                        wrapper.Data = tasks;
                        data = _serializer.Serialize(wrapper);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return PostAnalyticData<DocAnalyticData>(url, data);
        }

        private List<T> PostAnalyticData<T>(string url, string data)
        {
            AuthResponse authResponse = RunRequest(QueryMethod.POST, url, data);

            int status = (int)authResponse.Status;
            string message = authResponse.Data;
            
            List<T> obj = new List<T>();

            switch (status)
            {
                case 200:
                    {
                        switch (_format)
                        {
                            case "json":
                                {
                                    List<T> result = ProcessGetResponse<List<T>>(authResponse);
                                    if (result != null) obj = result;
                                }
                                break;
                            case "xml":
                                {
                                    T result = ProcessGetResponse<T>(authResponse);
                                    if (result != null)
                                        obj = (List<T>)result.GetType().InvokeMember("Data", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty, Type.DefaultBinder, result, null);
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        return obj;
                    }
                case 202:
                    return obj;
                default:
                    ResolveError<T>(authResponse);
                    return null;
            }
        }

        private void ResolveError<T>(AuthResponse authResponse)
        {
            int status = (int)authResponse.Status;
            string message = authResponse.Data;
            OnError(this, new ResponseErrorEventArgs(typeof(T), authResponse.Status, message));
        }

        #endregion
    }
}
