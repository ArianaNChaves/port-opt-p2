using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    [Header("Characters")]
    public List<Character> playerCharacters = new List<Character>();
    public List<Character> enemyCharacters = new List<Character>();
    
    [Header("Turn Settings")]
    public bool autoEndTurnWhenNoMovements = true;
    public float turnTransitionDelay = 0.5f;
    

    
    private int currentPlayerCharacterIndex = 0;
    private int currentEnemyCharacterIndex = 0;
    private bool isPlayerTurn = true;
    private Character currentCharacter;
    
    // Input state
    private bool inputEnabled = true;
    
    public System.Action<Character> OnTurnStart;
    public System.Action<Character> OnTurnEnd;
    public System.Action<bool> OnPhaseChange; // true for player phase, false for enemy phase
    
    private void Start()
    {
        InitializeTurnSystem();
    }
    
    private void Update()
    {
        if (!inputEnabled) return;
        
        HandleInput();
    }
    
    private void InitializeTurnSystem()
    {
        // Find all characters if not assigned
        if (playerCharacters.Count == 0)
        {
            Character[] allCharacters = FindObjectsOfType<Character>();
            playerCharacters = allCharacters.Where(c => c.isPlayerControlled).ToList();
            enemyCharacters = allCharacters.Where(c => !c.isPlayerControlled).ToList();
        }
        
        // Subscribe to character events
        foreach (var character in playerCharacters)
        {
            character.OnMovementPointsExhausted += OnCharacterMovementExhausted;
        }
        
        foreach (var character in enemyCharacters)
        {
            character.OnMovementPointsExhausted += OnCharacterMovementExhausted;
        }
        
        // Start the first turn
        StartPlayerTurn();
    }
    
    private void HandleInput()
    {
        if (currentCharacter == null) return;
        
        // Character selection (Tab key to cycle through player characters)
        if (isPlayerTurn && Input.GetKeyDown(KeyCode.Tab))
        {
            CycleToNextPlayerCharacter();
            return;
        }
        
        // Movement input (only for player characters during player turn)
        if (isPlayerTurn && currentCharacter.isPlayerControlled)
        {
            Vector2Int moveDirection = Vector2Int.zero;
            
            if (Input.GetKeyDown(KeyCode.W))
                moveDirection = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S))
                moveDirection = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A))
                moveDirection = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D))
                moveDirection = Vector2Int.right;
            
            if (moveDirection != Vector2Int.zero)
            {
                currentCharacter.TryMove(moveDirection);
            }
        }
        
        // Manual turn end (Space key)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndCurrentCharacterTurn();
        }
        
        // Skip to next phase (Enter key)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (isPlayerTurn)
                StartEnemyTurn();
            else
                StartPlayerTurn();
        }
    }
    
    private void CycleToNextPlayerCharacter()
    {
        if (playerCharacters.Count <= 1) return;
        
        // End current character's selection
        if (currentCharacter != null)
        {
            currentCharacter.SetSelected(false);
        }
        
        // Move to next character
        currentPlayerCharacterIndex = (currentPlayerCharacterIndex + 1) % playerCharacters.Count;
        currentCharacter = playerCharacters[currentPlayerCharacterIndex];
        currentCharacter.SetSelected(true);
        
        OnTurnStart?.Invoke(currentCharacter);
    }
    
    private void OnCharacterMovementExhausted(Character character)
    {
        if (autoEndTurnWhenNoMovements)
        {
            // Check if this was the current character
            if (character == currentCharacter)
            {
                EndCurrentCharacterTurn();
            }
        }
    }
    
    private void EndCurrentCharacterTurn()
    {
        if (currentCharacter != null)
        {
            currentCharacter.EndTurn();
            OnTurnEnd?.Invoke(currentCharacter);
        }
        
        if (isPlayerTurn)
        {
            // Check if there are more player characters with movements left
            if (HasPlayerCharactersWithMovements())
            {
                CycleToNextPlayerCharacter();
            }
            else
            {
                // All player characters are done, switch to enemy turn
                Invoke(nameof(StartEnemyTurn), turnTransitionDelay);
            }
        }
        else
        {
            // Handle enemy turn logic here (when enemies are implemented)
            // For now, just go back to player turn
            Invoke(nameof(StartPlayerTurn), turnTransitionDelay);
        }
    }
    
    private bool HasPlayerCharactersWithMovements()
    {
        return playerCharacters.Any(c => c.HasMovementPointsLeft());
    }
    
    private bool HasEnemyCharactersWithMovements()
    {
        return enemyCharacters.Any(c => c.HasMovementPointsLeft());
    }
    
    private void StartPlayerTurn()
    {
        isPlayerTurn = true;
        inputEnabled = true;
        
        // Reset all player characters for their turn
        foreach (var character in playerCharacters)
        {
            character.StartTurn();
            character.SetSelected(false); // Deselect all first
        }
        
        // Select the first character
        if (playerCharacters.Count > 0)
        {
            currentPlayerCharacterIndex = 0;
            currentCharacter = playerCharacters[currentPlayerCharacterIndex];
            currentCharacter.SetSelected(true);
            
            OnTurnStart?.Invoke(currentCharacter);
        }
        
        OnPhaseChange?.Invoke(true);
    }
    
    private void StartEnemyTurn()
    {
        isPlayerTurn = false;
        inputEnabled = false; // Disable input during enemy turn
        
        // Deselect all player characters
        foreach (var character in playerCharacters)
        {
            character.SetSelected(false);
        }
        
        // Reset all enemy characters for their turn
        foreach (var character in enemyCharacters)
        {
            character.StartTurn();
        }
        
        OnPhaseChange?.Invoke(false);
        
        // For now, immediately go back to player turn since enemies aren't implemented
        // TODO: Implement enemy AI behavior here
        Invoke(nameof(StartPlayerTurn), turnTransitionDelay * 2);
    }
    

    
    // Public methods for external control
    public void ForceEndPlayerTurn()
    {
        if (isPlayerTurn)
        {
            StartEnemyTurn();
        }
    }
    
    public void ForceEndEnemyTurn()
    {
        if (!isPlayerTurn)
        {
            StartPlayerTurn();
        }
    }
    
    public Character GetCurrentCharacter()
    {
        return currentCharacter;
    }
    
    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }
    
    public List<Character> GetPlayerCharacters()
    {
        return new List<Character>(playerCharacters);
    }
    
    public List<Character> GetEnemyCharacters()
    {
        return new List<Character>(enemyCharacters);
    }
} 