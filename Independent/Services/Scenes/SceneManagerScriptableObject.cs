using QuizCanners.Inspect;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using QuizCanners.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/Managers/" + FILE_NAME)]
    public class SceneManagerScriptableObject : ScriptableObject, IPEGI, INeedAttention, Service.ILoadingProgressForInspector
    {
        public const string FILE_NAME = "Scenes Manager";

        public List<EnumeratedScene> Scenes;

        public bool this[IigEnum_Scene scene] 
        {
            get => TryGet(scene, out var match) && match.IsLoadedOrLoading;
            set 
            {
                if (TryGet(scene, out var match))
                    match.IsLoadedOrLoading = value;
            }
        }
        public void LoadOnly(IigEnum_Scene scene)
        {
            foreach (var s in Scenes)
            {
                s.IsLoadedOrLoading = s.Type == scene;
            }
        }

        private bool TryGet(IigEnum_Scene scene, out EnumeratedScene result)
        {
            foreach (var s in Scenes)
            {
                if (s.Type == scene)
                {
                    result = s;
                    return true;
                }
            }
            result = null;
            return false;
        }

        public string NeedAttention() => pegi.NeedsAttention(Scenes);

        [Serializable]
        public class EnumeratedScene : IPEGI_ListInspect, IGotReadOnlyName, Service.ILoadingProgressForInspector
        {
            public IigEnum_Scene Type;
            public SerializableSceneReference SceneReference;
            public AsyncOperation LoadOperation;

            public string ScenePath => SceneReference.ScenePath;

            public bool IsLoadedOrLoading
            {
                get => (LoadOperation != null && !LoadOperation.isDone) || IsLoadedFully;
                set
                {
                    if (value)
                        Load(LoadSceneMode.Additive);
                    else
                        Unload();
                }
            }
            public bool IsLoadedFully  => SceneManager.GetSceneByPath(SceneReference.ScenePath).IsValid();
               
            public void Load(LoadSceneMode mode)
            {
                if (LoadOperation == null || LoadOperation.isDone)
                {
                    if (!IsLoadedFully)
                    {
                        LoadOperation = SceneManager.LoadSceneAsync(ScenePath, mode);
                    }
                }
                else
                    Debug.LogWarning("Already loading {0} : {1}".F(Type, ScenePath));
            }

            private void Unload() 
            {
                if (IsLoadedFully)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneByPath(ScenePath));
                    LoadOperation = null;
                }
            }

            #region Inspector

            public bool IsLoading(ref string state, ref float progress01)
            {
                if (LoadOperation != null && !LoadOperation.isDone) 
                {
                    progress01 = LoadOperation.progress;
                    state = ScenePath;
                    return true;
                }

                return false;
            }

            public void InspectInList(ref int edited, int ind)
            {
                pegi.editEnum(ref Type, width: 120);

                if (IsLoadedFully)
                {
                    var scene = SceneManager.GetSceneByPath(ScenePath);

                    if (scene.isSubScene == false)
                        "MAIN".write(60);
                    else if ("Unload".Click())
                    {
                        IsLoadedOrLoading = false;
                        return;
                    }

                    SceneManager.GetSceneByPath(ScenePath).name.write();

                }
                else
                {

                    if (LoadOperation != null && LoadOperation.isDone == false)
                    {
                        "Loading {0}... {1}%".F(ScenePath, Mathf.FloorToInt(LoadOperation.progress * 100)).write();
                    }
                    else
                    {
                        SceneReference.Nested_Inspect(fromNewLine: false);

                        if (Application.isPlaying)
                        {
                            if (icon.Add.Click())
                                IsLoadedOrLoading = true;

                            if (icon.Load.Click())
                                Load(LoadSceneMode.Single);
                        }
#if UNITY_EDITOR
                        else if ("Switch".ClickConfirm(
                            confirmationTag: "SwSc" + ScenePath, 
                            toolTip: "Save scene before switching to another. Sure you want to change?"))
                                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath);
#endif

                    }
                }
#if UNITY_EDITOR

                if (ScenePath.IsNullOrEmpty() == false)
                {
                    bool match = false;
                    var allScenes = EditorBuildSettings.scenes;
                    foreach (var sc in allScenes)
                    {
                        if (sc.path.Equals(ScenePath))
                        {
                            match = true;

                            var enbl = sc.enabled;

                            if (pegi.toggleIcon(ref enbl))
                            {
                                sc.enabled = enbl;
                                EditorBuildSettings.scenes = allScenes;
                            }

                            break;
                        }
                    }

                    if (!match && "Add To Build".Click())
                    {
                        var lst = new List<EditorBuildSettingsScene>(allScenes);
                        lst.Add(new EditorBuildSettingsScene(ScenePath, enabled: true));
                        EditorBuildSettings.scenes = lst.ToArray();
                    }
                }
#endif
            }

            public string GetNameForInspector() => "{0}: {1}".F(Type.ToString(), ScenePath);

          
            #endregion
        }

        #region Inspector
        public void Inspect()
        {
            "Scenes".edit_List(Scenes).nl();
        }

        public bool IsLoading(ref string state, ref float progress01)
        {
            foreach (var s in Scenes)
                if (s.IsLoading(ref state, ref progress01))
                    return true;

            return false;

        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(SceneManagerScriptableObject))] internal class SceneManagerScriptableObjectDrawer : PEGI_Inspector_Override { }
}
