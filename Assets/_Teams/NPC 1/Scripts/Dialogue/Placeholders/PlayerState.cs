using System.Collections.Generic;
using UnityEngine;
// Created by Jantina
// Placeholder script
public class PlayerState : MonoBehaviour
{
    public List<string> completedQuests = new List<string>();
    public List<string> inventory = new List<string>();
    public bool HasQuest(string questID) => completedQuests.Contains(questID);
    public bool HasItem(string itemID) => inventory.Contains(itemID);
}