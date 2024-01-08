using System.Collections.Generic;
using NUnit.Framework;

namespace Majiang.AI.Test
{
    public class PlayerTest
    {
        Reply _reply;

        public void reply(Reply msg = null)
        {
            _reply = msg ?? new Reply();
        }

        public class PlayerParam
        {
            public Majiang.Rule rule;
            public int? menfeng;
            public string shoupai;
            public string baopai;
        }

        public Player init_player(PlayerParam param = null)
        {
            if (param == null) param = new PlayerParam();

            Player player = new Player();

            var kaiju = new Kaiju
            {
                id = 0,
                rule = new Majiang.Rule(),
                title = "タイトル",
                player = new List<string> { "私", "下家", "対面", "上家" },
                qijia = 0
            };
            var qipai = new Qipai
            {
                zhuangfeng = 0,
                jushu = 0,
                changbang = 0,
                lizhibang = 0,
                defen = new List<int> { 25000, 25000, 25000, 25000 },
                baopai = "m1",
                shoupai = new List<string> { "", "", "", "" }
            };

            if (param.rule != null) kaiju.rule = param.rule;

            if (param.menfeng != null) qipai.jushu = (4 - param.menfeng.Value) % 4;
            int menfeng = (kaiju.id + 4 - kaiju.qijia + 4 - qipai.jushu) % 4;
            qipai.shoupai[menfeng] = param.shoupai != null ? param.shoupai : "m123p456s789z1123";
            if (param.baopai != null) qipai.baopai = param.baopai;

            player.action(new Message { kaiju = kaiju });
            player.action(new Message { qipai = qipai });

            _reply = null;

            return player;
        }

        public void set_dapai(Player player, int l, List<string> dapai) {
            foreach (var p in dapai) {
                player._suanpai.dapai(new Dapai { l = l, p = p });
            }
        }

        [Test, Description("action_kaiju(kaiju)")]
        public void TestActionKaiju()
        {
            var kaiju = new Message { kaiju = new Kaiju { id = 0, rule = new Majiang.Rule(), title = "タイトル", player = new List<string> { "私", "下家", "対面", "上家" }, qijia = 0 } };

            // 卓情報を設定すること
            var player = new Player();
            player.action(kaiju);
            Assert.AreEqual(player._model.title, "タイトル");

            // 応答を返すこと
            _reply = null;
            player.action(kaiju, reply);
            Assert.NotNull(_reply);
        }

        [Test, Description("action_qipai(qipai)")]
        public void TestActionQipai()
        {
            var kaiju = new Message { kaiju = new Kaiju { id = 0, rule = new Majiang.Rule(), title = "タイトル", player = new List<string> { "私", "下家", "対面", "上家" }, qijia = 0 } };
            var qipai = new Message { qipai = new Qipai { zhuangfeng = 0, jushu = 0, changbang = 0, lizhibang = 0, defen = new List<int> { 25000, 25000, 25000, 25000 }, baopai = "m1", shoupai = new List<string> { "m123p456s789z1123", "", "", "" } } };

            // 卓情報を設定すること
            var player = new Player();
            player.action(kaiju);
            player.action(qipai);
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s789z1123");

            // 牌数をカウントすること
            player = new Player();
            player.action(kaiju);
            player.action(qipai);
            Assert.AreEqual(player._suanpai._paishu, new Paishu {
                m = new List<int> { 1, 2, 3, 3, 4, 4, 4, 4, 4, 4 },
                p = new List<int> { 1, 4, 4, 4, 3, 3, 3, 4, 4, 4 },
                s = new List<int> { 1, 4, 4, 4, 4, 4, 4, 3, 3, 3 },
                z = new List<int> { 0, 2, 3, 3, 4, 4, 4, 4 }
            });

            // 応答を返すこと
            player = new Player();
            player.action(kaiju);
            _reply = null;
            player.action(qipai, reply);
            Assert.NotNull(_reply);
        }

        [Test, Description("action_zimo(zimo)")]
        public void TestActionZimo()
        {
            // 卓情報を設定すること
            var player = init_player();
            player.action(new Message { zimo = new Zimo { l = 0, p = "z1" } });
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s789z1123z1");

            // 牌数をカウントすること
            player = init_player();
            player.action(new Message { zimo = new Zimo { l = 0, p = "z1" } });
            Assert.AreEqual(player._suanpai._paishu, new Paishu {
                m = new List<int> { 1, 2, 3, 3, 4, 4, 4, 4, 4, 4 },
                p = new List<int> { 1, 4, 4, 4, 3, 3, 3, 4, 4, 4 },
                s = new List<int> { 1, 4, 4, 4, 4, 4, 4, 3, 3, 3 },
                z = new List<int> { 0, 1, 3, 3, 4, 4, 4, 4 }
            });

            // 応答を返すこと
            player = init_player();
            _reply = null;
            player.action(new Message { zimo = new Zimo { l = 0, p = "z1" } }, reply);
            Assert.NotNull(_reply);

            // 他者の手番では空応答を返すこと
            player = init_player();
            player.action(new Message { zimo = new Zimo { l = 1, p = "" } }, reply);
            Assert.AreEqual(_reply, new Reply {});

            // 槓自摸の場合
            player = init_player(new PlayerParam { shoupai = "m123p456s789z2,z1111" });
            player.action(new Message { gangzimo = new Zimo { l = 0, p = "z3" } }, reply);
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s789z2z3,z1111");
            Assert.AreEqual(player._n_gang, 1);
            Assert.NotNull(_reply);

            // 和了する
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1122" });
            _reply = null;
            player.action(new Message { zimo = new Zimo { l = 0, p = "z2" } }, reply);
            Assert.AreEqual(_reply, new Reply { hule = "-" });

            // 和了する(槓自摸)
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1,z222=2" });
            _reply = null;
            player.action(new Message { gangzimo = new Zimo { l = 0, p = "z1" } }, reply);
            Assert.AreEqual(_reply, new Reply { hule = "-" });

            // 九種九牌を選択する
            player = init_player(new PlayerParam { shoupai = "m19p234s56z123456" });
            _reply = null;
            player.action(new Message { zimo = new Zimo { l = 0, p = "z7" } }, reply);
            Assert.AreEqual(_reply, new Reply { daopai = "-" });

            // カンする
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1222" });
            _reply = null;
            player.action(new Message { zimo = new Zimo { l = 0, p = "z2" } }, reply);
            Assert.AreEqual(_reply, new Reply { gang = "z2222" });

            // 打牌する
            player = init_player(new PlayerParam { shoupai = "m26789p24s2449z57" });
            _reply = null;
            player.action(new Message { zimo = new Zimo { l = 0, p = "m4" } }, reply);
            Assert.AreEqual(_reply, new Reply { dapai = "z7" });
        }

