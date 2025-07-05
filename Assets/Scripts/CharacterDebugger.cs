using UnityEngine;

[System.Serializable]
public class CharacterDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public KeyCode debugKey = KeyCode.F1;
    
    private Character character;
    private TurnManager turnManager;
    
    private void Start()
    {
        character = GetComponent<Character>();
        turnManager = FindObjectOfType<TurnManager>();
        
        if (character == null)
        {
            Debug.LogError($"CharacterDebugger: No Character component found on {gameObject.name}!");
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            DebugCharacterStatus();
        }
    }
    
    private void DebugCharacterStatus()
    {
        if (!enableDebugLogs || character == null) return;
        
        Debug.Log("=== CHARACTER DEBUG INFO ===");
        Debug.Log($"Character Name: {character.characterName}");
        Debug.Log($"GameObject Name: {gameObject.name}");
        Debug.Log($"Is Selected: {character.IsSelected()}");
        Debug.Log($"Is Player Controlled: {character.isPlayerControlled}");
        Debug.Log($"Movement Points: {character.GetCurrentMovementPoints()}/{character.GetMaxMovementPoints()}");
        
        // Check selection indicator
        if (character.selectionIndicator == null)
        {
            Debug.LogError($"⚠️ SELECTION INDICATOR IS NULL on {gameObject.name}!");
            Debug.LogError("👉 Solution: Assign a GameObject to the 'Selection Indicator' field in Character component");
        }
        else
        {
            // Check if it's a prefab asset instead of scene instance
            bool isPrefabAsset = !character.selectionIndicator.scene.IsValid();
            
            Debug.Log($"Selection Indicator Object: {character.selectionIndicator.name}");
            Debug.Log($"Is Prefab Asset (NOT scene instance): {isPrefabAsset}");
            
            if (isPrefabAsset)
            {
                Debug.LogError($"🚨 CRITICAL: Selection indicator is a PREFAB ASSET, not a scene instance!");
                Debug.LogError($"👉 Solution: Right-click CharacterDebugger → 'Create Indicator From Current Prefab'");
                Debug.LogError($"👉 Or manually create scene instances as children of each character");
            }
            
            Debug.Log($"Selection Indicator Active: {character.selectionIndicator.activeInHierarchy}");
            Debug.Log($"Selection Indicator Position: {character.selectionIndicator.transform.position}");
            
            // Check if indicator is visible
            var renderer = character.selectionIndicator.GetComponent<Renderer>();
            var image = character.selectionIndicator.GetComponent<UnityEngine.UI.Image>();
            
            if (renderer != null)
            {
                Debug.Log($"Indicator Renderer Enabled: {renderer.enabled}");
                Debug.Log($"Indicator Visible: {renderer.isVisible}");
            }
            else if (image != null)
            {
                Debug.Log($"Indicator UI Image Color: {image.color}");
                Debug.Log($"Indicator UI Image Enabled: {image.enabled}");
            }
            else
            {
                Debug.LogWarning("⚠️ Selection indicator has no Renderer or UI Image component!");
            }
        }
        
        // Check if this is the current character
        if (turnManager != null)
        {
            var currentChar = turnManager.GetCurrentCharacter();
            bool isCurrentCharacter = currentChar == character;
            Debug.Log($"Is Current Character: {isCurrentCharacter}");
            
            if (currentChar != null)
            {
                Debug.Log($"Current Character: {currentChar.characterName}");
            }
        }
        
        Debug.Log("================================");
    }
    
    [ContextMenu("Force Show Indicator")]
    public void ForceShowIndicator()
    {
        if (character != null && character.selectionIndicator != null)
        {
            character.selectionIndicator.SetActive(true);
            Debug.Log($"Forced selection indicator ON for {character.characterName}");
        }
    }
    
    [ContextMenu("Force Hide Indicator")]
    public void ForceHideIndicator()
    {
        if (character != null && character.selectionIndicator != null)
        {
            character.selectionIndicator.SetActive(false);
            Debug.Log($"Forced selection indicator OFF for {character.characterName}");
        }
    }
    
    [ContextMenu("Create Simple Indicator")]
    public void CreateSimpleIndicator()
    {
        if (character == null) return;
        
        // Create a simple circle indicator
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = $"{gameObject.name}_SelectionIndicator";
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = new Vector3(0, -0.6f, 0); // Below character
        indicator.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f); // Flat circle
        
        // Make it green and glowing
        var renderer = indicator.GetComponent<Renderer>();
        var material = new Material(Shader.Find("Sprites/Default"));
        material.color = Color.green;
        renderer.material = material;
        
        // Remove collider
        var collider = indicator.GetComponent<Collider>();
        if (collider != null) DestroyImmediate(collider);
        
        // Assign to character
        character.selectionIndicator = indicator;
        indicator.SetActive(false); // Start hidden
        
        Debug.Log($"✅ Created simple selection indicator for {character.characterName}");
    }
    
    [ContextMenu("Create Indicator From Current Prefab")]
    public void CreateIndicatorFromPrefab()
    {
        if (character == null || character.selectionIndicator == null) 
        {
            Debug.LogError("No character or no prefab assigned to selection indicator!");
            return;
        }
        
        // Check if current indicator is a prefab asset (not scene instance)
        bool isPrefabAsset = !character.selectionIndicator.scene.IsValid();
        
        if (isPrefabAsset)
        {
            Debug.Log($"🔄 Converting prefab asset to scene instance for {character.characterName}");
            
            // Instantiate the prefab as a child of this character
            GameObject indicatorInstance = Instantiate(character.selectionIndicator, transform);
            indicatorInstance.name = $"{gameObject.name}_SelectionIndicator";
            indicatorInstance.transform.localPosition = new Vector3(0, -0.6f, 0); // Below character
            
            // Assign the new scene instance
            character.selectionIndicator = indicatorInstance;
            indicatorInstance.SetActive(false); // Start hidden
            
            Debug.Log($"✅ Created scene instance indicator from prefab for {character.characterName}");
        }
        else
        {
            Debug.Log($"Selection indicator is already a scene instance for {character.characterName}");
        }
    }
} 