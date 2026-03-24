using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PShadowGridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 3;
    [SerializeField] private int height = 3;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float cellSpacing = .2f;
    
    [Tooltip("Keep this low aprox. around 0.08 - 0.25")]
    [SerializeField] private float slidingTime = 0.15f;

    private PShadowPillar[,] grid;
    private bool isMoving;
    private int activeMoves;

    private void Awake()
    {
        grid = new PShadowPillar[width, height];
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        float step = cellSize + cellSpacing;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = transform.position + new Vector3((x + 0.5f) * step, 0f, (y + 0.5f) * step);

                Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.01f, cellSize));

                UnityEditor.Handles.Label(worldPos + Vector3.up * 0.1f, $"({y},{x})");
            }
        }
    }
#endif

    private Vector3 GridToWorld(Vector2Int pos, float pillarY)
    {
        float step = cellSize + cellSpacing;
        
        return transform.position + new Vector3((pos.x + .5f) * step, pillarY, (pos.y + .5f) * step);
    }

    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool RegisterPillar(PShadowPillar pillar, Vector2Int pos)
    {
        if (!IsInBounds(pos))
        {
            Debug.LogError($"{pillar.name} startPos {pos} is out of bounds!");
            return false;
        }
        if (grid[pos.x, pos.y])
        {
            Debug.LogError($"Cell {pos} is already occupied by {grid[pos.x, pos.y].name}!");
            return false;
        }
        
        grid[pos.x, pos.y] = pillar;
        pillar.transform.position = GridToWorld(pos, pillar.transform.position.y);
        return true;
    }

    public void ResetPillars()
    {
        List<PShadowPillar> pillarsToRegister = grid.Cast<PShadowPillar>().Where(pillar => pillar != null).ToList();
        foreach (PShadowPillar pillar in pillarsToRegister)
        {
            RegisterPillar(pillar, pillar.startPosition);
        }
    }

    public void TryPushLine(bool isRow, int index, int direction)
    {
        if (isMoving) return;
        if (direction != -1 && direction != 1) return;

        if (isRow)
        {
            if (index < 0 || index >= height) return;
            PushRow(index, direction);
        }
        else
        {
            if (index < 0 || index >= width) return;
            PushColumn(index, direction);
        }
    }
    
    // direction = -1 (left) or 1 (right)
    private void PushRow(int row, int direction)
    {
        List<MoveRequest> moves = new();
        
        if (direction == -1)
        {
            // Pack towards the left edge
            int targetX = 0;

            for (int x = 0; x < width; x++)
            {
                PShadowPillar pillar = grid[x, row];
                if (!pillar) continue;

                if (x != targetX)
                {
                    moves.Add(new MoveRequest(pillar, new(x, row), new(targetX, row)));
                }

                targetX++;
            }
        }
        else
        {
            // Pack towards the right edge
            int targetX = width - 1;

            for (int x = width - 1; x >= 0; x--)
            {
                PShadowPillar pillar = grid[x, row];
                if (!pillar) continue;

                if (x != targetX)
                {
                    moves.Add(new MoveRequest(pillar, new(x, row), new(targetX, row)));
                }
                
                targetX--;
            }
        }

        StartMoves(moves);
    }
    
    // direction = -1 (down) or 1 (up)
    private void PushColumn(int column, int direction)
    {
        List<MoveRequest> moves = new();
        
        if (direction == -1)    
        {
            // Pack towards the bottom edge
            int targetY = 0;
            
            for (int y = 0; y < height; y++)
            {
                PShadowPillar pillar = grid[column, y];
                if (!pillar) continue;

                if (y != targetY)
                {
                    moves.Add(new MoveRequest(pillar, new(column, y), new(column, targetY)));
                }
                
                targetY++;
            }
        }
        else
        {
            // Pack towards the top edge
            int targetY = height - 1;
            
            for (int y = height - 1; y >= 0; y--)
            {
                PShadowPillar pillar = grid[column, y];
                if  (!pillar) continue;

                if (y != targetY)
                {
                    moves.Add(new MoveRequest(pillar, new(column, y), new(column, targetY)));
                }
                
                targetY--;
            }
        }

        StartMoves(moves);
    }

    private void StartMoves(List<MoveRequest> moves)
    {
        if (moves.Count == 0) return;
        
        isMoving = true;
        activeMoves = moves.Count;

        foreach (MoveRequest move in moves)
        {
            // Update logical grid immediately to the final state
            grid[move.from.x, move.from.y] = null;
            grid[move.to.x, move.to.y] = move.pillar;
            
            // Animate the pillar
            move.pillar.MoveTo(
                GridToWorld(move.to, move.pillar.transform.position.y),
                slidingTime,
                OnPillarFinishedMoving
            );
        }
    }

    private void OnPillarFinishedMoving()
    {
        activeMoves--;
        if (activeMoves > 0) return;
        
        activeMoves = 0;
        isMoving = false;
    }

    private struct MoveRequest
    {
        public readonly PShadowPillar pillar;
        public readonly Vector2Int from;
        public readonly Vector2Int to;

        public MoveRequest(PShadowPillar pillar, Vector2Int from, Vector2Int to)
        {
            this.pillar = pillar;
            this.from = from;
            this.to = to;
        }
    }
}
