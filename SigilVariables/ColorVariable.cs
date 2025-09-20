using UnityEngine;
using System;

namespace Proselyte.Sigils
{
    [CreateAssetMenu(menuName = "Sigils/Variables/Color")]
    public class ColorVariable : ScriptableObject
    {
        public Color value;
    }
}
