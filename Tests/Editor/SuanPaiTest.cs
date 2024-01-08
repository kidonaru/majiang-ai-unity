using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Majiang.AI.Test
{
    public class SuanPaiTest
    {
        public class SuanPaiParam {
            public int? menfeng;
            public string baopai;
            public string shoupai;
        }

        public SuanPai init_suanpai(SuanPaiParam param = null)
        {
            if (param == null) param = new SuanPaiParam();

            var suanpai = new SuanPai(new Hongpai { m = 1, p = 1, s = 1 });

            var qipai = new Qipai
            {
                zhuangfeng = 0,
                jushu = 0,
                changbang = 0,
                lizhibang = 0,
                defen = new List<int> { 25000, 25000, 25000, 25000 },
                baopai = "m1",
                shoupai = new List<string> { "", "", "", "" },
            };
            var menfeng = param.menfeng != null ? param.menfeng.Value : 0;
            qipai.baopai = param.baopai != null ? param.baopai : "m1";
            qipai.shoupai[menfeng] = param.shoupai != null ? param.shoupai : "";

            suanpai.qipai(qipai, menfeng);

            return suanpai;
        }

        [Test, Description("constructor(hongpai)")]
        public void TestConstructor()
        {
            var suanpai = new SuanPai(new Hongpai { m = 1, p = 2, s = 3 });
            Assert.IsNotNull(suanpai); // インスタンスが生成できること
            Assert.AreEqual(new Paishu {
                    m = new List<int> { 1, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
                    p = new List<int> { 2, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
                    s = new List<int> { 3, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
                    z = new List<int> { 0, 4, 4, 4, 4, 4, 4, 4 }
                },
                suanpai._paishu); // 牌数が正しいこと
        }

        [Test, Description("qipai(qipai)")]
        public void TestQipai()
        {
            var suanpai = new SuanPai(new Hongpai { m = 1, p = 1, s = 1 });
            suanpai.qipai(new Qipai { zhuangfeng = 1, baopai = "m1", shoupai = new List<string> { "", "", "m123p406s789z1122", "" } }, 2);
            Assert.AreEqual(1, suanpai._zhuangfeng); // 場風が正しいこと
            Assert.AreEqual(2, suanpai._menfeng); // 自風が正しいこと
            CollectionAssert.AreEqual(new List<string> { "m1" }, suanpai._baopai); // ドラが正しいこと
            Assert.AreEqual(new Paishu {
                                m = new List<int> { 1, 2, 3, 3, 4, 4, 4, 4, 4, 4 },
                                p = new List<int> { 0, 4, 4, 4, 3, 3, 3, 4, 4, 4 },
                                s = new List<int> { 1, 4, 4, 4, 4, 4, 4, 3, 3, 3 },
                                z = new List<int> { 0, 2, 2, 4, 4, 4, 4, 4 }
                            }, suanpai._paishu); // 牌数が正しいこと

            // 副露のある手牌でも動作すること
            suanpai = new SuanPai(new Hongpai { m = 1, p = 1, s = 1 });
            suanpai.qipai(new Qipai { zhuangfeng = 1, baopai = "m1", shoupai = new List<string> { "", "", "p406s789z1122,m12-3", "" } }, 2);
            Assert.AreEqual(new Paishu {
                                m = new List<int> { 1, 2, 3, 3, 4, 4, 4, 4, 4, 4 },
                                p = new List<int> { 0, 4, 4, 4, 3, 3, 3, 4, 4, 4 },
                                s = new List<int> { 1, 4, 4, 4, 4, 4, 4, 3, 3, 3 },
                                z = new List<int> { 0, 2, 2, 4, 4, 4, 4, 4 }
                            }, suanpai._paishu);

            // 空の手牌でも動作すること
            suanpai = new SuanPai(new Hongpai { m = 1, p = 1, s = 1 });
            suanpai.qipai(new Qipai { zhuangfeng = 1, baopai = "m1", shoupai = new List<string> { "", "", "_____________", "" } }, 2);
            Assert.AreEqual(new Paishu {
                                m = new List<int> { 1, 3, 4, 4, 4, 4, 4, 4, 4, 4 },
                                p = new List<int> { 1, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
                                s = new List<int> { 1, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
                                z = new List<int> { 0, 4, 4, 4, 4, 4, 4, 4 }
                            }, suanpai._paishu);
        }

        [Test, Description("zimo(zimo)")]
        public void TestZimo()
        {
            // 自分の手番
            var suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.zimo(new Zimo { l = 0, p = "m0" });
            CollectionAssert.AreEqual(new List<int> { 0, 4, 4, 4, 4, 3, 4, 4, 4, 4 }, suanpai._paishu.m);

            // 他者の手番
            suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.zimo(new Zimo { l = 1, p = "m0" });
            CollectionAssert.AreEqual(new List<int> { 1, 4, 4, 4, 4, 4, 4, 4, 4, 4 }, suanpai._paishu.m);
        }

        [Test, Description("dapai(dapai)")]
        public void TestDapai()
        {
            // 自分の手番
            var suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.dapai(new Dapai { l = 0, p = "m0" });
            CollectionAssert.AreEqual(new List<int> { 1, 4, 4, 4, 4, 4, 4, 4, 4, 4 }, suanpai._paishu.m);

            // 他者の手番
            suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.dapai(new Dapai { l = 1, p = "m0" });
            CollectionAssert.AreEqual(new List<int> { 0, 4, 4, 4, 4, 3, 4, 4, 4, 4 }, suanpai._paishu.m);
        }

        [Test, Description("fulou(fulou)")]
        public void TestFulou()
        {
            // 自分の手番
            var suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.fulou(new Fulou { l = 0, m = "p34-0" });
            CollectionAssert.AreEqual(new List<int> { 1, 4, 4, 4, 4, 4, 4, 4, 4, 4 }, suanpai._paishu.p);

            // 他者の手番
            suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.fulou(new Fulou { l = 1, m = "p34-0" });
            CollectionAssert.AreEqual(new List<int> { 0, 4, 4, 3, 4, 3, 4, 4, 4, 4 }, suanpai._paishu.p);
        }

        [Test, Description("gang(gang)")]
        public void TestGang()
        {
            // 自分の手番
            var suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.gang(new Gang { l = 0, m = "s5550" });
            CollectionAssert.AreEqual(new List<int> { 1, 4, 4, 4, 4, 4, 4, 4, 4, 4 }, suanpai._paishu.s);

            // 他者の手番(暗槓)
            suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.gang(new Gang { l = 1, m = "s5550" });
            CollectionAssert.AreEqual(new List<int> { 0, 4, 4, 4, 4, 0, 4, 4, 4, 4 }, suanpai._paishu.s);

            // 他者の手番(加槓)
            suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.gang(new Gang { l = 1, m = "s555+0" });
            CollectionAssert.AreEqual(new List<int> { 0, 4, 4, 4, 4, 3, 4, 4, 4, 4 }, suanpai._paishu.s);
        }

        [Test, Description("kaigang(kaigang)")]
        public void TestKaigang()
        {
            // ドラが追加されること
            var suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.kaigang(new Kaigang { baopai = "s0" });
            CollectionAssert.AreEqual(new List<string> { "z1", "s0" }, suanpai._baopai);

            // 牌数が減算されること
            suanpai = init_suanpai(new SuanPaiParam { baopai = "z1" });
            suanpai.kaigang(new Kaigang { baopai = "s0" });
            CollectionAssert.AreEqual(new List<int> { 0, 4, 4, 4, 4, 3, 4, 4, 4, 4 }, suanpai._paishu.s);
        }

        [Test, Description("paishu_all()")]
        public void TestPaishuAll()
        {
            var suanpai = init_suanpai(new SuanPaiParam { shoupai = "m456p406s999z1122", baopai = "z1" });
            Assert.AreEqual(new Paishu {
                m = new List<int> { 1, 4, 4, 4, 3, 2, 3, 4, 4, 4 },
                p = new List<int> { 0, 4, 4, 4, 3, 3, 3, 4, 4, 4 },
                s = new List<int> { 1, 4, 4, 4, 4, 3, 4, 4, 4, 1 },
                z = new List<int> { 0, 1, 2, 4, 4, 4, 4, 4 }
            }, suanpai.paishu_all());
        }

        [Test, Description("paijia(p)")]
        public void TestPaijia()
        {
            Dictionary<char, List<int>> paijia_all(SuanPai suanpai) {
                var paijia = new Dictionary<char, List<int>>();
                foreach (var s in "mpsz") {
                    paijia[s] = new List<int>();
                    for (int n = 0; n < suanpai._paishu[s].Count; n++) {
                        paijia[s].Add(suanpai.paijia($"{s}{n}"));
                    }
                }
                return paijia;
            }

            // 牌価値の初期値が正しいこと
            var suanpai = new SuanPai(new Hongpai { m = 0, p = 1, s = 2 });
            CollectionAssert.AreEqual(new Dictionary<char, List<int>> {
                { 'm', new List<int> { 40, 12, 16, 20, 20, 20, 20, 20, 16, 12 } },
                { 'p', new List<int> { 42, 12, 16, 21, 21, 21, 21, 21, 16, 12 } },
                { 's', new List<int> { 44, 12, 16, 22, 22, 22, 22, 22, 16, 12 } },
                { 'z', new List<int> { 0, 16, 4, 4, 4, 8, 8, 8 } }
            }, paijia_all(suanpai));

            // 配牌後の牌価値が正しいこと
            suanpai = init_suanpai(new SuanPaiParam { shoupai = "m233p055s778z1123", baopai = "z1" });
            suanpai.kaigang(new Kaigang { baopai = "z1" });
            CollectionAssert.AreEqual(new Dictionary<char, List<int>> {
                { 'm', new List<int> { 38, 8, 9, 17, 17, 19, 21, 21, 16, 12 } },
                { 'p', new List<int> { 34, 12, 16, 17, 14, 17, 14, 17, 16, 12 } },
                { 's', new List<int> { 38, 12, 16, 21, 21, 19, 17, 17, 9, 8 } },
                { 'z', new List<int> { 0, 0, 48, 3, 4, 8, 8, 8 } }
            }, paijia_all(suanpai));
        }

        [Test, Description("make_paijia(shoupai)")]
        public void TestMakePaijia()
        {
            // 一色手を狙う場合、染め色の孤立牌の評価値は2倍とする
            var suanpai = init_suanpai();
            var paijia = suanpai.make_paijia(Majiang.Shoupai.fromString("p123s789z1234,p456-"));
            Assert.AreEqual(24, paijia("p1"));

            // 一色手を狙う場合、字牌の孤立牌の評価値は4倍とする
            suanpai = init_suanpai();
            paijia = suanpai.make_paijia(Majiang.Shoupai.fromString("p123456s789z1234"));
            Assert.AreEqual(16, paijia("z4"));

            // 風牌が9枚以上ある場合は、風牌の評価値を8倍とする
            suanpai = init_suanpai();
            paijia = suanpai.make_paijia(Majiang.Shoupai.fromString("m12p34z111222,z333="));
            Assert.AreEqual(32, paijia("z4"));

            // 三元牌が6枚以上ある場合は、三元牌の評価値を8倍とする
            suanpai = init_suanpai();
            paijia = suanpai.make_paijia(Majiang.Shoupai.fromString("m123p4567z555,z666="));
            Assert.AreEqual(64, paijia("z7"));

            // それ以外の場合は評価値は変化なし
            suanpai = init_suanpai();
            paijia = suanpai.make_paijia(Majiang.Shoupai.fromString("m123p456s789z1123"));
            Assert.AreEqual(12, paijia("p1"));
        }

        [Test, Description("suan_weixian(p, l, c)")]
        public void TestsuanWeixian()
        {
            var suanpai = new SuanPai(new Hongpai { m = 1, p = 1, s = 1 });

            // 現物: 0
            suanpai.dapai(new Dapai { l = 1, p = "z1" });
            Assert.AreEqual(0, suanpai.suan_weixian("z1", 1));

            // 字牌 生牌: 8
            Assert.AreEqual(8, suanpai.suan_weixian("z2", 1));
            suanpai.zimo(new Zimo { l = 0, p = "z3" });
            Assert.AreEqual(8, suanpai.suan_weixian("z3", 1, 1));

            // 字牌 1枚見え: 3
            suanpai.dapai(new Dapai { l = 2, p = "z2" });
            Assert.AreEqual(3, suanpai.suan_weixian("z2", 1));
            suanpai.dapai(new Dapai { l = 3, p = "z3" });
            Assert.AreEqual(3, suanpai.suan_weixian("z3", 1, 1));

            // 字牌 2枚見え: 1
            suanpai.dapai(new Dapai { l = 2, p = "z2" });
            Assert.AreEqual(1, suanpai.suan_weixian("z2", 1));
            suanpai.dapai(new Dapai { l = 3, p = "z3" });
            Assert.AreEqual(1, suanpai.suan_weixian("z3", 1, 1));

            // 字牌 ラス牌: 0
            suanpai.dapai(new Dapai { l = 2, p = "z2" });
            Assert.AreEqual(0, suanpai.suan_weixian("z2", 1));
            suanpai.dapai(new Dapai { l = 3, p = "z3" });
            Assert.AreEqual(0, suanpai.suan_weixian("z3", 1, 1));

            // 字牌 なし: 0
            suanpai.dapai(new Dapai { l = 2, p = "z2" });
            Assert.AreEqual(0, suanpai.suan_weixian("z2", 1));
            suanpai.dapai(new Dapai { l = 0, p = "z3" });
            Assert.AreEqual(0, suanpai.suan_weixian("z3", 1, 1));

            // 数牌 無スジ(一九牌): 13
            Assert.AreEqual(13, suanpai.suan_weixian("m1", 1));
            Assert.AreEqual(13, suanpai.suan_weixian("m9", 1));

            // 数牌 無スジ(二八牌): 16
            Assert.AreEqual(16, suanpai.suan_weixian("m2", 1));
            Assert.AreEqual(16, suanpai.suan_weixian("m8", 1));

            // 数牌 無スジ(三七牌): 19
            Assert.AreEqual(19, suanpai.suan_weixian("m3", 1));
            Assert.AreEqual(19, suanpai.suan_weixian("m7", 1));

            // 数牌 無スジ(四五六牌): 26
            Assert.AreEqual(26, suanpai.suan_weixian("m4", 1));
            Assert.AreEqual(26, suanpai.suan_weixian("m5", 1));
            Assert.AreEqual(26, suanpai.suan_weixian("m6", 1));

            // 数牌 スジ(一九牌): 3
            suanpai.dapai(new Dapai { l = 1, p = "m4" });
            Assert.AreEqual(3, suanpai.suan_weixian("m1", 1));
            suanpai.dapai(new Dapai { l = 1, p = "m6" });
            Assert.AreEqual(3, suanpai.suan_weixian("m9", 1));

            // 数牌 スジ(二八牌): 6
            suanpai.dapai(new Dapai { l = 1, p = "m5" });
            Assert.AreEqual(6, suanpai.suan_weixian("m2", 1));
            Assert.AreEqual(6, suanpai.suan_weixian("m8", 1));

            // 数牌 スジ(三七牌): 9
            Assert.AreEqual(9, suanpai.suan_weixian("m3", 1));
            Assert.AreEqual(9, suanpai.suan_weixian("m7", 1));

            // 数牌 片スジ(四五六牌): 16
            suanpai.dapai(new Dapai { l = 1, p = "p1" });
            Assert.AreEqual(16, suanpai.suan_weixian("p4", 1));
            suanpai.dapai(new Dapai { l = 1, p = "p2" });
            Assert.AreEqual(16, suanpai.suan_weixian("p5", 1));
            suanpai.dapai(new Dapai { l = 1, p = "p3" });
            Assert.AreEqual(16, suanpai.suan_weixian("p6", 1));

            // 数牌 両スジ(四五六牌): 6
            suanpai.dapai(new Dapai { l = 1, p = "p7" });
            Assert.AreEqual(6, suanpai.suan_weixian("p4", 1));
            suanpai.dapai(new Dapai { l = 1, p = "p8" });
            Assert.AreEqual(6, suanpai.suan_weixian("p0", 1));
            suanpai.dapai(new Dapai { l = 1, p = "p9" });
            Assert.AreEqual(6, suanpai.suan_weixian("p6", 1));

            // 数牌 五のカベ 三七牌: 9
            suanpai.gang(new Gang { l = 2, m = "s5550" });
            Assert.AreEqual(9, suanpai.suan_weixian("s3", 1));
            Assert.AreEqual(9, suanpai.suan_weixian("s7", 1));

            // 数牌 五のカベ 四六牌: 13
            Assert.AreEqual(13, suanpai.suan_weixian("s4", 1));
            Assert.AreEqual(13, suanpai.suan_weixian("s6", 1));

            // 数牌 二のカベ 生牌: 3
            suanpai.gang(new Gang { l = 2, m = "s2222" });
            Assert.AreEqual(3, suanpai.suan_weixian("s1", 1));

            // 数牌 二のカベ 1枚見え: 3
            suanpai.dapai(new Dapai { l = 2, p = "s1" });
            Assert.AreEqual(3, suanpai.suan_weixian("s1", 1));

            // 数牌 二のカベ 2枚見え: 1
            suanpai.dapai(new Dapai { l = 2, p = "s1" });
            Assert.AreEqual(1, suanpai.suan_weixian("s1", 1));

            // 数牌 二のカベ ラス牌: 0
            suanpai.dapai(new Dapai { l = 2, p = "s1" });
            Assert.AreEqual(0, suanpai.suan_weixian("s1", 1));

            // 数牌 二のカベ なし: 0
            suanpai.dapai(new Dapai { l = 2, p = "s1" });
            Assert.AreEqual(0, suanpai.suan_weixian("s1", 1));
        }

        [Test, Description("suan_weixian_all(bingpai)")]
        public void TestSuanWeixianAll()
        {
            var paistr = "m4579p478s6z14457";
            var suanpai = init_suanpai(new SuanPaiParam { shoupai = paistr, baopai = "p7", menfeng = 3 });
            var shoupai = Majiang.Shoupai.fromString(paistr);

            // リーチなし
            Assert.Null(suanpai.suan_weixian_all(shoupai._bingpai));

            // リーチあり
            suanpai.dapai(new Dapai { l = 1, p = "m3*" });
            var weixian = suanpai.suan_weixian_all(shoupai._bingpai);
            Assert.AreEqual(weixian("m0"), 26.0 / 544 * 100);

            // 2人リーチ
            suanpai.dapai(new Dapai { l = 0, p = "p3*" });
            weixian = suanpai.suan_weixian_all(shoupai._bingpai);
            Assert.AreEqual(weixian("m0"), Math.Max(26.0 / 515 * 100, 26.0 / 544 * 100 * 1.40));

            // 全ての牌が安全
            int i = 0;
            foreach (var s in "mpsz")
            {
                for (int n = 1; n <= (s == 'z' ? 7 : 9); n++)
                {
                    suanpai.dapai(new Dapai { l = i++ % 4, p = $"{s}{n}" });
                }
            }
            weixian = suanpai.suan_weixian_all(shoupai._bingpai);
            Assert.AreEqual(weixian("m0"), 0);
        }
    }
}