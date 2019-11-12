using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.SmartContract;
using Neo.Wallets;
using System;

namespace Neo.UnitTests
{
    [TestClass]
    public class UT_Keys
    {
        readonly string privateKey1 = "831cb932167332a768f1c898d2cf4586a14aa606b7f078eba028c849c306cce6";
        readonly string privateKey2 = "82a4ff38f5de304c2fae62d1a736c343816412f7e4fe3badaf5e1e940c8f07c3";
        readonly string privateKey3 = "31ab808b68c25377b2068500e264f164d1d75eda748a8e0a98db4c74db181b66";

        [TestMethod]
        public void GetHashData()
        {
            KeyPair keyPair1 = new KeyPair(privateKey1.HexToBytes());
            KeyPair keyPair2 = new KeyPair(privateKey2.HexToBytes());
            KeyPair keyPair3 = new KeyPair(privateKey3.HexToBytes());

            byte[] script = Contract.CreateMultiSigRedeemScript(2, keyPair1.PublicKey, keyPair2.PublicKey, keyPair3.PublicKey);

            Console.WriteLine(script.ToHexString());
        }


    }
}
