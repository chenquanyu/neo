using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Network.RPC;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.Wallets;

namespace Neo.UnitTests.Network.RPC
{
    [TestClass]
    public class IT_NeoAPI
    {
        KeyPair key1;
        KeyPair key2;
        string address1;
        string address2;
        WalletAPI neoAPI;

        [TestInitialize]
        public void TestSetup()
        {
            key1 = Utility.GetKeyPair("L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb");
            key2 = Utility.GetKeyPair("L3TbPZ3Gtqh3TTk2CWn44m9iiuUhBGZWoDJQuvVw5Zbx5NAjPbdb");
            address1 = Neo.Wallets.Helper.ToAddress(Contract.CreateSignatureRedeemScript(key1.PublicKey).ToScriptHash()); // "AJoQgnkK1i7YSAvFbPiPhwtgdccbaQ7rgq"
            address2 = Neo.Wallets.Helper.ToAddress(Contract.CreateSignatureRedeemScript(key2.PublicKey).ToScriptHash()); // "AKviBGFhWeS8xrAH3hqDQufZXE9QM5pCeP"
            neoAPI = new WalletAPI(new RpcClient("http://seed1t.neo.org:20332"));
        }
        [TestMethod]
        public void IntegrationTestTransfer()
        {
            Assert.AreEqual(900u, neoAPI.GetNeoBalance(address1) + neoAPI.GetNeoBalance(address2));
            var trans = neoAPI.Transfer(NativeContract.NEO.Hash.ToString(), "L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb", address2, 1);

            //var balanceBefore = neoAPI.GetNeoBalance(address1);
            //var trans = neoAPI.Transfer(NativeContract.NEO.Hash.ToString(), "L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb", address2, 1);
            //uint height = neoAPI.WaitTransaction(trans).Result;
            //Assert.AreEqual(1u, balanceBefore - neoAPI.GetNeoBalance(address1));
        }

        [TestMethod]
        public void IntegrationTestClaimGas()
        {
            Assert.IsTrue(neoAPI.GetUnclaimedGas(address1) > 0);
        }

        [TestMethod]
        public void IntegrationTestGetTokenInfo()
        {
            RpcClient client = new RpcClient("http://seed1t.neo.org:20332");
            Nep5API nep5API = new Nep5API(client);
            PolicyAPI policyAPI = new PolicyAPI(client);

            var neoInfo = nep5API.GetTokenInfo(NativeContract.NEO.Hash);
            var gasInfo = nep5API.GetTokenInfo(NativeContract.GAS.Hash);
            var feePerByte = policyAPI.GetFeePerByte();

            Assert.AreEqual(1_00000000, neoInfo.TotalSupply);
            Assert.AreEqual("GAS", gasInfo.Name);
            Assert.AreEqual(1000L, feePerByte);
        }
    }
}
