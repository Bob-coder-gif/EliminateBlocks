using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 模式B 主控制器。
    /// 只负责游戏流程调度，视觉交给 BlockRenderer，输入交给 BlockInput。
    /// 由 GameManager 动态创建。
    /// </summary>
    public class BlockGame : MonoBehaviour
    {
        // ---- 由 GameManager 赋值 ----
        public Sprite[] candySprites;
        public SpriteRenderer background;
        public GameObject clearEffectPrefab;

        // ---- 事件 ----
        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnHighScoreChanged;
        public event System.Action OnGameOver;
        public event System.Action<bool> OnUndoAvailableChanged;   // ★ UNDO 新事件

        // ---- 子系统 ----
        private BlockBoard board;
        private BlockRenderer blockRenderer;
        private BlockInput input;

        // ---- 数据 ----
        private const int ShapesPerRound = 3;
        private int[] roundShapeIndices = new int[ShapesPerRound];
        private int[] roundShapeTypes   = new int[ShapesPerRound];
        private bool[] roundShapePlaced = new bool[ShapesPerRound];
        private int score;
        private int highScore;
        private bool gameOver;

        // 消除位置缓存（避免每次 new）
        private List<Vector2Int> clearedCells = new List<Vector2Int>();

        // ★ UNDO —— 快照数据 ————————————————————————————
        private int[] snapshotGrid;
        private int   snapshotScore;
        private int[] snapshotShapeIndices = new int[ShapesPerRound];
        private int[] snapshotShapeTypes   = new int[ShapesPerRound];
        private bool[] snapshotShapePlaced = new bool[ShapesPerRound];
        private bool  canUndo;

        // BlockGame.cs 里加一个属性
public int CurrentScore => score;   // score 是你内部的分数变量
        // ——————————————————————————————————————————————————

        // ==============================================================
        //  初始化
        // ==============================================================

        private void Start()
        {
            board = new BlockBoard();
            blockRenderer = new BlockRenderer(transform, candySprites,
                                          clearEffectPrefab, 0.85f);
            input = new BlockInput(Camera.main, blockRenderer, board, transform);
            input.OnPlaced += HandlePlacement;

            highScore = BlockSaveData.GetHighScore();

            if (BlockSaveData.HasSave())
                LoadGame();
            else
                GenerateRound();

            blockRenderer.CreateGrid();
            RefreshAll();
            CenterCamera();
            FitBackground();

            OnScoreChanged?.Invoke(score);
            OnHighScoreChanged?.Invoke(highScore);
            OnUndoAvailableChanged?.Invoke(false);     // ★ UNDO
        }

        private void Update()
        {
            if (!gameOver) input.Tick();
        }

        // ==============================================================
        //  游戏流程
        // ==============================================================

        private void GenerateRound()
        {
            for (int i = 0; i < ShapesPerRound; i++)
            {
                roundShapeIndices[i] = Random.Range(0, BlockShapes.Count);
                roundShapeTypes[i]   = Random.Range(0, candySprites.Length);
                roundShapePlaced[i]  = false;
            }
            input.SetRoundData(roundShapeIndices, roundShapeTypes, roundShapePlaced);
        }

        private void HandlePlacement(int slotIndex, int gridRow, int gridCol)
        {
            // ★ UNDO —— 放置前保存快照
            SaveSnapshot();

            // 执行放置
            board.Place(roundShapeIndices[slotIndex], gridRow, gridCol,
                        roundShapeTypes[slotIndex]);
            roundShapePlaced[slotIndex] = true;

            // 消除满行/列
            int cleared = board.ClearFullLines(clearedCells);
            if (cleared > 0)
            {
                blockRenderer.SpawnClearEffects(clearedCells);

                score += BlockBoard.ScoreForLines(cleared);
                OnScoreChanged?.Invoke(score);

                if (score > highScore)
                {
                    highScore = score;
                    BlockSaveData.SetHighScore(highScore);
                    OnHighScoreChanged?.Invoke(highScore);
                }
            }

            blockRenderer.RefreshGrid(board);

            // 判断：3 个都放完了 → 新一轮
            if (AllPlaced())
            {
                GenerateRound();
                blockRenderer.RefreshTray(roundShapeIndices, roundShapeTypes, roundShapePlaced);

                if (!AnyShapeCanFit())
                {
                    EndGame();
                    return;
                }
            }
            else
            {
                // 检查剩余形状是否还能放
                if (!AnyRemainingCanFit())
                {
                    EndGame();
                    return;
                }
                blockRenderer.RefreshTray(roundShapeIndices, roundShapeTypes, roundShapePlaced);
            }

            BlockSaveData.Save(board, roundShapeIndices, roundShapeTypes,
                               roundShapePlaced, score);

            // ★ UNDO —— 放置成功后允许回溯
            SetCanUndo(true);
        }

        private void EndGame()
        {
            gameOver = true;
            BlockSaveData.ClearSave();
            OnGameOver?.Invoke();

            // ★ UNDO —— 游戏结束仍可回溯最后一步（给玩家后悔的机会）
            // canUndo 状态在 HandlePlacement 末尾已经设为 true，这里不改
        }

        // ==============================================================
        //  ★ UNDO —— 回溯
        // ==============================================================

        /// <summary>
        /// 保存当前状态快照，在每次放置之前调用。
        /// </summary>
        private void SaveSnapshot()
        {
            snapshotGrid  = board.Export();          // 需要 BlockBoard 提供
            snapshotScore = score;
            System.Array.Copy(roundShapeIndices, snapshotShapeIndices, ShapesPerRound);
            System.Array.Copy(roundShapeTypes,   snapshotShapeTypes,   ShapesPerRound);
            System.Array.Copy(roundShapePlaced,  snapshotShapePlaced,  ShapesPerRound);
        }

        /// <summary>
        /// 回溯一步。由 UI 按钮调用。
        /// </summary>
        public void Undo()
        {
            if (!canUndo || snapshotGrid == null) return;

            // 恢复棋盘
            board.Import(snapshotGrid);

            // 恢复分数
            score = snapshotScore;
            OnScoreChanged?.Invoke(score);

            // 恢复轮次数据
            System.Array.Copy(snapshotShapeIndices, roundShapeIndices, ShapesPerRound);
            System.Array.Copy(snapshotShapeTypes,   roundShapeTypes,   ShapesPerRound);
            System.Array.Copy(snapshotShapePlaced,  roundShapePlaced,  ShapesPerRound);
            input.SetRoundData(roundShapeIndices, roundShapeTypes, roundShapePlaced);

            // 如果是从 GameOver 回溯的，重置状态
            if (gameOver)
                gameOver = false;

            // 刷新显示
            RefreshAll();

            // 回溯后存档（存当前恢复的状态）
            BlockSaveData.Save(board, roundShapeIndices, roundShapeTypes,
                               roundShapePlaced, score);

            // 只能撤一步
            SetCanUndo(false);
        }

        private void SetCanUndo(bool value)
        {
            canUndo = value;
            OnUndoAvailableChanged?.Invoke(canUndo);
        }

        // ==============================================================
        //  查询
        // ==============================================================

        private bool AllPlaced()
        {
            for (int i = 0; i < ShapesPerRound; i++)
                if (!roundShapePlaced[i]) return false;
            return true;
        }

        private bool AnyRemainingCanFit()
        {
            for (int i = 0; i < ShapesPerRound; i++)
                if (!roundShapePlaced[i] && board.CanFitAnywhere(roundShapeIndices[i]))
                    return true;
            return false;
        }

        private bool AnyShapeCanFit()
        {
            for (int i = 0; i < ShapesPerRound; i++)
                if (board.CanFitAnywhere(roundShapeIndices[i]))
                    return true;
            return false;
        }

        // ==============================================================
        //  存档
        // ==============================================================

        private void LoadGame()
        {
            var data = BlockSaveData.Load();
            if (data == null) { GenerateRound(); return; }

            board.Import(data.grid);
            score = data.score;

            if (data.shapeIndices != null && data.shapeIndices.Length == ShapesPerRound)
            {
                System.Array.Copy(data.shapeIndices, roundShapeIndices, ShapesPerRound);
                System.Array.Copy(data.shapeTypes, roundShapeTypes, ShapesPerRound);
                System.Array.Copy(data.shapePlaced, roundShapePlaced, ShapesPerRound);
            }
            else
            {
                GenerateRound();
            }
            input.SetRoundData(roundShapeIndices, roundShapeTypes, roundShapePlaced);
        }

        private void RefreshAll()
        {
            blockRenderer.RefreshGrid(board);
            blockRenderer.RefreshTray(roundShapeIndices, roundShapeTypes, roundShapePlaced);
        }

        // ==============================================================
        //  摄像机 / 背景
        // ==============================================================

        private void CenterCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.orthographic = true;

            var o = blockRenderer.GridOrigin();
            float cs = blockRenderer.CellSize;
            float gridCenterX = o.x + BlockBoard.Cols * cs / 2f;
            float gridTop = o.y;
            float trayBottom = o.y - BlockBoard.Rows * cs - cs * 4f;
            float centerY = (gridTop + trayBottom) / 2f;

            cam.transform.position = new Vector3(gridCenterX, centerY, -10f);

            float halfH = (gridTop - trayBottom) / 2f + cs;
            float halfW = (BlockBoard.Cols * cs) / 2f + cs;
            float sizeByWidth = halfW / Mathf.Max(cam.aspect, 0.01f);
            cam.orthographicSize = Mathf.Max(sizeByWidth, halfH);
        }

        private void FitBackground()
        {
            if (background == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            float camH = cam.orthographicSize * 2f;
            float camW = camH * cam.aspect;
            Vector2 size = background.sprite.bounds.size;
            float scale = Mathf.Max(camW / size.x, camH / size.y);
            background.transform.localScale = new Vector3(scale, scale, 1f);
            background.transform.position = new Vector3(
                cam.transform.position.x, cam.transform.position.y,
                background.transform.position.z);
        }
    }
}