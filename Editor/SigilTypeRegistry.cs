#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Proselyte.Sigils
{
    internal static class SigilTypeRegistry
    {
        internal class SigilTypeEntry
        {
            public Type Type { get; }
            public string Alias { get; }

            public SigilTypeEntry(Type type, string alias)
            {
                Type = type;
                Alias = alias;
            }
        }

        internal static readonly SigilTypeEntry[] SupportedTypes = new[]
        {
            new SigilTypeEntry(typeof(bool), "bool"),
            new SigilTypeEntry(typeof(float), "float"),
            new SigilTypeEntry(typeof(int), "int"),
            new SigilTypeEntry(typeof(string), "string"),
            new SigilTypeEntry(typeof(Vector2), "Vector2"),
            new SigilTypeEntry(typeof(Vector3), "Vector3"),
            new SigilTypeEntry(typeof(Color), "Color"),
            // Add new sigil types here...
        };

        private const string k_SigilArchitecturePath = "Plugins/Sigils/";

        [MenuItem("Tools/Sigils/Register Types")]
        public static void RegisterTypes()
        {
            foreach(var type in SupportedTypes)
            {
                Debug.Log("Generating Script for Type: " + type.Type.Name);
                GenerateVariableScript(type);
                GenerateReferenceScript(type);
                GenerateDrawerScript(type);
            }

            AssetDatabase.Refresh();
        }

        private static void GenerateVariableScript(SigilTypeEntry sigilTypeEntry)
        {
            string capitalizedtypeAlias = char.ToUpper(sigilTypeEntry.Alias[0]) + sigilTypeEntry.Alias[1..];
            string className = $"{capitalizedtypeAlias}Variable";

            string code = $@"using UnityEngine;
using System;
namespace {nameof(Proselyte)+ "." +nameof(Proselyte.Sigils)}
{{
    [CreateAssetMenu(menuName = ""Variables /{capitalizedtypeAlias}"")]
    public class {className} : ScriptableObject
    {{
        public {sigilTypeEntry.Alias} value;
    }}
}}";
            string folderPath = Path.Combine(Application.dataPath, k_SigilArchitecturePath, "SigilVariables");
            string filePath = Path.Combine(folderPath, $"{className}.cs");
            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            File.WriteAllText(filePath, code);
        }

        private static void GenerateReferenceScript(SigilTypeEntry sigilTypeEntry)
        {
            string capitalizedtypeAlias = char.ToUpper(sigilTypeEntry.Alias[0]) + sigilTypeEntry.Alias[1..];
            string className = $"{capitalizedtypeAlias}Reference";

            string code = $@"using UnityEngine;
using System;

namespace {nameof(Proselyte) + "." + nameof(Proselyte.Sigils)}
{{
    [Serializable]
    public class {className}
    {{
        public bool UseConstant = true;
        public {sigilTypeEntry.Alias} ConstantValue;
        public {capitalizedtypeAlias}Variable Variable;
    
        public {sigilTypeEntry.Alias} Value => UseConstant ? ConstantValue : Variable.value;
    }}
}}";

            string folderPath = Path.Combine(Application.dataPath, k_SigilArchitecturePath, "SigilReferences");
            string filePath = Path.Combine(folderPath, $"{className}.cs");
            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            File.WriteAllText(filePath, code);
        }

        private static void GenerateDrawerScript(SigilTypeEntry sigilTypeEntry)
        {
            string capitalizedtypeAlias = char.ToUpper(sigilTypeEntry.Alias[0]) + sigilTypeEntry.Alias[1..];
            string className = $"{capitalizedtypeAlias}ReferenceDrawer";

            string code = $@"using UnityEngine;
using UnityEditor;

namespace {nameof(Proselyte) + "." + nameof(Proselyte.Sigils)}
{{
    [CustomPropertyDrawer(typeof({capitalizedtypeAlias}Reference))]
    public class {className} : BaseReferenceDrawer
    {{
        protected override string ConstantFieldName => ""ConstantValue"";
        protected override string VariableFieldName => ""Variable"";
    }}
}}";

            string folderPath = Path.Combine(Application.dataPath, k_SigilArchitecturePath, "Editor/SigilTypeDrawers");
            string filePath = Path.Combine(folderPath, $"{className}.cs");
            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            File.WriteAllText(filePath, code);
            Debug.Log("Save EditorDrawer at: " + folderPath);
        }
    }
}
#endif
