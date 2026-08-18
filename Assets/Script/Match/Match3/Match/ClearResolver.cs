using System.Collections.Generic;

namespace Match3
{
    /// <summary>一次消除结算的结果：要消的块 + 检测到的匹配连数。</summary>
    public struct ClearResult
    {
        public HashSet<Tile> Tiles;   // 本轮实际要消的所有块（去重）
        public int MatchedCount;      // 检测到的匹配连数（所有 run 长度之和）
    }

    /// <summary>
    /// 消除范围计算：拿到 MatchFinder 找出的所有连续段（run），按形状规则算出本轮该消除的全部块。
    /// 规则集中在这里，想调整特殊消除逻辑只改这一个类。
    ///
    /// 规则：
    ///   横向 4 连 → 消整行；横向 >=5 连 → 消三行（所在行 ±1）
    ///   纵向 4 连 → 消整列；纵向 >=5 连 → 消三列（所在列 ±1）
    ///   同时属于横向与纵向 run 的交汇块（T/L/+）→ 消该块所在的一行 + 一列
    ///   多个条件同时命中时取并集，不递归（整行/整列扫出的其它连不再触发各自特效）。
    /// </summary>
    public class ClearResolver
    {
        private readonly BoardGrid grid;
        private readonly MatchFinder matchFinder;

        public ClearResolver(BoardGrid grid, MatchFinder matchFinder)
        {
            this.grid = grid;
            this.matchFinder = matchFinder;
        }

        /// <summary>返回本轮要消除的所有块（含特殊消除）及检测到的连数。没有任何三连时 Tiles 为空。</summary>
        public ClearResult Resolve()
        {
            var runs = matchFinder.FindRuns();
            var result = new HashSet<Tile>();
            int matchedCount = 0;

            if (runs.Count == 0)
                return new ClearResult { Tiles = result, MatchedCount = 0 };

            var hTiles = new HashSet<Tile>();   // 属于横向 run 的块
            var vTiles = new HashSet<Tile>();   // 属于纵向 run 的块
            var rows = new HashSet<int>();       // 要整行消除的行号
            var cols = new HashSet<int>();       // 要整列消除的列号

            foreach (var run in runs)
            {
                matchedCount += run.Length;   // 累加检测到的连数

                foreach (var t in matchFinder.RunTiles(run))
                {
                    result.Add(t);   // 基础匹配块一定消
                    if (run.Horizontal) hTiles.Add(t); else vTiles.Add(t);
                }

                // 直线特殊消除
                if (run.Horizontal)
                {
                    if (run.Length == 4) rows.Add(run.Line);
                    else if (run.Length >= 5) AddRange(rows, run.Line - 1, run.Line + 1, grid.Height);
                }
                else
                {
                    if (run.Length == 4) cols.Add(run.Line);
                    else if (run.Length >= 5) AddRange(cols, run.Line - 1, run.Line + 1, grid.Width);
                }
            }

            // 交汇块（T/L/+）：既在横向又在纵向 run 里的块，消它所在的一行 + 一列
            foreach (var t in hTiles)
                if (vTiles.Contains(t))
                {
                    rows.Add(t.y);
                    cols.Add(t.x);
                }

            // 把要清除的整行 / 整列的块并进结果
            foreach (int y in rows)
                for (int x = 0; x < grid.Width; x++)
                {
                    var t = grid.Get(x, y);
                    if (t != null) result.Add(t);
                }
            foreach (int x in cols)
                for (int y = 0; y < grid.Height; y++)
                {
                    var t = grid.Get(x, y);
                    if (t != null) result.Add(t);
                }

            return new ClearResult { Tiles = result, MatchedCount = matchedCount };
        }

        /// <summary>把 [from, to] 内、落在 [0, size) 的整数加入集合（越界自动裁掉）。</summary>
        private void AddRange(HashSet<int> set, int from, int to, int size)
        {
            for (int i = from; i <= to; i++)
                if (i >= 0 && i < size) set.Add(i);
        }
    }
}