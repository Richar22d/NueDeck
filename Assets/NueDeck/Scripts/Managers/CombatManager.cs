﻿using System;
using System.Collections;
using System.Collections.Generic;
using NueDeck.Scripts.Characters;
using NueDeck.Scripts.Characters.Enemies;
using NueDeck.Scripts.Collection;
using NueDeck.Scripts.Enums;
using NueDeck.Scripts.UI;
using UnityEngine;

namespace NueDeck.Scripts.Managers
{
    public class CombatManager : MonoBehaviour
    {
        private CombatManager(){}
        public static CombatManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private List<Transform> enemyPosList;
        [SerializeField] private List<Transform> allyPosList;
        
        public List<EnemyBase> CurrentEnemiesList { get; private set; } = new List<EnemyBase>();
        public List<AllyBase> CurrentAlliesList { get; private set; }= new List<AllyBase>();

        public Action OnAllyTurnStarted;
        public Action OnEnemyTurnStarted;
        public List<Transform> EnemyPosList => enemyPosList;

        public List<Transform> AllyPosList => allyPosList;
        
        public CombatState CurrentCombatState
        {
            get => _currentCombatState;
            private set
            {
                ExecuteCombatState(value);
                _currentCombatState = value;
            }
        }
        
        private CombatState _currentCombatState;
        
