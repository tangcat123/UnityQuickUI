using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IPanel : MonoBehaviour {

    private Dictionary<string, IElement> _Alias2Element = new Dictionary<string, IElement>();


    public Dictionary<string, IElement> GetAlias2Element()
    {
        return _Alias2Element;
    }

    protected virtual void Awake()
    {
        // 遍历element
        List<IElement> Elements = new List<IElement>();
        transform.GetComponentsInChildren<IElement>(Elements);

        IElement element = null;
        for(int i = 0; i < Elements.Count; i++)
        {
            element = Elements[i];
            AddUIElement(element);
        }
    }

    private void AddUIElement(IElement element)
    {
        IElementContext context = ((MaskableGraphic)element).GetComponent<IElementContext>();
        element.ElementContext = context;
        if(_Alias2Element.ContainsKey(element.Alias))
        {
            Debug.LogError(string.Format("重复定义{0}", element.Alias));
        }
        _Alias2Element[element.Alias] = element;
        element.OnInit();
    }

    protected T GetUI<T>(string alias) where T : IElement
    {
        if (!_Alias2Element.ContainsKey(alias))
        {
            Debug.LogError(string.Format("Alias:{0} 不存在", alias));
            return default(T);
        }

        return (T)(_Alias2Element[alias]);
    }
}
