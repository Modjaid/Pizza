using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LanguageSystem
{
    public class LanguageChanger : MonoBehaviour
    {
        public void SetLanguage(int language)
        {
            TranslaterHandler.Language = (Languages)language;
        }
    }
}
