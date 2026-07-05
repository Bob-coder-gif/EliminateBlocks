using System.Collections.Generic;

namespace Match3
{
    /// <summary>一段连续同色的匹配（run）。横向时 Line 是行号 y、Start 是起始列 x；纵向反之。</summary>
    public class MatchRun
    {
        public bool Horizontal;
        public int Line;    // 横向：所在行 y；纵向：所在列 x
        public int Start;   // 横向：起始列 x；纵向：起始行 y
        public int Length;  // 连续长度（>=3）
    }

    /// <summary>
    /// 匹配检测：只负责“哪里有连续三连及以上”，不判断形状、不决定消除范围。
    /// FindRuns 给出所有横向 / 纵向连续段，交给 ClearResolver 去按规则算最终消除范围。
    /// FindAll / HasMatch 仍返回基础匹配块，供“交换是否有效”“死局判断”等只关心基础三连的地方使用。
    /// </summary>
    public class MatchFinder
    {
        private readonly BoardGrid grid;

        public MatchFinder(BoardGrid grid)
        {
            this.grid = grid;
        }

        /// <summary>找出所有横向和纵向的连续段（长度 >= 3）。</summary>
        public List<MatchRun> FindRuns()
        {
            var runs = new List<MatchRun>();
            int W = grid.Width, H = grid.Height;

            // 横向
            for (int y = 0; y < H; y++)
            {
                int runStart = 0;
                for (int x = 1; x <= W; x++)
                {
                    bool same = x < W
                                && grid.Get(x, y) != null && grid.Get(runStart, y) != null
                                && grid.Get(x, y).type == grid.Get(runStart, y).type;
                    if (!same)
                    {
                        int len = x - runStart;
                        if (len >= 3)
                            runs.Add(new MatchRun { Horizontal = true, Line = y, Start = runStart, Length = len });
                        runStart = x;
                    }
                }
            }

            // 纵向
            for (int x = 0; x < W; x++)
            {
                int runStart = 0;
                for (int y = 1; y <= H; y++)
                {
                    bool same = y < H
                                && grid.Get(x, y) != null && grid.Get(x, runStart) != null
                                && grid.Get(x, y).type == grid.Get(x, runStart).type;
                    if (!same)
                    {
                        int len = y - runStart;
                        if (len >= 3)
                            runs.Add(new MatchRun { Horizontal = false, Line = x, Start = runStart, Length = len });
                        runStart = y;
                    }
                }
            }

            return runs;
        }

        /// <summary>枚举一个 run 覆盖的所有块。</summary>
        public IEnumerable<Tile> RunTiles(MatchRun run)
        {
            for (int i = run.Start; i < run.Start + run.Length; i++)
            {
                Tile t = run.Horizontal ? grid.Get(i, run.Line) : grid.Get(run.Line, i);
                if (t != null) yield return t;
            }
        }

        /// <summary>所有基础匹配块（去重）。用于判断“有没有三连”。</summary>
        public HashSet<Tile> FindAll()
        {
            var matched = new HashSet<Tile>();
            foreach (var run in FindRuns())
                foreach (var t in RunTiles(run))
                    matched.Add(t);
            return matched;
        }

        public bool HasMatch() => FindRuns().Count > 0;
    }
}