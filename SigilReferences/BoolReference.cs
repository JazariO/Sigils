using UnityEngine;
using System;

namespace Proselyte.Sigils
{
    [Serializable]
    public class BoolReference
    {
        public bool UseConstant = true;
        public bool ConstantValue;
        public BoolVariable Variable;

        public bool Value => UseConstant ? ConstantValue : Variable.value;
    }
}
