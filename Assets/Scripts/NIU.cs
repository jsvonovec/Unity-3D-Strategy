using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Reflection;



/// <remarks>
/// Version 1.2
/// 
/// - changed Within() so that values can swap if min > max
/// - Added BlendingColorOverTime()
/// - Added descriptions for FindObjectWithTag and FindObjectsWithTag
/// - Changed HighestAncestorOf so it says it's deprecated
/// - Changed PlurY and PlurS so that (hopefully) they can now accept floats as well as ints
/// - Added FindObjectNamed
/// - Created regions
/// - Added Within(Vector3)
/// - Updated Within(Vector2) to just use Within(float) for each of its values
/// - V3Round and V2Round, V3RoundToInt
/// - V3Near
///</remarks>

/// <summary>
/// A collection of regular functions that are, for whatever reason, Not In Unity.
/// </summary>
public static class NIU
{
    #region Various
    public enum XYZ { X, Y, Z }
    #endregion


    #region Find _____ Named "____" and FindObjectWithTag
    /// <summary>
    /// Returns the first child found named 'name'. Goes down list on heirarchy.
    /// </summary>
    /// <param name="c">Type 'this'</param>
    /// <param name="name">The name of the child, as a string</param>
    /// <param name="exact">True: Strings must match exactly. False: name must only CONTAIN the input.</param>
    /// <param name="nullError">If true, it will Debug.Log() an error stating it found no children of that name.</param>
    /// <returns></returns>
    public static GameObject FindChildNamed(Component c, string name, bool nullError = false)
    {
        GameObject child = c?.transform?.Find(name)?.gameObject;

        if(nullError && child == null)
            Debug.LogWarning("We didn't find child named " + name + " for GameObject " + c?.name + "!");

        return child;

    }
    public static GameObject FindChildNamed(GameObject c, string name, bool nullError = false)
    {
        GameObject child = c?.transform?.Find(name)?.gameObject;

        if (nullError && child == null)
            Debug.LogWarning("We didn't find child named " + name + " for GameObject " + c?.name + "!");
 
        return child;

    }

    /// <summary>
    /// Returns the first object named [name].
    /// </summary>
    /// <param name="name">The name of the object, as a string.</param>
    /// <param name="nullError">If true, it will Debug.Log() an error stating it found no object of that name.</param>
    /// <returns></returns>
    public static GameObject FindObjectNamed(string name, bool nullError = false)
    {
        GameObject[] o = Object.FindObjectsOfType<GameObject>();

        foreach(GameObject ob in o)
        {
            if (ob.name == name)
                return ob;
        }

        if (nullError)
            Debug.LogWarning("We didn't find an object named " + name + "!");

        return null;

    }


    /// <summary>
    /// Finds one loaded, GameObject object in the scene with the tag [tag] using FindObject<>().
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static GameObject FindObjectWithTag(string tag)
    {
        GameObject[] allObj;
        //DIFFERENCE

        allObj = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObj)
        {
            if (go.CompareTag(tag))
                //DIFFERENCE
                return go;
        }

