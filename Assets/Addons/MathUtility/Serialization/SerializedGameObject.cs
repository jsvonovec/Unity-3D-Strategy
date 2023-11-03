﻿using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SerializedGameObject : MonoBehaviour {
  public List<JsonSerializer.TransformStruct> TransformHierarchy;
  void Start() {
    //Serialize Transform Hierarchy to json
    string jsonSerialization = JsonSerializer.serializeTransformHierarchy(transform, true);

    //Print the json
    Debug.Log(jsonSerialization, this);

    //Deserialize Transform Hierarchy from json
    TransformHierarchy = JsonSerializer.deserializeTransformHierarchy(jsonSerialization);
  }
}

//A json serialization utility for game object hierarchies and their associated components
//Uses Reflection to achieve a complete serialization of engine component's public properties
public static class JsonSerializer {
  [Serializable]
  public struct TransformStruct {
    public string name;
    public int instanceID;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Vector3 localScale;
    public int[] children;
    public ComponentStruct[] components;
  }

  [Serializable]
  public struct ComponentStruct {
    public string typeName;
    public int instanceID;
    public string sanitizedJson;
    public List<string> enginePropertyNames;
    public List<string> enginePropertyValues;
  }

  [Serializable]
  public struct PrimitiveWrapper<T> {
    public T value__;
  }
  static PrimitiveWrapper<T> createPrimWrapper<T>(T toWrap) {
    return new PrimitiveWrapper<T> { value__ = toWrap };
  }

  //SERIALIZE TO A List<TransformStruct> -------------------------------------------------------------------------------
  public static string serializeTransformHierarchy(Transform inObject, bool prettyPrint = false) {
    int index = 0;
    Queue<Transform> transformQueue = new Queue<Transform>();
    List<TransformStruct> transformList = new List<TransformStruct>();
    Dictionary<Type, Component> defaultComponentDictionary = new Dictionary<Type, Component>();
    GameObject defaultComponentContainer = new GameObject("DELETEME");

    //Add the first transform to the Hierarchy
    addTransform(inObject, transformList, transformQueue, ref defaultComponentDictionary, ref defaultComponentContainer, ref index);

    //While there are children remaining in the queue, add them to the hierarchy
    while (transformQueue.Count > 0) {
      addTransform(transformQueue.Dequeue(), transformList, transformQueue, ref defaultComponentDictionary, ref defaultComponentContainer, ref index);
    }

    UnityEngine.Object.Destroy(defaultComponentContainer);

    return JsonUtility.ToJson(createPrimWrapper(transformList), prettyPrint);
  }

  static void addTransform(Transform inObject, List<TransformStruct> transforms, Queue<Transform> transformQueue, 
    ref Dictionary<Type, Component> defaultComponentDictionary, ref GameObject defaultComponentContainer, ref int index) {
    //Construct this transform as a TransformStruct
    TransformStruct thisTransform = new TransformStruct();
    thisTransform.name = inObject.name;
    thisTransform.instanceID = inObject.GetInstanceID();
    thisTransform.localPosition = inObject.position;
    thisTransform.localRotation = inObject.rotation;
    thisTransform.localScale = inObject.localScale;

    //Count up the valid components
    int validComponentIndex = 0;
    Component[] components = inObject.GetComponents<Component>();
    for (int i = 0; i < components.Length; i++) {
      string typeName = components[i].GetType().ToString();
      if (typeName != "SerializedGameObject" && typeName != "UnityEngine.Transform") {
        validComponentIndex++;
      }
    }

    //Add all of the valid components
    thisTransform.components = new ComponentStruct[validComponentIndex];
    validComponentIndex = 0;
    for (int i = 0; i < components.Length; i++) {
      ComponentStruct component = new ComponentStruct();
      component.typeName = components[i].GetType().ToString();

      if (component.typeName != "SerializedGameObject" && component.typeName != "UnityEngine.Transform") {
        component.instanceID = components[i].GetInstanceID();
        if (!component.typeName.StartsWith("UnityEngine.")) {
          component.sanitizedJson = JsonUtility.ToJson(components[i]);
        } else {
          Component defaultComponent;
          if (!defaultComponentDictionary.TryGetValue(components[i].GetType(), out defaultComponent)) {
            defaultComponent = defaultComponentContainer.AddComponent(components[i].GetType());
            defaultComponentDictionary.Add(components[i].GetType(), defaultComponent);
          }
          serializeEngineComponent(ref component, ref components[i], defaultComponent);
        }

        thisTransform.components[validComponentIndex] = component;
        validComponentIndex++;
      }
    }

    //And all the children, too!
    thisTransform.children = new int[inObject.childCount];
    for (int i = 0; i < thisTransform.children.Length; i++) {
      thisTransform.children[i] = ++index;
      transformQueue.Enqueue(inObject.GetChild(i));
    }
    transforms.Add(thisTransform);
  }

