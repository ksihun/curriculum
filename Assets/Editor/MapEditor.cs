using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        MapGenerator map = target as MapGenerator;
        if (DrawDefaultInspector())
        {
            map.GeneratorMap();
        }
        
        if (GUILayout.Button("Generate Map"))
        {
            map.GeneratorMap();
        }
        
        
        
        
        
    }
}