        [Test, Description("action_dapai(dapai)")]
        public void TestActionDapai()
        {
            // 卓情報を設定すること
            var player = init_player();
            player.action(new Message { zimo = new Zimo { l = 0, p = "z1" } });
            player.action(new Message { dapai = new Dapai { l = 0, p = "z1_" } });
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s789z1123");

            // 牌数をカウントすること
            player = init_player();
            player.action(new Message { zimo = new Zimo { l = 1, p = "z1" } });
            player.action(new Message { dapai = new Dapai { l = 1, p = "z1_" } });
            Assert.AreEqual(player._suanpai._paishu, new Paishu {
                m = new List<int> { 1, 2, 3, 3, 4, 4, 4, 4, 4, 4 },
                p = new List<int> { 1, 4, 4, 4, 3, 3, 3, 4, 4, 4 },
                s = new List<int> { 1, 4, 4, 4, 4, 4, 4, 3, 3, 3 },
                z = new List<int> { 0, 1, 3, 3, 4, 4, 4, 4 }
            });

            // 応答を返すこと
            player = init_player();
            player.action(new Message { zimo = new Zimo { l = 1, p = "z2" } });
            _reply = null;
            player.action(new Message { dapai = new Dapai { l = 1, p = "z2_" } }, reply);
            Assert.NotNull(_reply);

            // 自身の手番では空応答を返すこと
            player = init_player();
            player.action(new Message { zimo = new Zimo { l = 0, p = "z1" } });
            _reply = null;
            player.action(new Message { dapai = new Dapai { l = 0, p = "z1_" } }, reply);
            Assert.AreEqual(_reply, new Reply {});

            // 和了する
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1122" });
            _reply = null;
            player.action(new Message { dapai = new Dapai { l = 1, p = "z1" } }, reply);
            Assert.AreEqual(_reply, new Reply { hule = "-" });

            // 副露する
            player = init_player(new PlayerParam { shoupai = "m123p456s578z1122" });
            _reply = null;
            player.action(new Message { dapai = new Dapai { l = 1, p = "z1" } }, reply);
            Assert.AreEqual(_reply, new Reply { fulou = "z111+" });

            // テンパイ宣言する(自分の手番)
            player = init_player(new PlayerParam { rule = new Majiang.Rule{ ノーテン宣言あり = true }, shoupai = "m123p456s789z11223" });
            while (player.shan.paishu > 0) player.shan.zimo();
            _reply = null;
            player.action(new Message { dapai = new Dapai { l = 0, p = "z3" } }, reply);
            Assert.AreEqual(_reply, new Reply { daopai = "-" });

            // テンパイ宣言する(他者の手番)
            player = init_player(new PlayerParam { rule = new Majiang.Rule{ ノーテン宣言あり = true }, shoupai = "m123p456s789z1122" });
            while (player.shan.paishu > 0) player.shan.zimo();
            _reply = null;
            player.action(new Message { dapai = new Dapai { l = 1, p = "z3" } }, reply);
            Assert.AreEqual(_reply, new Reply { daopai = "-" });
        }

