using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Entitas.Unity.Editor
{
    /// Line chart used by the DebugSystems inspector to show the execution duration over time.
    class Graph
    {
        const float LabelWidth = 48f;
        const float Padding = 4f;
        const int GridLineCount = 4;

        static readonly Color GridColor = new Color(1f, 1f, 1f, 0.15f);
        static readonly Color LineColor = Color.red;
        static readonly Color AverageColor = Color.yellow;

        readonly int _length;
        readonly Vector3[] _points;
        GUIStyle _labelStyle;

        public Graph(int length)
        {
            _length = length;
            _points = new Vector3[length];
        }

        public void Draw(float[] data, float height)
        {
            var rect = GUILayoutUtility.GetRect(EditorGUILayout.GetControlRect().width, height);
            if (Event.current.type != EventType.Repaint || data.Length == 0)
                return;

            _labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 9
            };

            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.2f));

            var max = Mathf.Max(data.Max(), 0.001f);
            var plot = new Rect(rect.x + LabelWidth, rect.y + Padding, rect.width - LabelWidth - Padding, rect.height - Padding * 2f);

            DrawGrid(plot, max);
            DrawAverage(plot, data, max);
            DrawLine(plot, data, max);
        }

        void DrawGrid(Rect plot, float max)
        {
            var previousColor = Handles.color;
            Handles.color = GridColor;
            for (var i = 0; i <= GridLineCount; i++)
            {
                var t = i / (float)GridLineCount;
                var y = Mathf.Lerp(plot.yMax, plot.yMin, t);
                Handles.DrawLine(new Vector3(plot.xMin, y), new Vector3(plot.xMax, y));

                var labelRect = new Rect(plot.xMin - LabelWidth, y - 8f, LabelWidth - 2f, 16f);
                GUI.Label(labelRect, (max * t).ToString("0.0"), _labelStyle);
            }

            Handles.color = previousColor;
        }

        void DrawAverage(Rect plot, float[] data, float max)
        {
            var y = Mathf.Lerp(plot.yMax, plot.yMin, data.Average() / max);
            var previousColor = Handles.color;
            Handles.color = AverageColor;
            Handles.DrawLine(new Vector3(plot.xMin, y), new Vector3(plot.xMax, y));
            Handles.color = previousColor;
        }

        void DrawLine(Rect plot, float[] data, float max)
        {
            var count = Mathf.Min(data.Length, _length);
            var step = plot.width / Mathf.Max(count - 1, 1);
            for (var i = 0; i < count; i++)
            {
                var x = plot.xMin + step * i;
                var y = Mathf.Lerp(plot.yMax, plot.yMin, data[i] / max);
                _points[i] = new Vector3(x, y);
            }

            var previousColor = Handles.color;
            Handles.color = LineColor;
            Handles.DrawAAPolyLine(2f, count, _points);
            Handles.color = previousColor;
        }
    }
}
