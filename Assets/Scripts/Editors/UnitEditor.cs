//C# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Unit))]
[CanEditMultipleObjects]
public class UnitEditor : Editor
{

    SerializedProperty damage;
    SerializedProperty attackCD;
    int ID;
    string realName;


    private void OnEnable()
    {
        damage = serializedObject.FindProperty("damage");
        attackCD = serializedObject.FindProperty("attackCD");
        ID = serializedObject.FindProperty("ID").intValue;
        realName = serializedObject.FindProperty("realName").stringValue;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //end it here if we don't want to show the custom inspector (like if the unit is still being built)
        if (!serializedObject.FindProperty("showCustomInspector").boolValue)
            return;


        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Maximum DPS: " + (float)(damage.floatValue / attackCD.floatValue));

        //Tell us whether we've added this unit to GameMaster yet
        GameObject go;
        //Our ID is in!
        if (go = GameMaster.def.everyPossibleUnitInGame.Find(w => w.GetComponent<Unit>().ID == ID))
        {
            //Our name is also correct!
            if(go.GetComponent<Unit>().realName == realName)
            {
                EditorGUILayout.LabelField("We are in GM's list of every unit in the game.");

            }

            //Our name is NOT correct
            else
            {
                Debug.LogError("The unit, " + realName + ", has the same ID ("+ID+") as another unit!");
                EditorGUILayout.LabelField("The unit, " + realName + ", has the same ID (" + ID + ") as another unit!");
            }
        }

        //Our ID is not in
        else
        {
            Debug.LogError("The unit, " + realName + ", doesn't have an ID (" + ID + ") in GM's list of every unit!");
            EditorGUILayout.LabelField("The unit, " + realName + ", doesn't have an ID (" + ID + ") in GM's list of every unit!");
            GameMaster.def.UpdateUnitIDs();
        }
    }
}