using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Entitas.Unity.Editor
{
    static class EditorLayout
    {
        const int DefaultFoldoutMargin = 11;

        static GUIStyle _sectionHeader;
        static GUIStyle _sectionContent;
        static GUIStyle _toolbarSearchTextField;
        static GUIStyle _toolbarSearchCancelButton;

        static GUIStyle SectionHeader => _sectionHeader ??= new GUIStyle("OL Title");

        static GUIStyle SectionContent => _sectionContent ??= new GUIStyle("OL Box") { stretchHeight = false };

        static GUIStyle ToolbarSearchTextField =>
            _toolbarSearchTextField ??= GUI.skin.FindStyle("ToolbarSearchTextField") ?? GUI.skin.textField;

        static GUIStyle ToolbarSearchCancelButton =>
            _toolbarSearchCancelButton ??= GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? GUI.skin.button;

        public static bool DrawSectionHeaderToggle(string header, bool value) =>
            GUILayout.Toggle(value, header, SectionHeader, Array.Empty<GUILayoutOption>());

        public static void BeginSectionContent() =>
            EditorGUILayout.BeginVertical(SectionContent, Array.Empty<GUILayoutOption>());

        public static void EndSectionContent() => EditorGUILayout.EndVertical();

        public static Rect BeginVerticalBox() =>
            EditorGUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());

        public static void EndVerticalBox() => EditorGUILayout.EndVertical();

        /// Finds an asset by label (e.g. "l:EntitasContextHierarchyIcon") and loads it as a texture.
        public static Texture2D LoadTexture(string label)
        {
            var guid = AssetDatabase.FindAssets(label).FirstOrDefault();
            return guid != null
                ? AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid))
                : null;
        }

        public static bool MiniButton(string c) => MiniButton(c, EditorStyles.miniButton);

        public static bool MiniButtonLeft(string c) => MiniButton(c, EditorStyles.miniButtonLeft);

        public static bool MiniButtonMid(string c) => MiniButton(c, EditorStyles.miniButtonMid);

        public static bool MiniButtonRight(string c) => MiniButton(c, EditorStyles.miniButtonRight);

        static bool MiniButton(string c, GUIStyle style)
        {
            var options = c.Length == 1
                ? new[] { GUILayout.Width(19) }
                : Array.Empty<GUILayoutOption>();

            var clicked = GUILayout.Button(c, style, options);
            if (clicked)
                GUI.FocusControl(null);

            return clicked;
        }

        public static bool Foldout(bool foldout, string content, int leftMargin = DefaultFoldoutMargin) =>
            Foldout(foldout, content, EditorStyles.foldout, leftMargin);

        public static bool Foldout(bool foldout, string content, GUIStyle style, int leftMargin = DefaultFoldoutMargin)
        {
            EditorGUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayout.Space(leftMargin);
            foldout = EditorGUILayout.Foldout(foldout, content, style);
            EditorGUILayout.EndHorizontal();
            return foldout;
        }

        public static string SearchTextField(string searchString)
        {
            var guiChanged = GUI.changed;
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            searchString = GUILayout.TextField(searchString, ToolbarSearchTextField, Array.Empty<GUILayoutOption>());
            if (GUILayout.Button(string.Empty, ToolbarSearchCancelButton, Array.Empty<GUILayoutOption>()))
                searchString = string.Empty;

            GUILayout.EndHorizontal();
            GUI.changed = guiChanged;
            return searchString;
        }

        /// True if 'str' contains any of the space separated words of 'search' (or if 'search' is empty).
        public static bool MatchesSearchString(string str, string search)
        {
            var words = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length == 0 || words.Any(str.Contains);
        }
    }
}
