using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HealAction : BaseAction
{
    [Header("Heal Settings")]
    public int baseHealAmount = 30;
    public bool useCharacterHealingPower = true; // Use character's healing power instead of base heal
    public bool canHealFullHealth = false; // Whether this action can heal characters at full health
    
    public HealAction()
    {
        actionName = "Heal";
        description = "Heal a nearby ally";
        actionPointCost = 1;
        range = 1;
        requiresTarget = true;
        canTargetSelf = true;
        canTargetAllies = true;
        canTargetEnemies = false;
    }
    
    public override bool CanPerformAction(Character performer, Character target = null)
    {
        if (performer == null) return false;
        
        // Check if character has enough action points
        if (!performer.GetCharacterStats().CanPerformAction(actionPointCost))
            return false;
        
        // Check if character is alive
        if (!performer.GetCharacterStats().IsAlive())
            return false;
        
        // If no target specified, check if any valid targets exist
        if (target == null)
        {
            var allCharacters = FindAllCharacters();
            var validTargets = GetValidTargets(performer, allCharacters);
            return validTargets.Count > 0;
        }
        
        // Check if specific target is valid
        if (!IsValidTarget(performer, target))
            return false;
        
        // Check if target needs healing (unless we can heal full health targets)
        if (!canHealFullHealth && target.GetCharacterStats().currentHealth >= target.GetCharacterStats().maxHealth)
            return false;
        
        // Check if target is alive
        if (!target.GetCharacterStats().IsAlive())
            return false;
        
        return true;
    }
    
    public override void PerformAction(Character performer, Character target = null)
    {
        if (!CanPerformAction(performer, target)) return;
        
        // Calculate healing amount
        int healAmount = useCharacterHealingPower ? performer.GetCharacterStats().healingPower : baseHealAmount;
        
        // Apply healing to target
        if (target != null)
        {
            int oldHealth = target.GetCharacterStats().currentHealth;
            target.GetCharacterStats().Heal(healAmount);
            int actualHealAmount = target.GetCharacterStats().currentHealth - oldHealth;
            
            Debug.Log($"{performer.characterName} heals {target.characterName} for {actualHealAmount} health! " +
                      $"({target.characterName} health: {target.GetCharacterStats().currentHealth}/{target.GetCharacterStats().maxHealth})");
        }
        
        // Use action points
        performer.GetCharacterStats().UseActionPoints(actionPointCost);
        
        // Play effects
        PlayEffects(performer, target);
        
        // Notify that action was performed
        performer.HandleActionPerformed(this, target);
    }
    
    public override List<Character> GetValidTargets(Character performer, List<Character> allCharacters)
    {
        List<Character> validTargets = new List<Character>();
        
        foreach (Character character in allCharacters)
        {
            if (IsValidTarget(performer, character))
            {
                // Additional check for healing - only include targets that need healing
                if (canHealFullHealth || character.GetCharacterStats().currentHealth < character.GetCharacterStats().maxHealth)
                {
                    // Only include alive targets
                    if (character.GetCharacterStats().IsAlive())
                    {
                        validTargets.Add(character);
                    }
                }
            }
        }
        
        return validTargets;
    }
    
    private List<Character> FindAllCharacters()
    {
        List<Character> allCharacters = new List<Character>();
        Character[] characters = Object.FindObjectsOfType<Character>();
        allCharacters.AddRange(characters);
        return allCharacters;
    }
} 