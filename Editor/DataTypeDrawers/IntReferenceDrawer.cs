using UnityEngine;
using UnityEditor;

namespace Proselyte.Sigils
{
    [CustomPropertyDrawer(typeof(IntReference))]
    internal class IntReferenceDrawer : BaseReferenceDrawer
    {
        protected override string ConstantFieldName => "ConstantValue";
        protected override string VariableFieldName => "Variable";
    }
}
