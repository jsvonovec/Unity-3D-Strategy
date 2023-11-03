//C# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(Team))]
[CanEditMultipleObjects]
public class TeamEditor : Editor
{
    Team team;
    int forceChangeEra = 0;

    private void OnEnable()
    {

        team = target as Team;

    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        forceChangeEra = EditorGUILayout.IntField("Force to Era:", forceChangeEra);

        if (GUILayout.Button("Change Era"))
            team.ChangeEra(forceChangeEra);
    }
}
