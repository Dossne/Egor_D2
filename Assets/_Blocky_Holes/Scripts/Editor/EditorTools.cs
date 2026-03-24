using UnityEngine;
using UnityEditor;

namespace ClawbearGames
{
    public class EditorTools : EditorWindow
    {
        [MenuItem("Tools/ClawbearGames/Reset PlayerPrefs")]
        public static void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("*************** PlayerPrefs Was Deleted ***************");
        }


        [MenuItem("Tools/Capture Screenshot To Desktop")]
        public static void CaptureScreenshot_Desktop()
        {
            string path = "C:/Users/TIENNQ/Desktop/icon.png";
            ScreenCapture.CaptureScreenshot(path);
        }


        //[MenuItem("Tools/ClawbearGames/Setup Models")]
        //public static void SetupModels()
        //{
        //    //string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        //    //foreach (string assetPath in assetPaths)
        //    //{
        //    //    System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

        //    //    if (assetType != null)
        //    //    {
        //    //        if (!assetPath.Contains(".prefab") && assetType.ToString().Contains("GameObject"))
        //    //        {
        //    //            ModelImporter importer = ModelImporter.GetAtPath(assetPath) as ModelImporter;
        //    //            importer.materialImportMode = ModelImporterMaterialImportMode.None;
        //    //        }
        //    //    }
        //    //}
        //    //AssetDatabase.Refresh();


        //string towerPaths = "Assets/_Highway_Deliver_Master/Models/Towers/";
        //for (int i = 0; i <= 15; i++)
        //{
        //    string matPath = towerPaths + "Tower_" + i.ToString() + "/" + "Tower_" + i.ToString() + ".mat";
        //    string texturePath = towerPaths + "Tower_" + i.ToString() + "/" + "Tower_" + i.ToString() + ".png";
        //    Material material = new Material(Shader.Find("Clawbear Games/Self Illumin Diffuse"));
        //    AssetDatabase.CreateAsset(material, matPath);
        //    material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        //    material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
        //}

        //    //foreach (GameObject cha in characterPrefabs)
        //    //{
        //    //    GameObject charTank = cha;
        //    //    GameObject charBody = charTank.transform.GetChild(0).gameObject;
        //    //    GameObject charTurret = charTank.transform.GetChild(1).gameObject;

        //    //    string rootPath = "Assets/_Tankie_Snaker_Attack/Models/Characters/" + cha.name;

        //    //    Material material = AssetDatabase.LoadAssetAtPath<Material>(rootPath + ("/" + cha.name + ".mat"));

        //    //    GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/Character_Tank_" + cha.name.Split('_')[1] + ".fbx");
        //    //    charTank.GetComponent<MeshFilter>().sharedMesh = tankModel.GetComponent<MeshFilter>().sharedMesh;
        //    //    charTank.GetComponent<MeshRenderer>().sharedMaterial = material;

        //    //    GameObject bodyModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/Character_Body_" + cha.name.Split('_')[1] + ".fbx");
        //    //    charBody.GetComponent<MeshFilter>().sharedMesh = bodyModel.GetComponent<MeshFilter>().sharedMesh;
        //    //    charBody.GetComponent<MeshRenderer>().sharedMaterial = material;

        //    //    GameObject turretModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/Character_Turret_" + cha.name.Split('_')[1] + ".fbx");
        //    //    charTurret.GetComponent<MeshFilter>().sharedMesh = turretModel.GetComponent<MeshFilter>().sharedMesh;
        //    //    charTurret.GetComponent<MeshRenderer>().sharedMaterial = material;
        //    //}

        //    //AssetDatabase.Refresh();

        //foreach (GameObject tank in characterPrefabs)
        //{
        //    GameObject charTank = tank;
        //    GameObject tankTurret = charTank.transform.GetChild(0).transform.GetChild(0).gameObject;

        //    string rootPath = "Assets/_Coin_Hoarder/Models/Others/Tanks/" + tank.name;

        //    Material material = AssetDatabase.LoadAssetAtPath<Material>(rootPath + ("/" + tank.name + ".mat"));

