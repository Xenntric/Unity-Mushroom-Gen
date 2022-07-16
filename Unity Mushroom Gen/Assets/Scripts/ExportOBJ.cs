using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class ExportOBJ : MonoBehaviour
{
    // Start is called before the first frame update
    public KeyCode saveKey = KeyCode.F12;
    public string saveName = "SavedMesh";
    public Transform selectedGameObject;
    private List<Mesh> mushroomMeshes;
    public GameObject scene;
    private List<GameObject> Mushrooms;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(saveKey))
        {
            print("Trying to Save");
            SaveAsset();
        }
    }
    
    void SaveAsset()
    {
        
        /* Steps to save
         * 1. Get Stem Mesh(Filter, Renderer, Materials)
         * 2. Get Cap Mesh(Filter, Renderer, Materials)
         * 3. Smush Together
         */
        var mushroomObj = new GameObject();
        mushroomObj.AddComponent<MeshFilter>();
        mushroomObj.AddComponent<MeshRenderer>();

        for (int i = 0; i < gameObject.transform.GetChild(0).childCount; i++)
        {
            var mushroom = gameObject.transform.GetChild(0).GetChild(0).gameObject;
            GameObject stemObj = mushroom.transform.GetChild(0).gameObject;
            GameObject capObj = stemObj.transform.GetChild(4).gameObject;

            for (int j = 0; j < capObj.transform.childCount; j++)
            {
                if (capObj.transform.GetChild(j).gameObject.activeSelf)
                {
                    capObj = capObj.transform.GetChild(j).gameObject;
                }
            }

            var stemMesh = stemObj.GetComponent<Mesh>();
            var capMesh = capObj.GetComponent<Mesh>();

            var stemMF = stemObj.GetComponent<MeshFilter>();
            var capMF = capObj.GetComponent<MeshFilter>();

            var stemMat = stemObj.GetComponent<Material>();
            var capMat = capObj.GetComponent<Material>();
            
            
            
        }

        




        /*if (true)
        {
            var savePath = "Assets/" + saveName + ".asset";
            Debug.Log("Saved Mesh to:" + savePath);
            AssetDatabase.CreateAsset(megaMush, savePath);
        }*/
    }
}
