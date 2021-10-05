using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{
    public class RadialArrayCreator : EditorWindow
    {
        // FIELDS
        private GameObject _objectToArray;
        private Transform _pivotPoint;
        private bool _previewVisible;
        private string[] _axis = new string[] { "XY", "XZ", "YZ" };
        private int _axisIndex;
        private string[] _lookAtAxle = new string[] { "X", "Y", "Z" };
        private int _lookAtAxleIndex = 2;
        private bool _parentToOrigin;
        private int _rings = 1;
        private float _ringOffset;
        private bool _isShowingPreview;
        private PrefabAssetType assetType;
        int _instancesAmount = 1;
        private float _distance = 0;
        private int _angleOffset = 0;
        private Material _previewMaterial;
        private List<GameObject> _allPreviewObjects = new List<GameObject>();
        private bool _isInstanciated;
        private bool _lookAtCenter;
        private bool _keepRingDensity;
        private bool _keepAsPrefab = true;
        private bool _avoidChildren;


        // Add menu item 
        [MenuItem("Tools/Productivity Toolkit/Radial Array Creator")]
        public static void ShowWindow()
        {
            RadialArrayCreator window = (RadialArrayCreator)GetWindow(typeof(RadialArrayCreator));
            window.titleContent = new GUIContent("Radial Array Creator");
            window.Show();
        }

        private void OnEnable()
        {
            _previewMaterial = new Material(Shader.Find("Unlit/Color"));
            _previewMaterial.color = Color.yellow;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField("Array Object", EditorStyles.boldLabel);
            _objectToArray = (GameObject)EditorGUILayout.ObjectField(_objectToArray, typeof(GameObject), true);
            if (GUILayout.Button("Add selected"))
            {
                _objectToArray = (GameObject)Selection.activeObject;
            }
            if (_objectToArray != null)
            {
                assetType = PrefabUtility.GetPrefabAssetType(_objectToArray);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField("Pivot Transform", EditorStyles.boldLabel);
            _pivotPoint = (Transform)EditorGUILayout.ObjectField(_pivotPoint, typeof(Transform), true);
            if (GUILayout.Button("Add selected"))
            {
                var selectedObject = (GameObject)Selection.activeObject;
                _pivotPoint = selectedObject.GetComponent<Transform>();
            }

            EditorGUILayout.EndVertical();
            _instancesAmount = EditorGUILayout.IntField("Instances:", _instancesAmount);
            _distance = EditorGUILayout.FloatField("Distance:", _distance);
            _angleOffset = EditorGUILayout.IntField("Angle Offset:", _angleOffset);
            _axisIndex = EditorGUILayout.Popup("Array Axis", _axisIndex, _axis);
            
            





            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField("Ring Transformation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            _rings = EditorGUILayout.IntField("Rings:", _rings);
            _keepRingDensity = EditorGUILayout.Toggle("Keep density per ring", _keepRingDensity);
            _ringOffset = EditorGUILayout.FloatField("Ring Offset Angle:", _ringOffset);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();


            // -------------- OPTIONS ------------- //
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            _lookAtCenter = EditorGUILayout.Toggle("Look at pivot object", _lookAtCenter);
            if (_lookAtCenter)
            {
                EditorGUI.indentLevel++;
                _lookAtAxleIndex = EditorGUILayout.Popup("Look at Axle", _lookAtAxleIndex, _lookAtAxle);
                EditorGUI.indentLevel--;
            }
            _parentToOrigin = EditorGUILayout.Toggle("Parent to pivot object", _parentToOrigin);
            _keepAsPrefab = EditorGUILayout.Toggle(new GUIContent("Keep as prefab", "If the object to be instanciated is a prefab, keep every copy as an instance of that prefab"), _keepAsPrefab);
            _avoidChildren = EditorGUILayout.Toggle(new GUIContent("Don't include children", "Avoid all children from the object to be instantiated. For prefabs, this function will be ignored as it is not possible to destroy prefab's children"), _avoidChildren);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            // ----------------------------------- //



            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField($"Total instances in array: {CalculateInstances()}"); 
            EditorGUILayout.EndVertical();
            ////////////////////////////////
            ////////////////////////////////

            GUILayout.FlexibleSpace();
            UtilitiesToolExtensions.DrawSplitter();
            _previewVisible = GUILayout.Toggle(_previewVisible, "Preview", "Button");
            if (_previewVisible && GUI.changed)
            {
                PreviewArray();
                _isShowingPreview = true;
            }
            else if (!_previewVisible && _isShowingPreview)
            {
                RemovePreviewArray();
                _isShowingPreview = false;
            }


            if (GUILayout.Button("Reset Values"))
            {
                ResetValues();
            }
            if (GUILayout.Button("Create array"))
            {
                CreateArray(false);
                _previewVisible = false;
            }
        }

        private int CalculateInstances()
        {
            int densityInstances = 0;
            if (_keepRingDensity)
            {
                for (int i = 0; i < _rings; i++)
                {

                    densityInstances += i * _instancesAmount;

                }
            }
            return _instancesAmount * _rings + (_keepRingDensity ? densityInstances : 0);
        }

        private void ResetValues()
        {
            _instancesAmount = 0;
            _distance = 0;
            _angleOffset = 0;
            _lookAtCenter = false;
            _parentToOrigin = false;
            _rings = 1;
            _keepRingDensity = false;
            _axisIndex = 0;
            _previewVisible = false;
            _objectToArray = null;
            _pivotPoint = null;
            _ringOffset = 0;
            _keepAsPrefab = true;
        }

        private void OnDestroy()
        {
            RemovePreviewArray();
        }

        private void CreateArray(bool isPreview)
        {
            float deltaAngle = 360f / (float)_instancesAmount;


            for (int r = 1; r <= _rings; r++)
            {
                for (int i = 0; i < _instancesAmount * (_keepRingDensity ? r : 1); i++)
                {
                    float angle = i * deltaAngle / (_keepRingDensity ? r : 1) + _ringOffset * r;
                    float cosAngle = Mathf.Cos((angle + _angleOffset) * Mathf.Deg2Rad);
                    float sinAngle = Mathf.Sin((angle + _angleOffset) * Mathf.Deg2Rad);

                    float xPos, yPos, zPos;

                    if (_axisIndex == 0) // XY
                    {
                        xPos = (cosAngle * _distance * r) + _pivotPoint.transform.position.x;
                        yPos = (sinAngle * _distance * r) + _pivotPoint.transform.position.y;
                        zPos = _pivotPoint.transform.position.z;
                    }
                    else if (_axisIndex == 1) //XZ
                    {
                        xPos = (cosAngle * _distance * r) + _pivotPoint.transform.position.x;
                        yPos = _pivotPoint.transform.position.y;
                        zPos = (sinAngle * _distance * r) + _pivotPoint.transform.position.z;
                    }
                    else // YZ
                    {
                        xPos = _pivotPoint.transform.position.x;
                        yPos = (cosAngle * _distance * r) + _pivotPoint.transform.position.y;
                        zPos = (sinAngle * _distance * r) + _pivotPoint.transform.position.z;
                    }


                    GameObject instantiatedObject = ObjectInstantiate();
                    instantiatedObject.transform.position = new Vector3(xPos, yPos, zPos);


                    if (_lookAtCenter)
                    {
                        LookAtArrayOrigin(_lookAtAxleIndex, instantiatedObject);
                    }

                    if (isPreview)
                    {
                        ObjectPreviewProperties(instantiatedObject);

                    }
                    else
                    {
                        Undo.RegisterCreatedObjectUndo(instantiatedObject, "Create Radial Array");
                    }
                }
            }


            _isInstanciated = true;
        }

        private GameObject ObjectInstantiate()
        {
            GameObject instantiatedObject;
            if (assetType == PrefabAssetType.NotAPrefab || !_keepAsPrefab)
            {
                instantiatedObject = _parentToOrigin ? Instantiate(_objectToArray, _pivotPoint) : Instantiate(_objectToArray);
            }
            else
            {
                var _objectToArrayPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(_objectToArray);
                GameObject _objecttoArrayPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(_objectToArrayPrefabPath, typeof(GameObject));
                instantiatedObject = _parentToOrigin ? (GameObject)PrefabUtility.InstantiatePrefab(_objecttoArrayPrefab, _pivotPoint) : (GameObject)PrefabUtility.InstantiatePrefab(_objecttoArrayPrefab);
            }

            if (_avoidChildren && assetType == PrefabAssetType.NotAPrefab)
            {
                var cc = instantiatedObject.transform.childCount-1;
                for (int i = cc; i >= 0; i--)
                {
                    DestroyImmediate(instantiatedObject.transform.GetChild(i).gameObject);

                }
            }
            return instantiatedObject;
        }


        private void LookAtArrayOrigin(int index, GameObject io)
        {
            io.transform.LookAt(_pivotPoint);
            if (index == 0)
            {
                io.transform.Rotate(Vector3.up, 90, Space.Self);
            }
            else if (index == 1)
            {
                io.transform.Rotate(Vector3.right, 90, Space.Self);
            }

        }


        private void RemovePreviewArray()
        {
            foreach (var go in _allPreviewObjects.ToArray())
            {
                DestroyImmediate(go);
            }
            _allPreviewObjects.Clear();
            _isInstanciated = false;
        }

        private void ObjectPreviewProperties(GameObject instantiatedObject)
        {
            instantiatedObject.hideFlags = HideFlags.HideInHierarchy;
            var allMeshRenderers = instantiatedObject.GetComponentsInChildren<Renderer>();
            foreach (var mr in allMeshRenderers)
            {
                mr.material = _previewMaterial;
            }
            _allPreviewObjects.Add(instantiatedObject);
        }


        private void PreviewArray()
        {
            if (_isInstanciated)
            {
                RemovePreviewArray();
            }
            CreateArray(true);
        }

    }
}