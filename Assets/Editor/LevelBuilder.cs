#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public static class LevelBuilder
{
    // NOTE: Auto-trigger removed. Use Tools > Build Fall Guys Level menu or batch mode to rebuild.
    // The [InitializeOnLoadMethod] was causing the level to rebuild every time Unity reloaded assemblies.

    [MenuItem("Tools/Build Fall Guys Level")]
    public static void BuildCourse()
    {
        // 1. Open the correct scene if not active
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName != "Karthik - level 8")
        {
            Debug.LogWarning("LevelBuilder: Active scene is not 'Karthik - level 8'. Opening scene...");
            string[] guids = AssetDatabase.FindAssets("Karthik - level 8 t:Scene");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
            }
            else
            {
                Debug.LogError("LevelBuilder: Scene 'Karthik - level 8' not found in project!");
                return;
            }
        }



        Debug.Log("LevelBuilder: Constructing Fall Guys Obstacle Course...");

        // 2. Clear previous level root if exists
        GameObject oldRoot = GameObject.Find("_FALL_GUYS_LEVEL_");
        if (oldRoot != null)
        {
            GameObject.DestroyImmediate(oldRoot);
            Debug.Log("LevelBuilder: Cleared previous obstacle course.");
        }

        // 3. Create root object
        GameObject levelRoot = new GameObject("_FALL_GUYS_LEVEL_");

        // 4. Create materials
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Materials/FallGuys"));
        AssetDatabase.Refresh();

        Material matBlue = CreateFallGuysMaterial("Mat_Blue", new Color(0f, 0.6f, 1f));
        Material matYellow = CreateFallGuysMaterial("Mat_Yellow", new Color(1f, 0.85f, 0f));
        Material matPink = CreateFallGuysMaterial("Mat_Pink", new Color(1f, 0f, 0.5f));
        Material matPurple = CreateFallGuysMaterial("Mat_Purple", new Color(0.5f, 0f, 0.75f));
        Material matGreen = CreateFallGuysMaterial("Mat_Green", new Color(0f, 0.8f, 0.3f));
        Material matOrange = CreateFallGuysMaterial("Mat_Orange", new Color(1f, 0.5f, 0f));
        Material matRed = CreateFallGuysMaterial("Mat_Red", new Color(0.9f, 0.1f, 0.1f));
        Material matWhite = CreateFallGuysMaterial("Mat_White", new Color(1f, 1f, 1f));
        Material matGrey = CreateFallGuysMaterial("Mat_Grey", new Color(0.4f, 0.4f, 0.4f));

        // 5. Setup Lighting and Environment
        GameObject dirLightGo = GameObject.Find("Directional Light");
        if (dirLightGo != null)
        {
            Light light = dirLightGo.GetComponent<Light>();
            if (light != null)
            {
                light.color = new Color(1f, 0.96f, 0.84f);
                light.intensity = 1.3f;
                light.shadows = LightShadows.Soft;
            }
            dirLightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // 6. Spawn Manager & Spawn Point
        GameObject spawnPointGo = GameObject.Find("Spawn Point");
        if (spawnPointGo == null)
        {
            spawnPointGo = new GameObject("Spawn Point");
        }
        spawnPointGo.transform.position = new Vector3(0f, -1.0f, 0f);
        spawnPointGo.transform.rotation = Quaternion.identity;

        GameObject checkpointManagerGo = GameObject.Find("Checkpoint Manager");
        if (checkpointManagerGo == null)
        {
            checkpointManagerGo = new GameObject("Checkpoint Manager");
            checkpointManagerGo.AddComponent<CheckpointManager>();
        }
        CheckpointManager checkpointManager = checkpointManagerGo.GetComponent<CheckpointManager>();
        
        SerializedObject managerSO = new SerializedObject(checkpointManager);
        SerializedProperty spawnPointProp = managerSO.FindProperty("spawnPoint");
        spawnPointProp.objectReferenceValue = spawnPointGo.transform;
        managerSO.ApplyModifiedProperties();

        // Load Checkpoint Animator Controller
        RuntimeAnimatorController checkpointController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Animations/Checkpoints/Checkpoint 1.controller"
        );

        // 7. BUILD SECTIONS
        
        // Start Platform
        GameObject startPlatform = CreatePlatform("Start_Platform", new Vector3(0f, -2f, 0f), new Vector3(8f, 2f, 20f), matBlue, levelRoot.transform);
        
        // ----------------- SECTION A: ROTATING SWEEP BARS -----------------
        GameObject secARoot = new GameObject("Section_A_Sweepers");
        secARoot.transform.SetParent(levelRoot.transform);
        secARoot.transform.position = new Vector3(0f, 0f, 23.5f);

        CreatePlatform("Floor_A", new Vector3(0f, -2f, 23.5f), new Vector3(8f, 2f, 25f), matYellow, secARoot.transform);
        
        // Rotating Sweep 1 (Left) - Slower and shorter
        GameObject sweeper1 = CreateSweeper("Sweeper_1", new Vector3(-4f, 0.5f, 23.5f), 50f, Vector3.up, matPink, matRed, secARoot.transform);
        // Rotating Sweep 2 (Right) - Slower and shorter
        GameObject sweeper2 = CreateSweeper("Sweeper_2", new Vector3(4f, 0.5f, 27.5f), -50f, Vector3.up, matPink, matRed, secARoot.transform);

        // Checkpoint 1
        CreatePlatform("Checkpoint_1_Floor", new Vector3(0f, -2f, 40f), new Vector3(8f, 2f, 6f), matBlue, levelRoot.transform);
        CreateCheckpointObject("Checkpoint_1", new Vector3(0f, -1f, 40f), checkpointManager, checkpointController, levelRoot.transform);

        // ----------------- SECTION B: MOVING PLATFORMS -----------------
        GameObject secBRoot = new GameObject("Section_B_MovingPlatforms");
        secBRoot.transform.SetParent(levelRoot.transform);
        secBRoot.transform.position = new Vector3(0f, 0f, 55.5f);

        // Safe Platform (Center static, wider)
        CreatePlatform("Floor_B_Static", new Vector3(0f, -2f, 55.5f), new Vector3(6f, 2f, 6f), matBlue, secBRoot.transform);
        
        // Risky Platform 1 (Centered - horizontal movement along X, wider)
        GameObject platB1 = CreatePlatform("Plat_B1_Left", new Vector3(0f, -2f, 47.5f), new Vector3(6f, 1.5f, 6f), matPink, secBRoot.transform);
        MovingPlatform mp1 = platB1.AddComponent<MovingPlatform>();
        mp1.movementDirection = Vector3.right;
        mp1.speed = 1.5f;
        mp1.distance = 3f;
        // Trigger BoxCollider for player tracking
        BoxCollider triggerB1 = platB1.AddComponent<BoxCollider>();
        triggerB1.isTrigger = true;
        triggerB1.size = new Vector3(1.02f, 2.0f, 1.02f);
        triggerB1.center = new Vector3(0f, 0.6f, 0f);

        // Risky Platform 2 (Centered - vertical movement along Y, wider)
        GameObject platB2 = CreatePlatform("Plat_B2_Right", new Vector3(0f, -3f, 63.5f), new Vector3(6f, 1.5f, 6f), matGreen, secBRoot.transform);
        MovingPlatform mp2 = platB2.AddComponent<MovingPlatform>();
        mp2.movementDirection = Vector3.up;
        mp2.speed = 2f;
        mp2.distance = 3f;
        // Trigger BoxCollider for player tracking
        BoxCollider triggerB2 = platB2.AddComponent<BoxCollider>();
        triggerB2.isTrigger = true;
        triggerB2.size = new Vector3(1.02f, 2.0f, 1.02f);
        triggerB2.center = new Vector3(0f, 0.6f, 0f);

        // Checkpoint 2
        CreatePlatform("Checkpoint_2_Floor", new Vector3(0f, -2f, 71f), new Vector3(8f, 2f, 6f), matBlue, levelRoot.transform);
        CreateCheckpointObject("Checkpoint_2", new Vector3(0f, -1f, 71f), checkpointManager, checkpointController, levelRoot.transform);

        // ----------------- SECTION C: SPINNING HAMMERS -----------------
        GameObject secCRoot = new GameObject("Section_C_SpinningHammers");
        secCRoot.transform.SetParent(levelRoot.transform);
        secCRoot.transform.position = new Vector3(0f, 0f, 89f);

        CreatePlatform("Floor_C", new Vector3(0f, -2f, 89f), new Vector3(8f, 2f, 28f), matOrange, secCRoot.transform);

        // Rotating Hammer 1 (Left wall, sweeps path, slower)
        CreateHammerObstacle("Hammer_1", new Vector3(-4f, 1.5f, 82f), 45f, Vector3.up, matPink, matWhite, secCRoot.transform);
        // Rotating Hammer 2 (Right wall, sweeps path, slower)
        CreateHammerObstacle("Hammer_2", new Vector3(4f, 1.5f, 96f), -45f, Vector3.up, matPink, matWhite, secCRoot.transform);

        // Checkpoint 3
        CreatePlatform("Checkpoint_3_Floor", new Vector3(0f, -2f, 107f), new Vector3(8f, 2f, 6f), matBlue, levelRoot.transform);
        CreateCheckpointObject("Checkpoint_3", new Vector3(0f, -1f, 107f), checkpointManager, checkpointController, levelRoot.transform);

        // ----------------- SECTION D: TILTING PLATFORMS -----------------
        GameObject secDRoot = new GameObject("Section_D_TiltingPlatforms");
        secDRoot.transform.SetParent(levelRoot.transform);
        secDRoot.transform.position = new Vector3(0f, 0f, 125.5f);

        // Pivot anchors and tilting seesaws
        GameObject tiltPlat1 = CreatePlatform("Seesaw_1", new Vector3(0f, -1f, 118f), new Vector3(8f, 1f, 14f), matPurple, secDRoot.transform);
        TiltingPlatform tp1 = tiltPlat1.AddComponent<TiltingPlatform>();
        tp1.maxTiltAngle = 6f; // lower tilt for single jump
        tp1.returnSpeed = 3f;  // faster return
        
        // Add a trigger zone above it for detecting players
        BoxCollider tiltTrig1 = tiltPlat1.AddComponent<BoxCollider>();
        tiltTrig1.isTrigger = true;
        tiltTrig1.size = new Vector3(8.5f, 4f, 14.5f);
        tiltTrig1.center = new Vector3(0f, 2f, 0f);

        GameObject tiltPlat2 = CreatePlatform("Seesaw_2", new Vector3(0f, -1f, 133f), new Vector3(8f, 1f, 14f), matYellow, secDRoot.transform);
        TiltingPlatform tp2 = tiltPlat2.AddComponent<TiltingPlatform>();
        tp2.maxTiltAngle = 6f;
        tp2.returnSpeed = 3f;
        
        BoxCollider tiltTrig2 = tiltPlat2.AddComponent<BoxCollider>();
        tiltTrig2.isTrigger = true;
        tiltTrig2.size = new Vector3(8.5f, 4f, 14.5f);
        tiltTrig2.center = new Vector3(0f, 2f, 0f);

        // Checkpoint 4
        CreatePlatform("Checkpoint_4_Floor", new Vector3(0f, -2f, 144f), new Vector3(8f, 2f, 6f), matBlue, levelRoot.transform);
        CreateCheckpointObject("Checkpoint_4", new Vector3(0f, -1f, 144f), checkpointManager, checkpointController, levelRoot.transform);

        // ----------------- SECTION E: JUMP PADS -----------------
        GameObject secERoot = new GameObject("Section_E_JumpPads");
        secERoot.transform.SetParent(levelRoot.transform);
        secERoot.transform.position = new Vector3(0f, 0f, 159.5f);

        // Lower floor
        CreatePlatform("Floor_E_Lower", new Vector3(0f, -2f, 154f), new Vector3(8f, 2f, 12f), matBlue, secERoot.transform);
        // Higher platform
        CreatePlatform("Floor_E_Upper", new Vector3(0f, 4f, 167f), new Vector3(8f, 2f, 14f), matGreen, secERoot.transform);

        // Jump pad 1 (Left)
        GameObject pad1 = CreatePlatform("JumpPad_Left", new Vector3(-2f, -0.9f, 154f), new Vector3(2f, 0.2f, 2f), matPink, secERoot.transform);
        BoxCollider collPad1 = pad1.GetComponent<BoxCollider>();
        collPad1.isTrigger = true;
        JumpPad jp1 = pad1.AddComponent<JumpPad>();
        jp1.launchForce = 16f;

        // Jump pad 2 (Right)
        GameObject pad2 = CreatePlatform("JumpPad_Right", new Vector3(2f, -0.9f, 154f), new Vector3(2f, 0.2f, 2f), matPink, secERoot.transform);
        BoxCollider collPad2 = pad2.GetComponent<BoxCollider>();
        collPad2.isTrigger = true;
        JumpPad jp2 = pad2.AddComponent<JumpPad>();
        jp2.launchForce = 16f;

        // Checkpoint 5 (located on the high platform)
        CreateCheckpointObject("Checkpoint_5", new Vector3(0f, 5.1f, 171f), checkpointManager, checkpointController, levelRoot.transform);

        // ----------------- SECTION F: CONVEYOR BELTS -----------------
        GameObject secFRoot = new GameObject("Section_F_ConveyorBelts");
        secFRoot.transform.SetParent(levelRoot.transform);
        secFRoot.transform.position = new Vector3(0f, 4f, 191f);

        GameObject conveyorFloor = CreatePlatform("Conveyor_Floor", new Vector3(0f, 4f, 191f), new Vector3(8f, 2f, 32f), matPurple, secFRoot.transform);
        ConveyorBelt cb = conveyorFloor.AddComponent<ConveyorBelt>();
        cb.pushDirection = Vector3.back;
        cb.pushSpeed = 2.5f; // Slower push for single-jump challenge
        BoxCollider cbColl = conveyorFloor.AddComponent<BoxCollider>();
        cbColl.isTrigger = true;
        cbColl.size = new Vector3(8.2f, 3f, 32.2f);
        cbColl.center = new Vector3(0f, 1.5f, 0f);

        // Small hurdles (cylinders to jump over on the conveyor)
        CreatePlatform("Hurdle_1", new Vector3(0f, 5.25f, 183f), new Vector3(8f, 0.5f, 0.5f), matOrange, secFRoot.transform);
        CreatePlatform("Hurdle_2", new Vector3(0f, 5.25f, 199f), new Vector3(8f, 0.5f, 0.5f), matOrange, secFRoot.transform);

        // Checkpoint 6
        CreatePlatform("Checkpoint_6_Floor", new Vector3(0f, 4f, 211f), new Vector3(8f, 2f, 6f), matBlue, levelRoot.transform);
        CreateCheckpointObject("Checkpoint_6", new Vector3(0f, 5.1f, 211f), checkpointManager, checkpointController, levelRoot.transform);

        // ----------------- SECTION G: FINAL CHALLENGE ZONE -----------------
        GameObject secGRoot = new GameObject("Section_G_FinalChallenge");
        secGRoot.transform.SetParent(levelRoot.transform);
        secGRoot.transform.position = new Vector3(0f, 0f, 247.5f);

        // Ramp leading down back to Y = -2 level
        GameObject ramp = CreatePlatform("Final_Ramp", new Vector3(0f, 1.0f, 227.5f), new Vector3(6f, 1f, 25f), matBlue, secGRoot.transform);
        ramp.transform.rotation = Quaternion.Euler(345f, 0f, 0f); // -15 degrees

        // Static narrow path
        CreatePlatform("Narrow_Path", new Vector3(0f, -2f, 256f), new Vector3(6f, 2f, 30f), matYellow, secGRoot.transform);

        // Spinning hammer on the narrow path - Slower
        CreateSweeper("Final_Sweeper", new Vector3(0f, 0.5f, 256f), 40f, Vector3.up, matPink, matRed, secGRoot.transform);

        // ----------------- FINISH AREA -----------------
        GameObject finishRoot = new GameObject("Finish_Area");
        finishRoot.transform.SetParent(levelRoot.transform);
        finishRoot.transform.position = new Vector3(0f, -2f, 282f);

        CreatePlatform("Finish_Platform", new Vector3(0f, -2f, 282f), new Vector3(12f, 2f, 20f), matGreen, finishRoot.transform);

        // Finish Line Archway
        GameObject archLeft = CreatePlatform("Arch_Left", new Vector3(-4f, 2.5f, 279f), new Vector3(1f, 7f, 1f), matOrange, finishRoot.transform);
        GameObject archRight = CreatePlatform("Arch_Right", new Vector3(4f, 2.5f, 279f), new Vector3(1f, 7f, 1f), matOrange, finishRoot.transform);
        GameObject archTop = CreatePlatform("Arch_Top", new Vector3(0f, 6.5f, 279f), new Vector3(11f, 1f, 1.5f), matYellow, finishRoot.transform);

        // Text display on arch
        GameObject textGo = new GameObject("FinishTextTMP");
        textGo.transform.SetParent(archTop.transform);
        textGo.transform.localPosition = new Vector3(0f, 0f, -0.8f);
        textGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        TextMeshPro textMesh = textGo.AddComponent<TextMeshPro>();
        textMesh.text = "FINISH!";
        textMesh.fontSize = 12;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;

        // Finish Line Trigger
        GameObject finishTriggerGo = new GameObject("FinishLineTrigger");
        finishTriggerGo.transform.SetParent(finishRoot.transform);
        finishTriggerGo.transform.position = new Vector3(0f, 1.5f, 279f);
        BoxCollider finishColl = finishTriggerGo.AddComponent<BoxCollider>();
        finishColl.isTrigger = true;
        finishColl.size = new Vector3(9f, 5f, 1f);
        FinishLine fl = finishTriggerGo.AddComponent<FinishLine>();

        // Confetti celebration particles
        ParticleSystem psLeft = CreateConfettiParticles(new Vector3(-3.5f, 2f, 279f), matPink);
        psLeft.transform.SetParent(finishRoot.transform);
        ParticleSystem psRight = CreateConfettiParticles(new Vector3(3.5f, 2f, 279f), matGreen);
        psRight.transform.SetParent(finishRoot.transform);
        
        fl.celebrationParticles = new ParticleSystem[] { psLeft, psRight };

        // ----------------- ELIMINATION ZONE & BOUNDARIES -----------------
        // Large elimination zone underneath
        GameObject elimZoneGo = new GameObject("Elimination_Zone");
        elimZoneGo.transform.SetParent(levelRoot.transform);
        elimZoneGo.transform.position = new Vector3(0f, -12f, 150f);
        BoxCollider elimColl = elimZoneGo.AddComponent<BoxCollider>();
        elimColl.isTrigger = true;
        elimColl.size = new Vector3(100f, 2f, 400f);
        elimZoneGo.AddComponent<EliminationZone>();

        // Side boundary barriers (invisible)
        GameObject barrierLeft = new GameObject("Boundary_Barrier_Left");
        barrierLeft.transform.SetParent(levelRoot.transform);
        barrierLeft.transform.position = new Vector3(-12f, 10f, 150f);
        BoxCollider barLeftColl = barrierLeft.AddComponent<BoxCollider>();
        barLeftColl.size = new Vector3(1f, 30f, 400f);

        GameObject barrierRight = new GameObject("Boundary_Barrier_Right");
        barrierRight.transform.SetParent(levelRoot.transform);
        barrierRight.transform.position = new Vector3(12f, 10f, 150f);
        BoxCollider barRightColl = barrierRight.AddComponent<BoxCollider>();
        barRightColl.size = new Vector3(1f, 30f, 400f);

        // 8. UPDATE PLAYER POSITION
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = spawnPointGo.transform.position + Vector3.up * 1f;
            player.transform.rotation = spawnPointGo.transform.rotation;
            if (cc != null) cc.enabled = true;

            PlayerDeath death = player.GetComponent<PlayerDeath>();
            if (death != null)
            {
                SerializedObject deathSO = new SerializedObject(death);
                SerializedProperty spawnPointProp2 = deathSO.FindProperty("spawnPoint");
                spawnPointProp2.objectReferenceValue = spawnPointGo.transform;
                deathSO.ApplyModifiedProperties();
            }
            Debug.Log("LevelBuilder: Teleported player to Start Platform.");
        }
        else
        {
            Debug.LogWarning("LevelBuilder: Player GameObject not found in scene!");
        }

        // 8b. SETUP CAMERA TO FOLLOW PLAYER
        SetupFollowCamera(player);

        // 9. CLEAN UP OLD SCENE GEOMETRY (Optional, to keep only the Fall Guys Level)
        // We look for old meshes in the scene and disable them to avoid clutter.
        // The user can re-enable them in the editor if desired.
        GameObject[] oldObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in oldObjects)
        {
            if (obj == null) continue;
            if (obj.transform.parent == null)
            {
                string n = obj.name;
                if (n.StartsWith("pb_Mesh") || n.StartsWith("Cube") || n.StartsWith("Checkpoint 1") || n.StartsWith("GameObject (2)") || n.StartsWith("GameObject (3)"))
                {
                    obj.SetActive(false);
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Debug.Log("LevelBuilder: Level built and saved successfully!");
        Debug.Log("LevelBuilder: Camera configured to follow player through obstacle course.");
    }

    private static GameObject CreatePlatform(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = pos;
        cube.transform.localScale = scale;
        cube.transform.SetParent(parent);
        
        MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
        renderer.material = mat;
        
        return cube;
    }

    private static GameObject CreateSweeper(string name, Vector3 pos, float speed, Vector3 axis, Material bodyMat, Material barMat, Transform parent)
    {
        GameObject sweeperRoot = new GameObject(name);
        sweeperRoot.transform.position = pos;
        sweeperRoot.transform.SetParent(parent);

        // Pivot base (Cylinder)
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.name = "Base";
        baseObj.transform.SetParent(sweeperRoot.transform, false);
        baseObj.transform.localScale = new Vector3(1.2f, 0.4f, 1.2f);
        baseObj.GetComponent<MeshRenderer>().material = bodyMat;

        // Rotating cylinder/bar
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bar.name = "Bar";
        bar.transform.SetParent(sweeperRoot.transform, false);
        bar.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        bar.transform.localScale = new Vector3(0.5f, 0.1f, 9f); // long sweep bar
        bar.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        bar.GetComponent<MeshRenderer>().material = barMat;

        // Attach scripts
        RotatingHammer rh = sweeperRoot.AddComponent<RotatingHammer>();
        rh.rotationSpeed = speed;
        rh.rotationAxis = axis;

        // Attach KnockbackObstacle to the bar
        KnockbackObstacle ko = bar.AddComponent<KnockbackObstacle>();
        ko.forceStrength = 18f;
        ko.verticalBoost = 6f;
        ko.useDirectionFromObstacle = true;

        return sweeperRoot;
    }

    private static GameObject CreateHammerObstacle(string name, Vector3 pos, float speed, Vector3 axis, Material frameMat, Material headMat, Transform parent)
    {
        GameObject hammerRoot = new GameObject(name);
        hammerRoot.transform.position = pos;
        hammerRoot.transform.SetParent(parent);

        // Support pillar (static, not child of pivot)
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "Pillar";
        pillar.transform.position = pos + new Vector3(0f, 1.5f, 0f);
        pillar.transform.localScale = new Vector3(0.6f, 2.5f, 0.6f);
        pillar.transform.SetParent(hammerRoot.transform);
        pillar.GetComponent<MeshRenderer>().material = frameMat;

        // Pivot point for rotation
        GameObject pivot = new GameObject("Pivot");
        pivot.transform.position = pos + new Vector3(0f, 4f, 0f); // top of pillar
        pivot.transform.SetParent(hammerRoot.transform);

        // Rotating arm (Cube)
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = "Arm";
        arm.transform.SetParent(pivot.transform, false);
        arm.transform.localPosition = new Vector3(0f, -1.5f, 0f);
        arm.transform.localScale = new Vector3(0.3f, 3f, 0.3f);
        arm.GetComponent<MeshRenderer>().material = frameMat;

        // Hammer Head (Cylinder)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        head.name = "Head";
        head.transform.SetParent(pivot.transform, false);
        head.transform.localPosition = new Vector3(0f, -3f, 0f);
        head.transform.localScale = new Vector3(1.8f, 0.8f, 1.8f);
        head.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        head.GetComponent<MeshRenderer>().material = headMat;

        // Attach rotating hammer script to the pivot
        RotatingHammer rh = pivot.AddComponent<RotatingHammer>();
        rh.rotationSpeed = speed;
        rh.rotationAxis = axis;

        // Attach KnockbackObstacle to the hammer head
        KnockbackObstacle ko = head.AddComponent<KnockbackObstacle>();
        ko.forceStrength = 22f;
        ko.verticalBoost = 8f;
        ko.useDirectionFromObstacle = true;

        return hammerRoot;
    }

    private static void CreateCheckpointObject(string name, Vector3 pos, CheckpointManager manager, RuntimeAnimatorController controller, Transform parent)
    {
        GameObject cpGo = new GameObject(name);
        cpGo.transform.position = pos;
        cpGo.transform.SetParent(parent);

        // Trigger area
        BoxCollider coll = cpGo.AddComponent<BoxCollider>();
        coll.isTrigger = true;
        coll.size = new Vector3(6f, 3f, 1.5f);
        coll.center = new Vector3(0f, 1.5f, 0f);

        Checkpoint cp = cpGo.AddComponent<Checkpoint>();

        // Setup Animator if controller is available
        if (controller != null)
        {
            Animator anim = cpGo.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            
            // Assign animator to the Checkpoint script via SerializedObject
            SerializedObject cpSO = new SerializedObject(cp);
            SerializedProperty animProp = cpSO.FindProperty("animator");
            animProp.objectReferenceValue = anim;
            cpSO.ApplyModifiedProperties();
        }

        // Checkpoint visual representation (archway)
        Material matGrey = CreateFallGuysMaterial("Mat_Grey", new Color(0.4f, 0.4f, 0.4f));
        Material matRed = CreateFallGuysMaterial("Mat_Red", new Color(0.9f, 0.1f, 0.1f));

        GameObject p1 = CreatePlatform("Post_Left", new Vector3(-3f, 1.5f, 0f) + pos, new Vector3(0.4f, 3f, 0.4f), matGrey, cpGo.transform);
        GameObject p2 = CreatePlatform("Post_Right", new Vector3(3f, 1.5f, 0f) + pos, new Vector3(0.4f, 3f, 0.4f), matGrey, cpGo.transform);
        GameObject top = CreatePlatform("Top_Bar", new Vector3(0f, 3.2f, 0f) + pos, new Vector3(6.4f, 0.4f, 0.4f), matRed, cpGo.transform);
    }

    private static ParticleSystem CreateConfettiParticles(Vector3 position, Material mat)
    {
        GameObject psGo = new GameObject("ConfettiParticles");
        psGo.transform.position = position;
        psGo.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // shoot upwards
        
        ParticleSystem ps = psGo.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.startLifetime = 3f;
        main.startSpeed = 10f;
        main.startSize = 0.3f;
        main.gravityModifier = 0.8f;
        main.maxParticles = 500;
        main.playOnAwake = false;
        
        var emission = ps.emission;
        emission.rateOverTime = 100;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 1.5f;
        
        var colorOverLifecycle = ps.colorOverLifetime;
        colorOverLifecycle.enabled = true;
        
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0f, 0.5f), 0f), 
                new GradientColorKey(new Color(1f, 0.85f, 0f), 0.5f), 
                new GradientColorKey(new Color(0f, 0.8f, 1f), 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f), 
                new GradientAlphaKey(1f, 0.8f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifecycle.color = grad;
        
        var renderer = psGo.GetComponent<ParticleSystemRenderer>();
        renderer.material = mat;
        
        return ps;
    }

    private static Material CreateFallGuysMaterial(string name, Color color)
    {
        string path = $"Assets/Materials/FallGuys/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            
            mat = new Material(shader);
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
            mat.SetFloat("_Smoothness", 0.1f);
            
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    /// <summary>
    /// Configures the camera system to follow the player through the obstacle course.
    /// Disables the old spline-based RoomCameraFollow and adds a PlayerFollowCamera.
    /// Also updates Room Starts/Ends markers to cover the new course extent.
    /// </summary>
    private static void SetupFollowCamera(GameObject player)
    {
        if (player == null)
        {
            Debug.LogWarning("LevelBuilder: Cannot setup camera - Player not found.");
            return;
        }

        // 1. Disable the old RoomCameraFollow system
        GameObject cameraSystem = GameObject.Find("Camera System");
        if (cameraSystem != null)
        {
            // Dynamically check and disable RoomCameraFollow or RailCameraFollow components to prevent compile-time errors
            foreach (var comp in cameraSystem.GetComponents<MonoBehaviour>())
            {
                if (comp != null && (comp.GetType().Name == "RoomCameraFollow" || comp.GetType().Name == "RailCameraFollow"))
                {
                    comp.enabled = false;
                    Debug.Log("LevelBuilder: Disabled old RoomCameraFollow component dynamically.");
                }
            }
        }

        // 2. Update Room Starts / Room Ends markers to cover the new course
        GameObject roomStarts = GameObject.Find("Room Starts");
        if (roomStarts != null)
        {
            roomStarts.transform.position = new Vector3(0f, 3f, -5f);
        }

        GameObject roomEnds = GameObject.Find("Room Ends");
        if (roomEnds != null)
        {
            roomEnds.transform.position = new Vector3(0f, 3f, 295f);
        }

        // 3. Find all cameras in the scene
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Camera mainCam = null;

        // Try to find the player camera under GameObject (1)
        foreach (var cam in allCameras)
        {
            if (cam.CompareTag("MainCamera"))
            {
                if (cam.transform.parent != null && cam.transform.parent.name == "GameObject (1)")
                {
                    mainCam = cam;
                    break;
                }
            }
        }

        // Fallback to any active MainCamera
        if (mainCam == null)
        {
            foreach (var cam in allCameras)
            {
                if (cam.CompareTag("MainCamera") && cam.gameObject.activeInHierarchy)
                {
                    mainCam = cam;
                    break;
                }
            }
        }

        if (mainCam == null)
        {
            Debug.LogWarning("LevelBuilder: No active MainCamera found!");
            return;
        }

        // Disable all other cameras with tag MainCamera to prevent conflicts
        foreach (var cam in allCameras)
        {
            if (cam != mainCam && cam.CompareTag("MainCamera"))
            {
                cam.gameObject.SetActive(false);
                Debug.Log($"LevelBuilder: Disabled secondary camera '{cam.name}' to prevent conflicts.");
            }
        }

        // Ensure the chosen main camera is active
        mainCam.gameObject.SetActive(true);

        // 4. Configure PlayerFollowCamera directly on the chosen main camera
        PlayerFollowCamera followCam = mainCam.GetComponent<PlayerFollowCamera>();
        if (followCam == null)
        {
            followCam = mainCam.gameObject.AddComponent<PlayerFollowCamera>();
        }
        
        followCam.target = player.transform;
        followCam.offset = new Vector3(0f, 5f, -8f);
        followCam.followSmoothTime = 0.15f;
        followCam.rotationSmoothSpeed = 8f;
        followCam.lookOffset = new Vector3(0f, 1f, 0f);
        followCam.lookAheadDistance = 3f;
        followCam.minYPosition = -8f;
        followCam.collisionLayers = 1; // Default layer

        // 5. Disable CinemachineBrain to let PlayerFollowCamera control it
        var brain = mainCam.GetComponent<Unity.Cinemachine.CinemachineBrain>();
        if (brain != null)
        {
            brain.enabled = false;
            Debug.Log("LevelBuilder: Disabled CinemachineBrain to allow PlayerFollowCamera.");
        }

        Debug.Log("LevelBuilder: PlayerFollowCamera configured on " + mainCam.name);
    }
}
#endif
