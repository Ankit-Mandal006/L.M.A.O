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
    private bool showDeathSettings = true;
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
            
            // Show return settings conditionally
            if (trollTrigger.returnToStart)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("returnDelay"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("returnOnPlayerDeath"));
            }

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

        // Death Settings
        showDeathSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showDeathSettings, "Death Settings");
        if (showDeathSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("killPlayerOnReturn"));
            
            if (trollTrigger.killPlayerOnReturn)
            {
                EditorGUILayout.HelpBox("⚠️ DANGER: Object becomes deadly when returning! Player will die on contact.", MessageType.Warning);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("killDelay"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("deadlyTag"));
                
                // Visual warning
                EditorGUILayout.Space(5);
                GUIStyle warningStyle = new GUIStyle(EditorStyles.boldLabel);
                warningStyle.normal.textColor = Color.red;
                EditorGUILayout.LabelField("⚡ Object will kill player during return!", warningStyle);
                
                // Helpful info layout logic
                EditorGUILayout.Space(5);
                string deathTriggerText = trollTrigger.returnOnPlayerDeath 
                    ? "1. Player exits trigger zone (or dies)" 
                    : "1. Player exits trigger zone";

                EditorGUILayout.HelpBox(
                    $"How it works:\n" +
                    $"{deathTriggerText}\n" +
                    "2. Wait 'Return Delay' seconds\n" +
                    "3. Wait 'Kill Delay' seconds\n" +
                    "4. Object becomes deadly (tagged as '" + trollTrigger.deadlyTag + "')\n" +
                    "5. Object returns to start position\n" +
                    "6. Player dies if touched during return",
                    MessageType.Info
                );
                
                // Ensure moving object has a trigger collider
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Setup Death Collision (Required)"))
                {
                    SetupDeathCollision();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Object is safe. Enable 'Kill Player On Return' to make it deadly.", MessageType.Info);
            }
            
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
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Deadly Crusher")) ApplyDeadlyCrusherPreset();
        if (GUILayout.Button("Trap Door")) ApplyTrapDoorPreset();
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

    private void SetupDeathCollision()
    {
        if (trollTrigger.movingObject == null)
        {
            EditorUtility.DisplayDialog("Error", "No moving object assigned! Assign a moving object first.", "OK");
            return;
        }

        Undo.RecordObject(trollTrigger.movingObject.gameObject, "Setup Death Collision");

        Collider[] colliders = trollTrigger.movingObject.GetComponents<Collider>();
        bool hasTrigger = false;

        foreach (Collider col in colliders)
        {
            if (col.isTrigger)
            {
                hasTrigger = true;
                break;
            }
        }

        if (!hasTrigger)
        {
            BoxCollider triggerCol = trollTrigger.movingObject.gameObject.AddComponent<BoxCollider>();
            triggerCol.isTrigger = true;
            EditorUtility.DisplayDialog("Success", "Added trigger collider to moving object! Player can now die when touching it.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Already Setup", "Moving object already has a trigger collider!", "OK");
        }

        EditorUtility.SetDirty(trollTrigger.movingObject.gameObject);
    }

    private void AddWaypoint()
    {
        GameObject waypoint = new GameObject($"Waypoint_{trollTrigger.pathPoints.Count + 1}");
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

    private void ApplyPopupSpikesPreset()
    {
        Undo.RecordObject(trollTrigger, "Apply Popup Spikes Preset");
        trollTrigger.movementType = TrollTrigger.MovementType.MoveToTarget;
        trollTrigger.moveSpeed = 15f;
        trollTrigger.reusable = false;
        trollTrigger.activationDelay = 0.1f;
        trollTrigger.movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        trollTrigger.killPlayerOnReturn = false;
        
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
        trollTrigger.killPlayerOnReturn = false;
        
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
        trollTrigger.returnDelay = 0f;
        trollTrigger.killPlayerOnReturn = false;
        
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
        trollTrigger.killPlayerOnReturn = false;
        
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

    private void ApplyDeadlyCrusherPreset()
    {
        Undo.RecordObject(trollTrigger, "Apply Deadly Crusher Preset");
        trollTrigger.movementType = TrollTrigger.MovementType.MoveToTarget;
        trollTrigger.moveSpeed = 10f;
        trollTrigger.reusable = true;
        trollTrigger.activationDelay = 0.5f;
        trollTrigger.returnToStart = true;
        trollTrigger.returnDelay = 1.5f;
        trollTrigger.killPlayerOnReturn = true;
        trollTrigger.killDelay = 0.2f;
        trollTrigger.deadlyTag = "Enemy";
        
        if (trollTrigger.pathPoints.Count == 0)
        {
            AddWaypoint();
            if (trollTrigger.pathPoints[0] != null)
            {
                trollTrigger.pathPoints[0].position = trollTrigger.transform.position + Vector3.down * 5f;
            }
        }
        
        EditorUtility.SetDirty(trollTrigger);
        EditorUtility.DisplayDialog("⚠️ DEADLY Preset Applied", "Deadly Crusher preset applied!\n\nThis object will KILL the player when returning!\n\nMake sure:\n1. Player has PlayerDeath script\n2. Moving object has trigger collider\n3. Click 'Setup Death Collision' button", "OK");
    }

    private void ApplyTrapDoorPreset()
    {
        Undo.RecordObject(trollTrigger, "Apply Trap Door Preset");
        trollTrigger.movementType = TrollTrigger.MovementType.MoveToTarget;
        trollTrigger.moveSpeed = 8f;
        trollTrigger.reusable = true;
        trollTrigger.activationDelay = 0.2f;
        trollTrigger.returnToStart = true;
        trollTrigger.returnDelay = 2f;
        trollTrigger.killPlayerOnReturn = true;
        trollTrigger.killDelay = 0f;
        trollTrigger.deadlyTag = "Enemy";
        
        if (trollTrigger.pathPoints.Count == 0)
        {
            AddWaypoint();
            if (trollTrigger.pathPoints[0] != null)
            {
                trollTrigger.pathPoints[0].position = trollTrigger.transform.position + Vector3.down * 3f;
            }
        }
        
        EditorUtility.SetDirty(trollTrigger);
        EditorUtility.DisplayDialog("⚠️ DEADLY Preset Applied", "Trap Door preset applied!\n\nDoor opens, then closes and becomes deadly!\n\nSetup required:\n1. Player has PlayerDeath script\n2. Click 'Setup Death Collision' button", "OK");
    }
}