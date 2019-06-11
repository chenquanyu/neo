using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.SDK
{
    /// <summary>
    /// Core Interfaces for NEO SDK 
    /// </summary>
    public interface INeoLight
    {
        /// <summary>
        /// Transfer Asset
        /// </summary>
        void TransferAsset(WalletAccount from, string toAddress, Dictionary<UIntBase, decimal> intents, decimal fee);

        /// <summary>
        /// Claim GAS
        /// </summary>
        void ClaimGas(string address);

        /// <summary>
        /// Invoke Contract
        /// </summary>
        void InvokeContract(WalletAccount account, string script);

    }
}
