using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BatchShaderChanger : EditorWindow
{
    private Shader targetShader;

    [MenuItem("Tools/Batch Shader Changer")]
    public static void ShowWindow()
    {
        GetWindow<BatchShaderChanger>("Shader Changer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Change Shaders", EditorStyles.boldLabel);
        GUILayout.Space(10);

        targetShader = (Shader)EditorGUILayout.ObjectField("Target Shader", targetShader, typeof(Shader), false);

        GUILayout.Space(20);

        if (GUILayout.Button("Change Shaders on Selected Assets"))
        {
            if (targetShader == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a Target Shader first.", "OK");
                return;
            }

            ChangeShaders();
        }
    }

    private void ChangeShaders()
    {
        Object[] selectedObjects = Selection.objects;
        HashSet<Material> materialsToChange = new HashSet<Material>();

        foreach (Object obj in selectedObjects)
        {
            // Check if it's a GameObject (Prefab)
            if (obj is GameObject go)
            {
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    foreach (Material mat in renderer.sharedMaterials)
                    {
                        if (mat != null)
                        {
                            materialsToChange.Add(mat);
                        }
                    }
                }
            }
            // Check if it's a Material directly
            else if (obj is Material mat)
            {
                materialsToChange.Add(mat);
            }
        }

        if (materialsToChange.Count == 0)
        {
            EditorUtility.DisplayDialog("Info", "No materials found in selection.", "OK");
            return;
        }

        Undo.RecordObjects(new List<Material>(materialsToChange).ToArray(), "Batch Change Shader");

        int count = 0;
        foreach (Material mat in materialsToChange)
        {
            mat.shader = targetShader;
            count++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Success", $"Updated shader on {count} materials.", "OK");
    }
}
