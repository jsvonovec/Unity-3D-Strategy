//C# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(GameMaster))]
[CanEditMultipleObjects]
public class GameMasterEditor : Editor
{
    GameMaster gm;


    private void OnEnable()
    {

        gm = target as GameMaster;

    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Update Unit IDs"))
            gm.UpdateUnitIDs();

    }
}