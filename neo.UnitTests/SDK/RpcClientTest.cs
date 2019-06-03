using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.IO.Json;
using Neo.SDK;
using Neo.SDK.RPC;
using Neo.SDK.RPC.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.UnitTests.SDK
{
    [TestClass]
    public class RpcClientTest
    {

        [TestMethod]
        public void TestGetAccountState()
        {
            var helper = new RpcHelper("https://seed1.neo.org:10331");
            var light = new NeoLight(helper);
            var request = new RPCRequest
            {
                Id = 1,
                Jsonrpc = "2.0",
                Method = "getaccountstate",
                Params = new[] { "AJBENSwajTzQtwyJFkiJSv7MAaaMc7DsRz" }
            };
            var response = light.GetAccountState(request);
            Assert.AreEqual(0, response.Result.Version);

        }


    }



}
