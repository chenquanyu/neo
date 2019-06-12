using Neo.SDK.RPC.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Neo.SDK.RPC
{
    public class HttpService : IRpcService
    {
        private readonly string url;
        private readonly HttpClient httpClient;

        public HttpService(string url)
        {
            this.url = url;
            httpClient = new HttpClient();
        }

        public async Task<T> SendAsync<T>(RPCRequest request)
        {
            var requestJson = JsonConvert.SerializeObject(request);
            var result = await httpClient.PostAsync(new Uri(url), new StringContent(requestJson, Encoding.UTF8));
            var content = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<RPCResponse<T>>(content);
            if (response.Error != null)
            {
                throw new NeoSdkException(response.Error.Code, response.Error.Message, response.Error.Data);
            }

            return response.Result;
        }

        public T Send<T>(RPCRequest request)
        {
            try
            {
                return SendAsync<T>(request).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.GetBaseException();
            }
        }

    }


}