        [Test, Description("action_fulou(fulou)")]
        public void TestActionFulou()
        {
            // 卓情報を設定すること
            var player = init_player();
            player.action(new Message { dapai = new Dapai { l = 1, p = "z1_" } });
            player.action(new Message { fulou = new Fulou { l = 0, m = "z111+" } });
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s789z23,z111+,");

            // 牌数をカウントすること
            player = init_player();
            player.action(new Message { dapai = new Dapai { l = 2, p = "z3_" } });
            player.action(new Message { fulou = new Fulou { l = 1, m = "z333+" } });
            Assert.AreEqual(player._suanpai._paishu, new Paishu {
                m = new List<int> { 1, 2, 3, 3, 4, 4, 4, 4, 4, 4 },
                p = new List<int> { 1, 4, 4, 4, 3, 3, 3, 4, 4, 4 },
                s = new List<int> { 1, 4, 4, 4, 4, 4, 4, 3, 3, 3 },
                z = new List<int> { 0, 2, 3, 0, 4, 4, 4, 4 }
            });

            // 応答を返すこと
            player = init_player();
            player.action(new Message { dapai = new Dapai { l = 1, p = "z1_" } });
            _reply = null;
            player.action(new Message { fulou = new Fulou { l = 0, m = "z111+" } }, reply);
            Assert.NotNull(_reply);

            // 他者の手番では空応答を返すこと
            player = init_player();
            player.action(new Message { dapai = new Dapai { l = 2, p = "z3_" } });
            _reply = null;
            player.action(new Message { fulou = new Fulou { l = 1, m = "z333+" } }, reply);
            Assert.AreEqual(_reply, new Reply {});

            // 打牌すること
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1123" });
            player.action(new Message { dapai = new Dapai { l = 1, p = "z1" } });
            _reply = null;
            player.action(new Message { fulou = new Fulou { l = 0, m = "z111+" } }, reply);
            Assert.AreEqual(_reply, new Reply { dapai = "z3" });

            // 自身の大明槓の後は打牌せず、空応答を返すこと
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1112" });
            player.action(new Message { dapai = new Dapai { l = 1, p = "z1" } });
            _reply = null;
            player.action(new Message { fulou = new Fulou { l = 0, m = "z1111+" } }, reply);
            Assert.AreEqual(_reply, new Reply {});
        }

        [Test, Description("action_gang(gang)")]
        public void TestActionGang()
        {
            // 卓情報を設定すること
            var player = init_player(new PlayerParam { shoupai = "m123p456s789z1112" });
            player.action(new Message { zimo = new Zimo { l = 0, p = "z1" } });
            player.action(new Message { gang = new Gang { l = 0, m = "z1111" } });
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s789z2,z1111");

            // 牌数をカウントすること
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1112" });
            player.action(new Message { zimo = new Zimo { l = 1, p = "z3" } });
            player.action(new Message { gang = new Gang { l = 1, m = "z3333" } });
            Assert.AreEqual(player._suanpai._paishu, new Paishu {
                m = new List<int> { 1, 2, 3, 3, 4, 4, 4, 4, 4, 4 },
                p = new List<int> { 1, 4, 4, 4, 3, 3, 3, 4, 4, 4 },
                s = new List<int> { 1, 4, 4, 4, 4, 4, 4, 3, 3, 3 },
                z = new List<int> { 0, 1, 3, 0, 4, 4, 4, 4 }
            });

            // 応答を返すこと
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1112" });
            player.action(new Message { zimo = new Zimo { l = 1, p = "z3" } });
            _reply = null;
            player.action(new Message { gang = new Gang { l = 1, m = "z3333" } }, reply);
            Assert.NotNull(_reply);

            // 自身の手番では空応答を返すこと
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1112" });
            player.action(new Message { zimo = new Zimo { l = 0, p = "z1" } });
            _reply = null;
            player.action(new Message { gang = new Gang { l = 0, m = "z1111" } }, reply);
            Assert.AreEqual(_reply, new Reply {});

            // 和了する
            player = init_player(new PlayerParam { shoupai = "m13p456s789z11,z222=" });
            player._model.shoupai[1].fulou("m222-");
            _reply = null;
            player.action(new Message { gang = new Gang { l = 1, m = "m222-2" } }, reply);
            Assert.AreEqual(_reply, new Reply { hule = "-" });
        }

        [Test, Description("kaigang(kaigang)")]
        public void TestKaigang()
        {
            // 卓情報を設定すること
            var player = init_player();
            player.action(new Message { kaigang = new Kaigang { baopai = "z1" } });
            Assert.AreEqual(player._model.shan.baopai, new List<string> { "m1", "z1" });

            // 牌数をカウントすること
            player = init_player();
            player.action(new Message { kaigang = new Kaigang { baopai = "z1" } });
            Assert.AreEqual(player._suanpai._paishu, new Paishu {
                m = new List<int> { 1, 2, 3, 3, 4, 4, 4, 4, 4, 4 },
                p = new List<int> { 1, 4, 4, 4, 3, 3, 3, 4, 4, 4 },
                s = new List<int> { 1, 4, 4, 4, 4, 4, 4, 3, 3, 3 },
                z = new List<int> { 0, 1, 3, 3, 4, 4, 4, 4 }
            });

            // 応答を返さないこと
            player = init_player();
            _reply = null;
            player.action(new Message { kaigang = new Kaigang { baopai = "z1" } }, reply);
            Assert.IsNull(_reply);
        }

        [Test, Description("action_hule(hule)")]
        public void TestActionHule()
        {
            // 卓情報を設定すること
            var player = init_player();
            player.action(new Message { hule = new Hule { l = 1, shoupai = "m123p456s789z1122z1", fubaopai = new List<string> { "s1" } } });
            Assert.AreEqual(player._model.shoupai[1].ToString(), "m123p456s789z1122z1");
            Assert.AreEqual(player.shan.fubaopai[0], "s1");

            // 応答を返すこと
            player = init_player();
            _reply = null;
            player.action(new Message { hule = new Hule { l = 1, shoupai = "m123p456s789z1122z1", fubaopai = new List<string> { "s1" } } }, reply);
            Assert.NotNull(_reply);
        }

