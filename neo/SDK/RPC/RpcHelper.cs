using Neo.IO.Json;
using Neo.SDK.RPC.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Neo.SDK.RPC
{
    public class RpcHelper
    {
        private readonly string url;
        private readonly HttpClient httpClient;

        public RpcHelper(string url)
        {
            this.url = url;
            httpClient = new HttpClient();
        }

        public async Task<T> SendAsync<T>(RPCRequest request)
        {
            var requestJson = JsonConvert.SerializeObject(request);
            var result = await httpClient.PostAsync(new Uri(url), new StringContent(requestJson, Encoding.UTF8));
            var response = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(response);
        }

        public T Send<T>(RPCRequest request)
        {
            return SendAsync<T>(request).ConfigureAwait(false).GetAwaiter().GetResult();
        }

    }


}
