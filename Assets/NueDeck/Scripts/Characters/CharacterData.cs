﻿using UnityEngine;

namespace NueDeck.Scripts.Characters
{
    public class CharacterData : ScriptableObject
    {
        public int characterID;
        public string characterName;
        [TextArea]
        public string characterDescription;
        public Sprite characterSprite;
        public int maxHealth;
        
    }
}