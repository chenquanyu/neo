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

        public GetAccountState GetAccountState(RPCRequest request)
        {
            return rpcHelper.Send<GetAccountState>(request);
        }
    }
}
