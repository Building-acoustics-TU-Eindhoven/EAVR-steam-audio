﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using TMPro;
using SteamAudio;
using Vector3 = UnityEngine.Vector3;
using Siccity.GLTFUtility;

public class GeometryManager : MonoBehaviour
{
    int curMaterial = 0;
    public TMP_Dropdown dropdown;
    public GameObject audioSourceLocation;
    public AudioSource audioSource;
    public GameObject _loadedGameObject;
    int totMaterials = -1;

    bool[] shouldChangeWallMaterial = new bool[6] { true, true, true, true, true, true };
    float scale = 1.0f;

    private List<Transform> selectedWalls = new List<Transform>();

private string filepath = "./Assets/Models/10mcube.gltf";
    List<UnityEngine.Material> visualMaterials = new List<UnityEngine.Material>();
    List<SteamAudioMaterial> steamAudioMaterials = new List<SteamAudioMaterial>();

    // Start is called before the first frame update
    void Start()
    {
        // SetActiveMaterial();
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Called in the Awake function of the GeometryManagerEditor
    public void CollectMaterials()
    {
        // Collect visual materials
        string[] visualMaterialFiles = System.IO.Directory.GetFiles("Assets/Materials", "*.mat", SearchOption.TopDirectoryOnly);

        foreach (var file in visualMaterialFiles)
        {
            visualMaterials.Add(AssetDatabase.LoadAssetAtPath(file, typeof(UnityEngine.Material)) as UnityEngine.Material);
        }

        // Collect Steam Audio materials
        string[] steamAudioMaterialFiles = System.IO.Directory.GetFiles("Assets/Plugins/SteamAudio/Resources/Materials", "*.asset", SearchOption.TopDirectoryOnly);

        foreach (var file in steamAudioMaterialFiles)
        {
            steamAudioMaterials.Add(AssetDatabase.LoadAssetAtPath(file, typeof(SteamAudioMaterial)) as SteamAudioMaterial);
        }
    }

    public void SetActiveMaterial()
    {

        if (selectedWalls.Count == 0)
            return;
            
        if (totMaterials == -1)
            totMaterials = selectedWalls[0].childCount;

        foreach (Transform selectedWall in selectedWalls)
        {
            int i = 0;
            foreach (Transform child in selectedWall)
            {
                if (i == curMaterial)
                    child.gameObject.SetActive(true);
                else if (i != selectedWall.childCount-1)
                    child.gameObject.SetActive(false);
                ++i;
            }
        }
        /// Changing all materials ///
        // GameObject root = GameObject.Find("root");
        // foreach (Transform child in root.transform.GetChild(0).transform)
        // {
        //     if (totMaterials == -1)
        //         totMaterials = child.childCount;

        //     for (int i = 0; i < totMaterials; ++i)
        //     {
        //         if (i == curMaterial)
        //             child.GetChild(i).gameObject.SetActive(true);
        //         else
        //             child.GetChild(i).gameObject.SetActive(false);
        //     }
        // }
        // return;

        /// Walls in geometry manager (old) ///
        // int j = 0;
        // foreach (Transform child in transform)
        // {
        //     if (child.tag == "Wall" && shouldChangeWallMaterial[j])
        //     {
        //         if (totMaterials == -1)
        //             totMaterials = child.childCount;
        //         for (int i = 0; i < totMaterials; ++i)
        //         {
        //             if (i == curMaterial)
        //                 child.GetChild(i).gameObject.SetActive(true);
        //             else
        //                 child.GetChild(i).gameObject.SetActive(false);
        //         }
        //     }
        //     ++j;
        // }

    }

    public void SetShouldChangeWallMaterial(int wall, bool shouldChange)
    {
        shouldChangeWallMaterial[wall] = shouldChange;
    }

    public void ChangeMaterial()
    {
        curMaterial = (curMaterial + 1) % totMaterials;
        SetActiveMaterial();
    }

    public void ChangeMaterial(int mat)
    {
        Debug.Log("Changing Material");
        // first option is "Choose material..
        if (mat == 0)
            return;

        if (selectedWalls.Count == 0)
        {
            Debug.LogWarning ("Please select a wall first!");
            return;
        }

        curMaterial = mat - 1;
        SetActiveMaterial();
    }


    public void ChangeSize(float multiplier)
    {
        scale = multiplier;
        this.transform.localScale = new Vector3(scale, scale, scale);
        audioSource.transform.position = audioSourceLocation.transform.position;
    }

    public void ClearSelectedWalls()
    {
        selectedWalls.Clear();
    }

    public void AddSelectedWall (Transform s)
    {
        selectedWalls.Add(s);
    }

    public List<Transform> GetSelectedWalls()
    {
        return selectedWalls;
    }
    
    public string GetActiveMaterialOf (int idx)
    {
        foreach (Transform child in selectedWalls[0])
        {
            if (child.gameObject.activeSelf)
            {
                return child.name.Substring (child.name.LastIndexOf('_') + 1);
            }
        }
        return "";
    }


    private Vector3 Average(MeshFilter [] _meshFilterArray){

        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;
        int l = _meshFilterArray.Length;
        foreach (MeshFilter m in _meshFilterArray){
           x += m.sharedMesh.bounds.center.x;
           y += m.sharedMesh.bounds.center.y;
           z += m.sharedMesh.bounds.center.z;
        }
        return new Vector3( x / l , y / l, z / l);
    }

    public void ImportProcedure()
    {
        
        if (_loadedGameObject != null)
        {
            // Regenerate temporary folder with Dynamic Object Assets
            UnityEngine.Windows.Directory.Delete("Assets/DynamicObjects/TempFolder");
            UnityEngine.Windows.Directory.CreateDirectory("Assets/DynamicObjects/TempFolder");

            // Get Meshfilters
            MeshFilter[] mff = _loadedGameObject.GetComponentsInChildren<MeshFilter>();

            Debug.Log("found this number of meshFilter groups : " + mff.Length);

            // Initialise every mesh filter with all possible materials
            int counter = 0;
            foreach (MeshFilter mf in mff)
            {
                // Add components to the imported meshfilter
                GameObject go = mf.gameObject;

                var name = go.name;
                if (name == "Room")
                    continue;

                // Add meshcollider
                go.layer = 10;

                //// Add children to each mesh: one per material ////
                string activeMaterial = GetMaterialBasedOnMeshName (go.name);

                GameObject selectionGo = Instantiate<GameObject> (mf.gameObject);
                bool firstChild = true;
                foreach (SteamAudioMaterial steamAudioMaterial in steamAudioMaterials)
                {
                    // Add child to mesh
                    GameObject goChild = Instantiate<GameObject>(firstChild ? mf.gameObject : mf.transform.GetChild(0).gameObject, mf.gameObject.transform);
                    if (firstChild)
                    {
                        goChild.AddComponent(typeof(MeshCollider));
                    }

                    goChild.name = counter + "_" + steamAudioMaterial.name;

                    // Add Steam Audio Components

                    if (firstChild)
                    {
                        // Add SteamAudioGeometry component
                        goChild.AddComponent (typeof(SteamAudioGeometry));
                        
                        // Add dynamic object component
                        goChild.AddComponent (typeof (SteamAudioDynamicObject));
                        
                        firstChild = false;
                    }

                    // Set active so we can do things with it
                    goChild.SetActive (true);

                    //// EDIT VISUAL MATERIAL ////
                    
                    // Get the visual material name from the steam audio material
                    string visualMaterialName = GetMaterialFromName (steamAudioMaterial.name);
                    EditMeshRendererMaterial (goChild, visualMaterialName);

                    // Set the Steam Audio Material property of the SteamAudioGeometry
                    SetSteamAudioMaterial (goChild, steamAudioMaterial);

                    // Crate the SerializedData for the SteamAudioDynamicObject
                    CreateDynamicObjectSerializedData (goChild, name, counter);

                    // Export the dynamic object
                    SteamAudioManager.ExportDynamicObject (goChild.GetComponent<SteamAudioDynamicObject>(), false);

                    goChild.SetActive(steamAudioMaterial.name == activeMaterial);
                }
                AddSelectionGeometry (selectionGo, mf, counter);

                mf.GetComponent<MeshRenderer>().enabled = false;
                ++counter;
            }

            Vector3 average = Average(mff);

            _loadedGameObject.transform.GetChild(0).position = new Vector3 (-average.x, 0, -average.z);
        }
        else
        {
            Debug.Log("loaded null");
        }
    }
    
    private void AddSelectionGeometry (GameObject selectionGo, MeshFilter mf, int counter)
    {
                            // Add child to mesh
        // GameObject goChild = Instantiate<GameObject>(mf.transform.GetChild(0).gameObject, mf.gameObject.transform);
        selectionGo.transform.SetParent (mf.gameObject.transform);

        selectionGo.name = counter + "_Selection";
        // Add Steam Audio Components

        // Set active so we can do things with it
        selectionGo.SetActive (true);

        //// EDIT VISUAL MATERIAL ////
        EditMeshRendererMaterial (selectionGo, "Selection");
        selectionGo.SetActive(false);

    }
    private void EditMeshRendererMaterial(GameObject goChild, string visualMaterialName)
    {
        // Serialize the material of the mesh renderer of the newly instantiated child
        UnityEditor.SerializedObject meshRenderer = new UnityEditor.SerializedObject (goChild.GetComponent<MeshRenderer>());
        var matsSO = meshRenderer.FindProperty("materials");

        // Set the visual material of the mesh renderer to the one 
        foreach (UnityEngine.Material mat in visualMaterials)
            if (mat.name == visualMaterialName)
                goChild.GetComponent<MeshRenderer>().sharedMaterial = mat;

        meshRenderer.ApplyModifiedProperties();

    }
    private void SetSteamAudioMaterial (GameObject goChild, SteamAudioMaterial newMaterialAsset)
    {
        // Make the component a SerializedObject
        UnityEditor.SerializedObject dynGeometry = new UnityEditor.SerializedObject (goChild.GetComponent<SteamAudioGeometry>());

        // Apply the material asset to the Material field 
        SerializedProperty matAsset = dynGeometry.FindProperty("material");        
        matAsset.objectReferenceValue = newMaterialAsset;
        
        // Apply the properties to the serializedobject
        dynGeometry.ApplyModifiedProperties();

    }

    private void CreateDynamicObjectSerializedData (GameObject goChild, string goName, int counter)
    {  
        // Create dynamic object asset ...
        string filename = "Assets/DynamicObjects/TempFolder/" + goChild.name + "_" + goName + ".asset";
        var dynObj = new UnityEditor.SerializedObject (goChild.GetComponent<SteamAudioDynamicObject>());
        SerializedProperty mAsset = dynObj.FindProperty("asset");

        // ... and add it to the dynamic object
        var dataAsset = ScriptableObject.CreateInstance<SerializedData>();
        AssetDatabase.CreateAsset(dataAsset, filename);
        mAsset.objectReferenceValue = dataAsset;
        dynObj.ApplyModifiedProperties();

    }
    public string GetMaterialBasedOnMeshName (string meshname)
    {
        switch (meshname)
        {
            case "Transparent":
                return "Transparent";
            case "AcousticCeilingTiles":
                return "Default";
            
            case "BrickBare":
            case "BrickPainted":
                return "Brick";
            
            case "ConcreteBlockCoarse":
            case "ConcreteBlockPainted":
            case "PolishedConcreteOrTile":
                return "Concrete";

            case "CurtainHeavy":
                return "Carpet";

            case "FiberglassInsulation":
                return "Default";

            case "GlassThin":
            case "GlassThick":
                return "Glass";

            case "Grass":
                return "Gravel"; //?;

            case "LinoleumOnConcrete":
                return "Default";
                
            case "Marble":
                return "Ceramic"; //?

            case "Metal":
                return "Metal";

            case "ParquetOnConcrete":
                return "Default";

            case "PlasterRough":
            case "PlasterSmooth":
                return "Plaster";

            case "PlywoodPanel":
                return "Default";

            case "Sheetrock":
                return "Rock";

            case "WaterOrIceSurface":
                return "Default";

            case "WoodCeiling":
            case "WoodPanel":
                return "Wood";

            case "Uniform":
                return "Default";

            case "CustomLeft":
            case "CustomRight":
            case "CustomUp":
            case "CustomDown":
            case "CustomFront":
            case "CustomBack":
                return "Default";

            default:
                Debug.LogError("Unknown Material: " + meshname);
                return "Default";
        }

    }

    public void RegenerateDynamicObjects()
    {
        GameObject root = GameObject.FindGameObjectWithTag("Model");
        if (root != null)
            DestroyImmediate(root);

        string filepath = EditorUtility.OpenFilePanel("Load GLTF", "", "gltf");
        // string filepath = "./Assets/Models/TrappenZaal.gltf";
        _loadedGameObject = Importer.LoadFromFile(filepath);
        _loadedGameObject.tag = "Model";
        ImportProcedure();
    }

    public string GetMaterialFromName(string name)
    {
        switch (name)
        {
            case "Brick":
            case "Carpet":
            case "Concrete":
            case "Glass":
            case "Gravel":
            case "Metal":
            case "Wood":
                return name;
            default:
                return "Default";
        }
    }

}
