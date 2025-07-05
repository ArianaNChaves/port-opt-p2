using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TurnBasedUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI turnInfoText;
    public TextMeshProUGUI movementPointsText;
    public TextMeshProUGUI instructionsText;
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
    
    private TurnManager turnManager;
    private List<CharacterStatusDisplay> characterStatusDisplays = new List<CharacterStatusDisplay>();
    
    [System.Serializable]
    public class CharacterStatusDisplay
    {
        public Character character;
        public GameObject statusObject;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI movementText;
        public Image backgroundImage;
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
            // Create a simple default UI element
            statusObj = new GameObject($"Character_{index}_Status");
            statusObj.transform.SetParent(characterListParent);
            
            // Add background image
            var bgImage = statusObj.AddComponent<Image>();
            bgImage.color = unselectedCharacterColor;
            
            // Add TextMeshPro for character name and movement points
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(statusObj.transform);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            text.fontSize = 14;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            
            // Set RectTransform
            var rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        
        // Setup the display
        var display = new CharacterStatusDisplay
        {
            character = character,
            statusObject = statusObj,
            nameText = statusObj.GetComponentInChildren<TextMeshProUGUI>(),
            backgroundImage = statusObj.GetComponent<Image>()
        };
        
        // Look for additional TextMeshPro components if using a prefab
        var textComponents = statusObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (textComponents.Length > 1)
        {
            display.movementText = textComponents[1];
        }
        
        characterStatusDisplays.Add(display);
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
            turnInfoText.text = phaseText + characterText;
        }
        
        if (movementPointsText != null && currentCharacter != null)
        {
            movementPointsText.text = $"Movements: {currentCharacter.GetCurrentMovementPoints()}/{currentCharacter.GetMaxMovementPoints()}";
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
            
            // Update name text
            if (display.nameText != null)
            {
                string text = display.character.characterName;
                if (display.movementText == null)
                {
                    // Include movement points in main text if no separate movement text
                    text += $"\n{display.character.GetCurrentMovementPoints()}/{display.character.GetMaxMovementPoints()}";
                }
                display.nameText.text = text;
            }
            
            // Update movement text if separate
            if (display.movementText != null)
            {
                display.movementText.text = $"{display.character.GetCurrentMovementPoints()}/{display.character.GetMaxMovementPoints()}";
            }
            
            // Update background color based on status
            if (display.backgroundImage != null)
            {
                Color targetColor;
                
                if (display.character == currentCharacter)
                {
                    targetColor = selectedCharacterColor;
                }
                else if (!display.character.HasMovementPointsLeft())
                {
                    targetColor = noMovementsColor;
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
    
    private void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnStart -= OnCharacterTurnStart;
            turnManager.OnTurnEnd -= OnCharacterTurnEnd;
            turnManager.OnPhaseChange -= OnPhaseChange;
        }
    }
} 