        [Test, Description("action_pingju(pingju)")]
        public void TestActionPingju()
        {
            // 卓情報を設定すること
            var player = init_player();
            player.action(new Message { dapai = new Dapai { l = 1, p = "m1*" } });
            player.action(new Message { pingju = new Pingju { name = "", shoupai = new List<string> { "", "m123p456s789z1122", "", "" } } });
            Assert.AreEqual(player._model.shoupai[1].ToString(), "m123p456s789z1122");
            Assert.AreEqual(player._model.lizhibang, 1);

            // 応答を返すこと
            player = init_player();
            _reply = null;
            player.action(new Message { pingju = new Pingju { name = "", shoupai = new List<string> { "", "", "", "" } } }, reply);
            Assert.NotNull(_reply);
        }

        [Test, Description("action_jieju(jieju)")]
        public void TestActionJieju()
        {
            // 卓情報を設定すること
            var player = init_player();
            var paipu = new Paipu { defen = new List<int> { 10000, 20000, 30000, 40000 } };
            player.action(new Message { jieju = paipu });
            Assert.AreEqual(player._model.defen, paipu.defen);
            Assert.NotNull(player._paipu);

            // 応答を返すこと
            player = init_player();
            paipu = new Paipu { defen = new List<int> { 10000, 20000, 30000, 40000 } };
            _reply = null;
            player.action(new Message { jieju = paipu }, reply);
            Assert.NotNull(_reply);
        }

        [Test, Description("select_hule(data, hupai)")]
        public void TestSelectHule()
        {
            // 和了できるときは必ず和了する(ツモ)
            var player = init_player(new PlayerParam { shoupai = "m123p456s789z11222" });
            Assert.IsTrue(player.select_hule((Dapai)null));

            // 和了できるときは必ず和了する(嶺上開花)
            player = init_player(new PlayerParam { shoupai = "m123p456s789z11,z222=2" });
            Assert.IsTrue(player.select_hule((Dapai)null, true));

            // 和了できるときは必ず和了する(ロン)
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1122" });
            Assert.IsTrue(player.select_hule(new Dapai { l = 1, p = "z1" }));

            // 和了できるときは必ず和了する(槍槓)
            player = init_player(new PlayerParam { shoupai = "m13p456s789z11,z222=" });
            Assert.IsTrue(player.select_hule(new Gang { l = 1, m = "m222=2" }, true));

            // 暗槓は槍槓できない
            player = init_player(new PlayerParam { shoupai = "m13p456s789z11,z222=" });
            Assert.IsFalse(player.select_hule(new Gang { l = 1, m = "m2222" }, true));

            // 引継情報域が設定された場合は、和了に関する検討情報を設定する
            List<HuleInfo> info = new List<HuleInfo>();
            player = init_player(new PlayerParam { shoupai = "m123p456z1122,s789-" });
            info.Clear();
            player.select_hule(new Dapai { l = 2, p = "z1" }, false, info);
            Assert.IsTrue(info.Count > 0);

            player = init_player(new PlayerParam { shoupai = "m123p456z11122,s789-" });
            info.Clear();
            player.select_hule((Dapai)null, false, info);
            Assert.IsTrue(info.Count > 0);

            player = init_player(new PlayerParam { shoupai = "m12p456z33444,s789-" });
            info.Clear();
            player.select_hule(new Gang { l = 2, m = "m333-3" }, true, info);
            Assert.IsTrue(info.Count > 0);

            player = init_player(new PlayerParam { shoupai = "m12p456z33444,s789-" });
            info.Clear();
            player.select_hule(new Dapai { l = 2, p = "m3" }, false, info);
            Assert.IsTrue(info.Count == 0);
            
        }

        [Test, Description("select_pingju(data)")]
        public void TestSelectPingju()
        {
            // 九種九牌は流す
            var player = init_player(new PlayerParam { shoupai = "m19p234s56z1234567" });
            Assert.IsTrue(player.select_pingju());

            // 九種十牌は流さない
            player = init_player(new PlayerParam { shoupai = "m19p134s56z1234567" });
            Assert.IsFalse(player.select_pingju());
        }

