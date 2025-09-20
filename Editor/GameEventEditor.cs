using UnityEditor;
using UnityEngine;

namespace Proselyte.Sigils
{
    [CustomEditor(typeof(GameEvent))]
    internal class GameEventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GameEvent gameEvent = (GameEvent)target;
            if(GUILayout.Button("Raise Event"))
            {
                gameEvent.Raise();
            }
        }
    }
}