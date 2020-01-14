using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

public class UIBatchEditorWindow : EditorWindow
{

    [MenuItem("Tools/UI/UIBatchAnalysis", priority = 400)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(UIBatchEditorWindow));

        EditorApplication.hierarchyWindowItemOnGUI -= HierarchWindowOnGui;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchWindowOnGui;
    }


    bool[] showBatchGroups = new bool[200];
    Vector2 scrollPosition = new Vector2(0, 0);
    void OnGUI()
    {
        titleContent.text = "UIBatch";

        GUILayout.BeginVertical();

        GUILayout.Label("请选择UI根节点进行分析:");
        if (GUILayout.Button("Analysis"))
        {
            Analysis();
        }

        // 画出每个batch
        int batchGroupIndex = 0;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach (var item in Index2BatchGroup)
        {
            int batchIndex = item.Key;
            OneBatchGroup group = item.Value;
            GUILayout.Label("Batch:" + batchIndex);
            //showBatchGroups[batchGroupIndex] = EditorGUILayout.Foldout(showBatchGroups[batchGroupIndex], "Batch:" + batchIndex);
            //if(showBatchGroups[batchGroupIndex])
            {
                for (int i = 0; i < group.BatchElements.Count; i++)
                {
                    GameObject obj = group.BatchElements[i].UIReference.gameObject;
                    EditorGUILayout.ObjectField("", obj, typeof(GameObject), true);
                }
            }

            batchGroupIndex++;
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }


    static Dictionary<int, UIBatchElement> Obj2BatchElement = new Dictionary<int, UIBatchElement>();
    List<UIBatchElement> batchElements = new List<UIBatchElement>();
    Dictionary<int, OneBatchGroup> Index2BatchGroup = new Dictionary<int, OneBatchGroup>();

    static void HierarchWindowOnGui(int instanceId, Rect selectionRect)
    {
        if (Obj2BatchElement.ContainsKey(instanceId))
        {
            UIBatchElement batchElement = Obj2BatchElement[instanceId];
            var obj = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (obj != null)
            {
                if (obj.GetComponent<MaskableGraphic>() != null)
                {
                    var style = new GUIStyle();
                    style.normal.textColor = Color.yellow;
                    style.hover.textColor = Color.green;
                    Rect r = new Rect(selectionRect);
                    r.x = r.width - 80;
                    r.width = 80;
                    GUI.Label(r, string.Format("[B:{0}][D:{1}]", batchElement.BatchIndex, batchElement.Depth), style);
                }
            }
        }

    }

    void Analysis()
    {
        Obj2BatchElement.Clear();
        Index2BatchGroup.Clear();

        GameObject selectObj = Selection.activeGameObject;
        MaskableGraphic[] uiElements = selectObj.GetComponentsInChildren<MaskableGraphic>(true);

        batchElements.Clear();
        // 根据renderdepth初始化并计算depth
        for (int renderDepth = 0; renderDepth < uiElements.Length; renderDepth++)
        {
            MaskableGraphic uiElement = uiElements[renderDepth];

            if (!uiElement.gameObject.activeInHierarchy) continue;

            UIBatchElement uiBatchElement = new UIBatchElement();
            uiBatchElement.UIReference = uiElement;
            uiBatchElement.RenderDepth = renderDepth;
            batchElements.Add(uiBatchElement);


            CalculateDepth(uiBatchElement, batchElements.GetRange(0, batchElements.Count - 1));
        }

        // 排序
        batchElements.Sort();

        // Batch
        int batchIndex = 0;
        UIBatchElement preBatchElement = null;
        for (int i = 0; i < batchElements.Count; i++)
        {
            UIBatchElement batchElement = batchElements[i];
            if (preBatchElement == null)
            {
                preBatchElement = batchElement;
                batchElement.BatchIndex = batchIndex;
                TryAddToGroup(batchElement);
                continue;
            }
            if (batchElement.Depth != preBatchElement.Depth ||
                batchElement.GetMaterialId() != preBatchElement.GetMaterialId() ||
                batchElement.GetTextureId() != preBatchElement.GetTextureId())
            {
                batchIndex++;
                preBatchElement = batchElement;
                batchElement.BatchIndex = batchIndex;
                TryAddToGroup(batchElement);
                continue;
            }
            preBatchElement = batchElement;
            batchElement.BatchIndex = batchIndex;
            TryAddToGroup(batchElement);

        }

        // Hierachy
        for (int i = 0; i < batchElements.Count; i++)
        {
            UIBatchElement batchElement = batchElements[i];
            Obj2BatchElement[batchElement.UIReference.gameObject.GetInstanceID()] = batchElement;
        }

        //EditorUtility.SetDirty();
    }

    void CalculateDepth(UIBatchElement curBatchElement, List<UIBatchElement> batchElements)
    {
        if (batchElements.Count == 0)
        {
            curBatchElement.Depth = 0;
        }
        else
        {
            int maxDepth = -1;
            // 找到下方最顶层depth元素列表
            List<UIBatchElement> topElements = new List<UIBatchElement>();
            for (int i = 0; i < batchElements.Count; i++)
            {
                UIBatchElement batchElement = batchElements[i];
                if (curBatchElement.GetRectTransform().rectOverlaps(batchElement.GetRectTransform()) && batchElement.Depth >= maxDepth)
                {
                    if (curBatchElement.Depth != maxDepth)
                    {
                        topElements.Clear();
                    }

                    maxDepth = batchElement.Depth;
                    topElements.Add(batchElement);
                }
            }

            // check break batch
            bool breakBatch = false;
            for (int index = 0; index < topElements.Count; index++)
            {
                bool isBreak = BreaksBatch(topElements[index], curBatchElement);
                breakBatch = breakBatch || isBreak;
            }

            if (topElements.Count == 0 || breakBatch)
            {
                ++maxDepth;
            }

            curBatchElement.Depth = maxDepth;
        }

    }

    // 先简化下
    bool BreaksBatch(UIBatchElement lfs, UIBatchElement rhs)
    {
        return (lfs.GetMaterialId() != rhs.GetMaterialId()) ||
            (lfs.GetTextureId() != rhs.GetTextureId());
    }

    public void TryAddToGroup(UIBatchElement batchElement)
    {
        if (Index2BatchGroup.ContainsKey(batchElement.BatchIndex))
        {
            Index2BatchGroup[batchElement.BatchIndex].Add(batchElement);
        }
        else
        {
            OneBatchGroup oneBatchGroup = new OneBatchGroup(batchElement.BatchIndex);
            oneBatchGroup.Add(batchElement);
            Index2BatchGroup[batchElement.BatchIndex] = oneBatchGroup;

        }
    }

}
