        //DIFFERENCE
        return null;
    }

    /// <summary>
    /// Finds all loaded, active GameObjects in the scene with the tag [tag] using FindObjects<>().
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static GameObject[] FindObjectsWithTag(string tag)
    {
        GameObject[] allObj;
        //DIFFERENCE
        List<GameObject> result = new List<GameObject>();

        allObj = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObj)
        {
            if (go.CompareTag(tag))
                //DIFFERENCE
                result.Add(go);
        }
        //DIFFERENCE
        return result.ToArray();
    }
    #endregion


    #region Randomness - Shuffle and Random with or without ranges
    /// <summary>
    /// Shuffles a list randomly, and returns the shuffled list. Doesn't affect the original list.
    /// </summary>
    /// <param name="list">The list to shuffle.</param>
    /// <returns></returns>
    public static List<T> Shuffle<T>(List<T> list)
    {
        List<T> oldList = new List<T>();
        oldList.AddRange(list);
        List<T> newList = new List<T>();

        int i = 0;
        //Pick a random item to add to new list
        while(oldList.Count > 0 && i < 1000)
        {
            //Random number between 0 and max number
            int r = Random(oldList.Count - 1);

            //add that number to the new list
            newList.Add(oldList[r]);
            oldList.RemoveAt(r);

            i++;

        }

        if (i > list.Count)
        {
            Debug.LogException(new UnityException("Shuffle() looped too many times."));
        }

        return newList;

    }
    /// <summary>
    /// Shuffles a list randomly. Changes the original list.
    /// </summary>
    /// <param name="list">The list to shuffle.</param>
    /// <returns></returns>
    public static void Shuffle<T>(ref List<T> list)
    {
        List<T> oldList = new List<T>();
        oldList.AddRange(list);
        List<T> newList = new List<T>();

        int i = 0;
        //Pick a random item to add to new list
        while (oldList.Count > 0 && i < 100)
        {
            //Random number between 0 and max number
            int r = Random(oldList.Count - 1);

            //add that number to the new list
            newList.Add(oldList[r]);
            oldList.RemoveAt(r);

            i++;

        }

        if (i > list.Count)
        {
            T t = default;
            Debug.LogWarning("Warning: we shuffled more items than we have? (Type " + t.GetType().ToString() + ")");
        }

        list = newList;

    }



    /// <summary>
    /// Returns a random int between 0 and max inclusive.
    /// </summary>
    /// <param name="max">THe maximum possible value.</param>
    /// <returns></returns>
    public static int Random(int max)
    {
        return Mathf.Min(Mathf.FloorToInt(UnityEngine.Random.value * (max + 1)), max);
    }
    /// <summary>
    /// Returns a random floating point number between 0 and max inclusive.
    /// </summary>
    /// <param name="max">THe maximum possible value.</param>
    /// <returns></returns>
    public static float Random(float max)
    {
        return UnityEngine.Random.value * max;
    }



    public static int RandomR(int min, int max)
    {
        if(min > max)
        {
            int m = min;
            min = max;
            max = m;
        }

        return (int)Mathf.Floor((UnityEngine.Random.value * (max - min + 1f)) + min);
    }
    public static float RandomR(float min, float max)
    {
        if (min > max)
        {
            float m = min;
            min = max;
            max = m;
        }

        return (UnityEngine.Random.value * (max - min)) + min;
    }
    #endregion


    #region Quaternion whose values are all __
    /// <summary>
    /// Returns a Quaternion where every value is the inputted float.
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static Quaternion QuaternionAll(float f)
    {
        f = f % 1;
        return new Quaternion(f, f, f, f);
    }
    /// <summary>
    /// Returns a Quaternoin of values (0,0,0,0).
    /// </summary>
    /// <returns></returns>
    public static Quaternion QuaternionAll()
    {
        return new Quaternion();
    }
    #endregion


    #region List stuff - DestroyAll, RandomMember(s)... (Shuffle is in Randomness)
    /// <summary>
    /// Destroys every gameObject related to each item in the list and modifies the list.
    /// </summary>
    /// <param name="list">List of objects to destroy. This list will be cleared after the deed is done.</param>
    public static void DestroyAll<T>(ref List<T> list)
    {

        List<T> hitlist = new List<T>();
        hitlist.AddRange(list);

        list.Clear();

        
        //int i = 0;
        foreach(T item in hitlist)
        {
            //Debug.Log(list.Count);
            if (item is Component) Object.Destroy((item as Component).gameObject);
            else if (item is GameObject) Object.Destroy((item as GameObject).gameObject);
            else if (item is MonoBehaviour) Object.Destroy((item as MonoBehaviour).gameObject);
            else if (item is Object) Object.Destroy(item as Object);
            else
            {
                Debug.LogWarning("We can't do this Type: " + item.GetType().ToString());
                return;
            }




            /*i++;
            if (i > 200)
            {
                Debug.LogError("Caught in while loop for list length " + list.Count);
                break;
            }*/
        }
    }
    /// <summary>
    /// Destroys every gameObject related to each item in the list. You'll still have to clear the list if you're using it for anything else.
    /// </summary>
    /// <param name="list">List of objects to destroy.</param>
    public static void DestroyAll<T>(List<T> list)
    {

        List<T> newlist = new List<T>();
        newlist.AddRange(list);



        //int i = 0;
        foreach (T item in newlist)
        {
            //Debug.Log(list.Count);

            list.Remove(item);
            if (item is Component) Object.Destroy((item as Component).gameObject);
            else if (item is GameObject) Object.Destroy((item as GameObject).gameObject);
            else if (item is MonoBehaviour) Object.Destroy((item as MonoBehaviour).gameObject);
            else
            {
                Debug.Log("We can't do this Type: " + item.GetType().ToString());
                return;
            }




            /*i++;
            if (i > 1000)
            {
                Debug.LogError("Caught in while loop for list length " + list.Count);
                break;
            }*/
        }
    }



    /// <summary>
    /// Returns a random member of the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T RandomMember<T>(List<T> list)
    {
        return list[Random(list.Count - 1)];
    }

    /// <summary>
    /// Returns count random members of the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static List<T> RandomMembers<T>(List<T> list, int count)
    {
        //Just shuffle the list if we're asking for too much
        if (count >= list.Count)
            return Shuffle(list);

        //If count isn't good, skip process
        if(count < 1)
        {
            Debug.LogWarning("We inputted a count of " + count + " into RandomMembers().");
            return null;
        }

        List<T> newList = new List<T>();

        for(int i = 0; i < count; i++)
        {
            T item = RandomMember(list);

            newList.Add(item);
            list.Remove(item);
        }

        return newList;
    }


    /// <summary>
    /// Removes the duplicate things in a list.
    /// </summary>
    /// <typeparam name="IEquatable"></typeparam>
    /// <param name="list"></param>
    public static void RemoveDuplicates<IEquatable>(ref List<IEquatable> list)
    {
        for(int i = list.Count - 1; i > 0; i--)
        {
            IEquatable t = list[i];
            for(int j = i - 1; j >= 0; j--)
            {
                if (list[j].Equals(t))
                {
                    list.RemoveAt(i);
                    break;
                }
            }
        }
    }
    #endregion


    #region Broadcasting
    /// <summary>
    /// Calls the function named METHOD with parameter as argument in every script that's currently enabled on the scene.
    /// </summary>
    public static void BroadcastWorld(string METHOD, Object parameter = null, SendMessageOptions option = SendMessageOptions.DontRequireReceiver, string tag = null)
    {
        List<GameObject> scriptHolders;
        scriptHolders = new List<GameObject>();
        
        foreach (MonoBehaviour m in Object.FindObjectsOfType<MonoBehaviour>())
        {
            GameObject root = HighestAncestorOf(m.gameObject, tag);

            if (!scriptHolders.Contains(root) && (tag == null || root.CompareTag(tag)))
            {
                scriptHolders.Add(root);

                if (parameter == null)
                    root.BroadcastMessage(METHOD, option);
                else
                    root.BroadcastMessage(METHOD, parameter, option);
            }
        }
    }
    #endregion


    #region Vector Operations: Vec3ByV2nZ, Vec3IgnoreY, Rounding, Path Length

    #region Getting a Vector3 by adding or subtracting one of the components

    [System.Obsolete("This is obsolete. You should really use V2AndFloat(Vector2, XYZ.Z, value) isntead.")]
    /// <summary>
    /// Creates a new Vector3 where the x and y components are from a Vector2 and the z component is tacked on. If z = 0, just use the Vector2 on its own.
    /// </summary>
    /// <param name="v2"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Vector3 Vec3ByV2nZ(Vector2 v2, float z)
    {
        return new Vector3(v2.x, v2.y, z);
    }

    [System.Obsolete("This is obsolete. You should really use V2AndFloat(Vector2, XYZ.Y, value) isntead.")]
    /// <summary>
    /// Creates a new Vector3 where the x and z components are from a Vector2 and the y component is tacked on.
    /// </summary>
    /// <param name="v2"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Vector3 Vec3ByV2nY(Vector2 v2, float y)
    {
        return new Vector3(v2.x, y, v2.y);
    }

    /// <summary>
    /// Creates a new Vector3 where two of the components are from a Vector2 and the remaining component is tacked on.
    /// </summary>
    /// <param name="v2">The Vector2 whose two values are becoming xy, xz, or yz components.</param>
    /// <param name="xyz">Which component should our tacked on value be: X, Y, or Z?</param>
    /// <param name="value">The value for the component of the new Vector3 we just chose.</param>
    /// <returns></returns>
    public static Vector3 V2AndFloat(Vector2 v2, XYZ xyz, float value = 0f)
    {
        if(xyz == XYZ.X)
            return new Vector3(value, v2.x, v2.y);
        else if (xyz == XYZ.Y)
            return new Vector3( v2.x, value, v2.y);
        else
            return new Vector3(v2.x, v2.y, value);
    }
    /// <summary>
    /// Creates a new Vector3 where two of the components are from a Vector2 and the remaining component is tacked on.
    /// </summary>
    /// <param name="v3">The Vector2 whose two values are becoming xy, xz, or yz components.</param>
    /// <param name="xyz">Which component should our tacked on value be: X, Y, or Z?</param>
    /// <param name="value">The value for the component of the new Vector3 we just chose.</param>
    /// <returns></returns>
    public static Vector3 V3AndFloat(Vector3 v3, XYZ xyz, float value = 0f)
    {
        if (xyz == XYZ.X)
            return new Vector3(value, v3.y, v3.z);
        else if (xyz == XYZ.Y)
            return new Vector3(v3.x, value, v3.z);
        else
            return new Vector3(v3.x, v3.y, value);
    }

    /// <summary>
    /// Put in a Vector3, spits out a Vector2. Which is just the Vector3 without the y component.
    /// </summary>
    /// <param name="v3"></param>
    /// <returns></returns>
    public static Vector2 Vec3IgnoreY(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }
    #endregion


    #region Vectors: Rounding and Near
    /// <summary>
    /// Rounds each component of a Vector3 to its nearest whole number.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3 V3Round(Vector3 vector)
    {
        return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
    }
    /// <summary>
    /// Rounds each component of a Vector3 to its nearest whole number. Changes the original vector.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3 V3Round(ref Vector3 vector)
    {
        return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
    }

    /// <summary>
    /// Rounds each component of a Vector3 to its nearest whole number.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3Int V3RoundToInt(Vector3 vector)
    {
        return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
    }
    /// <summary>
    /// Rounds each component of a Vector3 to its nearest whole number. Changes the original vector.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3Int V3RoundToInt(ref Vector3 vector)
    {
        return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
    }

    /// <summary>
    /// Rounds each component of a Vector2 to its nearest whole number.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector2 V2Round(Vector2 vector)
    {
        return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
    }
    /// <summary>
    /// Rounds each component of a Vector2 to its nearest whole number. Changes the original vector.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector2 V2Round(ref Vector2 vector)
    {
        return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
    }

    /// <summary>
    /// Returns true if the two Vector3s are within maxDistance of each other. Found using a straight line between the two.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    public static bool V3Near(Vector3 a, Vector3 b, float maxDistance)
    {
        if (Vector3.Distance(a, b) <= maxDistance)
            return true;
        return false;
    }
    #endregion


    #region Vectors: Path Length - Finding the total distance a path of Vector3s travel
    /// <summary>
    /// Finds the total distance between a set of Vector3s in order, like a string pulled through all of them, or a path.
    /// </summary>
    /// <param name="corners"></param>
    /// <returns></returns>
    public static float Vector3StringLength(Vector3[] corners)
    {
        float v = 0f;
        for(int i = 0; i < corners.Length - 1; i++)
        {
            v += Vector3.Distance(corners[i], corners[i + 1]);
        }

        return v;
    }

    /// <summary>
    /// Finds the total distance between a set of Vector2s in order, like a string pulled through all of them, or a path.
    /// </summary>
    /// <param name="corners"></param>
    /// <returns></returns>
    public static float Vector2StringLength(Vector2[] corners)
    {
        float v = 0f;
        for (int i = 0; i < corners.Length - 1; i++)
        {
            v += Vector2.Distance(corners[i], corners[i + 1]);
        }

        return v;
    }

    /// <summary>
    /// Finds the total distance between a set of Vector3s in order, like a string pulled through all of them, or a path.
    /// All corners are projected onto the given plane.
    /// </summary>
    /// <param name="corners">The list of corners of the string/path.</param>
    /// <param name="plane">The normal vector of the plane we're projecting onto.</param>
    /// <returns></returns>
    public static float Vector3StringLengthAlongPlane(Vector3 plane, params Vector3[] corners)
    {
        float v = 0f;
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 r = corners[i + 1] - corners[i];
            v += r.magnitude * Mathf.Sin(Vector3.Angle(plane, r) * Mathf.Deg2Rad);
        }

        return v;
    }

    #endregion



    /// <summary>
    /// Returns the Vector3 casted position in the world that the mouse is currently pointing at.
    /// </summary>
    /// <param name="maxDistance">Maximum distance for mouse to cast.</param>
    /// <param name="layerMask">The LayerMask to apply to the Raycast from the camera. If left at default, will not use a LayerMask.</param>
    /// <returns></returns>
    public static Vector3 MouseWorldPos(float maxDistance = -1f, int layerMask = -14)
    {
        if (maxDistance == -1f)
            maxDistance = 1000f;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rh;

        //no layermask
        if (layerMask == -14)
            Physics.Raycast(ray, out rh,maxDistance);
        //layermask
        else
            Physics.Raycast(ray, out rh, maxDistance, layerMask);

        return rh.point;
    }
    #endregion


    #region Children and Ancestor of ________
    /// <summary>
    /// Returns a list of every gameobject that's child of this one, and all children of their children.
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="cycle">Don't touch this!</param>
    /// <returns></returns>
    public static List<GameObject> EveryChildOf(GameObject orig, int cycle = 0, bool onlyFirstChildren = false)
    {
        cycle++;

        if (cycle > 50)
        {
            Debug.LogError("I just saved you from crashing. Currently on " + orig.name + ".");
            return null;
        }


        List<GameObject> children = new List<GameObject>();

        //If this child has children, do this exact function with that child. (please don't crash)
        for (int i = 0; i < orig.transform.childCount; i++)
        {
            //Repeat function for child
            if (!onlyFirstChildren && orig.transform.GetChild(i).childCount > 0)
                children.AddRange(EveryChildOf(orig.transform.GetChild(i).gameObject, cycle));
            //Add this child
            children.Add(orig.transform.GetChild(i).gameObject);
        }
        //Debug.Log("Cycle: " + cycle.ToString());
        return children;
    }


    /// <summary>
    /// DEPRECATED: Returns the outermost ancestor GameObject of a given GameObject. (just obj.transform.root!)
    /// </summary>
    /// <param name="obj">The GameObject to find the highest ancestor for.</param>
    /// <returns></returns>
    public static GameObject HighestAncestorOf(GameObject obj)
    {
        return obj.transform.root.gameObject;
    }
    /// <summary>
    /// Returns the outermost ancestor of a given GameObject with a given tag.
    /// </summary>
    /// <param name="obj">The GameObject to find the highest ancestor for.</param>
    /// <param name="tag">The tag to compare each parent with.</param>
    /// <returns></returns>
    public static GameObject HighestAncestorOf(GameObject obj, string tag)
    {
        //skip if there's no tag
        if (tag == null || tag == "")
            return HighestAncestorOf(obj);
        if (obj == null)
            return null;
        GameObject bestParent = null;

        if (obj.transform.parent != null)
            bestParent = HighestAncestorOf(obj.transform.parent.gameObject, tag);

        if (bestParent == null && obj.CompareTag(tag))
            return obj;

        return bestParent;
    }
    #endregion


    #region Is x between y and z? (float, int, Vector2, Vector3s, Vector3 with radius)
    /// <summary>
    /// Returns true if value is within bounds inclusive, false if not. If min isn't the lower number, the values will swap.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool Within(float value, float min, float max)
    {
        //flip values
        if(min > max)
        {
            float m = max;
            max = min;
            min = m;
        }

        return value >= min && value <= max;
    }
    /// <summary>
    /// Returns true if value is within bounds inclusive, false if not. If min isn't the lower number, the values will swap.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool Within(int value, int min, int max)
    {
        //flip values
        if (min > max)
        {
            int m = max;
            max = min;
            min = m;
        }

        return value >= min && value <= max;
    }

    /// <summary>
    /// Returns true if vector2 is between a square created by two other vector2s.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool Within(Vector2 value, Vector2 corner1, Vector2 corner2)
    {
        return Within(value.x, Mathf.Min(corner1.x, corner2.x), Mathf.Max(corner1.x, corner2.x))
            && Within(value.y, Mathf.Min(corner1.y, corner2.y), Mathf.Max(corner1.y, corner2.y));
    }
    /// <summary>
    /// Returns true if vector3 is between a cube created by two other vector3s.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool Within(Vector3 value, Vector3 corner1, Vector3 corner2)
    {
        return Within(value.x, Mathf.Min(corner1.x, corner2.x), Mathf.Max(corner1.x, corner2.x))
            && Within(value.y, Mathf.Min(corner1.y, corner2.y), Mathf.Max(corner1.y, corner2.y))
            && Within(value.z, Mathf.Min(corner1.z, corner2.z), Mathf.Max(corner1.z, corner2.z));
    }
    #endregion


    #region String grammatical stuff - -y vs -ies, -s vs no -s
    /// <summary>
    /// Used in strings and making things look nice. Returns 'y' if there's only 1 or -1, returns 'ies' for everything else.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string PlurY(float number)
    {
        if (number == 1 || number == -1)
            return "y";
        return "ies";
    }

    /// <summary>
    /// Used in strings and making things look nice. Returns nothing if there's only one, returns 's' for everything else.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string PlurS(float number)
    {
        if (number == 1 || number == -1)
            return "";
        return "s";
    }
    #endregion


    #region Bonus Debug - DebugDot, DebugConnectDots
    /// <summary>
    /// Debug.Draws a 6-pointed star at the designated position.
    /// </summary>
    /// <param name="position">The center of the star.</param>
    /// <param name="color">The color of the star.</param>
    /// <param name="rad">The length that each point extends from the center. Should be small-ish, like 0.1f or so.</param>
    /// <param name="duration">How long the star should stay. If empty, will stay until next frame.</param>
    public static void DebugDot(Vector3 position, Color color = default, float rad = 0.5f, float duration = -1f)
    {
        if (color == default)
            color = Color.white;

        if (duration < 0f)
        {
            Debug.DrawLine(position + (Vector3.right * rad), position + (Vector3.left * rad), color);
            Debug.DrawLine(position + (Vector3.up * rad), position + (Vector3.down * rad), color);
            Debug.DrawLine(position + (Vector3.forward * rad), position + (Vector3.back * rad), color);
        }
        else
        {
            Debug.DrawLine(position + (Vector3.right * rad), position + (Vector3.left * rad), color, duration);
            Debug.DrawLine(position + (Vector3.up * rad), position + (Vector3.down * rad), color, duration);
            Debug.DrawLine(position + (Vector3.forward * rad), position + (Vector3.back * rad), color, duration);
        }
    }

    /// <summary>
    /// Debug.DrawLines between several given positions. Will do nothing if only given one point.
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="color"></param>
    /// <param name="duration"></param>
    public static void DebugConnectDots(Vector3[] positions, Color color = default, float duration = -1f)
    {
        if (color == default)
            color = Color.white;

        for (int i = 0; i < positions.Length - 1; i++)
        {
            //No duration
            if (duration <= 0f)
            {
                Debug.DrawLine(positions[i], positions[i + 1], color);
            }

            //Some duration (defaults to white if there's no specified color, I'm pretty sure)
            else
            {
                Debug.DrawLine(positions[i], positions[i + 1], color, duration);
            }
        }
    }
    #endregion


    #region Colors - Random, Blending, Proximity
    /// <summary>
    /// Returns a random color with alpha randomly between min and max alpha.
    /// </summary>
    /// <param name="minAlpha"></param>
    /// <param name="maxAlpha"></param>
    /// <returns></returns>
    public static Color RandomColor(float minAlpha = 1, float maxAlpha = 1)
    {
        float alphaValue = Random(maxAlpha - minAlpha) + minAlpha;

        return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, alphaValue);
    }

    /// <summary>
    /// Returns a color that uses sin(Time.time) - over time, it blends over reds, greens and blues fluidly.
    /// </summary>
    /// <returns></returns>
    public static Color BlendingColorOverTime()
    {
        return new Color(Mathf.Sin(Time.time * 2.3f) / 2f + .5f, Mathf.Sin(Time.time * 1.8f) / 2f + .5f, Mathf.Sin(Time.time * 1f) / 2f + .5f);
    }

    /// <summary>
    /// Returns true if any color is within proximity of any other color of the list.
    /// </summary>
    /// <param name="colors">List of colors to check.</param>
    /// <param name="proximity">If we wanted to change one color to the other by adding to their RGB values, would we be able to reach it in this much change?</param>
    /// <returns></returns>
    public static bool ColorsTooClose(float proximity, params Color[] colors)
    {
        for(int i = 0; i < colors.Length; i++)
        {
            for(int j = 0; j < colors.Length; j++)
            {
                if (i == j) continue;

                if (Mathf.Abs(colors[i].r - colors[j].r)
                    + Mathf.Abs(colors[i].g - colors[j].g)
                    + Mathf.Abs(colors[i].b - colors[j].b) < proximity)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if color is within proximity of any of the colors of the list.
    /// </summary>
    /// <param name="color">The one color we want to compare to all the others.</param>
    /// <param name="colors">List of colors to check.</param>
    /// <param name="proximity">If we wanted to change one color to the other by adding to their RGB values, would we be able to reach it in this much change?</param>
    /// <returns></returns>
    public static bool ColorTooCloseToOthers(Color color, float proximity, params Color[] colors)
    {
        for (int j = 0; j < colors.Length; j++)
        {
            if (Mathf.Abs(color.r - colors[j].r)
                + Mathf.Abs(color.g - colors[j].g)
                + Mathf.Abs(color.b - colors[j].b) < proximity)
            {
                /*Debug.Log("Color is within proximity. Proximity: " + (Mathf.Abs(color.r - colors[j].r)
                + Mathf.Abs(color.g - colors[j].g)
                + Mathf.Abs(color.b - colors[j].b)));*/
                return true;
            }

            /*else Debug.Log("Color is far. Proximity: " + (Mathf.Abs(color.r - colors[j].r)
                + Mathf.Abs(color.g - colors[j].g)
                + Mathf.Abs(color.b - colors[j].b)));*/
        }

        return false;
    }
    #endregion


    #region Bonus mathematics - summation
    /// <summary>
    /// Returns the sum of all floats in a list.
    /// </summary>
    /// <param name="list">List of floats</param>
    /// <returns></returns>
    public static float Sum(List<float> list)
    {
        float r = 0;
        foreach (float f in list)
            r += f;

        return r;
    }

    /// <summary>
    /// Returns the sum of all ints in a list.
    /// </summary>
    /// <param name="list">List of ints</param>
    /// <returns></returns>
    public static int Sum(List<int> list)
    {
        int r = 0;
        foreach (int i in list)
            r += i;

        return r;
    }
    /*
    /// <summary>
    /// Uses Bresenham's line algorithm to find an n-dimensional raster of best fit for a line. 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public List<float[]> BresenhamLine(NavMeshPath path)
    {
        Vector3[] corners = path.corners;
    }
    */
    #endregion




}
