using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.SmartContract.Native;
using Neo.Wallets;
using Neo.VM;
using System;
using Neo.SmartContract;
using Neo.Network.RPC.Models;
using System.Linq;
using Newtonsoft.Json;
using Neo.Ledger;
using Neo.SmartContract.Manifest;
using System.Numerics;
using Neo.Wallets.NEP6;
using System.Security.Cryptography;

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
            KeyPair sendKey = Utility.GetKeyPair("L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb");
            UInt160 sender = Contract.CreateSignatureRedeemScript(sendKey.PublicKey).ToScriptHash();

            // get the scripthash of the account you want to transfer to
            UInt160 receiver = Utility.GetScriptHash("AKviBGFhWeS8xrAH3hqDQufZXE9QM5pCeP");

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
            WalletAPI neoAPI = new WalletAPI(client);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block {(await p).BlockHash}"));
        }

        [TestMethod]
        public void SendToMultiAccount()
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");

            // get the KeyPair of your account, this account will pay the system and network fee
            KeyPair sendKey = Utility.GetKeyPair("L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb");
            UInt160 sender = Contract.CreateSignatureContract(sendKey.PublicKey).ScriptHash;

            // get the KeyPair of your accounts
            KeyPair key2 = Utility.GetKeyPair("L2ynA5aq6KPJjpisXb8pGXnRvgDqYVkgC2Rw85GM51B9W33YcdiZ");
            KeyPair key3 = Utility.GetKeyPair("L3TbPZ3Gtqh3TTk2CWn44m9iiuUhBGZWoDJQuvVw5Zbx5NAjPbdb");

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
            WalletAPI neoAPI = new WalletAPI(client);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block {(await p).BlockHash}"));
        }

        [TestMethod]
        public void SendFromMultiAccount()
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
            WalletAPI neoAPI = new WalletAPI(client);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block {(await p).BlockHash}"));
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

            // export private key to hex string
            string privateHex = keyPair.PrivateKey.ToHexString();

            // get KeyPair from private hex string
            keyPair = Utility.GetKeyPair(privateHex);

            // export KeyPair as WIF
            string wif = keyPair.Export();

            // get KeyPair from WIF
            KeyPair keyPair1 = Utility.GetKeyPair(wif);

            // export public key hex string
            string publicHex = keyPair.PublicKey.ToString();

            // get public key from hex string
            Neo.Cryptography.ECC.ECPoint publicKey = Neo.Cryptography.ECC.ECPoint.Parse(publicHex, Neo.Cryptography.ECC.ECCurve.Secp256r1);

            // get ScriptHash of KeyPair account
            UInt160 scriptHash = Contract.CreateSignatureContract(keyPair.PublicKey).ScriptHash;
            string strScriptHash = scriptHash.ToString();

            // get address of KeyPair account
            string adddress = scriptHash.ToAddress();
            scriptHash = adddress.ToScriptHash();

            // create wallet
            string path = "wallet_new.json";
            string password = "MyPass";
            NEP6Wallet wallet_new = new NEP6Wallet(path);
            using (wallet_new.Unlock(password))
            {
                wallet_new.CreateAccount(keyPair.PrivateKey);
            }
            wallet_new.Save();

            // load wallet from nep6 wallet
            NEP6Wallet wallet = new NEP6Wallet(path);
            KeyPair keyPair2;
            using (wallet.Unlock(password))
            {
                keyPair2 = wallet.GetAccounts().Last().GetKey();
            }

            Assert.AreEqual(keyPair, keyPair1);
            Assert.AreEqual(keyPair, keyPair2);
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
            RpcBlock block = client.GetBlock("166396");
            block = client.GetBlock("0x953f6efa29c740b68c87e0a060942056382a6912a0ddeddc2f6641acb92d9700");

            // get transaction
            RpcTransaction transaction = client.GetRawTransaction("0x48ec3d235c6b386eee324a77a10b0f9e8e37d3c1ebb99626f3d1dd70db26d788");

            Console.WriteLine($"{hash}\n{height}\n{block.ToJson().ToString()}\n{transaction.ToJson()}");
        }

        [TestMethod]
        public void PolicyMointor()
        {
            // choose a neo node with rpc opened
            PolicyAPI policyAPI = new PolicyAPI(new RpcClient("http://seed1t.neo.org:20332"));

            // get the accounts blocked by policy
            UInt160[] blockedAccounts = policyAPI.GetBlockedAccounts(); // [], no account is blocked by now

            // get the system fee per byte
            long feePerByte = policyAPI.GetFeePerByte(); // 1000, 0.00001000 GAS per byte

            // get the max size of one block
            uint maxBlockSize = policyAPI.GetMaxBlockSize(); // 262144, (1024 * 256) bytes one block

            // get the max transaction count per block
            uint maxTransactionsPerBlock = policyAPI.GetMaxTransactionsPerBlock(); // 512, max 512 transactions one block

            Console.WriteLine($"{JsonConvert.SerializeObject(blockedAccounts.Select(p => p.ToString()))}\n{feePerByte}\n{maxBlockSize}\n{maxTransactionsPerBlock}");
        }

        [TestMethod]
        public void ContractMointor()
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");

            // get contract state
            ContractState contractState = client.GetContractState(NativeContract.NEO.Hash.ToString());

            // get nep5 token info
            Nep5API nep5API = new Nep5API(client);
            UInt160 scriptHash = NativeContract.NEO.Hash;
            RpcNep5TokenInfo tokenInfo = nep5API.GetTokenInfo(scriptHash);

            // get nep5 name
            string name = nep5API.Name(scriptHash);

            // get nep5 symbol
            string symbol = nep5API.Symbol(scriptHash);

            // get nep5 token decimals
            uint decimals = nep5API.Decimals(scriptHash);

            // get nep5 token total supply
            BigInteger totalSupply = nep5API.TotalSupply(scriptHash);

            Console.WriteLine($"{contractState.ToJson().ToString()}\n\n{JsonConvert.SerializeObject(tokenInfo)}\n");
        }

        [TestMethod]
        public void ContractDeploy()
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");
            ContractClient contractClient = new ContractClient(client);

            // contract script, it should be from compiled file, we use empty byte[] in this example
            byte[] script = new byte[1];

            // we use default ContractManifest in this example
            ContractManifest manifest = ContractManifest.CreateDefault(script.ToScriptHash());

            // deploy contract needs sender to pay the system fee
            KeyPair senderKey = Utility.GetKeyPair("L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb");

            // create the deploy transaction
            Transaction transaction = contractClient.CreateDeployContractTx(script, manifest, senderKey);

            // Broadcasts the transaction over the NEO network
            client.SendRawTransaction(transaction);
            Console.WriteLine($"Transaction {transaction.Hash.ToString()} is broadcasted!");

            // print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(client);
            neoAPI.WaitTransaction(transaction)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block height {await p}"));
        }

        [TestMethod]
        public void ContractInvoke()
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");
            ContractClient contractClient = new ContractClient(client);

            // get the contract hash
            UInt160 scriptHash = NativeContract.NEO.Hash;

            // test invoking the method provided by the contract 
            string name = contractClient.TestInvoke(scriptHash, "name")
                .Stack.Single().ToStackItem().GetString();

            // contract script, it should be from compiled file, we use empty byte[] in this example
            byte[] script = new byte[1];

            // we use default ContractManifest in this example
            ContractManifest manifest = ContractManifest.CreateDefault(script.ToScriptHash());

            // deploy contract needs sender to pay the system fee
            KeyPair senderKey = Utility.GetKeyPair("L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb");

            // create the deploy transaction
            Transaction transaction = contractClient.CreateDeployContractTx(script, manifest, senderKey);

            // Broadcasts the transaction over the NEO network
            client.SendRawTransaction(transaction);
            Console.WriteLine($"Transaction {transaction.Hash.ToString()} is broadcasted!");

            // print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(client);
            neoAPI.WaitTransaction(transaction)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block {(await p).BlockHash}"));
        }

        [TestMethod]
        public void WalletAPI()
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");
            WalletAPI walletAPI = new WalletAPI(client);

            // get the token balance of account
            string tokenHash = "0x43cf98eddbe047e198a3e5d57006311442a0ca15";
            string address = "AJoQgnkK1i7YSAvFbPiPhwtgdccbaQ7rgq";
            BigInteger tokenBalance = walletAPI.GetTokenBalance(tokenHash, address);

            // get the neo balance
            uint neoBalance = walletAPI.GetNeoBalance(address);

            // get the neo balance
            decimal gasBalance = walletAPI.GetGasBalance(address);

            // get the claimable GAS of one address
            decimal gasAmount = walletAPI.GetUnclaimedGas(address);
            Console.WriteLine(gasAmount);

            // claiming gas needs the KeyPair of account
            string wif = "L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb";
            Transaction transaction = walletAPI.ClaimGas(wif);

            // transfer 10 neo from wif to address
            walletAPI.Transfer(tokenHash, wif, address, 10);

            // print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(client);
            neoAPI.WaitTransaction(transaction)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block {(await p).BlockHash}"));
        }

    }
}
