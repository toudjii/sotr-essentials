//Anthony Ackermans
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{
    public class PickedObject
    {
        public GameObject TheGameObject;
        public Vector3 OriginalPosition;
        public Quaternion OriginalRotation;
        public Vector3 OriginalScale;
        public PrefabAssetType AssetType;

        public PickedObject(GameObject gameObject)
        {
            this.TheGameObject = gameObject;
            this.OriginalPosition = TheGameObject.GetComponent<Transform>().position;
            this.OriginalRotation = TheGameObject.GetComponent<Transform>().rotation;
            this.OriginalScale = TheGameObject.GetComponent<Transform>().localScale;
            this.AssetType = PrefabUtility.GetPrefabAssetType(gameObject);
        }
    }
}