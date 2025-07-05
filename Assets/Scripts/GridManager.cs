using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 4;
    public int columns = 6;
    public float cellSize = 1f;
    public float cellSpacing = 0.1f;

    [Header("Tile Settings")]
    public GameObject tilePrefab;

    [Header("Positioning")]
    public Vector2 originPosition = Vector2.zero;

    private void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        float centerX = (columns - 1) / 2.0f;
        float centerY = (rows - 1) / 2.0f;
        
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                float posX = originPosition.x + (x - centerX) * (cellSize + cellSpacing);
                float posY = originPosition.y + (y - centerY) * (cellSize + cellSpacing);
                Vector2 position = new Vector2(posX, posY);

                Instantiate(tilePrefab, position, Quaternion.identity, transform);
            }
        }
    }
}