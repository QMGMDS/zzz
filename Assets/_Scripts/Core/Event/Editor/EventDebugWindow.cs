using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Core.Event.Editor
{
    /// <summary>
    /// 事件通道调试窗口，在 Play Mode 下实时展示所有活跃事件通道的订阅数和触发次数
    /// </summary>
    public class EventDebugWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private readonly List<EventChannelSO> _channelsCache = new List<EventChannelSO>();
        private bool _isPlaying;

        [MenuItem("Window/Event Debugger")]
        private static void Open()
        {
            EventDebugWindow window = GetWindow<EventDebugWindow>("Event Debugger");
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            _isPlaying = EditorApplication.isPlaying;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (EditorApplication.isPlaying)
            {
                if (!_isPlaying)
                {
                    _isPlaying = true;
                }

                Repaint();
            }
            else if (_isPlaying)
            {
                _isPlaying = false;
                Repaint();
            }
        }

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("请进入 Play Mode 以查看事件通道状态", MessageType.Info);
                return;
            }

            DrawToolbar();
            EditorGUILayout.Space();

            _channelsCache.Clear();
            _channelsCache.AddRange(EventChannelRegistry.GetAll());

            DrawTableHeader();
            DrawChannelList();
            DrawFooter();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label($"活跃通道: {EventChannelRegistry.GetAll().Count()}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("重置统计", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                foreach (EventChannelSO channel in EventChannelRegistry.GetAll())
                {
                    channel.ResetStats();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTableHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("通道名", EditorStyles.boldLabel, GUILayout.Width(200));
            EditorGUILayout.LabelField("订阅数", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.LabelField("触发数", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            Rect separatorRect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(separatorRect, EditorGUIUtility.isProSkin
                ? new Color(0.3f, 0.3f, 0.3f)
                : new Color(0.7f, 0.7f, 0.7f));
        }

        private void DrawChannelList()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_channelsCache.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无活跃事件通道", MessageType.Warning);
            }
            else
            {
                foreach (EventChannelSO channel in _channelsCache)
                {
                    DrawChannelRow(channel);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawChannelRow(EventChannelSO channel)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.ObjectField(channel, typeof(EventChannelSO), false, GUILayout.Width(200));
            EditorGUILayout.LabelField(channel.SubscriberCount.ToString(), GUILayout.Width(60));
            EditorGUILayout.LabelField(channel.RaiseCount.ToString(), GUILayout.Width(80));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space();

            Rect separatorRect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(separatorRect, EditorGUIUtility.isProSkin
                ? new Color(0.3f, 0.3f, 0.3f)
                : new Color(0.7f, 0.7f, 0.7f));

            EditorGUILayout.LabelField(
                $"共 {_channelsCache.Count} 个通道，总触发 {GetTotalRaiseCount()} 次",
                EditorStyles.miniLabel);
        }

        private int GetTotalRaiseCount()
        {
            int total = 0;
            foreach (EventChannelSO channel in _channelsCache)
            {
                total += channel.RaiseCount;
            }

            return total;
        }
    }
}
