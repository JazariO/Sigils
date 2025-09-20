using UnityEngine;
using UnityEditor;

namespace Proselyte.Sigils
{
    [CustomPropertyDrawer(typeof(Vector3Reference))]
    internal class Vector3ReferenceDrawer : BaseReferenceDrawer
    {
        protected override string ConstantFieldName => "ConstantValue";
        protected override string VariableFieldName => "Variable";
    }
}
