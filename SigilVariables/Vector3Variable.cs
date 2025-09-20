using UnityEngine;
using System;

namespace Proselyte.Sigils
{
    [CreateAssetMenu(menuName = "Sigils/Variables/Vector3")]
    public class Vector3Variable : ScriptableObject
    {
        public Vector3 value;
    }
}