        [Test, Description("select_fulou(dapai)")]
        public void TestSelectFulou()
        {
            // 役ありでシャンテン数が進む場合、副露する
            var player = init_player(new PlayerParam { shoupai = "m123p456s58z11234" });
            player.dapai(new Dapai { l = 2, p = "z1" });
            Assert.AreEqual(player.select_fulou(new Dapai { l = 2, p = "z1" }), "z111=");

            // 役のない副露はしない
            player = init_player(new PlayerParam { shoupai = "m123p456s78z11223" });
            player.dapai(new Dapai { l = 2, p = "z2" });
            Assert.IsNull(player.select_fulou(new Dapai { l = 2, p = "z2" }));

            // 3シャンテンに戻る副露はしない
            player = init_player(new PlayerParam { shoupai = "m335p244899s2599" });
            player.dapai(new Dapai { l = 2, p = "p9" });
            Assert.IsNull(player.select_fulou(new Dapai { l = 2, p = "p9" }));

            // シャンテン数が変わらなくても期待値が上がる場合は副露を選択する
            player = init_player(new PlayerParam { shoupai = "m56778p4s2478z255", baopai = "z1" });
            player.dapai(new Dapai { l = 2, p = "z5" });
            Assert.AreEqual(player.select_fulou(new Dapai { l = 2, p = "z5" }), "z555=");

            // シャンテン数が進んでも期待値が上がらない場合は副露しない
            player = init_player(new PlayerParam { shoupai = "m334455p56888s78" });
            player.dapai(new Dapai { l = 3, p = "s6" });
            Assert.IsNull(player.select_fulou(new Dapai { l = 3, p = "s6" }));

            // 役ありでも2シャンテンまでは大明槓しない
            player = init_player(new PlayerParam { shoupai = "m123p147s78z11123" });
            player.dapai(new Dapai { l = 2, p = "z1" });
            Assert.IsNull(player.select_fulou(new Dapai { l = 2, p = "z1" }));

            // 役のない大明槓はしない
            player = init_player(new PlayerParam { shoupai = "m123p456s58z12223" });
            player.dapai(new Dapai { l = 2, p = "z2" });
            Assert.IsNull(player.select_fulou(new Dapai { l = 2, p = "z2" }));

            // リーチ者がいる場合、3シャンテンから副露しない
            player = init_player(new PlayerParam { shoupai = "m123p456s58z11234" });
            player.dapai(new Dapai { l = 2, p = "z1*" });
            Assert.IsNull(player.select_fulou(new Dapai { l = 2, p = "z1" }));
            Assert.IsNull(player.select_fulou(new Dapai { l = 2, p = "z1" }));

            // リーチ者がいる場合、テンパイとならない副露はしない
            player = init_player(new PlayerParam { shoupai = "m56778p4s24789z55" });
            player.dapai(new Dapai { l = 2, p = "z5*" });
            Assert.IsNull(player.select_fulou(new Dapai { l = 2, p = "z5*" }));

            // リーチ者がいる場合でも、超好形の1シャンテンとなる副露はする
            player = init_player(new PlayerParam { shoupai = "m11p235s788z22277", menfeng = 1 });
            player.dapai(new Dapai { l = 2, p = "z7*" });
            Assert.AreEqual(player.select_fulou(new Dapai { l = 2, p = "z7*" }), "z777+");

            // リーチ者がいる場合でも、テンパイとなる副露はする
            player = init_player(new PlayerParam { shoupai = "m1135p678s788,z777=", menfeng = 1 });
            player.dapai(new Dapai { l = 0, p = "m4*" });
            Assert.AreEqual(player.select_fulou(new Dapai { l = 0, p = "m4*" }), "m34-5");

            // リーチ者がいる場合、評価値500未満の副露テンパイにはとらない
            player = init_player(new PlayerParam { shoupai = "m1135p678s788,z777=", menfeng = 1 });
            player.dapai(new Dapai { l = 0, p = "s9*" });
            Assert.IsNull(player.select_fulou(new Dapai { l = 0, p = "s9*" }));

            // 引継情報域が設定された場合は、副露に関する検討情報を設定する
            List<HuleInfo> info = new List<HuleInfo>();
            player = init_player(new PlayerParam { shoupai = "m123p456s58z11234" });
            info.Clear();
            player.select_fulou(new Dapai { l = 2, p = "z1" }, info);
            Assert.AreEqual(info.Count, 2);

            player = init_player(new PlayerParam { shoupai = "m123p456s58z11234" });
            info.Clear();
            player.select_fulou(new Dapai { l = 2, p = "z2" }, info);
            Assert.AreEqual(info.Count, 0);

            player = init_player(new PlayerParam { shoupai = "m123p456s78z11223" });
            info.Clear();
            player.select_fulou(new Dapai { l = 2, p = "z1" }, info);
            Assert.AreEqual(info.Count, 2);

            player = init_player(new PlayerParam { shoupai = "m123p456s78z11223" });
            info.Clear();
            player.select_fulou(new Dapai { l = 2, p = "z2" }, info);
            Assert.AreEqual(info.Count, 1);
        }

        [Test, Description("select_gang(data)")]
        public void TestSelectGang()
        {
            // シャンテン数が変わらない場合、暗槓する
            var player = init_player(new PlayerParam { shoupai = "m234p147s1477z111z1" });
            Assert.AreEqual(player.select_gang(), "z1111");

            // シャンテン数が変わらない場合、加槓する
            player = init_player(new PlayerParam { shoupai = "m234p147s1477z1,z111+" });
            Assert.AreEqual(player.select_gang(), "z111+1");

            // シャンテン数が戻る暗槓はしない
            player = init_player(new PlayerParam { shoupai = "m569p269s12222z136" });
            Assert.IsNull(player.select_gang());

            // シャンテン数が戻っても期待値が上がる場合は暗槓する
            player = init_player(new PlayerParam { shoupai = "m88p0778888s2m5,s067-", baopai = "p4" });
            Assert.AreEqual(player.select_gang(), "p8888");

            // 期待値が上がらない場合シャンテン数が戻る暗槓はしない
            player = init_player(new PlayerParam { shoupai = "m111123p456s789z12" });
            Assert.IsNull(player.select_gang());

            // 3シャンテンに戻る暗槓はしない
            player = init_player(new PlayerParam { shoupai = "m133p405557999z36" });
            Assert.IsNull(player.select_gang());

            // リーチ者がいる場合、テンパイする前は槓しない
            player = init_player(new PlayerParam { shoupai = "m123p456s579z2,z111=" });
            player.dapai(new Dapai { l = 3, p = "m1*" });
            player.zimo(new Zimo { l = 0, p = "z1" });
            Assert.IsNull(player.select_gang());

            // リーチ者がいても、テンパイ後は槓する
            player = init_player(new PlayerParam { shoupai = "m123p456s789z2,z111=" });
            player.dapai(new Dapai { l = 3, p = "m1*" });
            player.zimo(new Zimo { l = 0, p = "z1" });
            Assert.AreEqual(player.select_gang(), "z111=1");

            // 引継情報域が設定された場合は、暗槓・加槓に関する検討情報を設定する
            List<HuleInfo> info = new List<HuleInfo>();
            player = init_player(new PlayerParam { shoupai = "m123p459s58z111123" });
            info.Clear();
            player.select_gang(info);
            Assert.IsTrue(info.Count > 0);

            player = init_player(new PlayerParam { shoupai = "m123p459s58z123,z111+" });
            info.Clear();
            player.select_gang(info);
            Assert.IsTrue(info.Count > 0);

            player = init_player(new PlayerParam { shoupai = "m123p456s78z111122" });
            info.Clear();
            player.select_gang(info);
            Assert.IsTrue(info.Count > 0);

            player = init_player(new PlayerParam { shoupai = "m123p456s78z122,z111+" });
            info.Clear();
            player.select_gang(info);
            Assert.IsTrue(info.Count > 0);
        }

