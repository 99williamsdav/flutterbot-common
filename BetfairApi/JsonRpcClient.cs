using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using BetfairApi.Models;
using System.Net;
using System.IO;
using BetfairApi.Json;
using log4net;
using System.IO.Compression;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;

namespace BetfairApi
{
    public class JsonRpcClient  : IClient
    {
        public static string[] SessionErrors = new[] { "INVALID_SESSION_INFORMATION", "NO_APP_KEY", "NO_SESSION", "INVALID_APP_KEY", "ACCESS_DENIED" };

        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonRpcClient));

        private const bool LogComms = false;

        public string EndPoint { get; private set; }
        private static readonly IDictionary<string, Type> operationReturnTypeMap = new Dictionary<string, Type>();
        public const string APPKEY_HEADER = "X-Application";
        public const string SESSION_TOKEN_HEADER = "X-Authentication";
        public NameValueCollection CustomHeaders { get; set; }
        private static readonly string LIST_EVENTS_METHOD = "SportsAPING/v1.0/listEvents";
        private static readonly string LIST_EVENT_TYPES_METHOD = "SportsAPING/v1.0/listEventTypes";
        private static readonly string LIST_MARKET_TYPES_METHOD = "SportsAPING/v1.0/listMarketTypes";
        private static readonly string LIST_MARKET_CATALOGUE_METHOD = "SportsAPING/v1.0/listMarketCatalogue";
        private static readonly string LIST_MARKET_BOOK_METHOD = "SportsAPING/v1.0/listMarketBook";
        private static readonly string PLACE_ORDERS_METHOD = "SportsAPING/v1.0/placeOrders";
        private static readonly string LIST_MARKET_PROFIT_AND_LOST_METHOD = "SportsAPING/v1.0/listMarketProfitAndLoss";
        private static readonly string LIST_CURRENT_ORDERS_METHOD = "SportsAPING/v1.0/listCurrentOrders";
        private static readonly string LIST_CLEARED_ORDERS_METHOD = "SportsAPING/v1.0/listClearedOrders";
        private static readonly string CANCEL_ORDERS_METHOD = "SportsAPING/v1.0/cancelOrders";
        private static readonly string REPLACE_ORDERS_METHOD = "SportsAPING/v1.0/replaceOrders";
        private static readonly string UPDATE_ORDERS_METHOD = "SportsAPING/v1.0/updateOrders";
        private static readonly String FILTER = "filter";
        private static readonly String LOCALE = "locale";
        private static readonly String CURRENCY_CODE = "currencyCode";
        private static readonly String MARKET_PROJECTION = "marketProjection";
        private static readonly String MATCH_PROJECTION = "matchProjection";
        private static readonly String ORDER_PROJECTION = "orderProjection";
        private static readonly String PRICE_PROJECTION = "priceProjection";
        private static readonly String SORT = "sort";
        private static readonly String MAX_RESULTS = "maxResults";
        private static readonly String MARKET_IDS = "marketIds";
        private static readonly String MARKET_ID = "marketId";
        private static readonly String INSTRUCTIONS = "instructions";
        private static readonly String CUSTOMER_REFERENCE = "customerRef";
        private static readonly String INCLUDE_SETTLED_BETS = "includeSettledBets";
        private static readonly String INCLUDE_BSP_BETS = "includeBspBets";
        private static readonly String NET_OF_COMMISSION = "netOfCommission";
        private static readonly String BET_IDS = "betIds";
        private static readonly String PLACED_DATE_RANGE = "placedDateRange";
        private static readonly String ORDER_BY = "orderBy";
        private static readonly String SORT_DIR = "sortDir";
        private static readonly String FROM_RECORD = "fromRecord";
        private static readonly String RECORD_COUNT = "recordCount";
        private static readonly string BET_STATUS = "betStatus";
        private static readonly string EVENT_TYPE_IDS = "eventTypeIds";
        private static readonly string EVENT_IDS = "eventIds";
        private static readonly string RUNNER_IDS = "runnerIds";
        private static readonly string SIDE = "side";
        private static readonly string SETTLED_DATE_RANGE = "settledDateRange";
        private static readonly string GROUP_BY = "groupBy";
        private static readonly string INCLUDE_ITEM_DESCRIPTION = "includeItemDescription";

        private readonly Func<string> _sessionTokenFunc;

        private readonly HttpClient Client = new HttpClient();

        public JsonRpcClient(string endPoint, string appKey, Func<string> sessionTokenFunc)
		{
            this.EndPoint = endPoint + "/json-rpc/v1";
            CustomHeaders = new NameValueCollection();
            if (appKey != null)
            {
                CustomHeaders[APPKEY_HEADER] = appKey;
            }
            CustomHeaders["Accept-Encoding"] = "gzip, deflate";

            this._sessionTokenFunc = sessionTokenFunc;
            this.ResetSessionToken();
        }

        public IList<Dictionary<string, object>> listRaceDetails(ISet<string> eventIds)
        {
            var args = new Dictionary<string, object>();
            args["meetingIds"] = eventIds;
            return InvokeAsync<List<Dictionary<string, object>>>("SportsAPING/v1.0/listRaceDetails", args).Result;
        }

        public IList<EventResult> listEvents(MarketFilter marketFilter, string locale = null)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            args[LOCALE] = locale;
            return InvokeAsync<List<EventResult>>(LIST_EVENTS_METHOD, args).Result;

        }

        public IList<EventTypeResult> listEventTypes(MarketFilter marketFilter, string locale = null)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            args[LOCALE] = locale;
            return InvokeAsync<List<EventTypeResult>>(LIST_EVENT_TYPES_METHOD, args).Result;
            
        }

        public IList<MarketCatalogue> listMarketCatalogue(MarketFilter marketFilter, ISet<MarketProjection> marketProjections, MarketSort marketSort, int maxResult = 1, string locale = null)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            args[MARKET_PROJECTION] = marketProjections;
            args[SORT] = marketSort;
            args[MAX_RESULTS] = maxResult.ToString();
            args[LOCALE] = locale;
            return InvokeAsync<List<MarketCatalogue>>(LIST_MARKET_CATALOGUE_METHOD, args).Result;
        }

        public IList<MarketTypeResult> listMarketTypes(MarketFilter marketFilter, string stringLocale = null)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            args[LOCALE] = stringLocale;
            return InvokeAsync<List<MarketTypeResult>>(LIST_MARKET_TYPES_METHOD, args).Result;
        }

        public IList<MarketBook> listMarketBook(IList<string> marketIds, PriceProjection priceProjection, OrderProjection? orderProjection = null, MatchProjection? matchProjection = null, string currencyCode = null, string locale = null)
        {
            var args = new Dictionary<string, object>();
            args[MARKET_IDS]= marketIds;
            args[PRICE_PROJECTION] = priceProjection;
            args[ORDER_PROJECTION] = orderProjection;
            args[MATCH_PROJECTION] = matchProjection;
            args[LOCALE] = locale;
            args[CURRENCY_CODE] = currencyCode;
            return InvokeAsync<List<MarketBook>>(LIST_MARKET_BOOK_METHOD, args).Result;
        }

        public PlaceExecutionReport placeOrders(string marketId, string customerRef, IList<PlaceInstruction> placeInstructions, string locale = null)
        {
            var args = new Dictionary<string, object>();
            
            args[MARKET_ID] =  marketId;
            args[INSTRUCTIONS] = placeInstructions;
            //args[CUSTOMER_REFERENCE] = customerRef;
            args[LOCALE] =  locale;

            return InvokeAsync<PlaceExecutionReport>(PLACE_ORDERS_METHOD, args).Result;
        }



        protected WebRequest CreateWebRequest(Uri uri)
        {
            WebRequest request = WebRequest.Create(new Uri(EndPoint));
            request.Method = "POST";
            request.ContentType = "application/json-rpc";
            request.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8");
            request.Headers.Add(CustomHeaders);
            ServicePointManager.Expect100Continue = false;
            return request;
        }



        private HttpContent CreateHttpContent(object content)
        {
            var ms = new MemoryStream();
            using (var sw = new StreamWriter(ms, new UTF8Encoding(false), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                var js = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
                js.Serialize(jtw, content);
                jtw.Flush();
            }
            ms.Seek(0, SeekOrigin.Begin);
            HttpContent httpContent = new StreamContent(ms);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json-rpc");
            //foreach (var key in CustomHeaders.AllKeys)
            //{
            //    var val = CustomHeaders[key];
            //    httpContent.Headers.Add(key, CustomHeaders[key]);
            //}

            return httpContent;
        }

        public async Task<T> InvokeAsync<T>(string method, IDictionary<string, object> args = null, int attempts = 0)
        {

            using (var request = new HttpRequestMessage(HttpMethod.Post, EndPoint))
            using (var httpContent = CreateHttpContent(new JsonRequest { Method = method, Id = 1, Params = args }))
            {
                request.Content = httpContent;
                foreach (var key in CustomHeaders.AllKeys)
                {
                    var val = CustomHeaders[key];
                    request.Headers.Add(key, CustomHeaders[key]);
                }

                using (HttpResponseMessage response = await Client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false))
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (Stream csStream = new GZipStream(stream, CompressionMode.Decompress))
                using (StreamReader reader = new StreamReader(csStream, Encoding.UTF8))
                {
                    var json = await reader.ReadToEndAsync();
                    if (string.IsNullOrEmpty(json))
                    {
                        throw new System.Exception("No response from server.");
                    }
                    var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonResponse<T>>(json);
                    if (jsonResponse.HasError)
                    {
                        var errorCode = jsonResponse.Error.Data.First.First.SelectToken("errorCode").ToObject<string>();
                        // get new session token and retry if error is session-related.
                        if (!SessionErrors.Contains(errorCode))
                        {
                            Log.Error($"Betfair API request failed: {jsonResponse.Error.Data.First}");
                            throw ReconstituteException(jsonResponse.Error);
                        }
                        else if (attempts < 3)
                        {
                            Log.Error($"Betfair API error. Refreshing session token and trying again: {jsonResponse.Error.Data.First}");
                            this.ResetSessionToken();
                            return await this.InvokeAsync<T>(method, args, attempts + 1);
                        }
                        else
                        {
                            Log.Error($"Betfair API request failed after {attempts} attempts.");
                            throw ReconstituteException(jsonResponse.Error);
                        }
                    }
                    else
                    {
                        return jsonResponse.Result;
                    }
                }
            }
        }

        private void ResetSessionToken()
        {
            var sessionToken = this._sessionTokenFunc();
            if (sessionToken == null)
            {
                Log.Error("No session token returned by server.");
                return;
            }

            CustomHeaders[SESSION_TOKEN_HEADER] = sessionToken;
        }


        private static System.Exception ReconstituteException(Models.Exception ex)
        {
            var data = ex.Data;

            // API-NG exception -- it must have "data" element to tell us which exception
            var exceptionName = data.Property("exceptionname").Value.ToString();
            var exceptionData = data.Property(exceptionName).Value.ToString();
            return Json.JsonConvert.Deserialize<APINGException>(exceptionData);
        }

        public IList<MarketProfitAndLoss> listMarketProfitAndLoss(IList<string> marketIds, bool includeSettledBets = false, bool includeBspBets = false, bool netOfCommission = false)
        {
            var args = new Dictionary<string, object>();
            args[MARKET_IDS] = marketIds;
            args[INCLUDE_SETTLED_BETS] = includeSettledBets;
            args[INCLUDE_BSP_BETS] = includeBspBets;
            args[NET_OF_COMMISSION] = netOfCommission;
            return InvokeAsync<List<MarketProfitAndLoss>>(LIST_MARKET_PROFIT_AND_LOST_METHOD, args).Result;
        }

        public CurrentOrderSummaryReport listCurrentOrders(ISet<String> betIds, ISet<String> marketIds, OrderProjection? orderProjection = null, TimeRange placedDateRange = null, OrderBy? orderBy = null, SortDir? sortDir = null, int? fromRecord = null, int? recordCount = null)
        {
            var args = new Dictionary<string, object>();
            args[BET_IDS] = betIds;
            args[MARKET_IDS] = marketIds;
            args[ORDER_PROJECTION] = orderProjection;
            args[PLACED_DATE_RANGE] = placedDateRange;
            args[ORDER_BY] = orderBy;
            args[SORT_DIR] = sortDir;
            args[FROM_RECORD] = fromRecord;
            args[RECORD_COUNT] = recordCount;

            return InvokeAsync<CurrentOrderSummaryReport>(LIST_CURRENT_ORDERS_METHOD, args).Result;
        }

        public ClearedOrderSummaryReport listClearedOrders(BetStatus betStatus, ISet<string> eventTypeIds = null, ISet<string> eventIds = null, ISet<string> marketIds = null, ISet<RunnerId> runnerIds = null, ISet<string> betIds = null, Side? side = null, TimeRange settledDateRange = null, GroupBy? groupBy = null, bool? includeItemDescription = null, String locale = null, int? fromRecord = null, int? recordCount = null)
        {
            var args = new Dictionary<string, object>();
            args[BET_STATUS] = betStatus;
            args[EVENT_TYPE_IDS] = eventTypeIds;
            args[EVENT_IDS] = eventIds;
            args[MARKET_IDS] = marketIds;
            args[RUNNER_IDS] = runnerIds;
            args[BET_IDS] = betIds;
            args[SIDE] = side;
            args[SETTLED_DATE_RANGE] = settledDateRange;
            args[GROUP_BY] = groupBy;
            args[INCLUDE_ITEM_DESCRIPTION] = includeItemDescription;
            args[LOCALE] = locale;
            args[FROM_RECORD] = fromRecord;
            args[RECORD_COUNT] = recordCount;

            return InvokeAsync<ClearedOrderSummaryReport>(LIST_CLEARED_ORDERS_METHOD, args).Result;
        }


        public CancelExecutionReport cancelOrders(string marketId, IList<CancelInstruction> instructions, string customerRef)
        {
            var args = new Dictionary<string, object>();
            args[MARKET_ID] = marketId;
            args[INSTRUCTIONS] = instructions;
            //args[CUSTOMER_REFERENCE] = customerRef;

            return InvokeAsync<CancelExecutionReport>(CANCEL_ORDERS_METHOD, args).Result;
        }

        public ReplaceExecutionReport replaceOrders(String marketId, IList<ReplaceInstruction> instructions, String customerRef)
        {
            var args = new Dictionary<string, object>();
            args[MARKET_ID] = marketId;
            args[INSTRUCTIONS] = instructions;
            args[CUSTOMER_REFERENCE] = customerRef;

            return InvokeAsync<ReplaceExecutionReport>(REPLACE_ORDERS_METHOD, args).Result;
        }

        public UpdateExecutionReport updateOrders(String marketId, IList<UpdateInstruction> instructions, String customerRef)
        {
            var args = new Dictionary<string, object>();
            args[MARKET_ID] = marketId;
            args[INSTRUCTIONS] = instructions;
            args[CUSTOMER_REFERENCE] = customerRef;

            return InvokeAsync<UpdateExecutionReport>(UPDATE_ORDERS_METHOD, args).Result;
        }

    }
}
