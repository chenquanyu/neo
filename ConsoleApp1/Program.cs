using Neo;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");

            // get the KeyPair of your account
            KeyPair receiverKey = Utility.GetKeyPair("L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb");
            KeyPair key2 = Utility.GetKeyPair("L2ynA5aq6KPJjpisXb8pGXnRvgDqYVkgC2Rw85GM51B9W33YcdiZ");
            KeyPair key3 = Utility.GetKeyPair("L3TbPZ3Gtqh3TTk2CWn44m9iiuUhBGZWoDJQuvVw5Zbx5NAjPbdb");

            // create multi-signature contract, this contract needs at least 2 KeyPairs to sign
            Contract multiContract = Contract.CreateMultiSigContract(2, receiverKey.PublicKey, key2.PublicKey, key3.PublicKey);

            // construct the script, in this example, we will transfer 10 GAS to receiver
            UInt160 scriptHash = NativeContract.GAS.Hash;
            UInt160 multiAccount = multiContract.Script.ToScriptHash();
            UInt160 receiver = Contract.CreateSignatureContract(receiverKey.PublicKey).ScriptHash;
            byte[] script = scriptHash.MakeScript("transfer", multiAccount, receiver, 10 * NativeContract.GAS.Factor);

            // add Cosigners, this is a collection of scripthashs which need to be signed
            Cosigner[] cosigners = new[] { new Cosigner { Scopes = WitnessScope.CalledByEntry, Account = multiAccount } };

            // initialize the TransactionManager with rpc client and sender scripthash
            Transaction tx = new TransactionManager(client, multiAccount)
                // fill the script, attribute, cosigner and network fee, multi-sign account need to fill networkfee by user
                .MakeTransaction(script, null, cosigners, 0_05000000)
                // add multi-signature for the transaction with sendKey, at least use 2 KeyPairs
                .AddMultiSig(receiverKey, 2, receiverKey.PublicKey, key2.PublicKey, key3.PublicKey)
                .AddMultiSig(key2, 2, receiverKey.PublicKey, key2.PublicKey, key3.PublicKey)
                // sign transaction with the added signature
                .Sign()
                .Tx;

            // broadcasts transaction over the NEO network.
            client.SendRawTransaction(tx);
            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            // print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(client);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block {(await p).BlockHash}"));

            Console.ReadKey();
        }
    }
}
