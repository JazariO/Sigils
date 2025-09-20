using UnityEngine;
using System;

namespace Proselyte.Sigils
{
    [Serializable]
    public class Vector2Reference
    {
        public bool UseConstant = true;
        public Vector2 ConstantValue;
        public Vector2Variable Variable;

        public Vector2 Value => UseConstant ? ConstantValue : Variable.value;
    }
}