        [Test, Description("select_dapai(data)")]
        public void TestSelectDapai()
        {
            // 待ちの枚数が一番多くなる一番右の牌を選択する
            var player = init_player(new PlayerParam { shoupai = "m26789p24s2449z57m4", baopai = "z5" });
            Assert.AreEqual(player.select_dapai(), "z5");

            // 同点の打牌候補がある場合は牌価値の低い方を選択する
            player = init_player(new PlayerParam { shoupai = "m188p3346789s113m0", baopai = "z2" });
            Assert.AreEqual(player.select_dapai(), "m1");

            // 打牌候補がない場合は一番評価値の低い牌を選択する
            player = init_player(new PlayerParam { shoupai = "m34p22567s234m5,z444=", baopai = "z5" });
            Assert.AreEqual(player.select_dapai(), "s2");

            // 副露を考慮した待ち牌の枚数で打牌を選択する
            player = init_player(new PlayerParam { shoupai = "m223057p2479s357p5", baopai = "z1" });
            Assert.AreEqual(player.select_dapai(), "p9");

            // 打点を考慮した評価値により打牌を選択する
            player = init_player(new PlayerParam { shoupai = "m12378p123s13488m6", baopai = "s9" });
            Assert.AreEqual(player.select_dapai(), "s4*");

            // 期待値が高くなる場合はシャンテン戻しを選択する
            player = init_player(new PlayerParam { shoupai = "m123p1234789s3388", baopai = "p0" });
            Assert.AreEqual(player.select_dapai(), "s3");

            // フリテンとなる場合はシャンテン戻しを選択しない
            player = init_player(new PlayerParam { shoupai = "m12p19s19z1234567m1", baopai = "s3" });
            Assert.AreEqual(player.select_dapai(), "m2*");

            // 赤牌を切ってシャンテン戻しする場合もある
            player = init_player(new PlayerParam { shoupai = "m899p789s3078s9,m111=", baopai = "m9" });
            Assert.AreEqual(player.select_dapai(), "s0");

            // 副露を考慮した期待値で打牌を選択する
            player = init_player(new PlayerParam { shoupai = "m66678p34s3077z77m9", baopai = "m1" });
            Assert.AreEqual(player.select_dapai(), "s3");

            // 同色が10枚以上の場合、染め手を狙う
            player = init_player(new PlayerParam { shoupai = "m235689p9s57z6z7,z111=" });
            Assert.AreNotEqual(player.select_dapai(), "z7_");

            // 風牌が9枚以上の場合、四喜和を狙う
            player = init_player(new PlayerParam { shoupai = "m147p1s0z22333z4,z111=" });
            Assert.AreNotEqual(player.select_dapai(), "z4_");

            // 三元牌が6枚以上の場合、大三元を狙う
            player = init_player(new PlayerParam { shoupai = "m125p2469s1z66z7,z555=" });
            Assert.AreNotEqual(player.select_dapai(), "z7_");

            // リーチ者がいる場合はシャンテン戻しを選択しない
            player = init_player(new PlayerParam { shoupai = "m123p1234789s3388" });
            set_dapai(player, 2, new List<string> { "s6*" });
            Assert.AreEqual(player.select_dapai(), "p1*");

            // 3シャンテン、安全牌あり → ベタオリ
            player = init_player(new PlayerParam { shoupai = "m2367p3566s33588s7" });
            set_dapai(player, 2, new List<string> { "s8*" });
            Assert.AreEqual(player.select_dapai(), "s8");

            // 愚形1シャンテン、安全牌なし → 押し
            player = init_player(new PlayerParam { shoupai = "s2357s8,z777=,m123-,p456-", menfeng = 1, baopai = "z2" });
            set_dapai(player, 2, new List<string> { "m4", "m5", "m6", "p4", "p5", "p6*" });
            Assert.AreEqual(player.select_dapai(), "s8_");

            // 好形2シャンテン、安全牌あり → 回し打ち
            player = init_player(new PlayerParam { shoupai = "m23344p2346s2355p8" });
            set_dapai(player, 2, new List<string> { "s8*" });
            Assert.AreEqual(player.select_dapai(), "p8_");

            // 好形1シャンテン、安全牌なし → 押し
            player = init_player(new PlayerParam { shoupai = "s2357s8,z777=,m123-,p406-", baopai = "z2" });
            set_dapai(player, 2, new List<string> { "m4", "m5", "m6", "p4", "p5", "p6*" });
            Assert.AreEqual(player.select_dapai(), "s8_");

            // 超好形1シャンテン → 全押し
            player = init_player(new PlayerParam { shoupai = "s2357s8,z777=,m123-,p456-", baopai = "z6" });
            set_dapai(player, 2, new List<string> { "m4", "m5", "m6", "p4", "p5", "p6", "s3", "s4", "s6*" });
            Assert.AreEqual(player.select_dapai(), "s8_");

            // テンパイ → 全押し
            player = init_player(new PlayerParam { shoupai = "s1234s6,z777=,m123-,p456-" });
            set_dapai(player, 2, new List<string> { "m4", "m5", "m6", "p4", "p5", "p6", "s5", "s7", "z1", "z2", "z3", "z4*" });
            Assert.AreEqual(player.select_dapai(), "s1");

            // 形式テンパイ → 回し打ち
            player = init_player(new PlayerParam { shoupai = "s2345s6,z444=,m123-,p456-" });
            set_dapai(player, 2, new List<string> { "m4", "m5", "m6", "p4", "p5", "p6", "s7", "s8", "z1", "z2", "z3", "z4*" });
            Assert.AreEqual(player.select_dapai(), "s2");

            // リーチ者がいても自身もテンパイした場合はリーチする
            player = init_player(new PlayerParam { shoupai = "m123p456s5789z1122" });
            set_dapai(player, 2, new List<string> { "p5*" });
            Assert.AreEqual(player.select_dapai(), "s5*");

            // リーチ者がいて自身がテンパイしても評価値200未満ならリーチしない
            player = init_player(new PlayerParam { shoupai = "m22345p123678s79z1", menfeng = 1, baopai = "z1" });
            player.dapai(new Dapai { l = 0, p = "s8" });
            player.fulou(new Fulou { l = 2, m = "s888=" });
            player.dapai(new Dapai { l = 3, p = "p5*" });
            Assert.AreEqual(player.select_dapai(), "z1_");

            // 引継情報域が設定された場合は、打牌に関する検討情報を設定する
            List<HuleInfo> info = new List<HuleInfo>();

            player = init_player(new PlayerParam { shoupai = "m123678p123s13488" });
            info.Clear();
            player.select_dapai(info);
            Assert.IsTrue(info.Count > 0);

            player = init_player(new PlayerParam { shoupai = "m123p1234789s3388" });
            info.Clear();
            player.select_dapai(info);
            Assert.IsTrue(info.Count > 0);

            player = init_player(new PlayerParam { shoupai = "m123p456s258z12345" });
            player.dapai(new Dapai { l = 2, p = "m1*" });
            info.Clear();
            player.select_dapai(info);
            Assert.IsTrue(info.Count > 0);

            player = init_player(new PlayerParam { shoupai = "m123p456s78z111122" });
            player.dapai(new Dapai { l = 2, p = "m1*" });
            info.Clear();
            player.select_gang(info);
            player.select_dapai(info);
            Assert.IsTrue(info.Count > 0);

            player = init_player(new PlayerParam { shoupai = "m123p789s1199z1122" });
            info.Clear();
            player.select_dapai(info);
            Assert.IsTrue(info.Count > 0);
        }

