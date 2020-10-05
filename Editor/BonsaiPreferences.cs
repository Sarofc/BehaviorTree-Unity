using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bonsai.Designer
{
    // Register a SettingsProvider using IMGUI for the drawing framework:
    static class BeahviorTreeSettingsIMGUIRegister
    {
        [SettingsProvider]
        private static SettingsProvider CreateBehaviorTreeSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("Preferences/BehaviorTree", SettingsScope.User)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "Behavior Tree",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = BonsaiPreferences.GetSerializedSettings();
                    var editor = Editor.CreateEditor(BonsaiPreferences.Instance);
                    editor.OnInspectorGUI();
                    //EditorGUILayout.PropertyField(settings.FindProperty("m_Number"), new GUIContent("My Number"));
                    //EditorGUILayout.PropertyField(settings.FindProperty("m_SomeString"), new GUIContent("My String"));
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Behavior Tree" })
            };

            return provider;
        }
    }


    [CreateAssetMenu(fileName = "BonsaiPreferences", menuName = "Bonsai/Preferences")]
    public class BonsaiPreferences : ScriptableObject
    {
        private const string k_PreferencePath = "Assets/Editor/BehaviorTree/Resources/";

        // The unit length of the grid in pixels.
        // Note: Grid Texture has 12.8 as length, fix texture to be even.
        private const int kGridSize = 12;

        [Header("Editor")]
        public int snapStep = kGridSize;
        public float zoomDelta = 0.2f;

        [Min(0.1f)]
        public float minZoom = 1f;

        public float maxZoom = 5f;
        public float panSpeed = 1.2f;

        [Space()]
        [SerializeField] private Texture2D m_GridTexture;
        [SerializeField] private Texture2D m_FailureSymbol;
        [SerializeField] private Texture2D m_SuccessSymbol;
        [SerializeField] private Texture2D m_BreakPoint;

        [Header("Node Textures")]
        [SerializeField] private Texture2D m_NodeBackgroundTexture;
        [SerializeField] private Texture2D m_NodeGradient;
        [SerializeField] private Texture2D m_PortTexture;


        public Texture2D gridTexture
        {
            get
            {
                if (m_GridTexture == null) m_GridTexture = Texture("Editor_Grid");
                return m_GridTexture;
            }
        }
        public Texture2D failureSymbol
        {
            get
            {
                if (m_FailureSymbol == null) m_FailureSymbol = Texture("Editor_SmallCross");
                return m_FailureSymbol;
            }
        }
        public Texture2D successSymbol
        {
            get
            {
                if (m_SuccessSymbol == null) m_SuccessSymbol = Texture("Editor_SmallCheckmark");
                return m_SuccessSymbol;
            }
        }
        public Texture2D breakPoint
        {
            get
            {
                if (m_BreakPoint == null) m_BreakPoint = Texture("Editor_Dot");
                return m_BreakPoint;
            }
        }

        public Texture2D nodeBackgroundTexture
        {
            get
            {
                if (m_NodeBackgroundTexture == null) m_NodeBackgroundTexture = Texture("Editor_PlainTexture");
                return m_NodeBackgroundTexture;
            }
        }
        public Texture2D nodeGradient
        {
            get
            {
                if (m_NodeGradient == null) m_NodeGradient = Texture("Editor_Gradient");
                return m_NodeGradient;
            }
        }
        public Texture2D portTexture
        {
            get
            {
                if (m_PortTexture == null) m_PortTexture = Texture("Editor_DarkTexture");
                return m_PortTexture;
            }
        }


        [Header("Node Colors")]
        public Color compositeColor = new Color(0.37f, 0.37f, 0.37f);
        public Color decoratorColor = new Color(0.45f, 0.38f, 0.72f);
        public Color conditionalColor = new Color(0.36f, 0.72f, 0.72f);
        public Color serviceColor = new Color(0.99f, 0.61f, 0.13f);
        public Color taskColor = new Color(0.59f, 0.64f, 0.53f);

        [Header("Status Colors")]
        public Color defaultNodeBackgroundColor = new Color(0.29f, 0.29f, 0.29f);
        public Color selectedColor = new Color(0.34f, 0.62f, 1.0f);
        public Color runningColor = new Color(0.29f, 1.0f, 0.61f);
        public Color abortColor = new Color(0.37f, 0.88f, 0.77f);
        public Color referenceColor = new Color(1.0f, 0.77f, 0.03f);
        public Color evaluateColor = new Color(0.0f, 0.43f, 1.0f);
        public Color rootSymbolColor = new Color(0.90f, 1.0f, 0.0f);

        [Header("Runtime Colors")]
        public Color runningStatusColor = new Color(0.1f, 1f, 0.54f, 1f);
        public Color successColor = new Color(0.1f, 1f, 0.54f, 0.25f);
        public Color failureColor = new Color(1f, 0.1f, 0.1f, 0.25f);
        public Color abortedColor = new Color(0.1f, 0.1f, 1f, 0.25f);
        public Color interruptedColor = new Color(0.7f, 0.5f, 0.3f, 0.4f);
        public Color defaultConnectionColor = Color.white;

        [Header("Connection Lines")]
        public float defaultConnectionWidth = 4f;
        public float runningConnectionWidth = 4f;

        [Header("Node Body Layout")]
        [Tooltip("Controls additional node size.")]
        public Vector2 nodeSizePadding = new Vector2(12f, 6f);

        [Tooltip("Controls the thickness of left and right edges.")]
        public float nodeWidthPadding = 12f;

        [Tooltip("Controls how thick the ports are. Changes the nodes overall height too.")]
        public float portHeight = 20f;

        [Tooltip("Control how far the ports extend in the node.")]
        public float portWidthTrim = 50f;

        public float iconSize = 32f;
        public float statusIconSize = 16f;

        private static BonsaiPreferences instance = null;

        public static BonsaiPreferences Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = LoadDefaultPreferences();
                }
                return instance;
            }
        }

        public static BonsaiPreferences LoadDefaultPreferences()
        {
            var prefs = Resources.Load<BonsaiPreferences>("DefaultBonsaiPreferences");

            if (prefs == null)
            {
                prefs = CreateInstance<BonsaiPreferences>();

                if (!Directory.Exists(k_PreferencePath))
                {
                    Directory.CreateDirectory(k_PreferencePath);
                }

                AssetDatabase.CreateAsset(prefs, k_PreferencePath + "DefaultBonsaiPreferences.asset");
                AssetDatabase.Refresh();
                Debug.LogWarning("Failed to load DefaultBonsaiPreferences. Create new one.");
            }

            return prefs;
        }

        public static Texture2D Texture(string name)
        {
            return Resources.Load<Texture2D>(name);
        }

        public static SerializedObject GetSerializedSettings()
        {
            instance = LoadDefaultPreferences();
            return new SerializedObject(Instance);
        }
    }
}
