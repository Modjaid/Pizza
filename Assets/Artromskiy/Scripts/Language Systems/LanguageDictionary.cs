using System.Collections.Generic;
using UnityEngine;
using System;

namespace LanguageSystem
{
    [Serializable]
    public class LanguageDictionary : Dictionary<Languages, string>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<LanguagePair> data = new List<LanguagePair>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            data.Clear();
            int enumCount = Enum.GetNames(typeof(Languages)).Length;
            for (int i = 0; i < enumCount; i++)
            {
                data.Add(new LanguagePair((Languages)i, " "));
            }
            for (int i = 0; i < Count; i++)
            {
                if (ContainsKey(data[i].key))
                {
                    data[i] = new LanguagePair(data[i].key, this[data[i].key]);
                }
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            Clear();

            for (int i = 0; i < data.Count; i++)
                Add(data[i].key, data[i].value);
        }

        [Serializable]
        private class LanguagePair
        {
            public Languages key;
            public string value;

            public LanguagePair(Languages key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }
    }
}