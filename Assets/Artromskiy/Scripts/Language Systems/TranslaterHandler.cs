using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LanguageSystem
{
    public static class TranslaterHandler
    {
        public static Languages Language
        {
            get => (Languages)PlayerPrefs.GetInt("Language");
            set
            {
                PlayerPrefs.SetInt("Language", (int)value);
                OnLanguageChanged?.Invoke();
            }
        }

        public static event System.Action OnLanguageChanged;
    }
}
