using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.IO.Json;
using Neo.SDK;
using Neo.SDK.RPC;
using Neo.SDK.RPC.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Neo.UnitTests.SDK
{
    [TestClass]
    public class RpcIntergationTest
    {
        RpcClient rpc;

        [TestInitialize]
        public void TestSetup()
        {
            var helper = new HttpService("https://seed1.neo.org:10331");
            rpc = new RpcClient(helper);
        }

        [TestMethod]
        public void TestGetAccountState()
        {
            var response = rpc.GetAccountState("AJBENSwajTzQtwyJFkiJSv7MAaaMc7DsRz");
            Assert.AreEqual(0, response.Version);
        }


        [TestMethod]
        [ExpectedException(typeof(NeoSdkException))]
        public void TestSendRawTransaction()
        {
            var response = rpc.SendRawTransaction("80000001195876cb34364dc38b730077156c6bc3a7fc570044a66fbfeeea56f71327e8ab0000029b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc500c65eaf440000000f9a23e06f74cf86b8827a9108ec2e0f89ad956c9b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc50092e14b5e00000030aab52ad93f6ce17ca07fa88fc191828c58cb71014140915467ecd359684b2dc358024ca750609591aa731a0b309c7fb3cab5cd0836ad3992aa0a24da431f43b68883ea5651d548feb6bd3c8e16376e6e426f91f84c58232103322f35c7819267e721335948d385fae5be66e7ba8c748ac15467dcca0693692dac");
            Assert.AreEqual(false, response);
        }


    }



}
