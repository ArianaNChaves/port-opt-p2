using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TestAction : BaseAction
{
    public TestAction()
    {
        actionName = "Test Action";
        description = "A simple test action that doesn't require targets";
        actionPointCost = 1;
        range = 1;
        requiresTarget = false;
        canTargetSelf = false;
        canTargetAllies = false;
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
        
        // This action doesn't require targets, so it's always available
        return true;
    }
    
    public override void PerformAction(Character performer, Character target = null)
    {
        if (!CanPerformAction(performer, target)) return;
        
        Debug.Log($"{performer.characterName} performs a test action! ⚡");
        
        // Use action points
        performer.GetCharacterStats().UseActionPoints(actionPointCost);
        
        // Play effects
        PlayEffects(performer, target);
        
        // Notify that action was performed
        performer.HandleActionPerformed(this, target);
    }
    
    public override List<Character> GetValidTargets(Character performer, List<Character> allCharacters)
    {
        // This action doesn't require targets
        return new List<Character>();
    }
} 