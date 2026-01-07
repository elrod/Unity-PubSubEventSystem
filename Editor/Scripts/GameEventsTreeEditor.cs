using System.Linq;
using com.elrod.pubsubeventsystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.elrod.pubsubeventssystem.editor
{
    public class GameEventsTreeEditor : EditorWindow
    {
        private const int INITIALIZATION_MAXIMUM_LEAVES = 10;

        private bool _exitingPlayMode;
        private GameEventTreeView _treeView;
        private bool _eventsManagerAvailable;

        
        [MenuItem("Tools/Elrod/Events Tree")]
        private static void OpenEventsManagerEditorWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = (GameEventsTreeEditor) GetWindow(typeof(GameEventsTreeEditor));
            window.titleContent = new GUIContent("Pub/Sub events visualizer");
        }
        
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();
        }
        
        private void EditorApplication_playModeStateChanged(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.ExitingPlayMode)
            {
                if (_eventsManagerAvailable)
                {
                    GameEventSystem.Instance.EventsTree.OnTreeChanged -= EventsTree_OnTreeChanged;
                    rootVisualElement.Clear();
                    _eventsManagerAvailable = false;
                    _exitingPlayMode = true;
                }
            }

            if (stateChange == PlayModeStateChange.EnteredPlayMode)
            {
                _exitingPlayMode = false;
            }
                
        }
        
        private void OnGUI()
        {
            if (!Application.isPlaying || _exitingPlayMode) return;
            UpdateEventsManager();
        }
        
        private void UpdateEventsManager()
        {
            if (!_eventsManagerAvailable)
            {
                if (GameEventSystem.Instance != null)
                {
                    GameEventSystem.Instance.EventsTree.OnTreeChanged += EventsTree_OnTreeChanged;
                    // Initial Draw
                    EventsTree_OnTreeChanged();
                    _eventsManagerAvailable = true;
                }
            }
        }
        
        private void EventsTree_OnTreeChanged()
        {
            rootVisualElement.Clear();
            ConstructTree(GameEventSystem.Instance.EventsTree, false);
        }

        private void ConstructTree(GameEventTree eventsTree, bool interactable = true)
        {
            _treeView = new GameEventTreeView(eventsTree, interactable)
            {
                name = "Events Tree"
            };
            _treeView.StretchToParentSize();
            rootVisualElement.Add(_treeView);
        }
    }
}

