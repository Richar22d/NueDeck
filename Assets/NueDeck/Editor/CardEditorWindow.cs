using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NueDeck.Scripts.Data.Collection;
using NueDeck.Scripts.Enums;
using NueExtentions;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace NueDeck.Editor
{
    public class CardEditorWindow : ExtendedEditorWindow
    {
        private static CardEditorWindow CurrentWindow { get; set; }
        private SerializedObject _serializedObject;
       
       
        #region Cache Card Data
        private static CardData CachedCardData { get; set; }
        private List<CardData> AllCardDataList { get; set; }
        private CardData SelectedCardData { get; set; }
        private string CardId { get; set; }
        private string CardName{ get; set; }
        private int ManaCost{ get; set; }
        private Sprite CardSprite{ get; set; }
        private bool UsableWithoutTarget{ get; set; }
        private List<CardActionData> CardActionDataList{ get; set; }
        private List<CardDescriptionData> CardDescriptionDataList{ get; set; }
        private List<SpecialKeywords> SpecialKeywordsList{ get; set; }
        private AudioActionType AudioType{ get; set; }

        private void CacheCardData()
        {
            CardId = SelectedCardData.Id;
            CardName = SelectedCardData.CardName;
            ManaCost = SelectedCardData.ManaCost;
            CardSprite = SelectedCardData.CardSprite;
            UsableWithoutTarget = SelectedCardData.UsableWithoutTarget;
            CardActionDataList = new List<CardActionData>(SelectedCardData.CardActionDataList);
            CardDescriptionDataList = new List<CardDescriptionData>(SelectedCardData.CardDescriptionDataList);
            SpecialKeywordsList = new List<SpecialKeywords>(SelectedCardData.KeywordsList);
            AudioType = SelectedCardData.AudioType;

        }
        
        private void ClearCachedCardData()
        {
            CardId = String.Empty;
            CardName = String.Empty;
            ManaCost = 0;
            CardSprite = null;
            UsableWithoutTarget = false;
            CardActionDataList?.Clear();
            CardDescriptionDataList?.Clear();
            SpecialKeywordsList?.Clear();
            AudioType = AudioActionType.Attack;
        }
        #endregion
        
        #region Setup
        [MenuItem("NueDeck/CardEditor")]
        public static void OpenCardEditor() =>  CurrentWindow = GetWindow<CardEditorWindow>("Card Editor");
        public static void OpenCardEditor(CardData targetData)
        {
            CachedCardData = targetData;
            OpenCardEditor();
        } 
        
        private void OnEnable()
        {
            
            AllCardDataList?.Clear();
            AllCardDataList = ListExtentions.GetAllInstances<CardData>().ToList();
            
            if (CachedCardData)
            {
                SelectedCardData = CachedCardData;
                _serializedObject = new SerializedObject(SelectedCardData);
                CacheCardData();
            }
            
            Selection.selectionChanged += Repaint;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= Repaint;
            CachedCardData = null;
            SelectedCardData = null;
        }

      
        #endregion

        #region Process
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            
            DrawAllCardButtons();
            EditorGUILayout.Space();
            DrawSelectedCard();
            
            EditorGUILayout.EndHorizontal();
        }
        #endregion
        
        #region Layout Methods
        private Vector2 _allCardButtonsScrollPos;
        private void DrawAllCardButtons()
        {
            _allCardButtonsScrollPos = EditorGUILayout.BeginScrollView(_allCardButtonsScrollPos, GUILayout.MinWidth(150), GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cards",EditorStyles.boldLabel);
            
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("Refresh",GUILayout.Width(75),GUILayout.Height(20)))
                RefreshCardData();
            GUI.backgroundColor = oldColor;
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            foreach (var data in AllCardDataList)
                if (GUILayout.Button(data.CardName,GUILayout.MaxWidth(150)))
                {
                    SelectedCardData = data;
                    _serializedObject = new SerializedObject(SelectedCardData);
                    CacheCardData();
                }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        private void DrawSelectedCard()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            if (!SelectedCardData)
            {
                EditorGUILayout.LabelField("Select card");
                return;
            }
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            ChangeId();
            ChangeCardName();
            ChangeManaCost();
            ChangeCardSprite();
            ChangeUsableWithoutTarget();
            ChangeCardActionDataList();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Save",GUILayout.Width(100),GUILayout.Height(30)))
                SaveCardData();
            
            GUI.backgroundColor = oldColor;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        
        #endregion

        #region Card Data Methods
        private void ChangeId()
        {
            CardId = EditorGUILayout.TextField("Card Id:", CardId);
        }
        private void ChangeCardName()
        {
            CardName = EditorGUILayout.TextField("Card Name:", CardName);
        }
        
        private void ChangeManaCost()
        {
            ManaCost = EditorGUILayout.IntField("Mana Cost:", ManaCost);
        }

        private void ChangeCardSprite()
        {
            EditorGUILayout.BeginHorizontal();
            CardSprite = (Sprite)EditorGUILayout.ObjectField("Card Sprite:", CardSprite,typeof(Sprite));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void ChangeUsableWithoutTarget()
        {
            UsableWithoutTarget = EditorGUILayout.Toggle("Usable Without Target:", UsableWithoutTarget);
        }

        private bool _isCardActionDataListFolded;
        private Vector2 _cardActionScrollPos;
        private void ChangeCardActionDataList()
        {
            _isCardActionDataListFolded =EditorGUILayout.BeginFoldoutHeaderGroup(_isCardActionDataListFolded, "Card Actions");
            if (_isCardActionDataListFolded)
            {
                _cardActionScrollPos = EditorGUILayout.BeginScrollView(_cardActionScrollPos,GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginHorizontal();
                List<CardActionData> _removedList = new List<CardActionData>();
                foreach (var cardActionData in CardActionDataList)
                {
                    EditorGUILayout.BeginVertical("box", GUILayout.Width(150), GUILayout.MaxHeight(50));
                    EditorGUILayout.BeginHorizontal();
                    var newActionType = (CardActionType)EditorGUILayout.EnumFlagsField(cardActionData.CardActionType);
                    var newActionTarget = (ActionTarget)EditorGUILayout.EnumFlagsField(cardActionData.ActionTarget);
                    if (GUILayout.Button("X",GUILayout.MaxWidth(25),GUILayout.MaxHeight(25)))
                        _removedList.Add(cardActionData);
                    
                    EditorGUILayout.EndHorizontal();
                    var newActionValue = EditorGUILayout.FloatField(cardActionData.ActionValue);
                    cardActionData.EditActionType(newActionType);
                    cardActionData.EditActionValue(newActionValue);
                    cardActionData.EditActionTarget(newActionTarget);
                    EditorGUILayout.EndVertical();
                }

                foreach (var cardActionData in _removedList)
                    CardActionDataList.Remove(cardActionData);

                if (GUILayout.Button("+",GUILayout.Width(50),GUILayout.Height(50)))
                {
                   CardActionDataList.Add(new CardActionData());
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void SaveCardData()
        {
            if (!SelectedCardData) return;
            
            SelectedCardData.EditId(CardId);
            SelectedCardData.EditCardName(CardName);
            SelectedCardData.EditManaCost(ManaCost);
            SelectedCardData.EditCardSprite(CardSprite);
            EditorUtility.SetDirty(SelectedCardData);
            AssetDatabase.SaveAssets();
        }

        private void RefreshCardData()
        {
            AllCardDataList?.Clear();
            AllCardDataList = ListExtentions.GetAllInstances<CardData>().ToList();
        }
      
        #endregion
    }
}
