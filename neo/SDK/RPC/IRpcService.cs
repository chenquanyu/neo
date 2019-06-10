using Neo.SDK.RPC.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neo.SDK.RPC
{
    public interface IRpcService
    {
        Task<T> SendAsync<T>(RPCRequest request);

        T Send<T>(RPCRequest request);
    }
}
