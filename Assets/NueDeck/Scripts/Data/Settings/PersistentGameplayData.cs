﻿using System;
using System.Collections.Generic;
using NueDeck.Scripts.Characters;
using NueDeck.Scripts.Data.Collection;
using NueDeck.Scripts.UI;

namespace NueDeck.Scripts.Data.Settings
{
    [Serializable]
    public class PersistentGameplayData
    {
        public int DrawCount { get; set; }
        public int MAXMana { get; set; }
        public int CurrentMana { get; set; }
        public bool CanUseCards { get; set; }
        public bool CanSelectCards { get; set; }
        public bool IsRandomHand { get; set; }
        public List<AllyBase> AllyList { get; set; }
        public int CurrentStageId { get; set; }
        public int CurrentEncounterId { get; set; }
        
        public List<CardData> CurrentCardsList{ get; set; }

        public int CurrentGold
        {
            get
            {
                return _currentGold;
            }
            set
            {
                _currentGold = value;
                if (UIManager.instance)  UIManager.instance.informationCanvas.goldText.text = _currentGold.ToString();
               
            }
        }

        private int _currentGold;

        private readonly GameplayData _gameplayData;
        
        public PersistentGameplayData(GameplayData gameplayData)
        {
            _gameplayData = gameplayData;

            InitData();
        }

        private void InitData()
        {
            DrawCount = _gameplayData.drawCount;
            MAXMana = _gameplayData.maxMana;
            CurrentMana = MAXMana;
            CanUseCards = _gameplayData.canUseCards;
            CanSelectCards = _gameplayData.canSelectCards;
            IsRandomHand = _gameplayData.isRandomHand;
            AllyList = _gameplayData.allyList;
            CurrentEncounterId = 0;
            CurrentStageId = 0;
            CurrentGold = 0;
            CurrentCardsList = new List<CardData>();
        }
    }
}