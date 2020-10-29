#if UNITY_EDITOR
using EuNet.Unity;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public class NetViewHandler : EditorWindow
{
    private const string AutoFixViewIdsMenuName = "EuNet/Auto fix view ids on hierarchy changed";
    private const string AutoFixViewIdsSettingName = "EuNetAutoFixViewIds";

    public static bool IsAutoFixViewIds
    {
        get { return EditorPrefs.GetBool(AutoFixViewIdsSettingName, true); }
        set { EditorPrefs.SetBool(AutoFixViewIdsSettingName, value); }
    }

    static NetViewHandler()
    {
        EditorApplication.hierarchyChanged -= () => HierarchyChange(true);
        EditorApplication.hierarchyChanged += () => HierarchyChange(true);
    }

    [MenuItem(AutoFixViewIdsMenuName)]
    private static void ToggleAction()
    {
        IsAutoFixViewIds = !IsAutoFixViewIds;
    }

    [MenuItem(AutoFixViewIdsMenuName, true)]
    private static bool ToggleActionValidate()
    {
        Menu.SetChecked(AutoFixViewIdsMenuName, IsAutoFixViewIds);
        return true;
    }

    [MenuItem("EuNet/Find and fix viewIds", false, 0)]
    public static void FindAndFixViewIds()
    {
        HierarchyChange(false);
    }

    internal static void HierarchyChange(bool isAuto)
    {
        if (Application.isPlaying)
            return;

        if (isAuto == true && IsAutoFixViewIds == false)
            return;

        HashSet<NetView> viewList = new HashSet<NetView>();
        HashSet<int> usedInstanceViewNumbers = new HashSet<int>();
        int fixedCount = 0;

        NetView[] findViewList = Resources.FindObjectsOfTypeAll<NetView>();

        foreach (NetView view in findViewList)
        {
            // 프리팹이나 기타 디스크에 저장되어 있다면
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(view.gameObject) != null || EditorUtility.IsPersistent(view.gameObject))
            {
                if (view.ViewId != 0)
                {
                    // 프리팹을 포함한 디스크 저장 오브젝트들은 무조건 viewId를 0으로 사용하자 (생성 시점에서 아이디 발급)
                    view.ViewId = 0;
                    view.IsSceneObject = false;
                    EditorUtility.SetDirty(view);
                    fixedCount++;
                }
            }
            else
            {
                if (view.IsSceneObject == false)
                {
                    view.IsSceneObject = true;
                    EditorUtility.SetDirty(view);
                    fixedCount++;
                }

                // 순수한 씬오브젝트라면 리스트에 추가
                viewList.Add(view);
            }
        }

        foreach (NetView view in viewList)
        {
            if (view.ViewId != 0)
            {
                if (usedInstanceViewNumbers.Contains(view.ViewId))
                {
                    view.ViewId = 0;
                }
                else
                {
                    usedInstanceViewNumbers.Add(view.ViewId);
                }
            }
        }

        int lastUsedId = 0;

        foreach (NetView view in viewList)
        {
            if (view.ViewId == 0)
            {
                int nextViewId = NetViewHandler.GetID(lastUsedId, usedInstanceViewNumbers);

                view.ViewId = nextViewId;

                lastUsedId = nextViewId;
                EditorUtility.SetDirty(view);
                fixedCount++;
            }
        }

        if (isAuto == false || fixedCount > 0)
        {
            Debug.Log($"ViewIds was fixed [{fixedCount}]");
        }
    }

    public static int GetID(int idOffset, HashSet<int> usedInstanceViewNumbers)
    {
        while (idOffset < NetViews.MaxGenerateViewIdPerSession)
        {
            idOffset++;
            if (!usedInstanceViewNumbers.Contains(idOffset))
            {
                break;
            }
        }

        return idOffset;
    }

    public static void LoadAllScenesToFix()
    {
        string[] scenes = System.IO.Directory.GetFiles(".", "*.unity", SearchOption.AllDirectories);

        foreach (string scene in scenes)
        {
            var currentScene = EditorSceneManager.OpenScene(scene);

            NetViewHandler.HierarchyChange(false);

            EditorSceneManager.SaveScene(currentScene);
        }

        Debug.Log("Corrected scene views where needed.");
    }
}
#endif