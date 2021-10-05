using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    /// <summary>
    /// Divide every member of the vector3 object with the values of the vector3 object in parameter respectively
    /// </summary>
    /// <param name="sourceVector"></param>
    /// <param name="dividingVector"></param>
    /// <returns></returns>
    public static Vector3 Divide(this Vector3 sourceVector, Vector3 dividingVector)
    {
        Vector3 result = new Vector3(sourceVector.x / dividingVector.x, sourceVector.y / dividingVector.y, sourceVector.z / dividingVector.z);
        return result;
    }
}
