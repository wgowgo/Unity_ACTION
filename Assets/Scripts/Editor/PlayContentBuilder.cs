using System.IO;
using Fusion;
using Fusion.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 수업용 준비 도구. 전투 프리팹을 만들고 Play 씬에 바닥을 연결한다.
public static class PlayContentBuilder
{
    const string Folder = "Assets/Resources/Network";
    const string PlayerPath = Folder + "/PlayerAvatar.prefab";
    const string SessionPath = Folder + "/SessionDirectory.prefab";
    const string MatchPath = Folder + "/MatchState.prefab";
    const string BalancePath = Folder + "/CombatBalance.asset";
    const string DummyPath = Folder + "/FighterDummies.asset";
    const string BlueDummy = "Assets/Kevin Iglesias/Human Character Dummy/Prefabs/HumanDummy_M Blue.prefab";
    const string RedDummy = "Assets/Kevin Iglesias/Human Character Dummy/Prefabs/HumanDummy_M Red.prefab";

    [MenuItem("Tools/Action Game/Build Combat Content")]
    public static void Build()
    {
        BuildInternal(false);
    }

    [InitializeOnLoadMethod]
    static void Schedule()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (File.Exists(PlayerPath) && File.Exists(SessionPath) && File.Exists(MatchPath))
            {
                EnsureDummyAsset();
                return;
            }

