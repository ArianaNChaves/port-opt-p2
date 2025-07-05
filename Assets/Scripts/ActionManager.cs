using UnityEngine;
using System.Collections.Generic;

public class ActionManager : MonoBehaviour
{
    [Header("Action Configurations")]
    public List<ActionTypeConfig> actionConfigs = new List<ActionTypeConfig>();
    
    [System.Serializable]
    public class ActionTypeConfig
    {
        public string actionName;
        public BaseAction actionInstance;
        public List<CharacterType> allowedCharacterTypes = new List<CharacterType>();
        
        public ActionTypeConfig(string name, BaseAction action)
        {
            actionName = name;
            actionInstance = action;
        }
    }
    
    private Dictionary<string, BaseAction> actionRegistry = new Dictionary<string, BaseAction>();
    private Dictionary<CharacterType, List<BaseAction>> characterTypeActions = new Dictionary<CharacterType, List<BaseAction>>();
    
    private void Awake()
    {
        InitializeActions();
    }
    
    private void InitializeActions()
    {
        // Create default actions if none are configured
        if (actionConfigs.Count == 0)
        {
            CreateDefaultActionConfigs();
        }
        else
        {
            // Create action instances for Inspector-configured actions that don't have instances
            foreach (var config in actionConfigs)
            {
                if (config.actionInstance == null && !string.IsNullOrEmpty(config.actionName))
                {
                    config.actionInstance = CreateActionByName(config.actionName);
                    Debug.Log($"Created action instance for: {config.actionName}");
                }
            }
        }
        
        // Register all actions
        foreach (var config in actionConfigs)
        {
            if (config.actionInstance != null)
            {
                Debug.Log($"Registering action: {config.actionName} for character types: {string.Join(", ", config.allowedCharacterTypes)}");
                actionRegistry[config.actionName] = config.actionInstance;
                
                // Add to character type mapping
                foreach (var characterType in config.allowedCharacterTypes)
                {
                    if (!characterTypeActions.ContainsKey(characterType))
                    {
                        characterTypeActions[characterType] = new List<BaseAction>();
                    }
                    characterTypeActions[characterType].Add(config.actionInstance);
                }
            }
            else
            {
                Debug.LogWarning($"Action config '{config.actionName}' has no action instance!");
            }
        }
        
        // Debug: Log what's registered
        foreach (var kvp in characterTypeActions)
        {
            Debug.Log($"Character type {kvp.Key} has {kvp.Value.Count} actions: {string.Join(", ", kvp.Value.ConvertAll(a => a.actionName))}");
        }
    }
    
    private void CreateDefaultActionConfigs()
    {
        // Attack Action - available to all character types
        var attackConfig = new ActionTypeConfig("Attack", new AttackAction());
        attackConfig.allowedCharacterTypes.Add(CharacterType.Fighter);
        attackConfig.allowedCharacterTypes.Add(CharacterType.Healer);
        attackConfig.allowedCharacterTypes.Add(CharacterType.Ranger);
        actionConfigs.Add(attackConfig);
        
        // Ranged Attack Action - only for Rangers
        var rangedAttackConfig = new ActionTypeConfig("RangedAttack", new RangedAttackAction());
        rangedAttackConfig.allowedCharacterTypes.Add(CharacterType.Ranger);
        actionConfigs.Add(rangedAttackConfig);
        
        // Heal Action - only for Healers
        var healConfig = new ActionTypeConfig("Heal", new HealAction());
        healConfig.allowedCharacterTypes.Add(CharacterType.Healer);
        actionConfigs.Add(healConfig);
    }
    
    private BaseAction CreateActionByName(string actionName)
    {
        switch (actionName.ToLower())
        {
            case "melee attack":
            case "attack":
                return new AttackAction();
                
            case "range attack":
            case "ranged attack":
                return new RangedAttackAction();
                
            case "heal ally":
            case "heal":
                return new HealAction();
                
            case "test action":
                return new TestAction();
                
            default:
                Debug.LogError($"Unknown action name: {actionName}");
                return null;
        }
    }
    
    public List<BaseAction> GetAvailableActions(Character character)
    {
        if (character == null) 
        {
            Debug.LogError("GetAvailableActions: Character is null!");
            return new List<BaseAction>();
        }

        CharacterType characterType = character.GetCharacterType();

        if (characterTypeActions.ContainsKey(characterType))
        {
            // Filter actions that the character can actually perform
            List<BaseAction> availableActions = new List<BaseAction>();
            foreach (var action in characterTypeActions[characterType])
            {
                if (action.CanPerformAction(character))
                {
                    availableActions.Add(action);
                }
            }
            
            return availableActions;
        }
        else
        {
            Debug.LogError($"No actions configured for character type: {characterType}");
        }
        
        return new List<BaseAction>();
    }
    
    public BaseAction GetActionByName(string actionName)
    {
        if (actionRegistry.ContainsKey(actionName))
        {
            return actionRegistry[actionName];
        }
        return null;
    }
    
    public bool PerformAction(Character performer, string actionName, Character target = null)
    {
        BaseAction action = GetActionByName(actionName);
        if (action == null)
        {
            Debug.LogError($"Action '{actionName}' not found!");
            return false;
        }
        
        if (!action.CanPerformAction(performer, target))
        {
            Debug.LogWarning($"Character {performer.characterName} cannot perform action '{actionName}'!");
            return false;
        }
        
        action.PerformAction(performer, target);
        return true;
    }
    
    public List<Character> GetValidTargetsForAction(Character performer, string actionName)
    {
        BaseAction action = GetActionByName(actionName);
        if (action == null) return new List<Character>();
        
        List<Character> allCharacters = new List<Character>();
        Character[] characters = FindObjectsOfType<Character>();
        allCharacters.AddRange(characters);
        
        return action.GetValidTargets(performer, allCharacters);
    }
    
    public bool HasActionsAvailable(Character character)
    {
        var actions = GetAvailableActions(character);
        return actions.Count > 0;
    }
    
    public void RegisterAction(string actionName, BaseAction action, List<CharacterType> allowedTypes = null)
    {
        actionRegistry[actionName] = action;
        
        if (allowedTypes != null)
        {
            foreach (var characterType in allowedTypes)
            {
                if (!characterTypeActions.ContainsKey(characterType))
                {
                    characterTypeActions[characterType] = new List<BaseAction>();
                }
                characterTypeActions[characterType].Add(action);
            }
        }
    }
    
    public void UnregisterAction(string actionName)
    {
        if (actionRegistry.ContainsKey(actionName))
        {
            BaseAction action = actionRegistry[actionName];
            actionRegistry.Remove(actionName);
            
            // Remove from character type mappings
            foreach (var kvp in characterTypeActions)
            {
                kvp.Value.Remove(action);
            }
        }
    }
    
    // Static instance for easy access
    private static ActionManager instance;
    public static ActionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ActionManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ActionManager");
                    instance = go.AddComponent<ActionManager>();
                }
            }
            return instance;
        }
    }
} 