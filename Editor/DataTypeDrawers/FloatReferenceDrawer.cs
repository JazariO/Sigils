using UnityEngine;
using UnityEditor;

namespace Proselyte.Sigils
{
    [CustomPropertyDrawer(typeof(FloatReference))]
    internal class FloatReferenceDrawer : BaseReferenceDrawer
    {
        protected override string ConstantFieldName => "ConstantValue";
        protected override string VariableFieldName => "Variable";
    }
}