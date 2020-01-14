using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class IContainerContext : IElementContext
{
    [ShowInInspector]
    public Dictionary<string, IElement> _Alias2Element = new Dictionary<string, IElement>();

    public Dictionary<string, IElement> GetAlias2Element()
    {
        return _Alias2Element;
    }

    [Button(ButtonSizes.Large, ButtonStyle.Box)]
    public void CollectInfo()
    {
        _Alias2Element.Clear();
        // 遍历element
        List<IElement> Elements = new List<IElement>();

        IContainer container = transform.GetComponent<IContainer>();
        container.ElementContext = this;

        _Recursive(Elements, transform);
    }

    private void _Recursive(List<IElement> Elements, Transform parent)
    {
        Transform childTran;
        for (int i = 0; i < parent.childCount; i++)
        {
            childTran = parent.GetChild(i);
            IElement element = childTran.GetComponent<IElement>();
            if (element != null)
            {
                AddUIElement(element);
                if(element is IContainer)
                {
                    continue;
                }
                _Recursive(Elements, childTran);
            }
            else
                _Recursive(Elements, childTran);
        }

    }

    private void AddUIElement(IElement element)
    {
        IElementContext context = ((MonoBehaviour)element).GetComponent<IElementContext>();
        element.ElementContext = context;
        if(!string.IsNullOrEmpty(element.Alias))
        {
            if (_Alias2Element.ContainsKey(element.Alias))
            {
                Debug.LogError(string.Format("重复定义{0}", element.Alias), context);
            }
            else
                _Alias2Element[element.Alias] = element;
        }
    }


    public T GetUI<T>(string alias) where T : IElement
    {
        if (!_Alias2Element.ContainsKey(alias))
        {
            Debug.LogError(string.Format("Alias:{0} 不存在", alias));
            return default(T);
        }

        return (T)(_Alias2Element[alias]);
    }
}
