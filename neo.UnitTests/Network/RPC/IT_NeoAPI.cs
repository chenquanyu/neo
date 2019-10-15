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
        NeoAPI neoAPI;

        [TestInitialize]
        public void TestSetup()
        {
            key1 = "L1rFMTamZj85ENnqNLwmhXKAprHuqr1MxMHmCWCGiXGsAdQ2dnhb".ToKeyPair();
            key2 = "L3TbPZ3Gtqh3TTk2CWn44m9iiuUhBGZWoDJQuvVw5Zbx5NAjPbdb".ToKeyPair();
            address1 = Neo.Wallets.Helper.ToAddress(key1.ToScriptHash()); // "AJoQgnkK1i7YSAvFbPiPhwtgdccbaQ7rgq"
            address2 = Neo.Wallets.Helper.ToAddress(key2.ToScriptHash()); // "AKviBGFhWeS8xrAH3hqDQufZXE9QM5pCeP"
            neoAPI = new NeoAPI(new RpcClient("http://127.0.0.1:20332"));
        }
        [TestMethod]
        public void IntegrationTestTransfer()
        {
            Assert.AreEqual(1000u, neoAPI.GetNeoBalance(address1) + neoAPI.GetNeoBalance(address2));
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
            RpcClient client = new RpcClient("http://127.0.0.1:20332");
            NeoAPI neoAPI = new NeoAPI(client);

            var neoInfo = neoAPI.Nep5API.GetTokenInfo(NativeContract.NEO.Hash);
            var gasInfo = neoAPI.Nep5API.GetTokenInfo(NativeContract.GAS.Hash);
            var feePerByte = neoAPI.PolicyAPI.GetFeePerByte();

            Assert.AreEqual(1_00000000, neoInfo.TotalSupply);
            Assert.AreEqual("GAS", gasInfo.Name);
            Assert.AreEqual(1000L, feePerByte);
        }
    }
}
