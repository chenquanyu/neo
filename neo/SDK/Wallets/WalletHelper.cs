using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.Wallets;
using Neo.Wallets.NEP6;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using static Neo.SDK.Wallets.WalletFile;

namespace Neo.SDK.Wallets
{
    public class WalletHelper
    {
        public static string DefaultLabel = "NeoSdkAccount";
        public static ScryptParams ScryptDefault { get; } = new ScryptParams { N = 16384, R = 8, P = 8 };

        public KeyPair CreateKeyPair()
        {
            byte[] privateKey = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(privateKey);
            }
            return new KeyPair(privateKey);
        }

        public Account CreateAccount(string label, string password, KeyPair pair, int n, int r, int p)
        {
            return new Account
            {
                Address = pair.PublicKeyHash.ToAddress(),
                Label = label,
                Key = pair.Export(password, n, r, p),
                Contract = new WalletFile.Contract
                {
                    Script = SmartContract.Contract.CreateSignatureRedeemScript(pair.PublicKey).ToHexString(),
                    Parameters = new[] { new WalletFile.ContractParameter {
                        Name = "signature",
                        Type = ContractParameterType.Signature.ToString()
                    } },
                    Deployed = false
                },
                Extra = null,
                IsDefault = true,
                Lock = false
            };
        }

        public Account CreateAccount(string password, KeyPair pair)
        {
            return CreateAccount(DefaultLabel, password, pair, ScryptDefault.N, ScryptDefault.R, ScryptDefault.P);
        }

        public WalletFile CreateWallet()
        {
            throw new NotImplementedException();
        }

    }
}
