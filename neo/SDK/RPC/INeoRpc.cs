using Neo.SDK.RPC.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.SDK.RPC
{

    /// <summary>
    /// Wrappar of NEO API
    /// </summary>
    public interface INeoRpc
    {
        GetAccountState GetAccountState(RPCRequest request);


    }
}
