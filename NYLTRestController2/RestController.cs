using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace NYLT
{

    namespace Rest
    {

        public class RestController
        {
            readonly string Host;

            /// <summary>
            /// Instantiates the REST controller object for a given hostname.
            /// </summary>
            /// <param name="Host">The hostname of the REST API</param>
            public RestController(string Host)
            {
                this.Host = Host;
            }

            ///// <summary>
            ///// Instantiates the REST controller object.
            ///// </summary>
            //public RestController()
            //{
            //    Host = "";
            //}

            /// <summary>
            /// Executes a GET command on a given endpoint.
            /// </summary>
            /// <typeparam name="T">The type of the expected response.</typeparam>
            /// <param name="EndpointURL">The service endpoint from which to GET the response.</param>
            /// <returns>The response object.</returns>
            public async Task<T> GetObjectAsync<T>(string EndpointURL)
            {
                try
                {
                    var rootFilter = new HttpBaseProtocolFilter();
                    rootFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
                    rootFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                    var get_uri = this.CreateUri(this.Host, EndpointURL);
                    var client = new HttpClient(rootFilter);
                    var response = await client.GetAsync(get_uri);
                    if (response.IsSuccessStatusCode)
                    {
                        T DeserializedResponse = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
                        return DeserializedResponse;
                    }
                    else
                    {
                        throw new Exception("The server failed to return a successful response.");
                    }
                }
                catch
                {
                    throw new Exception("The REST controller failed to complete the GET call.  There may not be an internet connection.");
                }
            }

            /// <summary>
            /// Upload a given object as JSON in the body of a POST command and get the response.
            /// </summary>
            /// <param name="EndpointURL">The service endpoint to which to POST.</param>
            /// <param name="ObjectToSend">The object to POST.</param>
            /// <returns>The HTTP response if successful - otherwise, null.</returns>
            public async Task<HttpResponseMessage> PostObjectAsync(string EndpointURL, Object ObjectToSend)
            {
                try
                {
                    var post_uri = CreateUri(Host, EndpointURL);
                    var client = new HttpClient();
                    var content = new HttpStringContent(JsonConvert.SerializeObject(ObjectToSend, new Newtonsoft.Json.Converters.IsoDateTimeConverter()), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                    var response = await client.PostAsync(post_uri, content);
                    return response.IsSuccessStatusCode ? response : null;
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// Update a given object via PUT command.
            /// </summary>
            /// <param name="URL">The service endpoint to which to PUT.</param>
            /// <param name="ObjectToSend">The object to PUT.</param>
            /// <returns>A boolean indicating whether or not the PUT response succeeded.</returns>
            public async Task<bool> PutObjectAsync(string EndpointURL, object ObjectToSend)
            {
                var put_uri = CreateUri(Host, EndpointURL);
                try
                {
                    var client = new HttpClient();
                    var content = new HttpStringContent(JsonConvert.SerializeObject(ObjectToSend, new Newtonsoft.Json.Converters.IsoDateTimeConverter()), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                    var response = await client.PutAsync(put_uri, content);
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Delete a given object via DELETE command.
            /// </summary>
            /// <param name="EndpointURL">The service endpoint to DELETE.</param>
            /// <returns>A boolean indicating whether or not the DELETE command succeeded.</returns>
            public async Task<bool> DeleteObjectAsync(string EndpointURL)
            {
                var delete_uri = CreateUri(Host, EndpointURL);
                try
                {
                    var client = new HttpClient();
                    var response = await client.DeleteAsync(delete_uri);
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }
            private Uri CreateUri(string host, string endpoint)
            {
                host = host.TrimEnd('/');
                endpoint = endpoint.TrimStart('/');
                return new Uri(string.Format("{0}/{1}", host, endpoint));
            }
        }
    }
}