            BuildInternal(true);
        };
    }

    static void BuildInternal(bool silent)
    {
        if (AssetDatabase.IsValidFolder("Assets/Resources") == false)
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (AssetDatabase.IsValidFolder(Folder) == false)
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Network");
        }

        EnsureDummyAsset();

        if (File.Exists(BalancePath) == false)
        {
            CombatBalanceConfig balance = ScriptableObject.CreateInstance<CombatBalanceConfig>();
            balance.ApplyDefaults();
            AssetDatabase.CreateAsset(balance, BalancePath);
        }

        if (File.Exists(SessionPath) == false)
        {
            SavePrefab(BuildSession(), SessionPath);
        }

        if (File.Exists(MatchPath) == false)
        {
            SavePrefab(BuildMatch(), MatchPath);
        }

        if (File.Exists(PlayerPath) == false)
        {
            SavePrefab(BuildPlayer(), PlayerPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        NetworkProjectConfigUtilities.RebuildPrefabTable();
        EnsureArena();

        if (silent == false)
        {
            EditorUtility.DisplayDialog("Action Game", "Combat prefabs and the Play scene floor are ready.", "OK");
        }
    }

    static GameObject BuildSession()
    {
        var root = new GameObject("SessionDirectory");
        root.AddComponent<NetworkObject>();
        root.AddComponent<PlayerSessionDirectory>();
        return root;
    }

    static GameObject BuildMatch()
    {
        var root = new GameObject("MatchState");
        root.AddComponent<NetworkObject>();
        root.AddComponent<MatchStateManager>();
        return root;
    }

    static GameObject BuildPlayer()
    {
        var root = new GameObject("PlayerAvatar");
        root.AddComponent<NetworkObject>();

        CharacterController controller = root.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.4f;
        controller.center = new Vector3(0f, 1f, 0f);
        controller.stepOffset = 0.3f;

        NetworkCharacterController motor = root.AddComponent<NetworkCharacterController>();
        motor.gravity = -20f;
        motor.jumpImpulse = 7.5f;
        motor.acceleration = 50f;
        motor.braking = 35f;
        motor.maxSpeed = GameConfig.WalkSpeed;
        motor.rotationSpeed = 0f;

        HitboxRoot hitboxRoot = root.AddComponent<HitboxRoot>();
        root.AddComponent<PlayerAvatar>();
        root.AddComponent<PlayerCamera>();

        CreateBody(root.transform);
        CreateWeapon(root.transform);
        CreateOrigin(root.transform);
        CreateHitbox(root.transform, "BodyHitbox", new Vector3(0f, 1f, 0f), 0.5f);
        CreateHitbox(root.transform, "HeadHitbox", new Vector3(0f, 1.65f, 0f), 0.28f);
        CreateHitbox(root.transform, "LegsHitbox", new Vector3(0f, 0.45f, 0f), 0.34f);
        hitboxRoot.InitHitboxes();
        hitboxRoot.SetMinBoundingRadius();
        return root;
    }

    static void CreateBody(Transform parent)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(parent, false);
        body.transform.localPosition = new Vector3(0f, 1f, 0f);
        body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        Object.DestroyImmediate(body.GetComponent<Collider>());
    }

    static void CreateWeapon(Transform parent)
    {
        GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        weapon.name = "Weapon";
        weapon.transform.SetParent(parent, false);
        weapon.transform.localPosition = new Vector3(0.35f, 1.15f, 0.45f);
        weapon.transform.localScale = new Vector3(0.12f, 0.12f, 0.85f);
        Object.DestroyImmediate(weapon.GetComponent<Collider>());
    }

    static void CreateOrigin(Transform parent)
    {
        var origin = new GameObject("AttackOrigin");
        origin.transform.SetParent(parent, false);
        origin.transform.localPosition = new Vector3(0f, 1.1f, 0.9f);
    }

    static void CreateHitbox(Transform parent, string name, Vector3 localPosition, float radius)
    {
        var hitboxObject = new GameObject(name);
        hitboxObject.transform.SetParent(parent, false);
        hitboxObject.transform.localPosition = localPosition;
        Hitbox hitbox = hitboxObject.AddComponent<Hitbox>();
        hitbox.Type = HitboxTypes.Sphere;
        hitbox.SphereRadius = radius;
    }

    static void SavePrefab(GameObject root, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    static void EnsureArena()
    {
        const string scenePath = "Assets/Scenes/Play.unity";
        Scene scene = EditorSceneManager.GetSceneByPath(scenePath);
        bool opened = false;
        if (scene.isLoaded == false)
        {
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            opened = true;
        }

        bool hasGround = false;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "ArenaGround")
            {
                hasGround = true;
            }

            if (root.name == "Main Camera")
            {
                root.transform.position = new Vector3(0f, 2.5f, -10.5f);
                root.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            }
        }

        if (hasGround == false)
        {
            Place(scene, "ArenaGround", PrimitiveType.Cube, new Vector3(0f, -0.5f, 0f), new Vector3(40f, 1f, 40f));
            Place(scene, "ArenaWallNorth", PrimitiveType.Cube, new Vector3(0f, 1.5f, 19f), new Vector3(40f, 3f, 1f));
            Place(scene, "ArenaWallSouth", PrimitiveType.Cube, new Vector3(0f, 1.5f, -19f), new Vector3(40f, 3f, 1f));
            Place(scene, "ArenaWallEast", PrimitiveType.Cube, new Vector3(19f, 1.5f, 0f), new Vector3(1f, 3f, 40f));
            Place(scene, "ArenaWallWest", PrimitiveType.Cube, new Vector3(-19f, 1.5f, 0f), new Vector3(1f, 3f, 40f));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        else if (opened == false)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (opened)
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    static void Place(Scene scene, string name, PrimitiveType type, Vector3 position, Vector3 scale)
    {
        GameObject box = GameObject.CreatePrimitive(type);
        box.name = name;
        box.transform.position = position;
        box.transform.localScale = scale;
        SceneManager.MoveGameObjectToScene(box, scene);
    }

    static void EnsureDummyAsset()
    {
        if (AssetDatabase.IsValidFolder("Assets/Resources") == false)
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (AssetDatabase.IsValidFolder(Folder) == false)
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Network");
        }

        GameObject blue = AssetDatabase.LoadAssetAtPath<GameObject>(BlueDummy);
        GameObject red = AssetDatabase.LoadAssetAtPath<GameObject>(RedDummy);
        FighterDummySet set = AssetDatabase.LoadAssetAtPath<FighterDummySet>(DummyPath);
        if (set == null)
        {
            set = ScriptableObject.CreateInstance<FighterDummySet>();
            AssetDatabase.CreateAsset(set, DummyPath);
        }

        if (set.PlayerOne == blue && set.PlayerTwo == red)
        {
            return;
        }

        set.PlayerOne = blue;
        set.PlayerTwo = red;
        EditorUtility.SetDirty(set);
        AssetDatabase.SaveAssets();
    }
}
