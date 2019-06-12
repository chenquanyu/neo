using Neo.Network.P2P.Payloads;
using Neo.SDK.RPC;
using Neo.SDK.RPC.Model;
using Neo.SDK.Wallets;
using System;
using System.Collections.Generic;

namespace Neo.SDK
{
    public class Core
    {
        public void ClaimGas(string address)
        {
            throw new NotImplementedException();
        }

        public void InvokeContract(WalletFile.Account account, InvocationTransaction invocation)
        {
            throw new NotImplementedException();
        }

        public void TransferAsset(WalletFile.Account from, string toAddress, Dictionary<UIntBase, decimal> intents, decimal fee)
        {
            throw new NotImplementedException();
        }

    }
}