        //    GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/Tank_" + tank.name.Split('_')[1] + ".fbx").transform.GetChild(0).gameObject;
        //    charTank.GetComponent<MeshFilter>().sharedMesh = tankModel.GetComponent<MeshFilter>().sharedMesh;
        //    charTank.GetComponent<MeshRenderer>().sharedMaterial = material;
        //    tank.GetComponent<MeshCollider>().sharedMesh = tankModel.GetComponent<MeshFilter>().sharedMesh;

        //    GameObject turretModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/Tank_" + tank.name.Split('_')[1] + ".fbx").transform.GetChild(1).gameObject;
        //    tankTurret.GetComponent<MeshFilter>().sharedMesh = turretModel.GetComponent<MeshFilter>().sharedMesh;
        //    tankTurret.GetComponent<MeshRenderer>().sharedMaterial = material;
        //}

        //AssetDatabase.Refresh();
        //}


        //[MenuItem("Custom/Copy Objects to Prefab")]
        //static void AddObjectsToPrefab()
        //{
        //    foreach (GameObject prefab in Selection.gameObjects)
        //    {
        //        // create an instance of the prefab
        //        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        //        GameObject clone = new GameObject();
        //        clone.name = "Shadow";
        //        clone.transform.parent = prefabInstance.transform;
        //        clone.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        //        clone.transform.localScale = new Vector3(1.7f, 1.7f, 1f);
        //        clone.transform.localRotation = Quaternion.identity;
        //        clone.transform.SetSiblingIndex(3);
        //        SpriteRenderer spriteRenderer = clone.AddComponent<SpriteRenderer>();
        //        spriteRenderer.color = Color.black;
        //        spriteRenderer.sortingOrder = 4;
        //        spriteRenderer.sprite = Resources.Load("Shadow") as Sprite;

        //        // apply the instance to the prefab
        //        PrefabUtility.ReplacePrefab(prefabInstance, PrefabUtility.GetPrefabParent(prefabInstance), ReplacePrefabOptions.ConnectToPrefab);

        //        // remove the instance from the scene
        //        DestroyImmediate(prefabInstance);
        //    }

        //}



        //HomeManager homeManager = FindObjectOfType<HomeManager>();
        //for(int i = 0; i < homeManager.fighterPrefabs.Length; i++)
        //{
        //    string index = homeManager.fighterPrefabs[i].name.Split('_')[1];
        //    string rootPath = "Assets/_Digger_Merge_Master/Models/Fighters/Fighter_" + index.ToString();
        //    Material material = AssetDatabase.LoadAssetAtPath<Material>(rootPath + "/Fighter_" + index.ToString() + ".mat");

        //    GameObject bodyModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/Fighter_" + index.ToString() + ".fbx").transform.GetChild(2).gameObject;
        //    GameObject footModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/Fighter_" + index.ToString() + ".fbx").transform.GetChild(0).gameObject;
        //    GameObject holderModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/Fighter_" + index.ToString() + ".fbx").transform.GetChild(1).gameObject;

        //    homeManager.fighterPrefabs[i].transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = bodyModel.GetComponent<MeshFilter>().sharedMesh;
        //    homeManager.fighterPrefabs[i].transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = material;


        //    homeManager.fighterPrefabs[i].transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = footModel.GetComponent<MeshFilter>().sharedMesh;
        //    homeManager.fighterPrefabs[i].transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial = material;

        //    homeManager.fighterPrefabs[i].transform.GetChild(2).GetComponent<MeshFilter>().sharedMesh = footModel.GetComponent<MeshFilter>().sharedMesh;
        //    homeManager.fighterPrefabs[i].transform.GetChild(2).GetComponent<MeshRenderer>().sharedMaterial = material;

        //    Transform holderTrans = homeManager.fighterPrefabs[i].transform.GetChild(3);
        //    for(int a = 0; a < holderTrans.childCount; a++)
        //    {
        //        holderTrans.GetChild(a).GetComponent<MeshFilter>().sharedMesh = holderModel.GetComponent<MeshFilter>().sharedMesh;
        //        holderTrans.GetChild(a).GetComponent<MeshRenderer>().sharedMaterial = material;
        //    }
        //}




