﻿using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.Network.RPC.Models;
using Neo.SDK.SC;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;
using System;

namespace Neo.SDK.TX
{
    /// <summary>
    /// This class helps to create transactions manually.
    /// </summary>
    public class TxManager
    {
        private static readonly Random rand = new Random();
        private readonly RpcClient rpcClient;
        private readonly UInt160 sender;

        public Transaction Tx { private set; get; }

        public TransactionContext Context { private set; get; }

        public TxManager(RpcClient neoRpc, UInt160 sender)
        {
            rpcClient = neoRpc;
            this.sender = sender;
        }

        /// <summary>
        /// Create an unsigned Transaction object with given parameters.
        /// will set 
        /// </summary>
        public TxManager MakeTransaction(TransactionAttribute[] attributes, byte[] script, long networkFee = 0)
        {
            uint height = rpcClient.GetBlockCount() - 1;
            Tx = new Transaction
            {
                Version = 0,
                Nonce = (uint)rand.Next(),
                Script = script,
                Sender = sender,
                ValidUntilBlock = height + Transaction.MaxValidUntilBlockIncrement,
                Attributes = attributes ?? new TransactionAttribute[0],
                Witnesses = new Witness[0]
            };

            RpcInvokeResult result = rpcClient.InvokeScript(script);
            Tx.SystemFee = Math.Max(long.Parse(result.GasConsumed) - ApplicationEngine.GasFree, 0);
            if (Tx.SystemFee > 0)
            {
                long d = (long)NativeContract.GAS.Factor;
                long remainder = Tx.SystemFee % d;
                if (remainder > 0)
                    Tx.SystemFee += d - remainder;
                else if (remainder < 0)
                    Tx.SystemFee -= remainder;
            }

            Context = new TransactionContext(Tx);

            // set networkfee to least cost when networkFee is 0
            Tx.NetworkFee = networkFee == 0 ? EstimateNetwotkFee() : networkFee;

            var gasBalance = new Nep5API(rpcClient).BalanceOf(NativeContract.GAS.Hash, sender);
            if (gasBalance >= Tx.SystemFee + Tx.NetworkFee) return this;
            throw new InvalidOperationException("Insufficient GAS");
        }

        /// <summary>
        /// Estimate NetwotkFee, assuming the witnesses are single Signature
        /// </summary>
        private long EstimateNetwotkFee()
        {
            long networkFee = 0;
            UInt160[] hashes = Tx.GetScriptHashesForVerifying(null);
            int size = Transaction.HeaderSize + Tx.Attributes.GetVarSize() + Tx.Script.GetVarSize() + IO.Helper.GetVarSize(hashes.Length);

            // assume the hashes are single Signature
            foreach (var hash in hashes)
            {
                size += 166;
                networkFee += ApplicationEngine.OpCodePrices[OpCode.PUSHBYTES64] + ApplicationEngine.OpCodePrices[OpCode.PUSHBYTES33] + InteropService.GetPrice(InteropService.Neo_Crypto_CheckSig, null);
            }

            networkFee += size * new PolicyAPI(rpcClient).GetFeePerByte();
            return networkFee;
        }

        /// <summary>
        /// Calculate NetworkFee with context items
        /// </summary>
        private long CalculateNetworkFee()
        {
            long networkFee = 0;
            UInt160[] hashes = Tx.GetScriptHashesForVerifying(null);
            int size = Transaction.HeaderSize + Tx.Attributes.GetVarSize() + Tx.Script.GetVarSize() + IO.Helper.GetVarSize(hashes.Length);
            foreach (UInt160 hash in hashes)
            {
                byte[] witness_script = Context.GetScript(hash);
                if (witness_script is null)
                {
                    try
                    {
                        witness_script = rpcClient.GetContractState(hash.ToString())?.Script;
                    }
                    catch { }
                }

                if (witness_script is null) continue;

                networkFee += Wallet.CalculateNetWorkFee(witness_script, ref size);
            }
            networkFee += size * new PolicyAPI(rpcClient).GetFeePerByte();
            return networkFee;
        }

        /// <summary>
        /// Add Signature
        /// </summary>
        public TxManager AddSignature(KeyPair key)
        {
            var contract = Contract.CreateSignatureContract(key.PublicKey);

            byte[] signature = Tx.Sign(key);
            if (!Context.AddSignature(contract, key.PublicKey, signature))
            {
                throw new Exception("AddSignature failed!");
            }

            return this;
        }

        /// <summary>
        /// Add Multi-Signature
        /// </summary>
        public TxManager AddMultiSig(KeyPair key, params ECPoint[] publicKeys)
        {
            Contract contract = Contract.CreateMultiSigContract(publicKeys.Length, publicKeys);

            byte[] signature = Tx.Sign(key);
            if (!Context.AddSignature(contract, key.PublicKey, signature))
            {
                throw new Exception("AddMultiSig failed!");
            }

            return this;
        }

        /// <summary>
        /// Add Witness with contract
        /// </summary>
        public TxManager AddWitness(Contract contract, params object[] parameters)
        {
            if (!Context.Add(contract, parameters))
            {
                throw new Exception("AddWitness failed!");
            };
            return this;
        }

        /// <summary>
        /// Add Witness with scriptHash
        /// </summary>
        public TxManager AddWitness(UInt160 scriptHash, params object[] parameters)
        {
            var contract = Contract.Create(scriptHash);
            return AddWitness(contract, parameters);
        }

        /// <summary>
        /// Verify Witness count and add witnesses
        /// </summary>
        public TxManager Sign()
        {
            // Verify witness count
            if (!Context.Completed)
            {
                throw new Exception($"Please add signature or witness first!");
            }

            // Calculate NetworkFee
            long leastNetworkFee = CalculateNetworkFee();
            if (Tx.NetworkFee < leastNetworkFee)
            {
                throw new InvalidOperationException("Insufficient NetworkFee");
            }

            Tx.Witnesses = Context.GetWitnesses();
            return this;
        }
    }
}
