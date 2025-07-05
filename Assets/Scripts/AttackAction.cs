using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AttackAction : BaseAction
{
    [Header("Attack Settings")]
    public int baseDamage = 25;
    public bool useCharacterDamage = true; // Use character's attack damage instead of base damage
    
    public AttackAction()
    {
        actionName = "Attack";
        description = "Attack a nearby enemy";
        actionPointCost = 1;
        range = 1;
        requiresTarget = true;
        canTargetSelf = false;
        canTargetAllies = false;
        canTargetEnemies = true;
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
        return IsValidTarget(performer, target);
    }
    
    public override void PerformAction(Character performer, Character target = null)
    {
        if (!CanPerformAction(performer, target)) return;
        
        // Calculate damage
        int damage = useCharacterDamage ? performer.GetCharacterStats().attackDamage : baseDamage;
        
        // Apply damage to target
        if (target != null)
        {
            target.GetCharacterStats().TakeDamage(damage);
            
            Debug.Log($"{performer.characterName} attacks {target.characterName} for {damage} damage! " +
                      $"({target.characterName} health: {target.GetCharacterStats().currentHealth}/{target.GetCharacterStats().maxHealth})");
            
            // Check if target was defeated
            if (!target.GetCharacterStats().IsAlive())
            {
                Debug.Log($"{target.characterName} has been defeated!");
                target.HandleCharacterDefeated();
            }
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
                validTargets.Add(character);
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

[System.Serializable]
public class RangedAttackAction : AttackAction
{
    public RangedAttackAction()
    {
        actionName = "Ranged Attack";
        description = "Attack an enemy from a distance";
        actionPointCost = 1;
        range = 3; // Longer range than basic attack
        requiresTarget = true;
        canTargetSelf = false;
        canTargetAllies = false;
        canTargetEnemies = true;
    }
} 