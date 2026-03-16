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

    private Pillar[,] grid;

    private void Awake()
    {
        grid = new Pillar[width, height];
    }
    
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

#if UNITY_EDITOR
                UnityEditor.Handles.Label(worldPos + Vector3.up * 0.1f, $"({x},{y})");
#endif
            }
        }
    }

    private Vector3 GridToWorld(Vector2Int pos, float pillarY)
    {
        float step = cellSize + cellSpacing;
        
        return transform.position + new Vector3((pos.x + .5f) * step, pillarY, (pos.y + .5f) * step);
    }

    public void RegisterPillar(Pillar pillar, Vector2Int pos)
    {
        grid[pos.x, pos.y] = pillar;
        pillar.gridPos = pos;
        pillar.transform.position = GridToWorld(pos, pillar.transform.position.y);
    }

    private void MovePillar(Vector2Int from, Vector2Int to)
    {
        Pillar pillar = grid[from.x, from.y];

        grid[to.x, to.y] = pillar;
        grid[from.x, from.y] = null;
        
        Vector3 target = GridToWorld(to, pillar.transform.position.y);
        pillar.MoveTo(target, to, slidingTime);
    }

    public void PushColumn(int column, int direction)
    {
        if (direction == -1)    // direction = -1 (down) or 1 (up)
        {
            for (int x = 1; x < width; x++)
            {
                if (grid[x, column] != null && grid[x - 1, column] == null)
                {
                    MovePillar(new Vector2Int(x, column), new Vector2Int(x - 1, column));
                }
            }
        }
        else
        {
            for (int x = width - 2; x >= 0; x--)
            {
                if (grid[x, column] != null && grid[x + 1, column] == null)
                {
                    MovePillar(new Vector2Int(x, column), new Vector2Int(x + 1, column));
                }
            }
        }
    }
    
    public void PushRow(int row, int direction)
    {
        if (direction == -1)
        {
            for (int y = 1; y < height; y++)
            {
                if (grid[row, y] != null && grid[row, y - 1] == null)
                {
                    MovePillar(new Vector2Int(row, y), new Vector2Int(row, y - 1));
                }
            }
        }
        else
        {
            for (int y = height - 2; y >= 0; y--)
            {
                if (grid[row, y] != null && grid[row, y + 1] == null)
                {
                    MovePillar(new Vector2Int(row, y), new Vector2Int(row, y + 1));
                }
            }
        }
    }
}
