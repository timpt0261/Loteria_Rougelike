using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FMODUnity
{
    public class EventCache : ScriptableObject, ISerializationCallbackReceiver
    {
        [field: SerializeField]
        public List<EditorBankRef> EditorBanks;
        [field: SerializeField]
        public List<EditorEventRef> EditorEvents;
        public Dictionary<string, int> EditorEventsDict;
        [field: SerializeField]
        public List<EditorParamRef> EditorParameters;
        [field: SerializeField]
        public List<EditorBankRef> MasterBanks;
        [field: SerializeField]
        public List<EditorBankRef> StringsBanks;
        [field: SerializeField]
        public int cacheVersion;
        [field: SerializeField]
        private Int64 cacheTime;
        [field: SerializeField]
        private List<DictionaryEntry> SerializableEventsDict;
        [Serializable]
        private struct DictionaryEntry
        {
            [field: SerializeField]
            public string key;
            [field: SerializeField]
            public int index;
        }

        public DateTime CacheTime
        {
            get { return new DateTime(cacheTime); }
            set { cacheTime = value.Ticks; }
        }

        public EventCache()
        {
            EditorBanks = new List<EditorBankRef>();
            EditorEvents = new List<EditorEventRef>();
            SerializableEventsDict = new List<DictionaryEntry>();
            EditorEventsDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            EditorParameters = new List<EditorParamRef>();
            MasterBanks = new List<EditorBankRef>();
            StringsBanks = new List<EditorBankRef>();
            cacheTime = 0;
        }

        public void OnBeforeSerialize()
        {
            if (SerializableEventsDict.Count == 0)
            {
                SerializableEventsDict = EditorEventsDict.Select(item => new DictionaryEntry { key = item.Key, index = item.Value }).ToList();
            }
        }

        public void OnAfterDeserialize()
        {
            if (SerializableEventsDict.Count > 0)
            {
                SerializableEventsDict.ForEach((item) =>
                {
                    EditorEventsDict.Add(item.key, item.index);
                });
                SerializableEventsDict.Clear();
            }
        }

        public void BuildDictionary()
        {
            EditorEventsDict.Clear();
            int index = 0;

            EditorEvents.ForEach((eventRef) =>
            {
                if (!EditorEventsDict.ContainsKey(eventRef.Path))
                {
                    EditorEventsDict.Add(eventRef.Path, index);
                }
                index++;
            });
        }
    }
}