        #region Setup
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            } 
            else
            {
                Instance = this;
                CurrentCombatState = CombatState.PrepareCombat;
            }
        }

        private void Start()
        {
            StartCombat();
        }

        public void StartCombat()
        {
            BuildEnemies();
            BuildAllies();
          
            CollectionManager.Instance.SetGameDeck();
           
            UIManager.Instance.combatCanvas.gameObject.SetActive(true);
            UIManager.Instance.informationCanvas.gameObject.SetActive(true);
            CurrentCombatState = CombatState.AllyTurn;
        }
        
        private void ExecuteCombatState(CombatState targetState)
        {
            switch (targetState)
            {
                case CombatState.PrepareCombat:
                    break;
                case CombatState.AllyTurn:

                    OnAllyTurnStarted?.Invoke();
                    
                    GameManager.Instance.PersistentGameplayData.CurrentMana = GameManager.Instance.PersistentGameplayData.MAXMana;
                    
                    CollectionManager.Instance.DrawCards(GameManager.Instance.PersistentGameplayData.DrawCount);
                    
                    GameManager.Instance.PersistentGameplayData.CanSelectCards = true;
                    
                    break;
                case CombatState.EnemyTurn:

                    OnEnemyTurnStarted?.Invoke();
                    
                    CollectionManager.Instance.DiscardHand();
                    
                    StartCoroutine(nameof(EnemyTurnRoutine));
                    
                    GameManager.Instance.PersistentGameplayData.CanSelectCards = false;
                    
                    break;
                case CombatState.EndCombat:
                    
                    GameManager.Instance.PersistentGameplayData.CanSelectCards = false;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetState), targetState, null);
            }
        }
        #endregion

        #region Public Methods
        public void EndTurn()
        {
            CurrentCombatState = CombatState.EnemyTurn;
        }
        public void OnAllyDeath(AllyBase targetAlly)
        {
            CurrentAlliesList.Remove(targetAlly);
            if (CurrentAlliesList.Count<=0)
                LoseCombat();
        }
        public void OnEnemyDeath(EnemyBase targetEnemy)
        {
            CurrentEnemiesList.Remove(targetEnemy);
            if (CurrentEnemiesList.Count<=0)
                WinCombat();
        }
        public void DeactivateCardHighlights()
        {
            foreach (var currentEnemy in CurrentEnemiesList)
                currentEnemy.EnemyCanvas.SetHighlight(false);

            foreach (var currentAlly in CurrentAlliesList)
                currentAlly.AllyCanvas.SetHighlight(false);
        }
        public void IncreaseMana(int target)
        {
            GameManager.Instance.PersistentGameplayData.CurrentMana += target;
            UIManager.Instance.combatCanvas.SetPileTexts();
        }
        public void HighlightCardTarget(ActionTarget targetTarget)
        {
            switch (targetTarget)
            {
                case ActionTarget.Enemy:
                    foreach (var currentEnemy in CurrentEnemiesList)
                        currentEnemy.EnemyCanvas.SetHighlight(true);
                    break;
                case ActionTarget.Ally:
                    foreach (var currentAlly in CurrentAlliesList)
                        currentAlly.AllyCanvas.SetHighlight(true);
                    break;
                case ActionTarget.AllEnemies:
                    break;
                case ActionTarget.AllAllies:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetTarget), targetTarget, null);
            }
        }
        #endregion
        
        #region Private Methods
        private void BuildEnemies()
        {
            var encounter = GameManager.Instance.EncounterData.GetEnemyEncounter(GameManager.Instance.PersistentGameplayData.CurrentStageId,GameManager.Instance.PersistentGameplayData.CurrentEncounterId,GameManager.Instance.PersistentGameplayData.IsFinalEncounter).enemyList;
            for (var i = 0; i < encounter.Count; i++)
            {
                var clone = Instantiate(encounter[i], EnemyPosList.Count >= i ? EnemyPosList[i] : EnemyPosList[0]);
                clone.BuildCharacter();
                CurrentEnemiesList.Add(clone);
            }
        }
        private void BuildAllies()
        {
            for (var i = 0; i < GameManager.Instance.PersistentGameplayData.AllyList.Count; i++)
            {
                var clone = Instantiate(GameManager.Instance.PersistentGameplayData.AllyList[i], AllyPosList.Count >= i ? AllyPosList[i] : AllyPosList[0]);
                clone.BuildCharacter();
                CurrentAlliesList.Add(clone);
            }
        }
        private void LoseCombat()
        {
            CurrentCombatState = CombatState.EndCombat;
            CollectionManager.Instance.DiscardHand();
            CollectionManager.Instance.DiscardPile.Clear();
            CollectionManager.Instance.DrawPile.Clear();
            CollectionManager.Instance.HandPile.Clear();
            CollectionManager.Instance.HandController.hand.Clear();
            UIManager.Instance.combatCanvas.gameObject.SetActive(true);
            UIManager.Instance.combatCanvas.combatLosePanel.SetActive(true);
        }
        private void WinCombat()
        {
            CurrentCombatState = CombatState.EndCombat;

            foreach (var currentAlly in CurrentAlliesList)
            {
                GameManager.Instance.PersistentGameplayData.CurrentHealthDict[currentAlly.AllyData.characterID] = currentAlly.CharacterStats.CurrentHealth;
                GameManager.Instance.PersistentGameplayData.MaxHealthDict[currentAlly.AllyData.characterID] = currentAlly.CharacterStats.MaxHealth;
            }
            
            CollectionManager.Instance.DiscardPile.Clear();
            CollectionManager.Instance.DrawPile.Clear();
            CollectionManager.Instance.HandPile.Clear();
            CollectionManager.Instance.HandController.hand.Clear();
            
           
            if (GameManager.Instance.PersistentGameplayData.IsFinalEncounter)
            {
                UIManager.Instance.combatCanvas.combatWinPanel.SetActive(true);
            }
            else
            {
                GameManager.Instance.PersistentGameplayData.CurrentEncounterId++;
                UIManager.Instance.combatCanvas.gameObject.SetActive(false);
                UIManager.Instance.rewardCanvas.gameObject.SetActive(true);
                UIManager.Instance.rewardCanvas.BuildReward(RewardType.Gold);
                UIManager.Instance.rewardCanvas.BuildReward(RewardType.Card);
            }
           
        }
        #endregion
        
        #region Routines
        private IEnumerator EnemyTurnRoutine()
        {
            var waitDelay = new WaitForSeconds(0.1f);

            foreach (var currentEnemy in CurrentEnemiesList)
            {
                yield return currentEnemy.StartCoroutine(nameof(EnemyExample.ActionRoutine));
                yield return waitDelay;
            }

            if (CurrentCombatState != CombatState.EndCombat)
                CurrentCombatState = CombatState.AllyTurn;
        }
        #endregion
    }
}