        [Test, Description("select_lizhi(p)")]
        public void TestSelectLizhi()
        {
            // リーチできるときは必ずリーチする
            var player = init_player(new PlayerParam { shoupai = "m123p456s789z12233" });
            Assert.IsTrue(player.select_lizhi("z1"));
        }

        [Test, Description("select_daopai()")]
        public void TestSelectDaopai()
        {
            // 流局時にテンパイなら必ずテンパイ宣言する
            var player = init_player(new PlayerParam { rule = new Majiang.Rule { ノーテン宣言あり = true }, shoupai = "m123p456s789z1122" });
            while (player.shan.paishu > 0) player.shan.zimo();
            Assert.IsTrue(player.select_daopai());
        }

        [Test, Description("xiangting(shoupai)")]
        public void Testxiangting()
        {
            // 役なし副露のシャンテン数は無限大
            var player = init_player(new PlayerParam { shoupai = "s789z44333,m123-,p456-" });
            Assert.AreEqual(player.xiangting(player.shoupai), double.PositiveInfinity);

            // 役牌副露のシャンテン数
            player = init_player(new PlayerParam { shoupai = "m123p456s789z23,z111=" });
            Assert.AreEqual(player.xiangting(player.shoupai), 0);

            // 役牌暗刻のシャンテン数
            player = init_player(new PlayerParam { shoupai = "p456s789z11123,m123-" });
            Assert.AreEqual(player.xiangting(player.shoupai), 0);

            // 役牌バックのシャンテン数
            player = init_player(new PlayerParam { shoupai = "m123p456s789z77,z333-" });
            Assert.AreEqual(player.xiangting(player.shoupai), 1);

            // 喰いタンのシャンテン数
            player = init_player(new PlayerParam { shoupai = "m123p456m66777,s6-78" });
            Assert.AreEqual(player.xiangting(player.shoupai), 0);

            // 喰いタンなし
            var rule = new Majiang.Rule { クイタンあり = false };
            player = init_player(new PlayerParam { shoupai = "m123p456m66,s6-78,m777=", rule = rule });
            Assert.AreEqual(player.xiangting(player.shoupai), double.PositiveInfinity);

            // トイトイのシャンテン数
            player = init_player(new PlayerParam { shoupai = "p222789s99z333,m111+" });
            Assert.AreEqual(player.xiangting(player.shoupai), 1);

            // 6対子形のシャンテン数
            player = init_player(new PlayerParam { shoupai = "p2277s5599z333,m111+," });
            Assert.AreEqual(player.xiangting(player.shoupai), 1);

            // 染め手のシャンテン数
            player = init_player(new PlayerParam { shoupai = "m2p89s2355z7,z333=,s7-89," });
            Assert.AreEqual(player.xiangting(player.shoupai), 2);
        }

