using UnityEngine;
using System;

namespace Proselyte.Sigils
{
    [Serializable]
    public class ColorReference
    {
        public bool UseConstant = true;
        public Color ConstantValue;
        public ColorVariable Variable;

        public Color Value => UseConstant ? ConstantValue : Variable.value;
    }
}
