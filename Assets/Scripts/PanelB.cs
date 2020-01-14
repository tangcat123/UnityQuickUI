using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelB : IContainer {
	// Use this for initialization
	void Start () {
        Show();
    }
	
    void Show()
    {
        GetUI<PanelA>("Left").Show();
        GetUI<PanelA>("Right").Show();
    }
	
}