        //private static int fighterIndex = 0;
        void OnGUI()
        {
            if (GUILayout.Button("Update next beamer"))
            {
                //foreach (GameObject towerPrefab in FindObjectOfType<HomeManager>().enemyBeamerPrefabs)
                //{
                //    string rootPath = "Assets/_Tankie_Battleground/Models/Towers/Player_Towers/" + towerPrefab.name.ToString();

                //    Material material = AssetDatabase.LoadAssetAtPath<Material>(rootPath + ("/" + towerPrefab.name.ToString() + ".mat"));
                //    GameObject baseModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/" + towerPrefab.name.ToString() + ".fbx").transform.GetChild(0).gameObject;
                //    GameObject bodyModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/" + towerPrefab.name.ToString() + ".fbx").transform.GetChild(1).gameObject;
                //    GameObject topModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/" + towerPrefab.name.ToString() + ".fbx").transform.GetChild(2).gameObject;
                //    GameObject turretModel = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath + "/" + towerPrefab.name.ToString() + ".fbx").transform.GetChild(3).gameObject;

                //    //FindObjectOfType<NewBehaviourScript>().baseMeshFilter.sharedMesh = baseModel.GetComponent<MeshFilter>().sharedMesh;
                //    //FindObjectOfType<NewBehaviourScript>().bodyMeshFilter.sharedMesh = bodyModel.GetComponent<MeshFilter>().sharedMesh;
                //    //FindObjectOfType<NewBehaviourScript>().topMeshFilter.sharedMesh = topModel.GetComponent<MeshFilter>().sharedMesh;
                //    //FindObjectOfType<NewBehaviourScript>().turretMeshFilter.sharedMesh = turretModel.GetComponent<MeshFilter>().sharedMesh;
                //    //FindObjectOfType<NewBehaviourScript>().baseMeshRenderer.sharedMaterial = material;
                //    //FindObjectOfType<NewBehaviourScript>().bodyMeshRenderer.sharedMaterial = material;
                //    //FindObjectOfType<NewBehaviourScript>().topMeshRenderer.sharedMaterial = material;
                //    //FindObjectOfType<NewBehaviourScript>().turretMeshRenderer.sharedMaterial = material;
                //    //EditorUtility.SetDirty(FindObjectOfType<NewBehaviourScript>().fighterOb);
                //    //AssetDatabase.Refresh();

                //    towerPrefab.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = material;
                //    towerPrefab.transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial = material;
                //    towerPrefab.transform.GetChild(2).GetComponent<MeshRenderer>().sharedMaterial = material;
                //    towerPrefab.transform.GetChild(3).transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = material;

                //    towerPrefab.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = baseModel.GetComponent<MeshFilter>().sharedMesh;
                //    towerPrefab.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = bodyModel.GetComponent<MeshFilter>().sharedMesh;
                //    towerPrefab.transform.GetChild(2).GetComponent<MeshFilter>().sharedMesh = topModel.GetComponent<MeshFilter>().sharedMesh;
                //    towerPrefab.transform.GetChild(3).transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = turretModel.GetComponent<MeshFilter>().sharedMesh;
                //    towerPrefab.transform.GetChild(0).GetComponent<MeshCollider>().sharedMesh = baseModel.GetComponent<MeshFilter>().sharedMesh;
                //    towerPrefab.transform.GetChild(1).GetComponent<MeshCollider>().sharedMesh = bodyModel.GetComponent<MeshFilter>().sharedMesh;
                //    towerPrefab.transform.GetChild(2).GetComponent<MeshCollider>().sharedMesh = topModel.GetComponent<MeshFilter>().sharedMesh;
                //    towerPrefab.transform.GetChild(3).GetComponent<MeshCollider>().sharedMesh = turretModel.GetComponent<MeshFilter>().sharedMesh;
                //    EditorUtility.SetDirty(towerPrefab);
                //    AssetDatabase.Refresh();
                //}
            }

            //if (GUILayout.Button("Create icon"))
            //{
            //    //TowerConfigObject tankConfigObject = FindObjectOfType<NewBehaviourScript>().fighterConfigObjects[fighterIndex];
            //    //string path = "D:/Unity/Projects/Tankie_Battleground/Assets/_Tankie_Battleground/Resources/FighterIcons/" + tankConfigObject.name + ".png";
            //    //ScreenCapture.CaptureScreenshot(path);
            //    //EditorUtility.SetDirty(FindObjectOfType<NewBehaviourScript>().fighterOb);
            //    //AssetDatabase.Refresh();
            //    fighterIndex++;
            //}
        }
    }
}

