#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Proselyte.Sigils
{
    internal class GameEventReferenceTracker : EditorWindow
    {
        private Dictionary<GameEvent, List<(Object source, GameEventUsageType usage)>> gameEventConnections = new();
        private Dictionary<GameEvent, bool> foldoutStates = new();
        private Vector2 scrollPosition;
        private bool autoRefresh;

        // Search functionality
        private string searchText = "";
        private List<GameEvent> searchSuggestions = new();
        private bool showSuggestions = false;
        private GameEvent selectedSearchEvent = null;
        private Vector2 suggestionsScrollPos;
        private const int maxSuggestions = 10;

        public enum GameEventUsageType
        {
            Listener,
            Reference,
            RaisesEvent,
            RegistersListener
        }

        [MenuItem("Tools/Sigils/Game Event Reference Tracker")]
        public static void ShowWindow()
        {
            GetWindow<GameEventReferenceTracker>("GameEvent Tracker");
        }

        private void OnEnable()
        {
            RefreshConnections();
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.projectChanged += OnProjectChanged;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        private void OnHierarchyChanged()
        {
            if(autoRefresh)
            {
                RefreshConnections();
            }
        }

        private void OnProjectChanged()
        {
            if(autoRefresh)
            {
                RefreshConnections();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // Header
            EditorGUILayout.LabelField("GameEvent Reference Tracker", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Controls
            EditorGUILayout.BeginHorizontal();
            autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);
            if(!autoRefresh)
            {
                if(GUILayout.Button("Refresh", GUILayout.Width(100)))
                {
                    RefreshConnections();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Show connection count
            int totalConnections = gameEventConnections.Values.Sum(list => list.Count);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Total GameEvents: {gameEventConnections.Count}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Total Connections: {totalConnections}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Search Bar
            DrawSearchBar();

            EditorGUILayout.Separator();

            // Scroll view for connections
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if(gameEventConnections.Count == 0)
            {
                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("No GameEvents found in project", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                DrawGameEventSections();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginVertical();

            // Search input field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));

            GUI.SetNextControlName("SearchField");
            string newSearchText = EditorGUILayout.TextField(searchText);

            // Clear button
            if(GUILayout.Button("✕", GUILayout.Width(20)))
            {
                newSearchText = "";
                selectedSearchEvent = null;
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();

            // Handle search text changes
            if(newSearchText != searchText)
            {
                searchText = newSearchText;
                UpdateSearchSuggestions();
            }

            // Show suggestions dropdown
            if(showSuggestions && searchSuggestions.Count > 0)
            {
                DrawSuggestionsDropdown();
            }

            EditorGUILayout.EndVertical();
        }

        private void UpdateSearchSuggestions()
        {
            searchSuggestions.Clear();
            showSuggestions = false;

            if(string.IsNullOrEmpty(searchText))
            {
                selectedSearchEvent = null;
                return;
            }

            // Find GameEvents that match the search text (prefix matching)
            var matches = gameEventConnections.Keys
                .Where(ge => ge != null && ge.name.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(ge => ge.name.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase)) // Prefer matches at the beginning
                .ThenBy(ge => ge.name)
                .Take(maxSuggestions)
                .ToList();

            searchSuggestions.AddRange(matches);
            showSuggestions = searchSuggestions.Count > 0;

            // Auto-select if there's an exact match
            var exactMatch = searchSuggestions.FirstOrDefault(ge =>
                string.Equals(ge.name, searchText, System.StringComparison.OrdinalIgnoreCase));
            if(exactMatch != null)
            {
                selectedSearchEvent = exactMatch;
                showSuggestions = false;
            }
        }

        private void DrawSuggestionsDropdown()
        {
            const float maxHeight = 150f;
            float itemHeight = EditorGUIUtility.singleLineHeight + 2f;
            float totalHeight = Mathf.Min(searchSuggestions.Count * itemHeight, maxHeight);

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(totalHeight));

            if(searchSuggestions.Count * itemHeight > maxHeight)
            {
                suggestionsScrollPos = EditorGUILayout.BeginScrollView(suggestionsScrollPos, GUILayout.Height(maxHeight));
            }

            for(int i = 0; i < searchSuggestions.Count; i++)
            {
                var gameEvent = searchSuggestions[i];
                if(gameEvent == null)
                    continue;

                EditorGUILayout.BeginHorizontal();

                // Highlight matching part of the name
                string displayName = gameEvent.name;
                int matchIndex = displayName.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase);

                if(GUILayout.Button(displayName, EditorStyles.miniButton, GUILayout.Height(itemHeight - 2f)))
                {
                    SelectGameEvent(gameEvent);
                }

                // Show connection count
                int connectionCount = gameEventConnections[gameEvent].Count(e => e.usage == GameEventUsageType.Listener);
                EditorGUILayout.LabelField($"({connectionCount})", EditorStyles.miniLabel, GUILayout.Width(30));

                EditorGUILayout.EndHorizontal();
            }

            if(searchSuggestions.Count * itemHeight > maxHeight)
            {
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void SelectGameEvent(GameEvent gameEvent)
        {
            selectedSearchEvent = gameEvent;
            searchText = gameEvent.name;
            showSuggestions = false;

            // Ensure the selected event's foldout is open
            if(!foldoutStates.ContainsKey(gameEvent))
            {
                foldoutStates[gameEvent] = true;
            }
            else
            {
                foldoutStates[gameEvent] = true;
            }

            GUI.FocusControl(null);
        }

        private void DrawGameEventSections()
        {
            var gameEventsToDisplay = GetOrderedGameEvents();

            foreach(var gameEvent in gameEventsToDisplay)
            {
                if(gameEventConnections.ContainsKey(gameEvent))
                {
                    DrawGameEventSection(gameEvent, gameEventConnections[gameEvent]);
                }
            }
        }

        private List<GameEvent> GetOrderedGameEvents()
        {
            var allGameEvents = gameEventConnections.Keys.Where(ge => ge != null).ToList();

            // If there's a selected search event, show it first
            if(selectedSearchEvent != null && gameEventConnections.ContainsKey(selectedSearchEvent))
            {
                var ordered = new List<GameEvent> { selectedSearchEvent };
                ordered.AddRange(allGameEvents.Where(ge => ge != selectedSearchEvent).OrderBy(ge => ge.name));
                return ordered;
            }

            // Otherwise, show all in alphabetical order
            return allGameEvents.OrderBy(ge => ge.name).ToList();
        }

        private void DrawGameEventSection(GameEvent gameEvent, List<(Object source, GameEventUsageType usage)> entries)
        {
            if(gameEvent == null)
                return;

            // Ensure foldout state exists
            if(!foldoutStates.ContainsKey(gameEvent))
            {
                foldoutStates[gameEvent] = false;
            }

            // Highlight selected search result
            Color originalColor = GUI.backgroundColor;
            if(selectedSearchEvent == gameEvent)
            {
                GUI.backgroundColor = Color.yellow * 0.7f;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = originalColor;

            // Count usage types
            int listenerCount = entries.Count(e => e.usage == GameEventUsageType.Listener);

            // GameEvent header with foldout
            EditorGUILayout.BeginHorizontal();
            foldoutStates[gameEvent] = EditorGUILayout.Foldout(
                foldoutStates[gameEvent],
                $"{gameEvent.name} ({listenerCount} listeners)",
                true
            );

            // Button to select the GameEvent asset
            if(GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = gameEvent;
                EditorGUIUtility.PingObject(gameEvent);
            }

            EditorGUILayout.EndHorizontal();

            // Show listeners if foldout is open
            if(foldoutStates[gameEvent])
            {
                EditorGUI.indentLevel++;

                var listenerEntries = entries
                    .Where(e => e.usage == GameEventUsageType.Listener)
                    .Select(e => e.source as GameEventListener)
                    .Where(l => l != null)
                    .OrderBy(l => l.gameObject.name);

                if(!listenerEntries.Any())
                {
                    EditorGUILayout.LabelField("No listeners found", EditorStyles.miniLabel);
                }
                else
                {
                    foreach(var listener in listenerEntries)
                    {
                        DrawListenerRow(listener, gameEvent);
                    }
                }

                // Other entries
                foreach(var entry in entries.Where(e => e.usage != GameEventUsageType.Listener))
                {
                    EditorGUILayout.BeginHorizontal();

                    string label = entry.source.name;
                    string usage = entry.usage.ToString();

                    EditorGUILayout.ObjectField($"{label} — {usage}", entry.source, typeof(UnityEngine.Object), true);

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void DrawListenerRow(GameEventListener listener, GameEvent gameEvent)
        {
            EditorGUILayout.BeginHorizontal();

            // GameObject icon (smaller)
            GUIContent content = new GUIContent(listener.gameObject.name, EditorGUIUtility.ObjectContent(listener.gameObject, typeof(GameObject)).image);

            // Clickable label to select the GameObject with smaller height
            if(GUILayout.Button(content, EditorStyles.miniLabel, GUILayout.Height(16)))
            {
                Selection.activeGameObject = listener.gameObject;
                EditorGUIUtility.PingObject(listener.gameObject);
            }

            GUILayout.FlexibleSpace();

            // Show scene info
            var scene = listener.gameObject.scene;
            if(scene.IsValid())
            {
                EditorGUILayout.LabelField($"({scene.name})", EditorStyles.miniLabel, GUILayout.Width(100));
            }
            else
            {
                EditorGUILayout.LabelField("(Prefab)", EditorStyles.miniLabel, GUILayout.Width(100));
            }

            // Show number of responses for this specific GameEvent
            int responseCount = listener.eventResponses?.Count(er => er.gameEvent == gameEvent) ?? 0;
            EditorGUILayout.LabelField($"{responseCount} response(s)", EditorStyles.miniLabel, GUILayout.Width(80));

            EditorGUILayout.EndHorizontal();
        }

        private void RefreshConnections()
        {
            gameEventConnections.Clear();

            // Get all GameEvents
            var allGameEventGuids = AssetDatabase.FindAssets("t:GameEvent");
            var allGameEvents = allGameEventGuids
                .Select(guid => AssetDatabase.LoadAssetAtPath<GameEvent>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(ge => ge != null)
                .ToList();

            // Initialize map
            foreach(var gameEvent in allGameEvents)
                gameEventConnections[gameEvent] = new();

            // Find all scripts in scene and prefabs
            var monoBehaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();

            foreach(var mb in monoBehaviours)
            {
                if(mb == null)
                    continue;
                var type = mb.GetType();
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach(var field in fields)
                {
                    if(field.FieldType != typeof(GameEvent))
                        continue;

                    var gameEvent = field.GetValue(mb) as GameEvent;
                    if(gameEvent == null)
                        continue;

                    gameEventConnections.TryAdd(gameEvent, new());
                    gameEventConnections[gameEvent].Add((mb, GameEventUsageType.Reference));
                }
            }

            // Detect GameEventListener usage
            var allListeners = monoBehaviours.OfType<GameEventListener>();
            foreach(var listener in allListeners)
            {
                foreach(var response in listener.eventResponses)
                {
                    if(response?.gameEvent == null)
                        continue;
                    gameEventConnections.TryAdd(response.gameEvent, new());
                    gameEventConnections[response.gameEvent].Add((listener, GameEventUsageType.Listener));
                }
            }

            // Naïve Raise() + RegisterListener() detection
            var scriptPaths = AssetDatabase.FindAssets("t:MonoScript")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .ToList();

            foreach(var path in scriptPaths)
            {
                var text = System.IO.File.ReadAllText(path);

                foreach(var gameEvent in allGameEvents)
                {
                    if(gameEvent == null || string.IsNullOrEmpty(gameEvent.name))
                        continue;

                    if(text.Contains($"{gameEvent.name}.Raise("))
                        gameEventConnections[gameEvent].Add((AssetDatabase.LoadAssetAtPath<MonoScript>(path), GameEventUsageType.RaisesEvent));

                    if(text.Contains($"{gameEvent.name}.RegisterListener("))
                        gameEventConnections[gameEvent].Add((AssetDatabase.LoadAssetAtPath<MonoScript>(path), GameEventUsageType.RegistersListener));
                }
            }

            // Foldout cleanup
            var keysToRemove = foldoutStates.Keys.Where(k => !gameEventConnections.ContainsKey(k)).ToList();
            foreach(var key in keysToRemove)
                foldoutStates.Remove(key);

            // Update search suggestions if search is active
            if(!string.IsNullOrEmpty(searchText))
            {
                UpdateSearchSuggestions();
            }
        }
    }
}
#endif
