using UnityEngine;
using UnityEditor;

namespace Proselyte.Sigils
{
    [CustomPropertyDrawer(typeof(ColorReference))]
    public class ColorReferenceDrawer : BaseReferenceDrawer
    {
        protected override string ConstantFieldName => "ConstantValue";
        protected override string VariableFieldName => "Variable";
    }
}
