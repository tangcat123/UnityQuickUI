using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(IPanel), true)]
public class PanelEditor : Editor
{
    IPanel panel;
    Dictionary<string, IElement> _Alias2Element = new Dictionary<string, IElement>();
    private void OnEnable()
    {
        panel = target as IPanel;
        _Alias2Element = panel.GetAlias2Element();
    }

    // 可以在这里显示Atlas列表,方便定位
    public override void OnInspectorGUI()
    {
        foreach(var item in _Alias2Element)
        {
            GUILayout.BeginHorizontal();
            GUILayout.TextField(item.Key);
            EditorGUILayout.ObjectField("", ((MaskableGraphic)item.Value).gameObject, typeof(GameObject), true);
            GUILayout.EndHorizontal();
        }
        
    }
}
