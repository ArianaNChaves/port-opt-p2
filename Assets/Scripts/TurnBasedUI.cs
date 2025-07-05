using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TurnBasedUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI turnInfoText;
    public TextMeshProUGUI movementPointsText;
    public TextMeshProUGUI actionPointsText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI availableActionsText;
    public TextMeshProUGUI actionModeText;
    public Transform characterListParent; // Parent for character status UI elements
    
    [Header("Character Status Prefabs")]
    public GameObject defaultCharacterStatusPrefab; // Default UI prefab for character status
    public CharacterUIPrefabMapping[] characterUIPrefabs; // Specific UI prefabs for different character types
    
    [System.Serializable]
    public class CharacterUIPrefabMapping
    {
        public string characterName; // Name to match (e.g., "Fighter", "Mage")
        public GameObject uiPrefab;  // UI prefab to use for this character type
    }
    
    [Header("Colors")]
    public Color selectedCharacterColor = Color.green;
    public Color unselectedCharacterColor = Color.white;
    public Color noMovementsColor = Color.red;
    public Color noActionsColor = Color.yellow;
    public Color deadCharacterColor = Color.gray;
    public Color lowHealthColor = Color.red;
    public Color mediumHealthColor = Color.yellow;
    public Color highHealthColor = Color.green;
    
    private TurnManager turnManager;
    private List<CharacterStatusDisplay> characterStatusDisplays = new List<CharacterStatusDisplay>();
    
    [System.Serializable]
    public class CharacterStatusDisplay
    {
        public Character character;
        public GameObject statusObject;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI movementText;
        public TextMeshProUGUI actionText;
        public TextMeshProUGUI healthText;
        public Image backgroundImage;
        public Image healthBar;
    }
    
    private void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        
        if (turnManager != null)
        {
            // Subscribe to turn manager events
            turnManager.OnTurnStart += OnCharacterTurnStart;
            turnManager.OnTurnEnd += OnCharacterTurnEnd;
            turnManager.OnPhaseChange += OnPhaseChange;
            turnManager.OnAvailableActionsChanged += OnAvailableActionsChanged;
            turnManager.OnActionModeChanged += OnActionModeChanged;
            
            // Create character status displays
            CreateCharacterStatusDisplays();
        }
        
        UpdateInstructionsText();
    }
    
    private void Update()
    {
        UpdateUI();
    }
    
    private void CreateCharacterStatusDisplays()
    {
        if (characterListParent == null) return;
        
        // Clear existing displays
        foreach (var display in characterStatusDisplays)
        {
            if (display.statusObject != null)
                DestroyImmediate(display.statusObject);
        }
        characterStatusDisplays.Clear();
        
        // Create displays for player characters
        var playerCharacters = turnManager.GetPlayerCharacters();
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            CreateCharacterStatusDisplay(playerCharacters[i], i);
        }
    }
    
    private void CreateCharacterStatusDisplay(Character character, int index)
    {
        GameObject statusObj;
        
        // Find the appropriate UI prefab for this character
        GameObject prefabToUse = GetUIPrefabForCharacter(character);
        
        if (prefabToUse != null)
        {
            statusObj = Instantiate(prefabToUse, characterListParent);
        }
        else
        {
            // Create a more detailed default UI element
            statusObj = new GameObject($"Character_{index}_Status");
            statusObj.transform.SetParent(characterListParent);
            
            // Add RectTransform component
            var rectTransform = statusObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 80);
            
            // Add background image
            var bgImage = statusObj.AddComponent<Image>();
            bgImage.color = unselectedCharacterColor;
            
            // Create text elements
            CreateTextElement(statusObj, "NameText", new Vector2(0, 30), 14, TextAlignmentOptions.Center);
            CreateTextElement(statusObj, "MovementText", new Vector2(-50, 10), 12, TextAlignmentOptions.Center);
            CreateTextElement(statusObj, "ActionText", new Vector2(50, 10), 12, TextAlignmentOptions.Center);
            CreateTextElement(statusObj, "HealthText", new Vector2(0, -10), 10, TextAlignmentOptions.Center);
            
            // Create health bar
            CreateHealthBar(statusObj);
        }
        
        // Setup the display
        var display = new CharacterStatusDisplay
        {
            character = character,
            statusObject = statusObj,
            backgroundImage = statusObj.GetComponent<Image>()
        };
        
        // Get text components
        var textComponents = statusObj.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in textComponents)
        {
            switch (text.name)
            {
                case "NameText":
                    display.nameText = text;
                    break;
                case "MovementText":
                    display.movementText = text;
                    break;
                case "ActionText":
                    display.actionText = text;
                    break;
                case "HealthText":
                    display.healthText = text;
                    break;
            }
        }
        
        // Get health bar
        var healthBarTransform = statusObj.transform.Find("HealthBar");
        if (healthBarTransform != null)
        {
            display.healthBar = healthBarTransform.GetComponent<Image>();
        }
        
        characterStatusDisplays.Add(display);
    }
    
    private void CreateTextElement(GameObject parent, string name, Vector2 position, int fontSize, TextAlignmentOptions alignment)
    {
        var textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform);
        
        var rectTransform = textObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(90, 20);
        
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        text.fontSize = fontSize;
        text.color = Color.black;
        text.alignment = alignment;
    }
    
    private void CreateHealthBar(GameObject parent)
    {
        var healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(parent.transform);
        
        var rectTransform = healthBarObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        rectTransform.anchorMax = new Vector2(0.9f, 0.2f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        var image = healthBarObj.AddComponent<Image>();
        image.color = highHealthColor;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
    }
    
    private GameObject GetUIPrefabForCharacter(Character character)
    {
        // First, try to find a specific UI prefab for this character type
        if (characterUIPrefabs != null)
        {
            foreach (var mapping in characterUIPrefabs)
            {
                if (mapping.characterName.Equals(character.characterName, System.StringComparison.OrdinalIgnoreCase) ||
                    mapping.characterName.Equals(character.gameObject.name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.uiPrefab;
                }
            }
        }
        
        // If no specific prefab found, use the default
        return defaultCharacterStatusPrefab;
    }
    
    private void UpdateUI()
    {
        if (turnManager == null) return;
        
        var currentCharacter = turnManager.GetCurrentCharacter();
        
        // Update main UI texts
        if (turnInfoText != null)
        {
            string phaseText = turnManager.IsPlayerTurn() ? "Player Turn" : "Enemy Turn";
            string characterText = currentCharacter != null ? $" - {currentCharacter.characterName}" : "";
            string modeText = turnManager.IsInActionMode() ? " (Action Mode)" : " (Movement Mode)";
            turnInfoText.text = phaseText + characterText + modeText;
        }
        
        if (currentCharacter != null)
        {
            // Update movement points
            if (movementPointsText != null)
            {
                movementPointsText.text = $"Movement: {currentCharacter.GetCurrentMovementPoints()}/{currentCharacter.GetMaxMovementPoints()}";
            }
            
            // Update action points
            if (actionPointsText != null)
            {
                var stats = currentCharacter.GetCharacterStats();
                actionPointsText.text = $"Actions: {stats.currentActionPoints}/{stats.maxActionPoints}";
            }
            
            // Update health
            if (healthText != null)
            {
                var stats = currentCharacter.GetCharacterStats();
                healthText.text = $"Health: {stats.currentHealth}/{stats.maxHealth}";
                
                // Color health text based on health percentage
                float healthPercentage = stats.GetHealthPercentage();
                if (healthPercentage > 0.6f)
                    healthText.color = highHealthColor;
                else if (healthPercentage > 0.3f)
                    healthText.color = mediumHealthColor;
                else
                    healthText.color = lowHealthColor;
            }
        }
        
        // Update action mode text
        if (actionModeText != null)
        {
            if (turnManager.IsInActionMode())
            {
                actionModeText.text = "ACTION MODE - Select action with number keys";
                actionModeText.color = Color.cyan;
            }
            else
            {
                actionModeText.text = "MOVEMENT MODE - Press Q for actions";
                actionModeText.color = Color.white;
            }
        }
        
        // Update character status displays
        UpdateCharacterStatusDisplays();
    }
    
    private void UpdateCharacterStatusDisplays()
    {
        var currentCharacter = turnManager.GetCurrentCharacter();
        
        foreach (var display in characterStatusDisplays)
        {
            if (display.character == null || display.statusObject == null) continue;
            
            var character = display.character;
            var stats = character.GetCharacterStats();
            
            // Update name text
            if (display.nameText != null)
            {
                display.nameText.text = $"{character.characterName} ({character.GetCharacterType()})";
            }
            
            // Update movement text
            if (display.movementText != null)
            {
                display.movementText.text = $"Move: {character.GetCurrentMovementPoints()}/{character.GetMaxMovementPoints()}";
            }
            
            // Update action text
            if (display.actionText != null)
            {
                display.actionText.text = $"Act: {stats.currentActionPoints}/{stats.maxActionPoints}";
            }
            
            // Update health text
            if (display.healthText != null)
            {
                display.healthText.text = $"HP: {stats.currentHealth}/{stats.maxHealth}";
            }
            
            // Update health bar
            if (display.healthBar != null)
            {
                float healthPercentage = stats.GetHealthPercentage();
                display.healthBar.fillAmount = healthPercentage;
                
                // Color health bar based on health percentage
                if (healthPercentage > 0.6f)
                    display.healthBar.color = highHealthColor;
                else if (healthPercentage > 0.3f)
                    display.healthBar.color = mediumHealthColor;
                else
                    display.healthBar.color = lowHealthColor;
            }
            
            // Update background color based on status
            if (display.backgroundImage != null)
            {
                Color targetColor;
                
                if (!character.IsAlive())
                {
                    targetColor = deadCharacterColor;
                }
                else if (character == currentCharacter)
                {
                    targetColor = selectedCharacterColor;
                }
                else if (!character.HasMovementPointsLeft() && !character.HasActionPointsLeft())
                {
                    targetColor = noMovementsColor;
                }
                else if (!character.HasActionPointsLeft())
                {
                    targetColor = noActionsColor;
                }
                else
                {
                    targetColor = unselectedCharacterColor;
                }
                
                display.backgroundImage.color = targetColor;
            }
        }
    }
    
    private void UpdateInstructionsText()
    {
        if (instructionsText != null)
        {
            instructionsText.text = "Controls:\n" +
                                  "WASD - Move selected character\n" +
                                  "Q - Toggle Action Mode\n" +
                                  "1-9 - Select action/target (in Action Mode)\n" +
                                  "ESC - Exit Action Mode\n" +
                                  "Tab - Switch between characters\n" +
                                  "Space - End current character's turn\n" +
                                  "Enter - End current phase";
        }
    }
    
    // Event handlers
    private void OnCharacterTurnStart(Character character)
    {
        // Character turn started - UI will update automatically
    }
    
    private void OnCharacterTurnEnd(Character character)
    {
        // Character turn ended - UI will update automatically
    }
    
    private void OnPhaseChange(bool isPlayerPhase)
    {
        // Phase changed - UI will update automatically
    }
    
    private void OnAvailableActionsChanged(Character character, List<BaseAction> availableActions)
    {
        if (availableActionsText != null)
        {
            if (availableActions.Count == 0)
            {
                availableActionsText.text = "No actions available";
            }
            else
            {
                string actionText = "Available Actions:\n";
                for (int i = 0; i < availableActions.Count; i++)
                {
                    var action = availableActions[i];
                    actionText += $"{i + 1}. {action.actionName} (Cost: {action.actionPointCost})\n";
                }
                availableActionsText.text = actionText;
            }
        }
    }
    
    private void OnActionModeChanged(bool isInActionMode)
    {
        if (availableActionsText != null)
        {
            availableActionsText.gameObject.SetActive(isInActionMode);
        }
    }
    
    private void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnStart -= OnCharacterTurnStart;
            turnManager.OnTurnEnd -= OnCharacterTurnEnd;
            turnManager.OnPhaseChange -= OnPhaseChange;
            turnManager.OnAvailableActionsChanged -= OnAvailableActionsChanged;
            turnManager.OnActionModeChanged -= OnActionModeChanged;
        }
    }
} 