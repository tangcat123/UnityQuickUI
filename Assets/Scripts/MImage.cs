using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextContext))]
public class MImage : Image, IElement
{
    public string Alias
    {
        get; set;
    }

    private IElementContext _ElementContext;
    public IElementContext ElementContext
    {
        get
        {
            return _ElementContext;
        }
        set
        {
            _ElementContext = value;
            Alias = _ElementContext.Alias;
        }
    }

}
