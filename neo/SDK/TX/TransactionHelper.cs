using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.SDK.RPC;
using Neo.SDK.RPC.Model;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using Neo.Persistence;

namespace Neo.SDK.TX
{
    /// <summary>
    /// This class helps to create transactions manually.
    /// </summary>
    public class TransactionHelper
    {
        private readonly INeoRpc _neoRpc;

        public TransactionHelper(INeoRpc neoRpc)
        {
            _neoRpc = neoRpc;
        }

        /// <summary>
        /// Call API method to get unspent coins from a specific address.
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="amount"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        protected Coin[] GetUnspentCoins(UInt256 assetId, Fixed8 amount, UInt160 from)
        {
            string fromStr = from.ToAddress();
            UtxoBalance balance = _neoRpc.GetUnspents(fromStr).Balances.Where(b => UInt256.Parse(b.AssetHash) == assetId).SingleOrDefault();
            if (balance == default(UtxoBalance)) return null;
            Coin[] coins = balance.Unspents.Select(u => new Coin()
                {
                    Reference = new CoinReference() { PrevHash = UInt256.Parse(u.TxId), PrevIndex = ushort.Parse(u.N.ToString()) },
                    Output = new TransactionOutput() { AssetId = assetId, ScriptHash = from, Value = Fixed8.FromDecimal(u.Value) }
                }).ToArray();
            Fixed8 sum = coins.Sum(p => p.Output.Value);
            if (sum < amount) return null;
            if (sum == amount) return coins;
            Coin[] coins_ordered = coins.OrderByDescending(p => p.Output.Value).ToArray();
            int i = 0;
            while (coins_ordered[i].Output.Value <= amount)
                amount -= coins_ordered[i++].Output.Value;
            if (amount == Fixed8.Zero)
                return coins_ordered.Take(i).ToArray();
            else
                return coins_ordered.Take(i).Concat(new[] { coins_ordered.Last(p => p.Output.Value >= amount) }).ToArray();
        }

        /// <summary>
        /// Call API method to get the balance of a specific NEP-5 token from a specific address.
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        protected BigInteger GetNep5Balance(UInt160 assetId, UInt160 from)
        {
            string fromStr = from.ToAddress();
            string balance = _neoRpc.GetNep5Balances(fromStr).Balances.Where(n => UInt160.Parse(n.AssetHash) == assetId).First().Amount;
            return BigInteger.Parse(balance);
        }

        /// <summary>
        /// Call API method to get claimable gas for a specific address.
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        protected CoinReference[] GetClaimableCoins(UInt160 to)
        {
            string toStr = to.ToAddress();
            CoinReference[] claimables = _neoRpc.GetClaimable(toStr).Claimables.Select(p => new CoinReference()
            {
                PrevHash = UInt256.Parse(p.TxId),
                PrevIndex = ushort.Parse(p.N.ToString())
            }).ToArray();

            return claimables;
        }

        //=========================================================================================================

        public ContractTransaction CreateContractTransaction(UInt256 assetId, UInt160 from, UInt160 to, Fixed8 amount, 
            UInt160 changeAddress = null, Fixed8 fee = default(Fixed8), List<TransactionAttribute> attributes = null)
        {
            ContractTransaction ctx = new ContractTransaction();

            ctx.Inputs = new CoinReference[0];
            ctx.Outputs = new TransactionOutput[] { new TransactionOutput() { AssetId = assetId, Value = amount, ScriptHash = to } };
            ctx.Attributes = attributes==null ? new List<TransactionAttribute>().ToArray() : attributes.ToArray();
            ctx.Witnesses = new Witness[0];

            ctx = MakeTransaction(ctx, from, changeAddress, fee);
            return ctx;
        }

