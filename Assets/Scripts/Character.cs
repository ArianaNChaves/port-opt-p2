using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Character Info")]
    public string characterName = "Character";
    public bool isPlayerControlled = true;
    public CharacterType characterType = CharacterType.Fighter;
    
    [Header("Character Stats")]
    public CharacterStats characterStats;
    
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
    public GameObject healthBar; // Optional health bar UI
    
    private int currentX;
    private int currentY;
    private int currentMovementPoints;
    
    private bool isMoving = false;
    private Vector2 targetPosition;
    private bool isSelected = false;
    
    // Events for turn management
    public System.Action<Character> OnMovementComplete;
    public System.Action<Character> OnMovementPointsExhausted;
    public System.Action<Character> OnActionPointsExhausted;
    public System.Action<Character> OnCharacterDefeated;
    public System.Action<BaseAction, Character> OnActionPerformed;
    
    private void Start()
    {
        InitializeCharacter();
    }
    
    private void InitializeCharacter()
    {
        // Initialize stats based on character type
        if (characterStats == null)
        {
            var config = new CharacterTypeConfig(characterType);
            characterStats = config.baseStats;
        }
        
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
        
        // Initialize health bar if present
        UpdateHealthBar();
    }
    
    private void Update()
    {
        if (gridManager == null) return;
        
        if (isMoving)
        {
            MoveToTarget();
        }
        
        UpdateHealthBar();
    }
    
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            // Update health bar visibility and scale based on current health
            healthBar.SetActive(characterStats.currentHealth < characterStats.maxHealth);
            
            // Scale the health bar based on health percentage
            float healthPercentage = characterStats.GetHealthPercentage();
            Vector3 scale = healthBar.transform.localScale;
            scale.x = healthPercentage;
            healthBar.transform.localScale = scale;
        }
    }
    
    public bool CanMove()
    {
        return !isMoving && currentMovementPoints > 0 && isSelected && characterStats.IsAlive();
    }
    
    public bool CanPerformActions()
    {
        return !isMoving && characterStats.HasActionPoints() && isSelected && characterStats.IsAlive();
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
    
    public bool TryPerformAction(string actionName, Character target = null)
    {
        if (!CanPerformActions()) return false;
        
        ActionManager actionManager = ActionManager.Instance;
        if (actionManager == null) return false;
        
        return actionManager.PerformAction(this, actionName, target);
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
        // Safety check: ensure character stats are properly initialized
        if (characterStats == null)
        {
            Debug.LogWarning($"Character {characterName} has null stats! Initializing...");
            var config = new CharacterTypeConfig(characterType);
            characterStats = config.baseStats;
        }
        
        // Additional safety check: ensure stats have valid values
        if (characterStats.maxActionPoints <= 0)
        {
            Debug.LogWarning($"Character {characterName} has invalid maxActionPoints: {characterStats.maxActionPoints}! Reinitializing...");
            var config = new CharacterTypeConfig(characterType);
            characterStats = config.baseStats;
        }
        
        currentMovementPoints = maxMovementPoints;
        characterStats.ResetForNewTurn();
        SetSelected(true);
    }
    
    public void EndTurn()
    {
        SetSelected(false);
        currentMovementPoints = 0;
        // Don't reset action points here - they persist until start of next turn
    }
    
    public void HandleCharacterDefeated()
    {
        Debug.Log($"{characterName} has been defeated!");
        SetSelected(false);
        
        // Disable character visually but keep for potential revival
        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
        
        // Make character semi-transparent
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0.5f;
            spriteRenderer.color = color;
        }
        
        OnCharacterDefeated?.Invoke(this);
    }
    
    public void HandleActionPerformed(BaseAction action, Character target)
    {
        Debug.Log($"{characterName} performed {action.actionName}" + (target != null ? $" on {target.characterName}" : ""));
        
        if (!characterStats.HasActionPoints())
        {
            OnActionPointsExhausted?.Invoke(this);
        }
        
        OnActionPerformed?.Invoke(action, target);
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
    
    public bool HasActionPointsLeft()
    {
        return characterStats.HasActionPoints();
    }
    
    public bool HasAnyPointsLeft()
    {
        return HasMovementPointsLeft() || HasActionPointsLeft();
    }
    
    public CharacterType GetCharacterType()
    {
        return characterType;
    }
    
    public CharacterStats GetCharacterStats()
    {
        return characterStats;
    }
    
    public bool IsAlive()
    {
        return characterStats.IsAlive();
    }
} 