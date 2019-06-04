using Neo.SDK.RPC;
using Neo.SDK.RPC.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.SDK
{
    public class NeoLight : INeoRpc
    {
        private readonly RpcHelper rpcHelper;

        public NeoLight(RpcHelper rpc)
        {
            rpcHelper = rpc;
        }

        public GetAccountState GetAccountState(string address)
        {
            var request = new RPCRequest
            {
                Id = 1,
                Jsonrpc = "2.0",
                Method = "getaccountstate",
                Params = new[] { address }
            };
            return rpcHelper.Send<GetAccountState>(request);
        }

        public SendRawTransaction SendRawTransaction(string rawTransaction)
        {
            var request = new RPCRequest
            {
                Id = 1,
                Jsonrpc = "2.0",
                Method = "sendrawtransaction",
                Params = new[] { rawTransaction }
            };
            return rpcHelper.Send<SendRawTransaction>(request);
        }
    }
}
