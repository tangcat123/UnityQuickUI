using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;


[RequireComponent(typeof(IContainerContext))]
public class IContainer : SerializedMonoBehaviour, IElement {

    private void Awake()
    {
        
    }

    public string Alias
    {
        get; set;
    }

    public IContainerContext _IContainerContext;
    public IElementContext ElementContext
    {
        get
        {
            return _IContainerContext;
        }
        set
        {
            _IContainerContext = value as IContainerContext;
            Alias = _IContainerContext.Alias;
            //_IPanelContext.GetAlias2Element()
        }
    }


    protected T GetUI<T>(string alias) where T : IElement
    {
        return _IContainerContext.GetUI<T>(alias);
    }

    //protected void StartAsFirst()
    //{
    //    IElementContext context = GetComponent<IElementContext>();
    //    ElementContext = context;
    //}
}