  static void serializeEngineComponent<T>(ref ComponentStruct component, ref T toSerialize, T defaultComponent) where T : Component {
    //Serialize the public properties of this engine component
    PropertyInfo[] info = GetAssemblyType(component.typeName).GetProperties();
    component.enginePropertyNames = new List<string>();
    component.enginePropertyValues = new List<string>();
    for (int j = 0; j < info.Length; j++) {
      if(info[j].Name == "mesh") { continue; }
      string json = serializeProperty(info[j], ref toSerialize);
      string defaultJson = serializeProperty(info[j], ref defaultComponent);
      if (json != "" && json != defaultJson) {
        component.enginePropertyNames.Add(info[j].Name);
        component.enginePropertyValues.Add(json);
      }
    }
  }

  static string serializeProperty<T>(PropertyInfo info, ref T toSerialize) where T : Component {
    if (info.CanRead) {
      object[] Attributes = info.GetCustomAttributes(false);
      if (!(Attributes != null && Attributes.Length > 0 && Attributes[0] is ObsoleteAttribute)) {
        try {
          object obj = info.GetValue(toSerialize, null);
          string componentJson = JsonUtility.ToJson(obj);
          if (componentJson == "{}") {
            if (obj is System.Single) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((System.Single)obj));
            } else if (obj is System.Single[]) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((System.Single[])obj));
            } else if (obj is System.Boolean) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((System.Boolean)obj));
            } else if (obj is System.Int32) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((System.Int32)obj));
            } else if (obj is System.UInt32) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((System.UInt32)obj));
            } else if (obj is System.String) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((System.String)obj));
            } else if (obj is Rect) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((Rect)obj));
            } else if (obj is Bounds) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((Bounds)obj));
            } else if (obj is UnityEngine.SceneManagement.Scene) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((UnityEngine.SceneManagement.Scene)obj));
            } else if (obj is Camera[]) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((Camera[])obj));
            } else if (obj is Material[]) {
              componentJson = JsonUtility.ToJson(createPrimWrapper((Material[])obj));
            } else {
              Debug.LogWarning("Serialization Primitive not supported... yet: " + obj.GetType(), toSerialize);
            }
          }
          if (componentJson != string.Empty && componentJson != "{}" && info.CanWrite) {
            return componentJson;
          }
        } catch (ArgumentException e) {
          //If it's an engine component, try serializing the reference (instance id)
          try {
            object obj = info.GetValue(toSerialize, null);
            string componentJson = JsonUtility.ToJson(createPrimWrapper((obj as UnityEngine.Object).GetInstanceID()));
            if (componentJson != string.Empty && componentJson != "{}" && info.CanWrite) {
              return componentJson;
            }
          } catch {
            Debug.LogWarning(toSerialize.name + "'s " + info.Name + "'s reference failed to serialize with: \n" + e, toSerialize);
          }
        } catch (Exception e){
          if (toSerialize != null) {
            Debug.LogWarning(toSerialize.name + "'s " + info.Name + "'s reference failed to serialize with: \n" + e, toSerialize);
          } else {
            Debug.LogWarning("toSerialize is null!");
          }
        }
      }
    }
    return "";
  }

  //DESERIALIZE FROM A List<TransformStruct> -------------------------------------------------------------------------------
  public static List<TransformStruct> deserializeTransformHierarchy(string json) {
    Dictionary<int, UnityEngine.Object> referenceDictionary = new Dictionary<int, UnityEngine.Object>();
    PrimitiveWrapper<List<TransformStruct>> transformStructWrapper = new PrimitiveWrapper<List<TransformStruct>>();
    transformStructWrapper = JsonUtility.FromJson<PrimitiveWrapper<List<TransformStruct>>>(json);
    createTransformHierarchy(ref transformStructWrapper.value__, ref referenceDictionary);
    fillComponents(transformStructWrapper.value__, ref referenceDictionary);
    return transformStructWrapper.value__;
  }

  static void createTransformHierarchy(ref List<TransformStruct> transforms, ref Dictionary<int, UnityEngine.Object> referenceDictionary, int transformIndex = 0, Transform parent = null) {
    GameObject deserializedGameObject = new GameObject(transforms[transformIndex].name);
    //Set the transform information
    deserializedGameObject.transform.parent = parent;
    deserializedGameObject.transform.localPosition = transforms[transformIndex].localPosition;
    deserializedGameObject.transform.localRotation = transforms[transformIndex].localRotation;
    deserializedGameObject.transform.localScale = transforms[transformIndex].localScale;
    referenceDictionary.Add(transforms[transformIndex].instanceID, deserializedGameObject.transform);

    //Add the skeletal components
    for (int i = 0; i < transforms[transformIndex].components.Length; i++) {
      string curType = transforms[transformIndex].components[i].typeName;
      referenceDictionary.Add(transforms[transformIndex].components[i].instanceID,
        deserializedGameObject.AddComponent(GetAssemblyType(curType)));
    }

    //Deserialize the children
    for (int i = 0; i < transforms[transformIndex].children.Length; i++) {
      createTransformHierarchy(ref transforms, ref referenceDictionary, transforms[transformIndex].children[i], deserializedGameObject.transform);
    }
  }

  static void fillComponents(List<TransformStruct> transforms, ref Dictionary<int, UnityEngine.Object> referenceDictionary) {
    for (int i = 0; i < transforms.Count; i++) {
      for (int j = 0; j < transforms[i].components.Length; j++) {
        if (transforms[i].components[j].sanitizedJson != string.Empty) {
          JsonUtility.FromJsonOverwrite(fixReferences(transforms[i].components[j].sanitizedJson, referenceDictionary), referenceDictionary[transforms[i].components[j].instanceID]);
        } else {
          deserializeEngineComponent(ref transforms[i].components[j], referenceDictionary[transforms[i].components[j].instanceID], ref referenceDictionary);
        }
      }
    }
  }

  static void deserializeEngineComponent<T>(ref ComponentStruct component, T toFill, ref Dictionary<int, UnityEngine.Object> referenceDictionary) where T : UnityEngine.Object {
    //Deserialize the properties of this engine component
    for (int i = 0; i < component.enginePropertyNames.Count; i++) {
      PropertyInfo info = toFill.GetType().GetProperty(component.enginePropertyNames[i]);
      try {
        object obj = info.GetValue(toFill, null);
        string componentJson = JsonUtility.ToJson(obj);
        if (componentJson != "{}") {
          try {
            info.SetValue(toFill, JsonUtility.FromJson(fixReferences(component.enginePropertyValues[i], referenceDictionary), obj.GetType()), null);
          } catch (NullReferenceException e) {
            try {
              UnityEngine.Object referencedComponent;
              if (referenceDictionary.TryGetValue(JsonUtility.FromJson<PrimitiveWrapper<int>>(component.enginePropertyValues[i]).value__, out referencedComponent)) {
                info.SetValue(toFill, referencedComponent, null);
                if (toFill is MeshFilter){// && info.Name == "sharedMesh") {
                  Debug.Log("SHARED MESH COMPONENT: " + info.Name);/// referencedComponent.GetInstanceID());
                  //MeshFilter filter = toFill as MeshFilter;
                  //filter.mesh = referencedComponent as Mesh;
                }
                Debug.Log("MESH FILTER?: " + (toFill as Component).name);
              }
            } catch {
              Debug.LogWarning(toFill.name + "'s " + info.Name + "'s reference failed to serialize with: \n" + e, toFill);
            }
          }
        } else {
          if (obj is System.Single) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<System.Single>>(component.enginePropertyValues[i]).value__, null);
          } else if (obj is System.Single[]) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<System.Single[]>>(component.enginePropertyValues[i]).value__, null);
          } else if (obj is System.Boolean) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<System.Boolean>>(component.enginePropertyValues[i]).value__, null);
          } else if (obj is System.Int32) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<System.Int32>>(component.enginePropertyValues[i]).value__, null);
          } else if (obj is System.UInt32) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<System.UInt32>>(component.enginePropertyValues[i]).value__, null);
          } else if (obj is System.String) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<System.String>>(component.enginePropertyValues[i]).value__, null);
          } else if (obj is Rect) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<Rect>>(component.enginePropertyValues[i]).value__, null);
          } else if (obj is Bounds) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<Bounds>>(component.enginePropertyValues[i]).value__, null);
          } else if (obj is UnityEngine.SceneManagement.Scene) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<UnityEngine.SceneManagement.Scene>>(component.enginePropertyValues[i]).value__, null);
          } else if (obj is Camera[]) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<Camera[]>>(fixReferences(component.enginePropertyValues[i], referenceDictionary)).value__, null);
          } else if (obj is Material[]) {
            info.SetValue(toFill, JsonUtility.FromJson<PrimitiveWrapper<Material[]>>(fixReferences(component.enginePropertyValues[i], referenceDictionary)).value__, null);
          }
        }
      } catch { }
    }
  }

  static string fixReferences(string json, Dictionary<int, UnityEngine.Object> referenceDictionary) {
    //Replace the instance IDs in this string with the IDs of components from our dictionary
    //Want to match strings like: {\"instanceID\":12148
    string pattern = "{\"instanceID\":-?[0-9]+";
    return
    Regex.Replace(json, pattern, new MatchEvaluator((match) => {
      //Debug.Log("Match: " + match.Value + " at index [" + match.Index + ", " + (match.Index + match.Length) + "]");
      UnityEngine.Object comp;
      if (referenceDictionary.TryGetValue(int.Parse(match.Value.Substring(14)), out comp)) {
        return "{\"instanceID\":" + comp.GetInstanceID();
      } else {
        return match.Value;
      }
    }));
  }

  public static Type GetAssemblyType(string typeName) {
    var type = Type.GetType(typeName);
    if (type != null) return type;
    foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) {
      type = a.GetType(typeName);
      if (type != null) return type;
    }
    return null;
  }
}
