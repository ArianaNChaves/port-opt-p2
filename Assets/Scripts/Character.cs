using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Character Info")]
    public string characterName = "Character";
    public bool isPlayerControlled = true;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public int maxMovementPoints = 3; // How many moves per turn
    
    [Header("Grid Reference")]
    public GridManager gridManager;
    
    [Header("Starting Position")]
    public int startX = 0;
    public int startY = 0;
    
    [Header("Visual Feedback")]
    public GameObject selectionIndicator; // Visual indicator when this character is selected
    
    private int currentX;
    private int currentY;
    private int currentMovementPoints;
    
    private bool isMoving = false;
    private Vector2 targetPosition;
    private bool isSelected = false;
    
    // Events for turn management
    public System.Action<Character> OnMovementComplete;
    public System.Action<Character> OnMovementPointsExhausted;
    
    private void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
     
        currentX = Mathf.Clamp(startX, 0, gridManager.columns - 1);
        currentY = Mathf.Clamp(startY, 0, gridManager.rows - 1);
        
        Vector2 startPosition = GetWorldPosition(currentX, currentY);
        transform.position = startPosition;
        targetPosition = startPosition;
        
        // Initialize movement points
        currentMovementPoints = maxMovementPoints;
        
        // Setup selection indicator
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (gridManager == null) return;
        
        if (isMoving)
        {
            MoveToTarget();
        }
    }
    
    public bool CanMove()
    {
        return !isMoving && currentMovementPoints > 0 && isSelected;
    }
    
    public bool TryMove(Vector2Int direction)
    {
        if (!CanMove()) return false;
        
        int newX = currentX + direction.x;
        int newY = currentY + direction.y;
        
        if (IsValidPosition(newX, newY))
        {
            StartMovement(newX, newY);
            UseMovementPoint();
            return true;
        }
        
        return false;
    }
    
    public bool TryMoveToPosition(int newX, int newY)
    {
        if (!CanMove()) return false;
        
        if (IsValidPosition(newX, newY))
        {
            StartMovement(newX, newY);
            UseMovementPoint();
            return true;
        }
        
        return false;
    }
    
    private void UseMovementPoint()
    {
        currentMovementPoints--;
        
        if (currentMovementPoints <= 0)
        {
            OnMovementPointsExhausted?.Invoke(this);
        }
    }
    
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridManager.columns && y >= 0 && y < gridManager.rows;
    }
    
    private void StartMovement(int newX, int newY)
    {
        currentX = newX;
        currentY = newY;
        targetPosition = GetWorldPosition(currentX, currentY);
        isMoving = true;
    }
    
    private void MoveToTarget()
    {
        Vector2 currentPosition = transform.position;
        
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
        transform.position = newPosition;
        
        if (Vector2.Distance(newPosition, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;
            OnMovementComplete?.Invoke(this);
        }
    }
    
    private Vector2 GetWorldPosition(int gridX, int gridY)
    {
        float centerX = (gridManager.columns - 1) / 2.0f;
        float centerY = (gridManager.rows - 1) / 2.0f;
        
        float posX = gridManager.originPosition.x + (gridX - centerX) * (gridManager.cellSize + gridManager.cellSpacing);
        float posY = gridManager.originPosition.y + (gridY - centerY) * (gridManager.cellSize + gridManager.cellSpacing);
        
        return new Vector2(posX, posY);
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selectionIndicator != null)
        {
            Debug.Log($"🔄 SetSelected({selected}) called on {characterName}. Indicator: {selectionIndicator.name}");
            
            // Fix indicator position if it's at world origin
            if (selectionIndicator.transform.position == Vector3.zero)
            {
                Debug.LogWarning($"⚠️ Indicator at world origin! Moving to character position.");
                FixIndicatorPosition();
            }
            
            selectionIndicator.SetActive(selected);
            Debug.Log($"✅ Indicator SetActive({selected}) completed. Active state: {selectionIndicator.activeInHierarchy}");
        }
        else
        {
            Debug.LogError($"❌ SetSelected({selected}) called on {characterName} but selectionIndicator is NULL!");
        }
    }
    
    private void FixIndicatorPosition()
    {
        if (selectionIndicator != null)
        {
            // Position indicator below character
            Vector3 indicatorPos = transform.position;
            indicatorPos.y -= 0.6f; // Place below character
            selectionIndicator.transform.position = indicatorPos;
            
            // If indicator is not a child, make it one for proper positioning
            if (selectionIndicator.transform.parent != transform)
            {
                selectionIndicator.transform.SetParent(transform);
                selectionIndicator.transform.localPosition = new Vector3(0, -0.6f, 0);
            }
            
            Debug.Log($"🔧 Fixed indicator position: {selectionIndicator.transform.position}");
        }
    }
    
    public void StartTurn()
    {
        currentMovementPoints = maxMovementPoints;
        SetSelected(true);
    }
    
    public void EndTurn()
    {
        SetSelected(false);
        currentMovementPoints = 0;
    }
    
    // Getters
    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(currentX, currentY);
    }
    
    public bool IsCurrentlyMoving()
    {
        return isMoving;
    }
    
    public int GetCurrentMovementPoints()
    {
        return currentMovementPoints;
    }
    
    public int GetMaxMovementPoints()
    {
        return maxMovementPoints;
    }
    
    public bool IsSelected()
    {
        return isSelected;
    }
    
    public bool HasMovementPointsLeft()
    {
        return currentMovementPoints > 0;
    }
} 