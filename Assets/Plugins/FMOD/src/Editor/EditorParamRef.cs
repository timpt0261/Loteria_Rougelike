using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FMODUnity
{
    public enum ParameterType
    {
        Continuous,
        Discrete,
        Labeled,
    }

    public class EditorParamRef : ScriptableObject
    {
        [field: SerializeField]
        public string Name;
        [field: SerializeField]
        public string StudioPath;
        [field: SerializeField]
        public float Min;
        [field: SerializeField]
        public float Max;
        [field: SerializeField]
        public float Default;
        [field: SerializeField]
        public ParameterID ID;
        [field: SerializeField]
        public ParameterType Type;
        [field: SerializeField]
        public bool IsGlobal;
        [field: SerializeField]
        public string[] Labels = { };

        public bool Exists;

        [Serializable]
        public struct ParameterID
        {
            public static implicit operator ParameterID(FMOD.Studio.PARAMETER_ID source)
            {
                return new ParameterID
                {
                    data1 = source.data1,
                    data2 = source.data2,
                };
            }

            public static implicit operator FMOD.Studio.PARAMETER_ID(ParameterID source)
            {
                return new FMOD.Studio.PARAMETER_ID
                {
                    data1 = source.data1,
                    data2 = source.data2,
                };
            }

            public bool Equals(FMOD.Studio.PARAMETER_ID other)
            {
                return data1 == other.data1 && data2 == other.data2;
            }

            public uint data1;
            public uint data2;
        }
    }
}
