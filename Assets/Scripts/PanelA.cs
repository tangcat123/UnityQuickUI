using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelA : IPanel {

	// Use this for initialization
	void Start () {


        GetUI<MText>("A").text = "xxxxx";

        Material material = new Material(Shader.Find("UI/Default"));
        material.color = Color.red;
        GetUI<MImage>("B").material = material;
	}
	
	
}
