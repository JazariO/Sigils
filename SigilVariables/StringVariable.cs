using UnityEngine;
using System;

namespace Proselyte.Sigils
{
    [CreateAssetMenu(menuName = "Sigils/Variables/String")]
    public class StringVariable : ScriptableObject
    {
        public string value;
    }
}