public static class OverlapExtensionMethod
{
    public static Rect GetWorldRect(this RectTransform rectTrans)
    {
        //左下、左上、右上、右下 
        Vector3[] corners = new Vector3[4];
        rectTrans.GetWorldCorners(corners);
        Vector3 bottomLeft = corners[3];
        Canvas canvas = rectTrans.GetComponentInParent<Canvas>();

        Vector2 pos = new Vector2();

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Camera.main.WorldToScreenPoint(bottomLeft), null, out pos);
            CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
            return new Rect(pos / canvasScaler.referencePixelsPerUnit, new Vector2(rectTrans.rect.width, rectTrans.rect.height));
            
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, canvas.worldCamera.WorldToScreenPoint(bottomLeft), canvas.worldCamera, out pos);
            return new Rect(pos, new Vector2(rectTrans.rect.width, rectTrans.rect.height));
        }

        
    }


    public static bool rectOverlaps(this RectTransform rectTrans1, RectTransform rectTrans2)
    {
        //Rect rect1 = new Rect(rectTrans1.anchoredPosition.x + rectTrans1.rect.x, rectTrans1.anchoredPosition.y + rectTrans1.rect.y, rectTrans1.rect.width, rectTrans1.rect.height);
        //Rect rect2 = new Rect(rectTrans2.anchoredPosition.x + rectTrans2.rect.x, rectTrans2.anchoredPosition.y + rectTrans2.rect.y, rectTrans2.rect.width, rectTrans2.rect.height);
        Rect rect1 = rectTrans1.GetWorldRect();
        Rect rect2 = rectTrans2.GetWorldRect();
        return rect1.Overlaps(rect2);
    }
}



public class UIBatchElement : IComparable<UIBatchElement>
{
    public int RenderDepth { get; set; }
    public int Depth { get; set; }
    public int BatchIndex { get; set; }

    public MaskableGraphic UIReference { get; set; }

    public RectTransform GetRectTransform()
    {
        return UIReference.GetComponent<RectTransform>();
    }

    public override string ToString()
    {
        return string.Format("UIReference:{0},Depth:{1},BatchIndex:{2}", UIReference, Depth, BatchIndex);
    }

    public int GetMaterialId()
    {
        return UIReference.material.GetInstanceID();
    }

    public int GetTextureId()
    {
        return UIReference.mainTexture.GetInstanceID();
    }

    public int CompareTo(UIBatchElement other)
    {
        if (Depth != other.Depth)
        {
            return Depth.CompareTo(other.Depth);
        }
        if (GetTextureId() != other.GetTextureId())
        {
            return GetTextureId().CompareTo(other.GetTextureId());
        }
        if (GetMaterialId() != other.GetMaterialId())
        {
            return GetMaterialId().CompareTo(other.GetMaterialId());
        }
        if (RenderDepth != other.RenderDepth)
        {
            return RenderDepth.CompareTo(other.RenderDepth);
        }
        return 0;
    }
}

public class OneBatchGroup
{
    public int BatchIndex { get; set; }

    private List<UIBatchElement> _BatchElements = new List<UIBatchElement>();

    public List<UIBatchElement> BatchElements
    {
        get
        {
            return _BatchElements;
        }
    }


    public OneBatchGroup(int index)
    {
        BatchIndex = index;
    }

    public void Add(UIBatchElement batchElement)
    {
        _BatchElements.Add(batchElement);
    }
}