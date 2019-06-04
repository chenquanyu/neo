using Neo.SDK.RPC.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.SDK.RPC
{

    /// <summary>
    /// Wrappar of NEO APIs
    /// </summary>
    public interface INeoRpc
    {
        /// <summary>
        /// Queries global assets (NEO, GAS, and etc.) of the account, according to the account address.
        /// </summary>
        GetAccountState GetAccountState(string address);

        /// <summary>
        /// Broadcasts a transaction over the NEO network.
        /// </summary>
        SendRawTransaction SendRawTransaction(string rawTransaction);

        /// <summary>
        /// Returns information of the unspent UTXO assets (e.g. NEO, GAS) at the specified address.
        /// </summary>
        GetUnspents GetUnspents(string address);


    }
}
