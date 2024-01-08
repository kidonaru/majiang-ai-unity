using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Majiang.AI
{
    /// <summary>
    /// 牌数
    /// </summary>
    public class Paishu : EntityBase
    {
        public List<int> m = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<int> p = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<int> s = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<int> z = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };

        public List<int> this[char key]
        {
            get
            {
                switch (key)
                {
                    case 'm': return m;
                    case 'p': return p;
                    case 's': return s;
                    case 'z': return z;
                    default: return null;
                }
            }
        }

        public int this[string p]
        {
            get
            {
                return this[p[0]][p[1] - '0'];
            }
            set
            {
                this[p[0]][p[1] - '0'] = value;
            }
        }

        public override string ToString()
        {
            return $"Paishu(m={m.JoinJS()} p={p.JoinJS()} s={s.JoinJS()} z={z.JoinJS()})";
        }
    }

    /// <summary>
    /// 算牌
    /// </summary>
    public class SuanPai
    {
        /// <summary>
        /// 牌数
        /// </summary>
        public Paishu _paishu;
        /// <summary>
        /// 場風(0: 東、1: 南、2: 西、3: 北)。
        /// </summary>
        public int _zhuangfeng;
        /// <summary>
        /// 自風
        /// </summary>
        public int _menfeng;
        /// <summary>
        /// ドラ表示牌の配列。
        /// </summary>
        public List<string> _baopai;
        /// <summary>
        /// 各プレイヤーの打牌
        /// </summary>
        public List<HashSet<string>> _dapai;
        /// <summary>
        /// 各プレイヤーのリーチ状態
        /// </summary>
        public List<bool> _lizhi;

        public SuanPai(Hongpai hongpai)
        {
            _paishu = new Paishu
            {
                m = new List<int> { hongpai.m, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
                p = new List<int> { hongpai.p, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
                s = new List<int> { hongpai.s, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
                z = new List<int> { 0, 4, 4, 4, 4, 4, 4, 4 },
            };
            _zhuangfeng = 0;
            _menfeng = 0;
            _baopai = new List<string>();

            _dapai = new List<HashSet<string>> {
                new HashSet<string>(),
                new HashSet<string>(),
                new HashSet<string>(),
                new HashSet<string>(),
            };
            _lizhi = new List<bool> { false, false, false, false };
        }

        /// <summary>
        /// 牌数を減らす
        /// </summary>
        /// <param name="p"></param>
        public void decrease(string p)
        {
            this._paishu[p[0]][int.Parse(p[1].ToString())]--;
            if (p[1] == '0') this._paishu[p[0]][5]--;
        }

        /// <summary>
        /// 牌を配る
        /// </summary>
        /// <param name="qipai"></param>
        /// <param name="menfeng"></param>
        public void qipai(Qipai qipai, int menfeng)
        {
            this._zhuangfeng = qipai.zhuangfeng;
            this._menfeng = menfeng;

            this._baopai = new List<string> { qipai.baopai };
            this.decrease(qipai.baopai);

            var paistr = qipai.shoupai[menfeng];
            foreach (var suitstr in Regex.Matches(paistr, @"[mpsz]\d[\d\+\=\-]*"))
            {
                var s = suitstr.ToString()[0];
                foreach (var n in Regex.Matches(suitstr.ToString(), @"\d"))
                {
                    this.decrease($"{s}{n}");
                }
            }
        }

        /// <summary>
        /// 自摸
        /// </summary>
        /// <param name="zimo"></param>
        public void zimo(Zimo zimo)
        {
            if (zimo.l == this._menfeng) this.decrease(zimo.p);
        }

        /// <summary>
        /// 打牌
        /// </summary>
        /// <param name="dapai"></param>
        public void dapai(Dapai dapai)
        {
            if (dapai.l != this._menfeng)
            {
                this.decrease(dapai.p);
                if (dapai.p.EndsWith("*")) this._lizhi[dapai.l] = true;
            }
            var p = $"{dapai.p[0]}{(dapai.p[1] == '0' ? 5 : int.Parse(dapai.p[1].ToString()))}";
            this._dapai[dapai.l].Add(p);
            for (int l = 0; l < 4; l++)
            {
                if (this._lizhi[l]) this._dapai[l].Add(p);
            }
        }

        /// <summary>
        /// 副露
        /// </summary>
        /// <param name="fulou"></param>
        public void fulou(Fulou fulou)
        {
            if (fulou.l != this._menfeng)
            {
                var s = fulou.m[0];
                foreach (var n in Regex.Matches(fulou.m, @"\d(?![\+\=\-])"))
                {
                    this.decrease($"{s}{n}");
                }
            }
        }

        /// <summary>
        /// 槓
        /// </summary>
        /// <param name="gang"></param>
        public void gang(Gang gang)
        {
            if (gang.l != this._menfeng)
            {
                if (Regex.IsMatch(gang.m, @"^[mpsz]\d{4}$"))
                {
                    var s = gang.m[0];
                    foreach (var n in Regex.Matches(gang.m, @"\d"))
                    {
                        this.decrease($"{s}{n}");
                    }
                }
                else
                {
                    var s = gang.m[0];
                    var n = gang.m[gang.m.Length - 1];
                    this.decrease($"{s}{n}");
                }
            }
        }

        /// <summary>
        /// 開槓 （槓をした後のドラ表示牌の追加）
        /// </summary>
        /// <param name="kaigang"></param>
        public void kaigang(Kaigang kaigang)
        {
            this._baopai.Add(kaigang.baopai);
            this.decrease(kaigang.baopai);
        }

        /// <summary>
        /// 全ての牌の数を取得
        /// </summary>
        /// <returns></returns>
        public Paishu paishu_all()
        {
            var paishu = new Paishu();
            foreach (var s in "mpsz")
            {
                foreach (var n in s == 'z' ? new List<int> { 1, 2, 3, 4, 5, 6, 7 } : new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })
                {
                    paishu[s][n] = n == 5 ? this._paishu[s][5] - this._paishu[s][0]
                                            : this._paishu[s][n];
                }
            }
            return paishu;
        }

        /// <summary>
        /// 牌価 (牌の価値を計算)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int paijia(string p)
        {
            Func<char, int, int> weight = (s, n) =>
            {
                if (n < 1 || 9 < n) return 0;
                var rv = 1;
                foreach (var baopai in this._baopai)
                {
                    if ($"{s}{n}" == Majiang.Shan.zhenbaopai(baopai)) rv *= 2;
                }
                return rv;
            };

            var rv = 0;
            var s = p[0];
            var n = p[1] == '0' ? 5 : int.Parse(p[1].ToString());
            var num = this._paishu[s];

            if (s == 'z')
            {
                rv = p[1] != '0' ? num[n] * weight(s, n) : 0;
                if (n == this._zhuangfeng + 1) rv *= 2;
                if (n == this._menfeng + 1) rv *= 2;
                if (5 <= n && n <= 7) rv *= 2;
            }
            else
            {
                var left = (1 <= n - 2) ? Math.Min(num[n - 2], num[n - 1]) : 0;
                var center = (1 <= n - 1 && n + 1 <= 9) ? Math.Min(num[n - 1], num[n + 1]) : 0;
                var right = (n + 2 <= 9) ? Math.Min(num[n + 1], num[n + 2]) : 0;
                var n_pai = new List<int>
                {
                    left,
                    Math.Max(left, center),
                    num[n],
                    Math.Max(center, right),
                    right
                };
                rv = n_pai[0] * weight(s, n - 2)
                    + n_pai[1] * weight(s, n - 1)
                    + n_pai[2] * weight(s, n)
                    + n_pai[3] * weight(s, n + 1)
                    + n_pai[4] * weight(s, n + 2);
                rv += num[0] == 0 ? 0
                    : n == 7 ? Math.Min(num[0], n_pai[0]) * weight(s, n - 2)
                    : n == 6 ? Math.Min(num[0], n_pai[1]) * weight(s, n - 1)
                    : n == 5 ? Math.Min(num[0], n_pai[2]) * weight(s, n)
                    : n == 4 ? Math.Min(num[0], n_pai[3]) * weight(s, n + 1)
                    : n == 3 ? Math.Min(num[0], n_pai[4]) * weight(s, n + 2)
                    : 0;
                if (p[1] == '0') rv *= 2;
            }
            rv *= weight(s, n);

            return rv;
        }

        /// <summary>
        /// 牌価の計算関数を作成
        /// </summary>
        /// <param name="shoupai"></param>
        /// <returns></returns>
        public Func<string, int> make_paijia(Shoupai shoupai)
        {
            var n_suit = new Dictionary<char, int>();
            foreach (var s in "mpsz")
            {
                n_suit[s] = shoupai._bingpai[s].Skip(1).Sum();
            }
            var n_sifeng = shoupai._bingpai['z'].Skip(1).Take(4).Sum();
            var n_sanyuan = shoupai._bingpai['z'].Skip(5).Sum();
            foreach (var m in shoupai._fulou)
            {
                n_suit[m[0]] += 3;
                if (Regex.IsMatch(m, @"^z[1234]")) n_sifeng += 3;
                if (Regex.IsMatch(m, @"^z[567]")) n_sanyuan += 3;
            }

            var paijia = new Dictionary<string, int>();

            return p => paijia.ContainsKey(p) ? paijia[p] : (paijia[p] = this.paijia(p)
                        * (Regex.IsMatch(p, @"^z[1234]") && n_sifeng >= 9 ? 8
                        : Regex.IsMatch(p, @"^z[567]") && n_sanyuan >= 6 ? 8
                        : p[0] == 'z' && "mps".Select(s => n_suit[s]).Max() + n_suit['z'] >= 10 ? 4
                        : n_suit[p[0]] + n_suit['z'] >= 10 ? 2
                        : 1));
        }

        /// <summary>
        /// 危険牌の計算
        /// </summary>
        /// <param name="p"></param>
        /// <param name="l"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public int suan_weixian(string p, int l, int c = 0)
        {
            var s = p[0];
            var n = p[1] == '0' ? 5 : int.Parse(p[1].ToString());

            var r = 0;
            if (this._dapai[l].Contains($"{s}{n}")) return r;

            var paishu = this._paishu[s];

            r += paishu[n] - (c > 0 ? 0 : 1) == 3 ? (s == 'z' ? 8 : 3)
                : paishu[n] - (c > 0 ? 0 : 1) == 2 ? 3
                : paishu[n] - (c > 0 ? 0 : 1) == 1 ? 1
                : 0;
            if (s == 'z') return r;

            r += n - 2 < 1 ? 0
                : Math.Min(paishu[n - 2], paishu[n - 1]) == 0 ? 0
                : n - 2 == 1 ? 3
                : this._dapai[l].Contains($"{s}{n - 3}") ? 0
                : 10;
            r += n - 1 < 1 ? 0
                : n + 1 > 9 ? 0
                : Math.Min(paishu[n - 1], paishu[n + 1]) == 0 ? 0
                : 3;
            r += n + 2 > 9 ? 0
                : Math.Min(paishu[n + 1], paishu[n + 2]) == 0 ? 0
                : n + 2 == 9 ? 3
                : this._dapai[l].Contains($"{s}{n + 3}") ? 0
                : 10;
            return r;
        }

        /// <summary>
        /// 全ての危険牌の計算
        /// </summary>
        /// <param name="bingpai"></param>
        /// <returns></returns>
        public Func<string, double> suan_weixian_all(Bingpai bingpai)
        {
            Dictionary<string, double> weixian_all = null;
            for (var l = 0; l < 4; l++)
            {
                if (!this._lizhi[l]) continue;
                if (weixian_all == null) weixian_all = new Dictionary<string, double>();
                var weixian = new Dictionary<string, double>();
                var sum = 0.0;
                foreach (var s in "mpsz")
                {
                    for (var n = 1; n < this._paishu[s].Count; n++)
                    {
                        weixian[$"{s}{n}"] = this.suan_weixian($"{s}{n}", l, bingpai[s][n]);
                        sum += weixian[$"{s}{n}"];
                    }
                }
                foreach (var p in weixian.Keys.ToList())
                {
                    weixian[p] = weixian[p] / (sum == 0 ? 1 : sum) * 100 * (l == 0 ? 1.40 : 1);
                    if (!weixian_all.ContainsKey(p)) weixian_all[p] = 0;
                    weixian_all[p] = Math.Max(weixian_all[p], weixian[p]);
                }
            }
            if (weixian_all != null) return p => weixian_all[p[0].ToString() + (p[1] == '0' ? 5 : int.Parse(p[1].ToString()))];
            return null;
        }
    }
}