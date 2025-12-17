using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace FMODUnity
{
    public class EditorEventRef : ScriptableObject
    {
        [field: SerializeField]
        public string Path;

        [field: SerializeField]
        public FMOD.GUID Guid;

        [field: SerializeField]
        public List<EditorBankRef> Banks;
        [field: SerializeField]
        public bool IsStream;
        [field: SerializeField]
        public bool Is3D;
        [field: SerializeField]
        public bool IsOneShot;
        [field: SerializeField]
        public List<EditorParamRef> Parameters;
        [field: SerializeField]
        public float MinDistance;
        [field: SerializeField]
        public float MaxDistance;
        [field: SerializeField]
        public int Length;

        public List<EditorParamRef> LocalParameters
        {
            get { return Parameters.Where(p => p.IsGlobal == false).OrderBy(p => p.Name).ToList(); }
        }

        public List<EditorParamRef> GlobalParameters
        {
            get { return Parameters.Where(p => p.IsGlobal == true).OrderBy(p => p.Name).ToList(); }
        }
    }
}
