﻿using NueDeck.Scripts.Data.Containers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NueDeck.Scripts.UI
{
    public class StatusIcon : MonoBehaviour
    {
        public Image statusImage;
        public TextMeshProUGUI statusValueText;

        public StatusIconData MyStatusIconData { get; private set; } = null;
        public void SetStatus(StatusIconData statusIconData)
        {
            MyStatusIconData = statusIconData;
            statusImage.sprite = statusIconData.iconSprite;
            
        }

        public void SetStatusValue(int statusValue)
        {
            statusValueText.text = statusValue.ToString();
        }
    }
}