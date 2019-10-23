using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.SmartContract.Native;
using Neo.Wallets;
using Neo.VM;
using System;
using System.Security.Cryptography;
using Neo.SmartContract;

namespace Neo.UnitTests.Network.RPC
{
    [TestClass]
    public class RpcExamples
    {
        [TestMethod]
        public void TransManager()
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");

            // get the KeyPair of your account, this account will pay the system and network fee
            KeyPair sendKey = "L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb".ToKeyPair();
            UInt160 sender = sendKey.ToScriptHash();

            // get the scripthash of the account you want to transfer to
            UInt160 receiver = "AKviBGFhWeS8xrAH3hqDQufZXE9QM5pCeP".ToUInt160();

            // construct the script, in this example, we will transfer 1 NEO to receiver
            UInt160 scriptHash = NativeContract.NEO.Hash;
            byte[] script = scriptHash.MakeScript("transfer", sender, receiver, 1);

            // add Cosigners, this is a collection of scripthashs which need to be signed
            Cosigner[] cosigners = new[] { new Cosigner { Scopes = WitnessScope.CalledByEntry, Account = sender } };

            // initialize the TransactionManager with rpc client and sender scripthash
            Transaction tx = new TransactionManager(client, sender)
                // fill the script, attribute, cosigner and network fee
                .MakeTransaction(script, null, cosigners, 0)
                // add signature for the transaction with sendKey
                .AddSignature(sendKey)
                // sign transaction with the added signature
                .Sign()
                .Tx;

            // broadcasts transaction over the NEO network.
            client.SendRawTransaction(tx);
            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            // print a message after the transaction is on chain
            NeoAPI neoAPI = new NeoAPI(client);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block height {await p}"));
        }

        [TestMethod]
        public void SendToMultiAccount()
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");

            // get the KeyPair of your account, this account will pay the system and network fee
            KeyPair sendKey = "L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb".ToKeyPair();
            UInt160 sender = sendKey.ToScriptHash();

            // get the KeyPair of your accounts
            KeyPair key2 = "L2ynA5aq6KPJjpisXb8pGXnRvgDqYVkgC2Rw85GM51B9W33YcdiZ".ToKeyPair();
            KeyPair key3 = "L3TbPZ3Gtqh3TTk2CWn44m9iiuUhBGZWoDJQuvVw5Zbx5NAjPbdb".ToKeyPair();

            // create multi-signature contract, this contract needs at least 2 KeyPairs to sign
            Contract multiContract = Contract.CreateMultiSigContract(2, sendKey.PublicKey, key2.PublicKey, key3.PublicKey);
            // get the scripthash of the multi-signature Contract
            UInt160 multiAccount = multiContract.Script.ToScriptHash();

            // construct the script, in this example, we will transfer 10 GAS to multi-sign account
            // in contract parameter, the amount type is BigInteger, so we need to muliply the contract factor
            UInt160 scriptHash = NativeContract.GAS.Hash;
            byte[] script = scriptHash.MakeScript("transfer", sender, multiAccount, 10 * NativeContract.GAS.Factor);

            // add Cosigners, this is a collection of scripthashs which need to be signed
            Cosigner[] cosigners = new[] { new Cosigner { Scopes = WitnessScope.CalledByEntry, Account = sender } };

            // initialize the TransactionManager with rpc client and sender scripthash
            Transaction tx = new TransactionManager(client, sender)
                // fill the script, attribute, cosigner and network fee
                .MakeTransaction(script, null, cosigners, 0)
                // add signature for the transaction with sendKey
                .AddSignature(sendKey)
                // sign transaction with the added signature
                .Sign()
                .Tx;

            // broadcasts transaction over the NEO network.
            client.SendRawTransaction(tx);
            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            // print a message after the transaction is on chain
            NeoAPI neoAPI = new NeoAPI(client);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block height {await p}"));
        }

        [TestMethod]
        public void SendFromMultiAccount()
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");

            // get the KeyPair of your account
            KeyPair receiverKey = "L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb".ToKeyPair();
            KeyPair key2 = "L2ynA5aq6KPJjpisXb8pGXnRvgDqYVkgC2Rw85GM51B9W33YcdiZ".ToKeyPair();
            KeyPair key3 = "L3TbPZ3Gtqh3TTk2CWn44m9iiuUhBGZWoDJQuvVw5Zbx5NAjPbdb".ToKeyPair();

            // create multi-signature contract, this contract needs at least 2 KeyPairs to sign
            Contract multiContract = Contract.CreateMultiSigContract(2, receiverKey.PublicKey, key2.PublicKey, key3.PublicKey);

            // construct the script, in this example, we will transfer 10 GAS to receiver
            UInt160 scriptHash = NativeContract.GAS.Hash;
            UInt160 multiAccount = multiContract.Script.ToScriptHash();
            UInt160 receiver = receiverKey.ToScriptHash();
            byte[] script = scriptHash.MakeScript("transfer", multiAccount, receiver, 1 * NativeContract.GAS.Factor);

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
            NeoAPI neoAPI = new NeoAPI(client);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block height {await p}"));
        }

        [TestMethod]
        public void CreateAccount()
        {
            // create a new private key and KeyPair
            byte[] privateKey = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(privateKey);
            }
            KeyPair keyPair = new KeyPair(privateKey);

            // export KeyPair as WIF
            string wif = keyPair.Export();

            // get private key and KeyPair from WIF
            byte[] privateKey1 = Wallet.GetPrivateKeyFromWIF(wif);
            KeyPair keyPair1 = new KeyPair(privateKey1);

            Assert.AreEqual(keyPair, keyPair1);
        }

        [TestMethod]
        public void Mointor()
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");

            // get the highest block hash
            string hash = client.GetBestBlockHash();

            // get the highest block height
            uint height = client.GetBlockCount() - 1;

            // get block data
            var block = client.GetBlock(hash);

            Console.WriteLine($"{hash},{height},{block.ToJson().ToString()}");
        }

    }
}