        [Test, Description("tingpai(shoupai)")]
        public void TestTingpai()
        {
            // 役なし副露に有効牌なし
            var player = init_player(new PlayerParam { shoupai = "s789z4433,m123-,p456-" });
            Assert.AreEqual(player.tingpai(player.shoupai), new List<string>());

            // 役牌バックの有効牌
            player = init_player(new PlayerParam { shoupai = "m12p456s789z77,z333-" });
            Assert.AreEqual(player.tingpai(player.shoupai), new List<string> { "m1", "m2", "z7+" });

            // 喰いタンの有効牌
            player = init_player(new PlayerParam { shoupai = "m23p456m66777,s6-78" });
            Assert.AreEqual(player.tingpai(player.shoupai), new List<string> { "m4" });

            // トイトイの有効牌
            player = init_player(new PlayerParam { shoupai = "p22278s99z333,m111+" });
            Assert.AreEqual(player.tingpai(player.shoupai), new List<string> { "p7", "p8", "s9+" });

            // 染め手の有効牌
            player = init_player(new PlayerParam { shoupai = "p9s2355z7,z333=,s7-89" });
            Assert.AreEqual(player.tingpai(player.shoupai), new List<string> { "s1-", "s4-", "s5+", "z7" });
        }

        [Test, Description("get_defen(shoupai, rongpai)")]
        public void TestGetDefen()
        {
            // 親・リーチ・ロン
            var player = init_player(new PlayerParam { shoupai = "m123p456s789z1122*", baopai = "z2" });
            Assert.AreEqual(player.get_defen(player.shoupai, "z1="), 7700);
            Assert.AreEqual(player._defen_cache["m123p456s789z1122z1=*"], 7700);

            // 子・副露・ツモ
            player = init_player(new PlayerParam { shoupai = "m123s79z11222s8,p4-56", menfeng = 1 });
            Assert.AreEqual(player.get_defen(player.shoupai), 2700);
            Assert.AreEqual(player._defen_cache["m123s79z11222s8,p4-56"], 2700);

            // 親・メンゼン・ロン
            player = init_player(new PlayerParam { shoupai = "m123p456s789z1122", baopai = "p1" });
            Assert.AreEqual(player.get_defen(player.shoupai, "z1-"), 7700);
            Assert.AreEqual(player._defen_cache["m123p456s789z1122z1-"], 7700);

            // 子・リーチ・暗槓あり・ロン
            player = init_player(new PlayerParam { shoupai = "m123s79z11222*,s2222", baopai = "p1", menfeng = 1 });
            Assert.AreEqual(player.get_defen(player.shoupai, "s8+"), 3900);
            Assert.AreEqual(player._defen_cache["m123s79z11222s8+*,s2222"], 3900);

            // キャッシュを使用(ロン和了)
            player = init_player();
            player._defen_cache["m1112345678999m1="] = 1000;
            Assert.AreEqual(player.get_defen(Majiang.Shoupai.fromString("m1112345678999"), "m1="), 1000);

            // キャッシュを使用(ツモ和了)
            player = init_player();
            player._defen_cache["m1112345678999m1"] = 1000;
            Assert.AreEqual(player.get_defen(Majiang.Shoupai.fromString("m1112345678999m1")), 1000);
        }

        [Test, Description("eval_shoupai(shoupai, paishu)")]
        public void TestEvalShoupai()
        {
            // 和了形の場合は打点を評価値とする
            var player = init_player(new PlayerParam { shoupai = "m123678p123s1388s2*", menfeng = 1, baopai = "s9" });
            var paishu = player._suanpai.paishu_all();
            Assert.AreEqual(player.eval_shoupai(player.shoupai, paishu), 8000);

            // テンパイ形の場合は、和了打点×枚数 の総和を評価値とする
            player = init_player(new PlayerParam { shoupai = "m123678p123s1388*", menfeng = 1, baopai = "s9" });
            paishu = player._suanpai.paishu_all();
            Assert.AreEqual(player.eval_shoupai(player.shoupai, paishu), 32000.0 / 12);

            // 打牌可能な牌姿の場合は、打牌後の牌姿の評価値の最大値を評価値とする
            player = init_player(new PlayerParam { shoupai = "m123678p123s13488", menfeng = 1, baopai = "s9" });
            paishu = player._suanpai.paishu_all();
            Assert.AreEqual(player.eval_shoupai(player.shoupai, paishu), 32000.0 / 12);

            // 残り枚数0の牌は評価時に手牌に加えない
            player = init_player(new PlayerParam { shoupai = "m34p123456s789z13z3", menfeng = 1, baopai = "m0" });
            paishu = player._suanpai.paishu_all();
            Assert.AreEqual(player.eval_shoupai(player.shoupai, paishu), 18900.0 / 12);

            // 3シャンテン以上の場合は鳴きを考慮した待ち牌数を評価値とする
            player = init_player(new PlayerParam { shoupai = "m569p4s5778z11335", menfeng = 1, baopai = "s9" });
            paishu = player._suanpai.paishu_all();
            Assert.AreEqual(player.eval_shoupai(player.shoupai, paishu), 61);
        }
    }
}