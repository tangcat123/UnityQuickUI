using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public interface IElement  {

    // 名字
	string Alias { get; set; }

    //Type Type { get; set; }

    IElementContext ElementContext { get;set; }

    void OnInit();
}
