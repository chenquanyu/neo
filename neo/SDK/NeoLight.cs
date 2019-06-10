using Neo.SDK.RPC;
using Neo.SDK.RPC.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.SDK
{
    public class NeoLight : INeoRpc
    {
        private readonly IRpcService rpcHelper;

        public NeoLight(IRpcService rpc)
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

        public GetNep5Balances GetNep5Balances(string address)
        {
            var request = new RPCRequest
            {
                Id = 1,
                Jsonrpc = "2.0",
                Method = "getnep5balances",
                Params = new[] { address }
            };
            return rpcHelper.Send<GetNep5Balances>(request);
        }

        public GetUnspents GetUnspents(string address)
        {
            var request = new RPCRequest
            {
                Id = 1,
                Jsonrpc = "2.0",
                Method = "getunspents",
                Params = new[] { address }
            };
            return rpcHelper.Send<GetUnspents>(request);
        }

        public bool SendRawTransaction(string rawTransaction)
        {
            var request = new RPCRequest
            {
                Id = 1,
                Jsonrpc = "2.0",
                Method = "sendrawtransaction",
                Params = new[] { rawTransaction }
            };
            return rpcHelper.Send<bool>(request);
        }
    }
}
