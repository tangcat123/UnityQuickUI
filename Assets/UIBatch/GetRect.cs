﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetRect : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Graphic graphic = GetComponent<Graphic>();
        Debug.Log(graphic.GetPixelAdjustedRect()); 
	}
	
	
}
