using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public abstract class BaseAction
{
    [Header("Action Settings")]
    public string actionName;
    public string description;
    public int actionPointCost = 1;
    public int range = 1;
    public bool requiresTarget = true;
    public bool canTargetSelf = false;
    public bool canTargetAllies = false;
    public bool canTargetEnemies = true;
    
    [Header("Visual Effects")]
    public GameObject actionEffect; // Optional effect to spawn when action is performed
    public AudioClip actionSound; // Optional sound to play
    
    public abstract bool CanPerformAction(Character performer, Character target = null);
    public abstract void PerformAction(Character performer, Character target = null);
    public abstract List<Character> GetValidTargets(Character performer, List<Character> allCharacters);
    
    protected virtual bool IsValidTarget(Character performer, Character target)
    {
        if (target == null) return false;
        if (target == performer && !canTargetSelf) return false;
        
        // Check if target is within range
        Vector2Int performerPos = performer.GetGridPosition();
        Vector2Int targetPos = target.GetGridPosition();
        int distance = Mathf.Abs(performerPos.x - targetPos.x) + Mathf.Abs(performerPos.y - targetPos.y);
        
        if (distance > range) return false;
        
        // Check if target is correct type (ally/enemy)
        bool isAlly = performer.isPlayerControlled == target.isPlayerControlled;
        
        if (isAlly && !canTargetAllies) return false;
        if (!isAlly && !canTargetEnemies) return false;
        
        return true;
    }
    
    protected virtual void PlayEffects(Character performer, Character target = null)
    {
        // Play sound effect
        if (actionSound != null && performer != null)
        {
            AudioSource.PlayClipAtPoint(actionSound, performer.transform.position);
        }
        
        // Spawn visual effect
        if (actionEffect != null)
        {
            Vector3 effectPosition = target != null ? target.transform.position : performer.transform.position;
            GameObject.Instantiate(actionEffect, effectPosition, Quaternion.identity);
        }
    }
} 