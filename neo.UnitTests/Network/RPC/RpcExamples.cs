using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.SmartContract.Native;
using Neo.Wallets;
using Neo.VM;
using System;

namespace Neo.UnitTests.Network.RPC
{
    [TestClass]
    public class RpcExamples
    {
        [TestMethod]
        public void TransManager()
        {
            // 1. choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://127.0.0.1:20332");

            // 2. get the KeyPair of your account, this account will pay the system and network fee
            KeyPair sendKey = "L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb".ToKeyPair();
            UInt160 sender = sendKey.ToScriptHash();

            // 3. add Cosigners, this is a collection of scripthashs which need to be signed
            Cosigner[] cosigners = new[] { new Cosigner { Scopes = WitnessScope.CalledByEntry, Account = sender } };

            // 4. get the scripthash of the account you want to transfer to
            UInt160 receiver = "AKviBGFhWeS8xrAH3hqDQufZXE9QM5pCeP".ToUInt160();

            // 5. construct the script, in this example, we will use NEO transfer
            UInt160 scriptHash = NativeContract.NEO.Hash;
            byte[] script = scriptHash.MakeScript("transfer", sender, receiver, 0);

            // 6. initialize the TransactionManager with rpc client and sender scripthash
            Transaction tx = new TransactionManager(client, sender)
                // fill the script, attribute, cosigner and network fee
                .MakeTransaction(script, null, cosigners, 0)
                // add signature for the transaction with sendKey
                .AddSignature(sendKey)
                // sign transaction with the added signature
                .Sign()
                .Tx;

            // 7. broadcasts transaction over the NEO network.
            client.SendRawTransaction(tx);

            // 8. print a message after the transaction is on chain
            NeoAPI neoAPI = new NeoAPI(client);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is successly on block height {await p}"));
        }



    }
}
