﻿using Neo.IO.Json;
using Neo.Network.P2P.Payloads;

namespace Neo.SDK.RPC.Model
{
    public class SDK_Transaction
    {
        public Transaction Transaction { get; set; }

        public UInt256 BlockHash { get; set; }

        public int? Confirmations { get; set; }

        public uint? BlockTime { get; set; }

        public JObject ToJson()
        {
            JObject json = Transaction.ToJson();
            if (Confirmations != null)
            {
                json["blockhash"] = BlockHash.ToString();
                json["confirmations"] = Confirmations;
                json["blocktime"] = BlockTime;
            }
            return json;
        }

        public static SDK_Transaction FromJson(JObject json)
        {
            SDK_Transaction transaction = new SDK_Transaction();
            transaction.Transaction = Transaction.FromJson(json);
            if (json["confirmations"] != null)
            {
                transaction.BlockHash = UInt256.Parse(json["blockhash"].AsString());
                transaction.Confirmations = (int)json["confirmations"].AsNumber();
                transaction.BlockTime = (uint)json["blocktime"].AsNumber();
            }
            return transaction;
        }
    }


}
