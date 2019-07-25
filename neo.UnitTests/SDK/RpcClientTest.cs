using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.SDK;
using Neo.SDK.RPC;
using Neo.SDK.RPC.Model;
using Neo.VM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Neo.SDK.Wallet.WalletFile;

namespace Neo.UnitTests.SDK
{
    [TestClass]
    public class RpcClientTest
    {
        RpcClient rpc;
        Mock<HttpMessageHandler> handlerMock;

        [TestInitialize]
        public void TestSetup()
        {
            handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            // use real http client with mocked handler here
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://seed1.neo.org:10331"),
            };

            var helper = new HttpService(httpClient);
            rpc = new RpcClient(helper);
        }

        private void MockResponse(string content)
        {
            handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(content),
               })
               .Verifiable();
        }

        [TestMethod]
        public void TestGetAccountState()
        {
            MockResponse(@"{
    ""jsonrpc"": ""2.0"",
    ""id"": 1,
    ""result"": {
                ""version"": 0,
        ""script_hash"": ""0x1179716da2e9523d153a35fb3ad10c561b1e5b1a"",
        ""frozen"": false,
        ""votes"": [],
        ""balances"": [
            {
                ""asset"": ""0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b"",
                ""value"": ""94""
            }
        ]
    }
    }");
            var response = rpc.GetAccountState("AJBENSwajTzQtwyJFkiJSv7MAaaMc7DsRz");
            Assert.AreEqual("0x1179716da2e9523d153a35fb3ad10c561b1e5b1a", response.ScriptHash);
        }

        [TestMethod]
        public void TestTransactionDes()
        {
            Transaction tx = Transaction.DeserializeFrom("80000005baa46f629740c0951ac5e453d42d41390b2f8e2518091c224e78c55021dc67dc0000f7e9ac305c544de883edd61c26912499add70089e77e4622055f26eda209e15b00004736b13b042ded3e8557fd967cc283e9ff4003ef3cb21f1cf123f47f7007e7610000bb26c75cb9f1551a9ba50655e858f5bef3d396890bcb269f8cd189667d880a5d000066df6b24283b4287ad0ffabdd924d17b512d58d23630a0e0274891b86d063e940000029b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc500c817a80400000071ce86222b86f79d0bb6fdf44c38fb19d910a18b9b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc50027b929000000009854ab65ea574a66bb8534cda6fbc1ac4251e89d044140c53580c4f3ef91ef4ab51853011912260d5f147f6f99f9e120b2d7e7093a5a614ce00175e4b7341c06d79fc7bf67e029dbc8ce4e95704c9dd7ed8bf4e5a635c6232102781faba071b6f6c0ddda07290dfe58573cfe08635cdec25c95a99814a8edc1d8ac4140e300519a01f491d466aa10842d8961ef2d641ec92f25ab8953646d358aed29c4e83663270fb68869f4066887f71e42d65fabc2cad5407ec61f9ca99e4ce29884232102f9eab6679a85f43c5dc773d0229d8fa71c967f55b7c13ba9ccb7c661351db847ac41405188974c7ec407d1d4d32a00282f8ef0bb28041a45d69a417dce9128b66d97499cb9f028f532dd3da635e1f08bae744e8325be1a9e418b0f30007370b42cd6f123210265e3b1da66b06b776bd28eacd58d81ae7b89d7931aec868e66740b2379757684ac4140cadec96d981cedf5488ef589298ee9b2ca9d67a319c9e0571b8a297c65e677426f5ba1ac49c0645aa1d51d7479090517532b8ae1027a9bfb547c60b0ecf61e5b232103517d3ccd3e1ebe1801529bc90d5a5ad9499d5be42edb2d20d863413c10a27aebac".HexToBytes());
            var hashs = tx.Witnesses.Select(p => p.ScriptHash).ToList();
            var ordered = hashs.OrderBy(p => p).ToList();
            Assert.AreEqual(hashs[0], ordered[0]);


        }

        [TestMethod]
        public void TestScriptBuilder()
        {
            byte[] script;
            UInt160 asset_id_160 = UInt160.Parse("0xeb8dee66910af9f21caaf4d5d7fd33187666ff1f");
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitAppCall(asset_id_160, "mintTokens");
                script = sb.ToArray();
            }

            string hex = script.ToHexString();

        }


    }



}
