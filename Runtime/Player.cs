using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Majiang.AI
{
    /// <summary>
    /// 検討情報
    /// </summary>
    public class HuleInfo
    {
        public string p;
        public string m;
        public double n_xiangting;
        public double ev;
        public string shoupai;
        public List<string> tingpai;
        public int n_tingpai;
        public double weixian;
    }

    public class Player : Majiang.Player
    {
        public static readonly List<int> width = new List<int> { 12, 12 * 6, 12 * 6 * 3 };
        
        public ConcurrentDictionary<string, int> _defen_cache = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, double> _eval_cache = new ConcurrentDictionary<string, double>();
        public SuanPai _suanpai;

        public List<string> add_hongpai(List<string> tingpai)
        {
            var pai = new List<string>();
            foreach (var p in tingpai)
            {
                if (p[0] != 'z' && p[1] == '5') pai.Add(p.Replace("5", "0"));
                pai.Add(p);
            }
            return pai;
        }

        public override void qipai(Qipai qipai)
        {
            this._defen_cache.Clear();
            this._eval_cache.Clear();
            this._suanpai = new SuanPai(this._rule.赤牌);
            this._suanpai.qipai(
                qipai, (this._id + 4 - this._model.qijia + 4 - qipai.jushu) % 4);
            base.qipai(qipai);
        }

        public override void zimo(Zimo zimo, bool gangzimo = false)
        {
            if (zimo.l == this._menfeng) this._eval_cache.Clear();
            this._suanpai.zimo(zimo);
            base.zimo(zimo, gangzimo);
        }

        public override void dapai(Dapai dapai)
        {
            if (dapai.l != this._menfeng) this._eval_cache.Clear();
            this._suanpai.dapai(dapai);
            base.dapai(dapai);
        }

        public override void fulou(Fulou fulou)
        {
            this._suanpai.fulou(fulou);
            base.fulou(fulou);
        }

        public override void gang(Gang gang)
        {
            this._suanpai.gang(gang);
            base.gang(gang);
        }

        public override void kaigang(Kaigang kaigang)
        {
            this._defen_cache.Clear();
            this._eval_cache.Clear();
            this._suanpai.kaigang(kaigang);
            base.kaigang(kaigang);
        }

        public override void action_kaiju(Kaiju kaiju)
        {
            this._callback?.Invoke(null);
        }

        public override void action_qipai(Qipai qipai)
        {
            this._callback?.Invoke(null);
        }

        public override void action_zimo(Zimo zimo, bool gangzimo)
        {
            if (zimo.l != this._menfeng) 
            {
                this._callback?.Invoke(null);
                return;
            }
            string m;
            if (this.select_hule((Dapai)null, gangzimo))
            {
                this._callback?.Invoke(new Reply { hule = "-" });
            }
            else if (this.select_pingju())
            {
                this._callback?.Invoke(new Reply { daopai = "-" });
            }
            else if ((m = this.select_gang()) != null)
            {
                this._callback?.Invoke(new Reply { gang = m });
            }
            else
            {
                this._callback?.Invoke(new Reply { dapai = this.select_dapai() });
            }
        }

        public override void action_dapai(Dapai dapai)
        {
            if (dapai.l == this._menfeng)
            {
                if (this.select_daopai())
                {
                    this._callback?.Invoke(new Reply { daopai = "-" });
                }
                else
                {
                    this._callback?.Invoke(null);
                }
                return;
            }
            string m;
            if (this.select_hule(dapai))
            {
                this._callback?.Invoke(new Reply { hule = "-" });
            }
            else if ((m = this.select_fulou(dapai)) != null)
            {
                this._callback?.Invoke(new Reply { fulou = m });
            }
            else if (this.select_daopai())
            {
                this._callback?.Invoke(new Reply { daopai = "-" });
            }
            else
            {
                this._callback?.Invoke(null);
            }
        }

        public override void action_fulou(Fulou fulou)
        {
            if (fulou.l != this._menfeng || Regex.IsMatch(fulou.m, @"^[mpsz]\d{4}"))
            {
                this._callback?.Invoke(null);
            }
            else
            {
                this._callback?.Invoke(new Reply { dapai = this.select_dapai() });
            }
        }

        public override void action_gang(Gang gang)
        {
            if (gang.l == this._menfeng || !this.select_hule(gang, true))
            {
                this._callback?.Invoke(null);
            }
            else
            {
                this._callback?.Invoke(new Reply { hule = "-" });
            }
        }

        public override void action_hule(Hule hule)
        {
            this._callback?.Invoke(null);
        }

        public override void action_pingju(Pingju pingju)
        {
            this._callback?.Invoke(null);
        }

        public override void action_jieju(Paipu paipu)
        {
            this._callback?.Invoke(null);
        }

        public bool select_hule(Dapai data, bool hupai = false, List<HuleInfo> info = null)
        {
            string rongpai = null;
            if (data != null)
            {
                string d = new string[] { "", "+", "=", "-" }[(4 + data.l - this._menfeng) % 4];
                rongpai = data.p.Substring(0, 2) + d;
            }
            bool hule = this.allow_hule(this.shoupai, rongpai, hupai);

            if (info != null && hule)
            {
                Shoupai shoupai = this.shoupai.clone();
                if (rongpai != null) shoupai.zimo(rongpai);
                info.Add(new HuleInfo
                {
                    m = "",
                    n_xiangting = -1,
                    ev = this.get_defen(this.shoupai, rongpai),
                    shoupai = shoupai.ToString()
                });
            }

            return hule;
        }

        public bool select_hule(Gang data, bool hupai = false, List<HuleInfo> info = null)
        {
            string rongpai = null;
            if (data != null)
            {
                if (data.m != null && Regex.IsMatch(data.m, @"^[mpsz]\d{4}$")) return false;
                string d = new string[] { "", "+", "=", "-" }[(4 + data.l - this._menfeng) % 4];
                rongpai = data.m[0] + data.m.Substring(data.m.Length - 1) + d;
            }
            bool hule = this.allow_hule(this.shoupai, rongpai, hupai);

            if (info != null && hule)
            {
                Shoupai shoupai = this.shoupai.clone();
                if (rongpai != null) shoupai.zimo(rongpai);
                info.Add(new HuleInfo
                {
                    m = "",
                    n_xiangting = -1,
                    ev = this.get_defen(this.shoupai, rongpai),
                    shoupai = shoupai.ToString()
                });
            }

            return hule;
        }

        public bool select_pingju()
        {
            if (Majiang.Util.xiangting(this.shoupai) < 4) return false;
            return this.allow_pingju(this.shoupai);
        }

        public string select_fulou(Dapai dapai, List<HuleInfo> info = null)
        {
            double n_xiangting = Majiang.Util.xiangting(this.shoupai);
            if (this._model.shoupai.Find(s => s.lizhi) != null && n_xiangting >= 3) return null;

            string d = new string[] { "", "+", "=", "-" }[(4 + dapai.l - this._menfeng) % 4];
            string p = dapai.p.Substring(0, 2) + d;

            if (n_xiangting < 3)
            {
                List<string> mianzi = this.get_gang_mianzi(this.shoupai, p)
                                        .Concat(this.get_peng_mianzi(this.shoupai, p))
                                        .Concat(this.get_chi_mianzi(this.shoupai, p))
                                        .ToList();
                if (mianzi.Count == 0) return null;

                string fulou = null;
                Paishu paishu = this._suanpai.paishu_all();
                double max = this.eval_shoupai(this.shoupai, paishu, "");

                if (info != null)
                {
                    info.Add(new HuleInfo
                    {
                        m = "",
                        n_xiangting = n_xiangting,
                        ev = max,
                        shoupai = this.shoupai.ToString()
                    });
                }

                foreach (string m in mianzi)
                {
                    Shoupai shoupai = this.shoupai.clone().fulou(m);
                    double x = Majiang.Util.xiangting(shoupai);
                    if (x >= 3) continue;

                    double ev = this.eval_shoupai(shoupai, paishu);

                    if (info != null && ev > 0)
                    {
                        info.Add(new HuleInfo
                        {
                            m = m,
                            n_xiangting = x,
                            ev = ev,
                            shoupai = shoupai.ToString()
                        });
                    }

                    if (this._model.shoupai.Find(s => s.lizhi) != null)
                    {
                        if (x > 0 && ev < 1200) continue;
                        if (x == 0 && ev < 500) continue;
                    }

                    if (ev - max > 0.0000001)
                    {
                        max = ev;
                        fulou = m;
                    }
                }
                return fulou;
            }
            else
            {
                List<string> mianzi = this.get_peng_mianzi(this.shoupai, p)
                                        .Concat(this.get_chi_mianzi(this.shoupai, p))
                                        .ToList();
                if (mianzi.Count == 0) return null;

                n_xiangting = this.xiangting(this.shoupai);

                Paishu paishu = null;
                if (info != null)
                {
                    paishu = this._suanpai.paishu_all();
                    double ev = this.eval_shoupai(this.shoupai, paishu);
                    int n_tingpai = Majiang.Util.tingpai(this.shoupai)
                                            .Select(p => this._suanpai._paishu[p])
                                            .Sum();
                    info.Add(new HuleInfo
                    {
                        m = "",
                        n_xiangting = n_xiangting,
                        ev = ev,
                        n_tingpai = n_tingpai,
                        shoupai = this.shoupai.ToString()
                    });
                }

                foreach (string m in mianzi)
                {
                    Shoupai shoupai = this.shoupai.clone().fulou(m);
                    double x = this.xiangting(shoupai);
                    if (x >= n_xiangting) continue;

                    if (info != null)
                    {
                        info.Add(new HuleInfo
                        {
                            m = m,
                            n_xiangting = x,
                            shoupai = shoupai.ToString()
                        });
                    }

                    return m;
                }
            }
            return null;
        }

        public string select_gang(List<HuleInfo> info = null)
        {
            double n_xiangting = Majiang.Util.xiangting(this.shoupai);
            if (this._model.shoupai.Find(s => s.lizhi) != null && n_xiangting > 0) return null;

            Paishu paishu = this._suanpai.paishu_all();

            if (n_xiangting < 3)
            {
                string gang = null;
                double max = this.eval_shoupai(this.shoupai, paishu);
                foreach (string m in this.get_gang_mianzi(this.shoupai))
                {
                    Shoupai shoupai = this.shoupai.clone().gang(m);
                    double x = Majiang.Util.xiangting(shoupai);
                    if (x >= 3) continue;

                    double ev = this.eval_shoupai(shoupai, paishu);

                    if (info != null)
                    {
                        string p = Regex.IsMatch(m, @"\d{4}$") ? m.Substring(0, 2)
                                                            : m[0] + m.Substring(m.Length - 1);
                        List<string> tingpai = Majiang.Util.tingpai(shoupai);
                        int n_tingpai = tingpai.Select(p => this._suanpai._paishu[p])
                                            .Sum();
                        info.Add(new HuleInfo
                        {
                            p = p,
                            m = m,
                            n_xiangting = x,
                            ev = ev,
                            tingpai = tingpai,
                            n_tingpai = n_tingpai
                        });
                    }

                    if (ev - max > -0.0000001)
                    {
                        gang = m;
                        max = ev;
                    }
                }
                return gang;
            }
            else
            {
                n_xiangting = this.xiangting(this.shoupai);

                foreach (string m in this.get_gang_mianzi(this.shoupai))
                {
                    Shoupai shoupai = this.shoupai.clone().gang(m);
                    if (this.xiangting(shoupai) == n_xiangting)
                    {
                        if (info != null)
                        {
                            string p = Regex.IsMatch(m, @"\d{4}$") ? m.Substring(0, 2)
                                                                : m[0] + m.Substring(m.Length - 1);
                            double ev = this.eval_shoupai(shoupai, paishu);
                            List<string> tingpai = Majiang.Util.tingpai(shoupai);
                            int n_tingpai = tingpai.Select(p => this._suanpai._paishu[p]).Sum();
                            info.Add(new HuleInfo
                            {
                                p = p,
                                m = m,
                                n_xiangting = n_xiangting,
                                ev = ev,
                                tingpai = tingpai,
                                n_tingpai = n_tingpai
                            });
                        }

                        return m;
                    }
                }
            }
            return null;
        }

        public string select_dapai(List<HuleInfo> info = null)
        {
            string anquan = null;
            double min = double.PositiveInfinity;
            Func<string, double> weixian = this._suanpai.suan_weixian_all(this.shoupai._bingpai);
            if (weixian != null)
            {
                foreach (string p in this.get_dapai(this.shoupai))
                {
                    if (weixian(p) < min)
                    {
                        min = weixian(p);
                        anquan = p;
                    }
                }
            }

            string dapai = anquan;
            double max = -1;
            int min_tingpai = 0;
            List<string> backtrack = new List<string>();
            double n_xiangting = Majiang.Util.xiangting(this.shoupai);
            Paishu paishu = this._suanpai.paishu_all();
            Func<string, int> paijia = this._suanpai.make_paijia(this.shoupai);
            Func<string, int> cmp = x => paijia(x);
            foreach (string p in this.get_dapai(this.shoupai).Reversed().OrderBy(x => paijia(x)))
            {
                if (dapai == null) dapai = p;
                Shoupai shoupai = this.shoupai.clone().dapai(p);
                if (n_xiangting > 2 && this.xiangting(shoupai) > n_xiangting ||
                    Majiang.Util.xiangting(shoupai) > n_xiangting)
                {
                    if (anquan != null) continue;
                    if (n_xiangting < 2) backtrack.Add(p);
                    continue;
                }

                double ev = this.eval_shoupai(shoupai, paishu);

                List<string> tingpai = Majiang.Util.tingpai(shoupai);
                int n_tingpai = tingpai.Select(p => this._suanpai._paishu[p]).Sum();

                if (info != null)
                {
                    info.ForEach(i =>
                    {
                        if (i.p == p.Substring(0, 2) && i.m != null)
                            i.weixian = weixian != null ? weixian(p) : 0;
                    });
                    if (!info.Any(i => i.p == p.Substring(0, 2) && i.m == null))
                    {
                        info.Add(new HuleInfo
                        {
                            p = p.Substring(0, 2),
                            n_xiangting = n_xiangting,
                            ev = ev,
                            tingpai = tingpai,
                            n_tingpai = n_tingpai,
                            weixian = weixian != null ? weixian(p) : 0
                        });
                    }
                }

                if (weixian != null && weixian(p) > min)
                {
                    if (weixian(p) >= 13.5) continue;
                    if (n_xiangting > 2 || n_xiangting > 0 && ev < 300)
                    {
                        if (weixian(p) >= 8.0) continue;
                        if (min < 3.0) continue;
                    }
                    else if (n_xiangting > 0 && ev < 1200 ||
                            n_xiangting == 0 && ev < 200)
                    {
                        if (weixian(p) >= 8.0) continue;
                        if (min < 3.0 && weixian(p) >= 3.0) continue;
                    }
                }

                if (ev - max > 0.0000001)
                {
                    max = ev;
                    dapai = p;
                    min_tingpai = n_tingpai * 6;
                }
            }
            double tmp_max = max;

            foreach (string p in backtrack)
            {
                Shoupai shoupai = this.shoupai.clone().dapai(p);
                List<string> tingpai = Majiang.Util.tingpai(shoupai);
                int n_tingpai = tingpai.Select(p => this._suanpai._paishu[p]).Sum();
                if (n_tingpai < min_tingpai) continue;

                string back = p[0].ToString() + ((int.Parse(p[1].ToString()) != 0) ? p[1].ToString() : "5");
                double ev = this.eval_backtrack(shoupai, paishu, back, tmp_max * 2);

                if (info != null && ev > 0)
                {
                    if (!info.Any(i => i.p == p.Substring(0, 2) && i.m == null))
                    {
                        info.Add(new HuleInfo
                        {
                            p = p.Substring(0, 2),
                            n_xiangting = n_xiangting + 1,
                            ev = ev,
                            tingpai = tingpai,
                            n_tingpai = n_tingpai
                        });
                    }
                }

                if (ev - max > 0.0000001)
                {
                    max = ev;
                    dapai = p;
                }
            }

            if (anquan != null)
            {
                if (info != null && dapai == anquan
                    && !info.Any(i => i.m == null && i.p == anquan.Substring(0, 2)))
                {
                    info.Add(new HuleInfo
                    {
                        p = anquan.Substring(0, 2),
                        n_xiangting = Majiang.Util.xiangting(this.shoupai.clone().dapai(anquan)),
                        weixian = weixian != null ? weixian(anquan) : 0
                    });
                }
            }

            if (this.select_lizhi(dapai) && max >= 200) dapai += "*";
            return dapai;
        }

        public bool select_lizhi(string p)
        {
            return this.allow_lizhi(this.shoupai, p) != null;
        }

        public bool select_daopai()
        {
            return this.allow_no_daopai(this.shoupai);
        }

        public double xiangting(Shoupai shoupai)
        {
            Func<Shoupai, double> xiangting_menqian = (Shoupai shoupai) =>
            {
                return shoupai.menqian ? Majiang.Util.xiangting(shoupai.clone()) : double.PositiveInfinity;
            };

            Func<Shoupai, int, int, SuanPai, double> xiangting_fanpai = (Shoupai shoupai, int zhuangfeng, int menfeng, SuanPai suanpai) =>
            {
                int n_fanpai = 0;
                string back = null;
                foreach (int n in new int[] { zhuangfeng + 1, menfeng + 1, 5, 6, 7 })
                {
                    if (shoupai._bingpai.z[n] >= 3) n_fanpai++;
                    else if (shoupai._bingpai.z[n] == 2 && suanpai._paishu.z[n] != 0) back = $"z{n}{n}{n}+";
                    foreach (string m in shoupai._fulou)
                    {
                        if (m[0] == 'z' && int.Parse(m[1].ToString()) == n) n_fanpai++;
                    }
                }
                if (n_fanpai != 0) return Majiang.Util.xiangting(shoupai.clone());
                if (back != null)
                {
                    Shoupai new_shoupai = shoupai.clone();
                    new_shoupai.fulou(back, false);
                    new_shoupai._zimo = null;
                    return Majiang.Util.xiangting(new_shoupai) + 1;
                }
                return double.PositiveInfinity;
            };

            Func<Shoupai, Rule, double> xiangting_duanyao = (Shoupai shoupai, Rule rule) =>
            {
                if (!rule.クイタンあり && !shoupai.menqian) return double.PositiveInfinity;
                if (shoupai._fulou.Any(m => Regex.IsMatch(m, @"^z|[19]"))) return double.PositiveInfinity;
                Shoupai new_shoupai = shoupai.clone();
                foreach (char s in "mps")
                {
                    new_shoupai._bingpai[s][1] = 0;
                    new_shoupai._bingpai[s][9] = 0;
                }
                new_shoupai._bingpai.z.Clear();
                return Majiang.Util.xiangting(new_shoupai);
            };

            Func<Shoupai, double> xiangting_duidui = (Shoupai shoupai) =>
            {
                if (shoupai._fulou.Select(m => m.Replace("0", "5")).Any(m => !Regex.IsMatch(m, @"^[mpsz](\d)\1\1"))) return double.PositiveInfinity;
                int n_kezi = shoupai._fulou.Count, n_duizi = 0;
                foreach (char s in "mpsz")
                {
                    int[] bingpai = shoupai._bingpai[s];
                    for (int n = 1; n < bingpai.Length; n++)
                    {
                        if (bingpai[n] >= 3) n_kezi++;
                        else if (bingpai[n] == 2) n_duizi++;
                    }
                }
                if (n_kezi + n_duizi > 5) n_duizi = 5 - n_kezi;
                return 8 - n_kezi * 2 - n_duizi;
            };

            Func<Shoupai, char, double> xiangting_yise = (Shoupai shoupai, char suit) =>
            {
                Regex regexp = new Regex($"^[z{suit}]");
                if (shoupai._fulou.Any(m => !regexp.IsMatch(m))) return double.PositiveInfinity;
                Shoupai new_shoupai = shoupai.clone();
                foreach (char s in "mps")
                {
                    if (s != suit) new_shoupai._bingpai[s].Clear();
                }
                return Majiang.Util.xiangting(new_shoupai);
            };

            var tasks = new Task<double>[]
            {
                Task.Run(() => xiangting_menqian(shoupai)),
                Task.Run(() => xiangting_fanpai(shoupai, this._model.zhuangfeng, this._menfeng, this._suanpai)),
                Task.Run(() => xiangting_duanyao(shoupai, this._rule)),
                Task.Run(() => xiangting_duidui(shoupai)),
                Task.Run(() => xiangting_yise(shoupai, 'm')),
                Task.Run(() => xiangting_yise(shoupai, 'p')),
                Task.Run(() => xiangting_yise(shoupai, 's'))
            };

            Task.WaitAll(tasks);

            return tasks.Min(task => task.Result);
        }

        public List<string> tingpai(Shoupai shoupai)
        {
            double n_xiangting = this.xiangting(shoupai);

            List<string> pai = new List<string>();
            foreach (string p in Majiang.Util.tingpai(shoupai, s => this.xiangting(s)))
            {
                if (n_xiangting > 0)
                {
                    foreach (string m in this.get_peng_mianzi(shoupai, p + "+"))
                    {
                        Shoupai new_shoupai = shoupai.clone().fulou(m);
                        if (this.xiangting(new_shoupai) < n_xiangting)
                        {
                            pai.Add(p + "+");
                            break;
                        }
                    }
                    if (pai.Count > 0 && pai[pai.Count - 1] == p + "+") continue;

                    foreach (string m in this.get_chi_mianzi(shoupai, p + "-"))
                    {
                        Shoupai new_shoupai = shoupai.clone().fulou(m);
                        if (this.xiangting(new_shoupai) < n_xiangting)
                        {
                            pai.Add(p + "-");
                            break;
                        }
                    }
                    if (pai.Count > 0 && pai[pai.Count - 1] == p + "-") continue;
                }
                pai.Add(p);
            }
            return pai;
        }

        public int get_defen(Shoupai shoupai, string rongpai = null)
        {
            string paistr = shoupai.ToString();
            if (rongpai != null)
                paistr = Regex.Replace(paistr, @"^([^\*\,]*)(.*)$", $"$1{rongpai}$2");
            if (this._defen_cache.TryGetValue(paistr, out var cache)) return cache;

            var param = new HuleParam
            {
                rule = this._rule,
                zhuangfeng = this._model.zhuangfeng,
                menfeng = this._menfeng,
                hupai = new Hupai { lizhi = shoupai.menqian ? 1 : 0 },
                baopai = this.shan.baopai,
                jicun = new Jicun { changbang = 0, lizhibang = 0 }
            };
            var hule = Majiang.Util.hule(shoupai, rongpai, param);

            this._defen_cache[paistr] = hule.defen;
            return hule.defen;
        }

        public double eval_shoupai(Shoupai shoupai, Paishu paishu, string back = null)
        {
            string paistr = shoupai.ToString() + (back != null ? $":{back}" : "");
            if (this._eval_cache.TryGetValue(paistr, out var cache)) return cache;

            double rv = 0;
            double n_xiangting = Majiang.Util.xiangting(shoupai);

            if (n_xiangting == -1)
            {
                rv = this.get_defen(shoupai);
            }
            else if (shoupai._zimo != null)
            {
                foreach (string p in this.get_dapai(shoupai))
                {
                    Shoupai new_shoupai = shoupai.clone().dapai(p);
                    if (Majiang.Util.xiangting(new_shoupai) > n_xiangting) continue;

                    double ev = this.eval_shoupai(new_shoupai, paishu, back);

                    if (ev > rv) rv = ev;
                }
            }
            else if (n_xiangting < 3)
            {
                foreach (string p in add_hongpai(Majiang.Util.tingpai(shoupai)))
                {
                    if (p == back) { rv = 0; break; }
                    if (paishu[p] == 0) continue;
                    Shoupai new_shoupai = shoupai.clone().zimo(p);
                    paishu[p]--;

                    double ev = this.eval_shoupai(new_shoupai, paishu, back);
                    if (back == null)
                    {
                        if (n_xiangting > 0)
                            ev += this.eval_fulou(shoupai, p, paishu, back);
                    }

                    paishu[p]++;
                    rv += ev * paishu[p];
                }
                rv /= width[(int) n_xiangting];
            }
            else
            {
                foreach (string p in add_hongpai(this.tingpai(shoupai)))
                {
                    if (paishu[p.Substring(0, 2)] == 0) continue;

                    char d = p.Length > 2 ? p[2] : (char) 0;
                    rv += paishu[p.Substring(0, 2)] * (d == '+' ? 4 : d == '-' ? 2 : 1);
                }
            }

            this._eval_cache[paistr] = rv;
            return rv;
        }

        public double eval_backtrack(Shoupai shoupai, Paishu paishu, string back, double min)
        {
            double n_xiangting = Majiang.Util.xiangting(shoupai);

            double rv = 0;
            foreach (string p in add_hongpai(Majiang.Util.tingpai(shoupai)))
            {
                if (p.Replace("0", "5") == back) continue;
                if (paishu[p] == 0) continue;

                Shoupai new_shoupai = shoupai.clone().zimo(p);
                paishu[p]--;

                double ev = this.eval_shoupai(new_shoupai, paishu, back);

                paishu[p]++;
                if (ev - min > 0.0000001) rv += ev * paishu[p];
            }
            return rv / width[(int) n_xiangting];
        }

        public double eval_fulou(Shoupai shoupai, string p, Paishu paishu, string back)
        {
            double n_xiangting = Majiang.Util.xiangting(shoupai);

            double peng_max = 0;
            foreach (string m in this.get_peng_mianzi(shoupai, p + "+"))
            {
                Shoupai new_shoupai = shoupai.clone().fulou(m);
                if (Majiang.Util.xiangting(new_shoupai) >= n_xiangting) continue;
                peng_max = Math.Max(this.eval_shoupai(new_shoupai, paishu, back), peng_max);
            }

            double chi_max = 0;
            foreach (string m in this.get_chi_mianzi(shoupai, p + "-"))
            {
                Shoupai new_shoupai = shoupai.clone().fulou(m);
                if (Majiang.Util.xiangting(new_shoupai) >= n_xiangting) continue;
                chi_max = Math.Max(this.eval_shoupai(new_shoupai, paishu, back), chi_max);
            }

            return peng_max > chi_max ? peng_max * 3 : peng_max * 2 + chi_max;
        }
    }
}