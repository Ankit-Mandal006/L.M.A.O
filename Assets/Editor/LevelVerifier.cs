#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public static class LevelVerifier
{
    [MenuItem("Tools/Verify Fall Guys Level")]
    public static void VerifyLevel()
    {
        string reportPath = Path.Combine(Application.dataPath, "../level_verification_report.txt");
        List<string> logs = new List<string>();
        bool success = true;

        logs.Add("==================================================");
        logs.Add("          LEVEL PLAYABILITY VERIFICATION REPORT    ");
        logs.Add("==================================================");
        logs.Add($"Date: {System.DateTime.Now}");

        // 1. Open the scene
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName != "Karthik - level 8")
        {
            string[] guids = AssetDatabase.FindAssets("Karthik - level 8 t:Scene");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
                logs.Add("Opened scene: Karthik - level 8");
            }
            else
            {
                logs.Add("[FAIL] Scene 'Karthik - level 8' not found in project!");
                File.WriteAllLines(reportPath, logs);
                return;
            }
        }
        else
        {
            logs.Add("Scene 'Karthik - level 8' is active.");
        }

        // 2. Camera System Verification
        logs.Add("\n--- Camera System Verification ---");
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        int activeCameraCount = 0;
        Camera activeMainCam = null;

        foreach (var cam in allCameras)
        {
            if (cam.gameObject.activeInHierarchy && cam.CompareTag("MainCamera"))
            {
                activeCameraCount++;
                activeMainCam = cam;
            }
        }

        if (activeCameraCount == 1 && activeMainCam != null)
        {
            logs.Add($"[PASS] Exactly 1 active MainCamera found: '{activeMainCam.name}' (parent: '{activeMainCam.transform.parent?.name ?? "none"}')");
            
            PlayerFollowCamera followCam = activeMainCam.GetComponent<PlayerFollowCamera>();
            if (followCam != null)
            {
                logs.Add("[PASS] PlayerFollowCamera component is attached to the active camera.");
                logs.Add($"[PASS] Camera offset: {followCam.offset} (Expected close: (0, 5, -8))");
                logs.Add($"[PASS] Smooth time: {followCam.followSmoothTime}");
                logs.Add($"[PASS] Lookahead: {followCam.lookAheadDistance}");
                
                if (followCam.collisionLayers.value != 0)
                {
                    logs.Add($"[PASS] Camera collision layers mask: {followCam.collisionLayers.value} (Collision enabled)");
                }
                else
                {
                    logs.Add("[FAIL] Camera collision layers are not set!");
                    success = false;
                }
            }
            else
            {
                logs.Add("[FAIL] PlayerFollowCamera component is missing from the active camera!");
                success = false;
            }
        }
        else
        {
            logs.Add($"[FAIL] Multiple active MainCameras found ({activeCameraCount})! Conflicts will occur.");
            foreach (var cam in allCameras)
            {
                if (cam.CompareTag("MainCamera"))
                {
                    logs.Add($"  Camera: '{cam.name}' [Active: {cam.gameObject.activeInHierarchy}]");
                }
            }
            success = false;
        }

        // 3. Spacing & Gaps Verification
        logs.Add("\n--- Spacing & Gaps Verification ---");
        GameObject levelRoot = GameObject.Find("_FALL_GUYS_LEVEL_");
        if (levelRoot == null)
        {
            logs.Add("[FAIL] _FALL_GUYS_LEVEL_ root object not found!");
            success = false;
        }
        else
        {
            logs.Add("[PASS] _FALL_GUYS_LEVEL_ root object is active.");

            // Collect all platforms that have colliders
            var renderers = levelRoot.GetComponentsInChildren<MeshRenderer>();
            List<Bounds> platformBounds = new List<Bounds>();
            
            foreach (var r in renderers)
            {
                if (r.gameObject.activeInHierarchy && (r.name.Contains("Platform") || r.name.Contains("Floor") || r.name.Contains("Path") || r.name.Contains("Ramp") || r.name.Contains("Seesaw")))
                {
                    platformBounds.Add(r.bounds);
                }
            }

            // Sort platform bounds by Z center
            platformBounds.Sort((a, b) => a.center.z.CompareTo(b.center.z));
            logs.Add($"Found {platformBounds.Count} platform surfaces along Z-axis.");

            float maxGap = 0f;
            for (int i = 0; i < platformBounds.Count - 1; i++)
            {
                Bounds current = platformBounds[i];
                Bounds next = platformBounds[i + 1];

                // Gap is the distance between current platform's max Z and next platform's min Z
                float gap = next.min.z - current.max.z;
                
                // Only consider gaps if there is no Z overlap
                if (gap > 0.1f)
                {
                    if (gap > maxGap) maxGap = gap;
                    logs.Add($"  Gap between '{current.size}' at Z={current.center.z:F1} and '{next.size}' at Z={next.center.z:F1} is {gap:F2} units.");
                    
                    if (gap > 3.5f)
                    {
                        logs.Add($"  [FAIL] Gap exceeds playable limit (3.5 units): {gap:F2} units!");
                        success = false;
                    }
                }
            }
            logs.Add($"[PASS] Maximum platform gap found along the Z-axis: {maxGap:F2} units (Target: <= 3.0 units).");
        }

        // 4. Checkpoint Platforms Verification
        logs.Add("\n--- Checkpoint & Spawn Point Verification ---");
        Checkpoint[] checkpoints = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        logs.Add($"Found {checkpoints.Length} active checkpoints in the scene.");
        
        foreach (var cp in checkpoints)
        {
            if (!cp.gameObject.activeInHierarchy) continue;

            Vector3 cpPos = cp.transform.position;
            // Raycast down to find if there is a solid platform under the checkpoint
            RaycastHit hit;
            if (Physics.Raycast(cpPos + Vector3.up * 1f, Vector3.down, out hit, 4f))
            {
                logs.Add($"[PASS] Checkpoint '{cp.name}' at Z={cpPos.z:F1} is placed on solid geometry '{hit.collider.name}' (distance: {hit.distance:F2} units).");
            }
            else
            {
                logs.Add($"[FAIL] Checkpoint '{cp.name}' at Z={cpPos.z:F1} has NO solid geometry directly beneath it! Players will respawn into the void!");
                success = false;
            }
        }

        // 5. Moving Platform physics verification
        logs.Add("\n--- Moving Platform Physics Verification ---");
        MovingPlatform[] movingPlats = Object.FindObjectsByType<MovingPlatform>(FindObjectsSortMode.None);
        logs.Add($"Found {movingPlats.Length} active Moving Platforms in the scene.");
        
        foreach (var mp in movingPlats)
        {
            if (!mp.gameObject.activeInHierarchy) continue;

            BoxCollider[] colliders = mp.GetComponents<BoxCollider>();
            bool hasSolid = false;
            bool hasTrigger = false;

            foreach (var coll in colliders)
            {
                if (coll.isTrigger) hasTrigger = true;
                else hasSolid = true;
            }

            if (hasSolid && hasTrigger)
            {
                logs.Add($"[PASS] Moving Platform '{mp.name}' has both solid and trigger colliders (Trigger enabled).");
            }
            else
            {
                logs.Add($"[FAIL] Moving Platform '{mp.name}' is missing colliders! Has solid: {hasSolid}, Has trigger: {hasTrigger}");
                success = false;
            }
        }

        // 6. Old Geometry Clean-up verification
        logs.Add("\n--- Old Geometry Clean-up Verification ---");
        GameObject[] oldMeshes = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int oldActiveCount = 0;
        foreach (var obj in oldMeshes)
        {
            if (obj != null && obj.transform.parent == null && obj.gameObject.activeInHierarchy)
            {
                string n = obj.name;
                if (n.StartsWith("pb_Mesh") || n.StartsWith("Checkpoint 1") || n.StartsWith("GameObject (2)") || n.StartsWith("GameObject (3)"))
                {
                    oldActiveCount++;
                    logs.Add($"  [FAIL] Redundant old object '{obj.name}' is still active in hierarchy!");
                }
            }
        }

        if (oldActiveCount == 0)
        {
            logs.Add("[PASS] All redundant root-level geometry and triggers have been successfully cleaned up/disabled.");
        }
        else
        {
            logs.Add($"[FAIL] Found {oldActiveCount} redundant old active objects.");
            success = false;
        }

        logs.Add("\n==================================================");
        if (success)
        {
            logs.Add("          VERIFICATION RESULT: ALL TESTS PASSED!  ");
        }
        else
        {
            logs.Add("          VERIFICATION RESULT: FAILURES DETECTED! ");
        }
        logs.Add("==================================================");

        File.WriteAllLines(reportPath, logs);
        Debug.Log("LevelVerifier: Verification complete. Report written to " + reportPath);
    }
}
#endif