        public InvocationTransaction CreateInvocationTransaction(UInt160 assetId, UInt160 from, UInt160 to, BigDecimal amount, 
            UInt160 changeAddress = null, Fixed8 fee = default(Fixed8), List<TransactionAttribute> attributes = null)
        {
            InvocationTransaction tx;
            HashSet<UInt160> sAttributes = new HashSet<UInt160>();
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                //byte[] script;

                //using (ScriptBuilder sb2 = new ScriptBuilder())
                //{
                //    sb2.EmitAppCall(assetId, "balanceOf", from);
                //    script = sb2.ToArray();
                //}
                //ApplicationEngine engine = ApplicationEngine.Run(script);
                //if (engine.State.HasFlag(VMState.FAULT)) return null;
                //BigInteger balance = engine.ResultStack.Pop().GetBigInteger();

                BigInteger balance = GetNep5Balance(assetId, from);
                if (balance < amount.Value) return null;
                BigInteger change = balance - amount.Value;
                sAttributes.Add(from);
                if (change > 0 && changeAddress != null)
                {
                    sb.EmitAppCall(assetId, "transfer", from, changeAddress, change);
                    sb.Emit(OpCode.THROWIFNOT);
                }
                sb.EmitAppCall(assetId, "transfer", from, to, amount);
                sb.Emit(OpCode.THROWIFNOT);
                byte[] nonce = new byte[8];
                Random rand = new Random();
                rand.NextBytes(nonce);
                sb.Emit(OpCode.RET, nonce);
                tx = new InvocationTransaction
                {
                    Version = 1,
                    Script = sb.ToArray()
                };
            }
            if (attributes == null) attributes = new List<TransactionAttribute>();
            attributes.AddRange(sAttributes.Select(p => new TransactionAttribute
            {
                Usage = TransactionAttributeUsage.Script,
                Data = p.ToArray()
            }));
            tx.Attributes = attributes.ToArray();
            tx.Inputs = new CoinReference[0];
            tx.Outputs = new TransactionOutput[0];
            tx.Witnesses = new Witness[0];
            ApplicationEngine engine = ApplicationEngine.Run(tx.Script, tx);
            if (engine.State.HasFlag(VMState.FAULT)) return null;
            InvocationTransaction itx = new InvocationTransaction
            {
                Version = tx.Version,
                Script = tx.Script,
                Gas = InvocationTransaction.GetGas(engine.GasConsumed),
                Attributes = tx.Attributes,
                Inputs = tx.Inputs,
                Outputs = tx.Outputs
            };
            tx = MakeTransaction(itx, from, changeAddress, fee);
            return tx;
        }

        public ClaimTransaction CreateClaimTransaction(UInt160 to)
        {
            CoinReference[] claims = GetClaimableCoins(to);
            if (claims.Length == 0) return null;
            const int MAX_CLAIMS_AMOUNT = 50;
            ClaimTransaction tx;
            using (Snapshot snapshot = Blockchain.Singleton.GetSnapshot())
            {
                tx = new ClaimTransaction
                {
                    Claims = claims.ToArray(),
                    Attributes = new TransactionAttribute[0],
                    Inputs = new CoinReference[0],
                    Outputs = new[]
                    {
                        new TransactionOutput
                        {
                            AssetId = Blockchain.UtilityToken.Hash,
                            Value = snapshot.CalculateBonus(claims.Take(MAX_CLAIMS_AMOUNT)),
                            ScriptHash = to
                        }
                    }
                };
            }
            return tx;
        }

        private T MakeTransaction<T>(T tx, UInt160 from, UInt160 change_address = null, Fixed8 fee = default(Fixed8)) where T : Transaction
        {
            if (tx.Outputs == null) tx.Outputs = new TransactionOutput[0];
            if (tx.Attributes == null) tx.Attributes = new TransactionAttribute[0];
            fee += tx.SystemFee;
            var pay_total = (typeof(T) == typeof(IssueTransaction) ? new TransactionOutput[0] : tx.Outputs).GroupBy(p => p.AssetId, (k, g) => new
            {
                AssetId = k,
                Value = g.Sum(p => p.Value)
            }).ToDictionary(p => p.AssetId);

            if (fee > Fixed8.Zero)
            {
                if (pay_total.ContainsKey(Blockchain.UtilityToken.Hash))
                {
                    pay_total[Blockchain.UtilityToken.Hash] = new
                    {
                        AssetId = Blockchain.UtilityToken.Hash,
                        Value = pay_total[Blockchain.UtilityToken.Hash].Value + fee
                    };
                }
                else
                {
                    pay_total.Add(Blockchain.UtilityToken.Hash, new
                    {
                        AssetId = Blockchain.UtilityToken.Hash,
                        Value = fee
                    });
                }
            }
            var pay_coins = pay_total.Select(p => new
            {
                AssetId = p.Key,
                Unspents = GetUnspentCoins(p.Key, p.Value.Value, from)
            }).ToDictionary(p => p.AssetId);
            if (pay_coins.Any(p => p.Value.Unspents == null)) return null;
            var input_sum = pay_coins.Values.ToDictionary(p => p.AssetId, p => new
            {
                p.AssetId,
                Value = p.Unspents.Sum(q => q.Output.Value)
            });
            if (change_address == null) change_address = from;
            List<TransactionOutput> outputs_new = new List<TransactionOutput>(tx.Outputs);
            foreach (UInt256 asset_id in input_sum.Keys)
            {
                if (input_sum[asset_id].Value > pay_total[asset_id].Value)
                {
                    outputs_new.Add(new TransactionOutput
                    {
                        AssetId = asset_id,
                        Value = input_sum[asset_id].Value - pay_total[asset_id].Value,
                        ScriptHash = change_address
                    });
                }
            }
            tx.Inputs = pay_coins.Values.SelectMany(p => p.Unspents).Select(p => p.Reference).ToArray();
            tx.Outputs = outputs_new.ToArray();
            return tx;
        }
    }
}