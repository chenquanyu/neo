using Neo.SDK.RPC.Model;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Neo.SDK.RPC
{
    public class RestHelper : IRpcService
    {
        private readonly RestClient httpClient;

        public RestHelper(string url)
        {
            httpClient = new RestClient(url);
        }

        public async Task<T> SendAsync<T>(RPCRequest request)
        {
            var requestJson = JsonConvert.SerializeObject(request);
            var req = new RestRequest(Method.POST);
            req.AddJsonBody(requestJson);

            var result = await httpClient.ExecutePostTaskAsync(req);
            var content = result.Content;
            var response = JsonConvert.DeserializeObject<RPCResponse<T>>(content);
            if (response.Error != null)
            {
                throw new NeoSdkException(response.Error.Code, response.Error.Message, response.Error.Data);
            }

            return response.Result;
        }

        public T Send<T>(RPCRequest request)
        {
            var requestJson = JsonConvert.SerializeObject(request);
            var req = new RestRequest(Method.POST);
            req.AddJsonBody(requestJson);

            var result = httpClient.Post(req);
            var content = result.Content;
            var response = JsonConvert.DeserializeObject<RPCResponse<T>>(content);
            if (response.Error != null)
            {
                throw new NeoSdkException(response.Error.Code, response.Error.Message, response.Error.Data);
            }

            return response.Result;
        }

    }


}
