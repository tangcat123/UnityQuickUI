using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelA : IContainer {
	// Use this for initialization
	void Start () {
        //Show();
    }
	
    public void Show()
    {
        GetUI<MText>("A").text = "xxxxx";

        Material material = new Material(Shader.Find("UI/Default"));
        material.color = Color.red;
        GetUI<MImage>("B").material = material;
    }
	
}
