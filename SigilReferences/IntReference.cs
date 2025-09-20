using UnityEngine;
using System;

namespace Proselyte.Sigils
{
    [Serializable]
    public class IntReference
    {
        public bool UseConstant = true;
        public int ConstantValue;
        public IntVariable Variable;

        public int Value => UseConstant ? ConstantValue : Variable.value;
    }
}
