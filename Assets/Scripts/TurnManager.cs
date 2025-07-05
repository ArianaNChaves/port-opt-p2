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
    public bool autoEndTurnWhenNoActions = true;
    public float turnTransitionDelay = 0.5f;
    
    [Header("Action System")]
    public ActionManager actionManager;
    
    private int currentPlayerCharacterIndex = 0;
    private int currentEnemyCharacterIndex = 0;
    private bool isPlayerTurn = true;
    private Character currentCharacter;
    private bool isInActionMode = false; // Toggle between movement and action mode
    private List<Character> availableTargets = new List<Character>();
    private string selectedActionName = "";
    
    // Input state
    private bool inputEnabled = true;
    
    public System.Action<Character> OnTurnStart;
    public System.Action<Character> OnTurnEnd;
    public System.Action<bool> OnPhaseChange; // true for player phase, false for enemy phase
    public System.Action<Character, List<BaseAction>> OnAvailableActionsChanged;
    public System.Action<bool> OnActionModeChanged; // true when entering action mode
    
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
        // Get ActionManager reference
        if (actionManager == null)
        {
            actionManager = ActionManager.Instance;
        }
        
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
            character.OnActionPointsExhausted += OnCharacterActionExhausted;
            character.OnActionPerformed += OnCharacterActionPerformed;
        }
        
        foreach (var character in enemyCharacters)
        {
            character.OnMovementPointsExhausted += OnCharacterMovementExhausted;
            character.OnActionPointsExhausted += OnCharacterActionExhausted;
            character.OnActionPerformed += OnCharacterActionPerformed;
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
        
        // Toggle between movement and action mode (Q key)
        if (isPlayerTurn && Input.GetKeyDown(KeyCode.Q))
        {
            ToggleActionMode();
            return;
        }
        
        // Handle input based on current mode
        if (isInActionMode)
        {
            HandleActionInput();
        }
        else
        {
            HandleMovementInput();
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
    
    private void HandleMovementInput()
    {
        if (!isPlayerTurn || !currentCharacter.isPlayerControlled) return;
        
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
    
    private void HandleActionInput()
    {
        if (!isPlayerTurn || !currentCharacter.isPlayerControlled) return;
        
        // Get available actions for current character
        var availableActions = actionManager.GetAvailableActions(currentCharacter);
        if (availableActions.Count == 0)
        {
            Debug.Log("No actions available for this character!");
            ExitActionMode();
            return;
        }
        
        // Select action with number keys
        for (int i = 0; i < availableActions.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedActionName = availableActions[i].actionName;
                SelectAction(availableActions[i]);
                return;
            }
        }
        
        // If action is selected, handle target selection
        if (!string.IsNullOrEmpty(selectedActionName))
        {
            HandleTargetSelection();
        }
        
        // ESC to exit action mode
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitActionMode();
        }
    }
    
    private void HandleTargetSelection()
    {
        // Get valid targets for the selected action
        availableTargets = actionManager.GetValidTargetsForAction(currentCharacter, selectedActionName);
        
        if (availableTargets.Count == 0)
        {
            Debug.Log("No valid targets for this action!");
            ExitActionMode();
            return;
        }
        
        // Select target with number keys
        for (int i = 0; i < availableTargets.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                Character target = availableTargets[i];
                PerformAction(selectedActionName, target);
                ExitActionMode();
                return;
            }
        }
        
        // If only one target or action doesn't require target, auto-select
        if (availableTargets.Count == 1 || !actionManager.GetActionByName(selectedActionName).requiresTarget)
        {
            Character target = availableTargets.Count > 0 ? availableTargets[0] : null;
            PerformAction(selectedActionName, target);
            ExitActionMode();
        }
    }
    
    private void SelectAction(BaseAction action)
    {
        selectedActionName = action.actionName;
        Debug.Log($"Selected action: {action.actionName}");
        
        // If action doesn't require target, perform immediately
        if (!action.requiresTarget)
        {
            PerformAction(selectedActionName, null);
            ExitActionMode();
        }
    }
    
    private void PerformAction(string actionName, Character target)
    {
        bool success = currentCharacter.TryPerformAction(actionName, target);
        if (success)
        {
            Debug.Log($"Action {actionName} performed successfully!");
        }
        else
        {
            Debug.Log($"Failed to perform action {actionName}");
        }
    }
    
    private void ToggleActionMode()
    {
        isInActionMode = !isInActionMode;
        
        if (isInActionMode)
        {
            EnterActionMode();
        }
        else
        {
            ExitActionMode();
        }
    }
    
    private void EnterActionMode()
    {
        isInActionMode = true;
        selectedActionName = "";
        availableTargets.Clear();
        
        Debug.Log("Entered Action Mode - Select an action with number keys");
        
        // Get available actions and notify UI
        var availableActions = actionManager.GetAvailableActions(currentCharacter);
        OnAvailableActionsChanged?.Invoke(currentCharacter, availableActions);
        OnActionModeChanged?.Invoke(true);
    }
    
    private void ExitActionMode()
    {
        isInActionMode = false;
        selectedActionName = "";
        availableTargets.Clear();
        
        Debug.Log("Exited Action Mode");
        OnActionModeChanged?.Invoke(false);
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
        
        // Exit action mode when switching characters
        ExitActionMode();
        
        OnTurnStart?.Invoke(currentCharacter);
    }
    
    private void OnCharacterMovementExhausted(Character character)
    {
        if (autoEndTurnWhenNoMovements && character == currentCharacter)
        {
            // Only end turn if character also has no action points
            if (autoEndTurnWhenNoActions && !character.HasActionPointsLeft())
            {
                EndCurrentCharacterTurn();
            }
        }
    }
    
    private void OnCharacterActionExhausted(Character character)
    {
        if (autoEndTurnWhenNoActions && character == currentCharacter)
        {
            // Only end turn if character also has no movement points
            if (autoEndTurnWhenNoMovements && !character.HasMovementPointsLeft())
            {
                EndCurrentCharacterTurn();
            }
        }
    }
    
    private void OnCharacterActionPerformed(BaseAction action, Character target)
    {
        Debug.Log($"Action {action.actionName} was performed by {currentCharacter.characterName}");
        
        // Check if character should auto-end turn
        if (ShouldAutoEndTurn(currentCharacter))
        {
            EndCurrentCharacterTurn();
        }
    }
    
    private bool ShouldAutoEndTurn(Character character)
    {
        if (autoEndTurnWhenNoMovements && autoEndTurnWhenNoActions)
        {
            return !character.HasMovementPointsLeft() && !character.HasActionPointsLeft();
        }
        else if (autoEndTurnWhenNoMovements)
        {
            return !character.HasMovementPointsLeft();
        }
        else if (autoEndTurnWhenNoActions)
        {
            return !character.HasActionPointsLeft();
        }
        
        return false;
    }
    
    private void EndCurrentCharacterTurn()
    {
        if (currentCharacter != null)
        {
            currentCharacter.EndTurn();
            OnTurnEnd?.Invoke(currentCharacter);
        }
        
        // Exit action mode
        ExitActionMode();
        
        if (isPlayerTurn)
        {
            // Check if there are more player characters with actions/movements left
            if (HasPlayerCharactersWithPointsLeft())
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
    
    private bool HasPlayerCharactersWithPointsLeft()
    {
        return playerCharacters.Any(c => c.HasAnyPointsLeft() && c.IsAlive());
    }
    
    private bool HasEnemyCharactersWithPointsLeft()
    {
        return enemyCharacters.Any(c => c.HasAnyPointsLeft() && c.IsAlive());
    }
    
    private void StartPlayerTurn()
    {
        isPlayerTurn = true;
        inputEnabled = true;
        
        // Reset all player characters for their turn
        foreach (var character in playerCharacters)
        {
            if (character.IsAlive())
            {
                character.StartTurn();
                character.SetSelected(false); // Deselect all first
            }
        }
        
        // Select the first alive character
        if (playerCharacters.Count > 0)
        {
            currentPlayerCharacterIndex = 0;
            
            // Find first alive character
            for (int i = 0; i < playerCharacters.Count; i++)
            {
                if (playerCharacters[i].IsAlive())
                {
                    currentPlayerCharacterIndex = i;
                    break;
                }
            }
            
            currentCharacter = playerCharacters[currentPlayerCharacterIndex];
            currentCharacter.SetSelected(true);
            
            OnTurnStart?.Invoke(currentCharacter);
        }
        
        ExitActionMode();
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
            if (character.IsAlive())
            {
                character.StartTurn();
            }
        }
        
        ExitActionMode();
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
    
    public bool IsInActionMode()
    {
        return isInActionMode;
    }
    
    public string GetSelectedActionName()
    {
        return selectedActionName;
    }
    
    public List<Character> GetAvailableTargets()
    {
        return new List<Character>(availableTargets);
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