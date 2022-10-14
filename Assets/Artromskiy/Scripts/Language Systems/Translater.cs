using System;
using UnityEngine;
using UnityEngine.UI;

namespace LanguageSystem
{
    [RequireComponent(typeof(Text))]
    public class Translater : MonoBehaviour
    {
        public LanguageDictionary translation = new LanguageDictionary();
        private Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        private void OnEnable()
        {
            TranslaterHandler.OnLanguageChanged += UpdateText;
            UpdateText();
        }

        private void UpdateText()
        {
            text.text = translation[TranslaterHandler.Language];
        }

        private void OnDisable()
        {
            TranslaterHandler.OnLanguageChanged -= UpdateText;
        }
    }
}
