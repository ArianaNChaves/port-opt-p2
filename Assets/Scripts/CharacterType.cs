using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum CharacterType
{
    Fighter,    // Close combat specialist with attack abilities
    Healer,     // Support character with healing abilities
    Ranger      // Ranged combat specialist with longer range attacks
}

[System.Serializable]
public class CharacterStats
{
    [Header("Health System")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("Action System")]
    public int maxActionPoints = 2;
    public int currentActionPoints;
    
    [Header("Combat Stats")]
    public int attackDamage = 25;
    public int healingPower = 30;
    public int defense = 5;
    
    public CharacterStats()
    {
        currentHealth = maxHealth;
        currentActionPoints = maxActionPoints;
    }
    
    public void ResetForNewTurn()
    {
        currentActionPoints = maxActionPoints;
    }
    
    public bool HasActionPoints()
    {
        return currentActionPoints > 0;
    }
    
    public bool CanPerformAction(int actionCost)
    {
        return currentActionPoints >= actionCost;
    }
    
    public void UseActionPoints(int amount)
    {
        currentActionPoints = Mathf.Max(0, currentActionPoints - amount);
    }
    
    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(1, damage - defense);
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }
}

[System.Serializable]
public class CharacterTypeConfig
{
    public CharacterType type;
    public CharacterStats baseStats;
    public List<string> availableActionTypes; // Names of actions this character type can perform
    
    public CharacterTypeConfig(CharacterType characterType)
    {
        type = characterType;
        baseStats = new CharacterStats();
        availableActionTypes = new List<string>();
        
        // Set default configurations based on type
        switch (characterType)
        {
            case CharacterType.Fighter:
                baseStats.maxHealth = 120;
                baseStats.attackDamage = 35;
                baseStats.healingPower = 0;
                baseStats.defense = 8;
                baseStats.maxActionPoints = 2;
                availableActionTypes.Add("Attack");
                break;
                
            case CharacterType.Healer:
                baseStats.maxHealth = 80;
                baseStats.attackDamage = 15;
                baseStats.healingPower = 45;
                baseStats.defense = 3;
                baseStats.maxActionPoints = 3;
                availableActionTypes.Add("Heal");
                availableActionTypes.Add("Attack");
                break;
                
            case CharacterType.Ranger:
                baseStats.maxHealth = 100;
                baseStats.attackDamage = 30;
                baseStats.healingPower = 0;
                baseStats.defense = 5;
                baseStats.maxActionPoints = 2;
                availableActionTypes.Add("RangedAttack");
                break;
        }
        
        // Initialize current values
        baseStats.currentHealth = baseStats.maxHealth;
        baseStats.currentActionPoints = baseStats.maxActionPoints;
    }
} 