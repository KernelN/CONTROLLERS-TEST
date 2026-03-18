using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Universal.Gameplay
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI timerText;
        [SerializeField] bool showOnTimer0 = false;
        public void ShowCountdown(float timer, float maxTime)
        {
            if(!showOnTimer0) timerText.enabled = timer > 0;
            timer = maxTime - timer;
            timerText.text = $"{timer/60:00}:{timer%60:00}";
        }
    }
}