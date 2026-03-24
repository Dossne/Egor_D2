using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ClawbearGames
{
    public class LevelEditor : EditorWindow
    {
        private LevelManager levelManager = null;

        private float smallBtnHeight = 20;
        private float bigBtnHeight = 35;
        private int currentLevel = 1;
        private int levelTemp = 0;

        private const string LevelEditorScenePath = "Assets/_Blocky_Holes/Scenes/LevelEditor.unity";


        [MenuItem("Tools/ClawbearGames/Level Editor")]
        public static void ShowWindow()
        {
            // Ask for a change scene confirmation if not on level editor scene
            if (!EditorSceneManager.GetActiveScene().path.Equals(LevelEditorScenePath))
            {
                if (EditorUtility.DisplayDialog(
                        "Open Level Editor",
                        "Do you want to close the current scene and open LevelEditor scene? Unsaved changes in this scene will be discarded.", "Yes", "No"))
                {
                    EditorSceneManager.OpenScene(LevelEditorScenePath);
                    GetWindow(typeof(LevelEditor));
                }
            }
            else
            {
                GetWindow(typeof(LevelEditor));
            }
        }

        private void Update()
        {
            // Check if is in LevelEditor scene.
            Scene activeScene = EditorSceneManager.GetActiveScene();

            // Auto exit if not in level editor scene.
            if (!activeScene.path.Equals(LevelEditorScenePath))
            {
                Close();
                return;
            }
        }

        private void OnGUI()
        {
            if (levelManager == null)
            {
                levelManager = FindFirstObjectByType<LevelManager>();
                currentLevel = levelManager.TotalLevel;
            }

            // Disable the whole editor window if the game is in playing mode
            EditorGUI.BeginDisabledGroup(Application.isPlaying);


            /////////////////////////////////////////////////////////////////////////////////////// Create And Load Levels Section
            #region Create And Load Levels Section
            GUILayout.Space(5);
            GUILayout.BeginVertical("Box", GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("TOTAL LEVEL: " + levelManager.TotalLevel.ToString(), EditorStyles.boldLabel);
            GUILayout.Space(5);
            EditorGUILayout.LabelField("CURRENT LEVEL: " + currentLevel.ToString(), EditorStyles.boldLabel);

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();

            //////////////Decrease level number
            EditorGUI.BeginDisabledGroup(levelManager.TotalLevel == 0 || currentLevel <= 1); //If current level is 1, disable load previous level
            if (GUILayout.Button("←", GUILayout.ExpandWidth(true), GUILayout.Height(smallBtnHeight)))
            {
                currentLevel--;
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();
            //////////////////////////////////

            EditorGUI.BeginDisabledGroup(levelManager.TotalLevel == 0);
            currentLevel = EditorGUILayout.IntField(currentLevel, GUILayout.ExpandWidth(true), GUILayout.Height(smallBtnHeight));
            EditorGUI.EndDisabledGroup();

            //////////////Increase level number
            EditorGUI.BeginDisabledGroup(levelManager.TotalLevel == 0 || currentLevel == levelManager.TotalLevel); //If current level is equals the max level, disable load next level
            if (GUILayout.Button("→", GUILayout.ExpandWidth(true), GUILayout.Height(smallBtnHeight)))
            {
                currentLevel++;
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();
            //////////////////////////////////


            /////////////Create new level section
            EditorGUI.BeginDisabledGroup(levelManager.TotalLevel > 0 && levelManager.IsLevelNullData(currentLevel) && currentLevel>0 && currentLevel<levelManager.TotalLevel);
            if (GUILayout.Button("New Level", GUILayout.ExpandWidth(true), GUILayout.Height(smallBtnHeight)))
            {
                if (levelManager.TotalLevel == 0)
                {
                    //Create new level here
                    levelManager.CreateLevel();
                    currentLevel = levelManager.TotalLevel;
                }
                else if (levelManager.IsLevelNullData(levelManager.TotalLevel))
                {
                    string title = "Level Unsaved!";
                    string message = "Please save level " + levelManager.TotalLevel.ToString() + " before create a new one!";
                    EditorUtility.DisplayDialog(title, message, "OK");
                }
                else
                {
                    //Create new level here
                    levelManager.CreateLevel();
                    currentLevel = levelManager.TotalLevel;
                }

                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion
            /////////////////////////////////////////////////////////////////////////////////////// Create And Load Levels Section


            /////////////////////////////////////////////////////////////////////////////////////// Level Parameters And Warnings Section
            #region Level Parameters And Warnings Section


            GUILayout.Space(5);
            GUILayout.BeginVertical("Box", GUILayout.ExpandWidth(true));


            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Total Object Amount: " + levelManager.TotalObjectAmount.ToString());
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target Object Amount: ");
            levelManager.targetObjectAmount = EditorGUILayout.IntField(levelManager.targetObjectAmount, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();


            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time To Complete Level: ");
            levelManager.timeToCompleteLevel = EditorGUILayout.IntField(levelManager.timeToCompleteLevel, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();


            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player Movement Speed ");
            levelManager.playerMovementSpeed = EditorGUILayout.FloatField(levelManager.playerMovementSpeed, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Background Material:");
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            for(int i = 0; i < levelManager.GroundTextures.Length; i++)
            {
                if(GUILayout.Button(levelManager.GroundTextures[i], GUILayout.Width(50), GUILayout.Height(50)))
                {
                    levelManager.GroundMaterial.SetTexture("_Main_Texture", levelManager.GroundTextures[i]);
                }
            }
            GUILayout.EndHorizontal();


            /////////////Show warning section 
            if (levelManager.TotalLevel == 0)
            {
                EditorGUILayout.HelpBox("There is no level to load. Please create new level!", MessageType.Warning);
            }
            else if (currentLevel <= 0 || currentLevel > levelManager.TotalLevel)
            {
                EditorGUILayout.HelpBox("Please enter a valid number!", MessageType.Error);
            }
            else
            {
                /////////////Actualy load level section 
                if (levelTemp != currentLevel)
                {
                    levelTemp = currentLevel;
                    if (!levelManager.IsLevelNullData(currentLevel))
                        levelManager.LoadLevel(currentLevel);
                }

                /////////////Save and overwrite level section
                GUILayout.Space(5);
                string btnName;
                if (levelManager.IsLevelNullData(currentLevel))
                {
                    btnName = "SAVE LEVEL";
                    EditorGUILayout.HelpBox("Please save level before create a new one!", MessageType.Warning);
                }
                else
                {
                    btnName = "OVERWRITE LEVEL";
                }

                if (GUILayout.Button("CLEAR SCENE", GUILayout.ExpandWidth(true), GUILayout.Height(bigBtnHeight)))
                {
                    levelManager.ClearScene();
                }
                if (GUILayout.Button(btnName, GUILayout.ExpandWidth(true), GUILayout.Height(bigBtnHeight)))
                {
                    if (!HasTargetObject())
                    {
                        string title = "Level Unsaved!";
                        string message = "There is no target object in the level! Make sure that you have at least one target object in the scene.";
                        EditorUtility.DisplayDialog(title, message, "OK");
                    }
                    else
                    {
                        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                        levelManager.SaveLevel(currentLevel);
                        AssetDatabase.Refresh();
                    }
                }
            }
            GUILayout.EndVertical();
            #endregion
            /////////////////////////////////////////////////////////////////////////////////////// Level Parameters And Warnings Section


            //EndDisabledGroup for Application.isPlaying
            EditorGUI.EndDisabledGroup();
        }



        /// <summary>
        /// Check if the current level scene has any target object (objects with component TargetObjectController in it)
        /// </summary>
        /// <returns></returns>
        private bool HasTargetObject()
        {
            return FindObjectsByType<TargetObjectController>(FindObjectsSortMode.None).Length > 0;
        }
    }
}
