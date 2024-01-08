using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Majiang.AI.Editor
{
    public static class TestPlay
    {
        public static void Run(TestPlayOptions options, View view = null)
        {
            var players = new List<Majiang.Player>{ null, null, null, null };
            for (int i = 0; i < 4; i++)
            {
                players[i] = new AI.Player();
            }

            var script = get_shan(options.Shan) ?? new List<string>();
            for (int i = 0; i < options.Skip; i++) script.Shift();

            var rule = options.Rule;

            int times = options.Times;

            var paipu = new List<Paipu>();
            Action<Paipu> callback = (log) =>
            {
                paipu.Add(log);
                if (!options.OutputPath.IsNullOrEmpty())
                {
                    using (StreamWriter writer = new StreamWriter(options.OutputPath, false, Encoding.UTF8))
                    {
                        var json = JsonConvert.SerializeObject(log, new JsonSerializerSettings()
                        {
                            Formatting = Formatting.Indented,
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        writer.WriteLine(json);
                    }
                }
            };
            Debug.Log($"[{times}] {DateTime.Now.ToLongTimeString()}");

            while (times > 0)
            {
                --times;
                string s = script.Shift();
                if (!s.IsNullOrEmpty())
                {
                    // TODO: parse shan
                    throw new NotImplementedException();
                }
                else
                {
                    var game = new Game(players, callback, rule);
                    game.view = view ?? new DebugLogView();
                    game._model.title += $" #{paipu.Count}";
                    game.do_sync();
                    Debug.Log($"[{times}] {DateTime.Now.ToLongTimeString()} {game._paipu.rank[0]} {game._paipu.point[0]}");
                }
            }
        }

        private static List<string> get_shan(string text)
        {
            return null;
        }
    }
}