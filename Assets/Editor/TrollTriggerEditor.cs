using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TrollTrigger))]
public class TrollTriggerEditor : Editor
{
    private TrollTrigger trollTrigger;
    private bool showTriggerSettings = true;
    private bool showMovementSettings = true;
    private bool showPathSettings = true;
    private bool showCollisionSettings = true;
    private bool showVisualSettings = true;
    private bool showAudioSettings = false;

    private void OnEnable()
    {
        trollTrigger = (TrollTrigger)target;
        
        // Make sure we have a trigger zone
        if (trollTrigger.triggerZone == null)
        {
            trollTrigger.triggerZone = trollTrigger.GetComponent<BoxCollider>();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.Space(10);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 16;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("TROLL TRIGGER SYSTEM", headerStyle);
        EditorGUILayout.Space(10);

        // Quick Action Buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Trigger Zone", GUILayout.Height(30)))
        {
            CreateOrAdjustTriggerZone();
        }
        if (GUILayout.Button("Add Waypoint", GUILayout.Height(30)))
        {
            AddWaypoint();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Trigger Settings
        showTriggerSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showTriggerSettings, "Trigger Settings");
        if (showTriggerSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerZone"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetTag"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reusable"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("activationDelay"));
            
            // Trigger zone adjustment
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Trigger Size:", GUILayout.Width(100));
            if (GUILayout.Button("Small (2x2x2)")) SetTriggerSize(new Vector3(2, 2, 2));
            if (GUILayout.Button("Medium (5x5x5)")) SetTriggerSize(new Vector3(5, 5, 5));
            if (GUILayout.Button("Large (10x10x10)")) SetTriggerSize(new Vector3(10, 10, 10));
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // Movement Settings
        showMovementSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMovementSettings, "Movement Settings");
        if (showMovementSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movingObject"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementCurve"));

            // Quick preset buttons
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Movement Presets:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Linear")) SetMovementCurve(AnimationCurve.Linear(0, 0, 1, 1));
            if (GUILayout.Button("Ease In Out")) SetMovementCurve(AnimationCurve.EaseInOut(0, 0, 1, 1));
            if (GUILayout.Button("Bounce")) SetMovementCurve(CreateBounceCurve());
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // Path Settings
        showPathSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showPathSettings, "Path Settings");
        if (showPathSettings)
        {
            EditorGUI.indentLevel++;
            
            // Path points list
            SerializedProperty pathPointsProp = serializedObject.FindProperty("pathPoints");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Waypoints ({pathPointsProp.arraySize})", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                AddWaypoint();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            for (int i = 0; i < pathPointsProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(pathPointsProp.GetArrayElementAtIndex(i), new GUIContent($"Point {i + 1}"));
                
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    SelectWaypoint(i);
                }
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    RemoveWaypoint(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loopPath"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("returnToStart"));

            // Helper buttons
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear All Points"))
            {
                if (EditorUtility.DisplayDialog("Clear Waypoints", "Remove all waypoints?", "Yes", "Cancel"))
                {
                    ClearAllWaypoints();
                }
            }
            if (GUILayout.Button("Create Point Here"))
            {
                CreateWaypointAtSceneView();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // Player Collision Settings
        showCollisionSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCollisionSettings, "Player Collision Settings");
        if (showCollisionSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("disablePlayerOnCollision"));
            
            if (trollTrigger.disablePlayerOnCollision)
            {
                EditorGUILayout.HelpBox("When enabled, player controller will be disabled if they collide with the moving object.", MessageType.Info);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("collisionObjects"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("controllerScriptName"));
                
                // Helper button to auto-populate collision objects
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Auto-Add Moving Object"))
                {
                    Undo.RecordObject(trollTrigger, "Auto-Add Collision Object");
                    if (trollTrigger.movingObject != null && !trollTrigger.collisionObjects.Contains(trollTrigger.movingObject.gameObject))
                    {
                        trollTrigger.collisionObjects.Add(trollTrigger.movingObject.gameObject);
                        EditorUtility.SetDirty(trollTrigger);
                    }
                }
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // Visual Settings
        showVisualSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showVisualSettings, "Visual Settings");
        if (showVisualSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pathColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showTriggerInGame"));
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // Audio Settings
        showAudioSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAudioSettings, "Audio Settings (Optional)");
        if (showAudioSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("activationSound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSound"));
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // Preset Templates
        EditorGUILayout.LabelField("Quick Troll Presets", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Popup Spikes")) ApplyPopupSpikesPreset();
        if (GUILayout.Button("Moving Platform")) ApplyMovingPlatformPreset();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Pushing Wall")) ApplyPushingWallPreset();
        if (GUILayout.Button("Falling Ground")) ApplyFallingGroundPreset();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Test button
        if (Application.isPlaying)
        {
            if (GUILayout.Button("TEST TRIGGER NOW", GUILayout.Height(40)))
            {
                trollTrigger.ResetTrigger();
                trollTrigger.SendMessage("Activate");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void CreateOrAdjustTriggerZone()
    {
        Undo.RecordObject(trollTrigger, "Create Trigger Zone");
        
        if (trollTrigger.triggerZone == null)
        {
            trollTrigger.triggerZone = trollTrigger.gameObject.AddComponent<BoxCollider>();
        }
        
        trollTrigger.triggerZone.isTrigger = true;
        trollTrigger.triggerZone.size = new Vector3(5, 5, 5);
        
        EditorUtility.SetDirty(trollTrigger);
    }

    private void SetTriggerSize(Vector3 size)
    {
        if (trollTrigger.triggerZone != null)
        {
            Undo.RecordObject(trollTrigger.triggerZone, "Set Trigger Size");
            trollTrigger.triggerZone.size = size;
            EditorUtility.SetDirty(trollTrigger.triggerZone);
        }
    }

    private void AddWaypoint()
    {
        GameObject waypoint = new GameObject($"Waypoint_{trollTrigger.pathPoints.Count + 1}");
        
        // Position waypoint in front of the object
        Vector3 offset = trollTrigger.transform.forward * 5f + trollTrigger.transform.up * 2f;
        waypoint.transform.position = trollTrigger.transform.position + offset;
        waypoint.transform.SetParent(trollTrigger.transform);
        
        Undo.RegisterCreatedObjectUndo(waypoint, "Add Waypoint");
        Undo.RecordObject(trollTrigger, "Add Waypoint to List");
        
        trollTrigger.pathPoints.Add(waypoint.transform);
        
        EditorUtility.SetDirty(trollTrigger);
        Selection.activeGameObject = waypoint;
    }

    private void RemoveWaypoint(int index)
    {
        if (index >= 0 && index < trollTrigger.pathPoints.Count)
        {
            Undo.RecordObject(trollTrigger, "Remove Waypoint");
            
            if (trollTrigger.pathPoints[index] != null)
            {
                Undo.DestroyObjectImmediate(trollTrigger.pathPoints[index].gameObject);
            }
            
            trollTrigger.pathPoints.RemoveAt(index);
            EditorUtility.SetDirty(trollTrigger);
        }
    }

    private void SelectWaypoint(int index)
    {
        if (index >= 0 && index < trollTrigger.pathPoints.Count && trollTrigger.pathPoints[index] != null)
        {
            Selection.activeGameObject = trollTrigger.pathPoints[index].gameObject;
            SceneView.lastActiveSceneView.FrameSelected();
        }
    }

    private void ClearAllWaypoints()
    {
        Undo.RecordObject(trollTrigger, "Clear All Waypoints");
        
        foreach (Transform waypoint in trollTrigger.pathPoints)
        {
            if (waypoint != null)
            {
                Undo.DestroyObjectImmediate(waypoint.gameObject);
            }
        }
        
        trollTrigger.pathPoints.Clear();
        EditorUtility.SetDirty(trollTrigger);
    }

    private void CreateWaypointAtSceneView()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            GameObject waypoint = new GameObject($"Waypoint_{trollTrigger.pathPoints.Count + 1}");
            waypoint.transform.position = sceneView.camera.transform.position + sceneView.camera.transform.forward * 10f;
            waypoint.transform.SetParent(trollTrigger.transform);
            
            Undo.RegisterCreatedObjectUndo(waypoint, "Create Waypoint at Scene View");
            Undo.RecordObject(trollTrigger, "Add Waypoint");
            
            trollTrigger.pathPoints.Add(waypoint.transform);
            EditorUtility.SetDirty(trollTrigger);
        }
    }

    private void SetMovementCurve(AnimationCurve curve)
    {
        Undo.RecordObject(trollTrigger, "Set Movement Curve");
        trollTrigger.movementCurve = curve;
        EditorUtility.SetDirty(trollTrigger);
    }

    private AnimationCurve CreateBounceCurve()
    {
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 0f);
        curve.AddKey(0.5f, 1.2f);
        curve.AddKey(1f, 1f);
        return curve;
    }

    // Preset Templates
    private void ApplyPopupSpikesPreset()
    {
        Undo.RecordObject(trollTrigger, "Apply Popup Spikes Preset");
        trollTrigger.movementType = TrollTrigger.MovementType.MoveToTarget;
        trollTrigger.moveSpeed = 15f;
        trollTrigger.reusable = false;
        trollTrigger.activationDelay = 0.1f;
        trollTrigger.movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        if (trollTrigger.pathPoints.Count == 0)
        {
            AddWaypoint();
            if (trollTrigger.pathPoints[0] != null)
            {
                trollTrigger.pathPoints[0].position = trollTrigger.transform.position + Vector3.up * 3f;
            }
        }
        
        EditorUtility.SetDirty(trollTrigger);
        EditorUtility.DisplayDialog("Preset Applied", "Popup Spikes preset applied! Position the waypoint where you want the spikes to pop up.", "OK");
    }

    private void ApplyMovingPlatformPreset()
    {
        Undo.RecordObject(trollTrigger, "Apply Moving Platform Preset");
        trollTrigger.movementType = TrollTrigger.MovementType.PingPong;
        trollTrigger.moveSpeed = 3f;
        trollTrigger.reusable = true;
        trollTrigger.activationDelay = 0f;
        
        if (trollTrigger.pathPoints.Count == 0)
        {
            AddWaypoint();
            if (trollTrigger.pathPoints[0] != null)
            {
                trollTrigger.pathPoints[0].position = trollTrigger.transform.position + Vector3.right * 10f;
            }
        }
        
        EditorUtility.SetDirty(trollTrigger);
        EditorUtility.DisplayDialog("Preset Applied", "Moving Platform preset applied! Set the waypoint to where the platform should move.", "OK");
    }

    private void ApplyPushingWallPreset()
    {
        Undo.RecordObject(trollTrigger, "Apply Pushing Wall Preset");
        trollTrigger.movementType = TrollTrigger.MovementType.MoveToTarget;
        trollTrigger.moveSpeed = 8f;
        trollTrigger.reusable = false;
        trollTrigger.activationDelay = 0.2f;
        trollTrigger.returnToStart = true;
        
        if (trollTrigger.pathPoints.Count == 0)
        {
            AddWaypoint();
            if (trollTrigger.pathPoints[0] != null)
            {
                trollTrigger.pathPoints[0].position = trollTrigger.transform.position + trollTrigger.transform.forward * 8f;
            }
        }
        
        EditorUtility.SetDirty(trollTrigger);
        EditorUtility.DisplayDialog("Preset Applied", "Pushing Wall preset applied! Position the waypoint where the wall should push to.", "OK");
    }

    private void ApplyFallingGroundPreset()
    {
        Undo.RecordObject(trollTrigger, "Apply Falling Ground Preset");
        trollTrigger.movementType = TrollTrigger.MovementType.MoveToTarget;
        trollTrigger.moveSpeed = 12f;
        trollTrigger.reusable = false;
        trollTrigger.activationDelay = 0.3f;
        
        if (trollTrigger.pathPoints.Count == 0)
        {
            AddWaypoint();
            if (trollTrigger.pathPoints[0] != null)
            {
                trollTrigger.pathPoints[0].position = trollTrigger.transform.position + Vector3.down * 20f;
            }
        }
        
        EditorUtility.SetDirty(trollTrigger);
        EditorUtility.DisplayDialog("Preset Applied", "Falling Ground preset applied! Position the waypoint where the ground should fall to.", "OK");
    }
}