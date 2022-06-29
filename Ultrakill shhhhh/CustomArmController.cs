using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[BepInPlugin("CustomArms", "CustomArms", "1.0.0")]

public class Plugin : BaseUnityPlugin
{
    public static bool patched = false;

    public void Start()
    {
        if (!patched) // I've run into issues before with Start running twice and harmony being weird, so I'm resolving that here
        {
            Debug.Log("Starting custom arms");
            new Harmony("tempy.customArms").PatchAll();
            patched = true;
            StartCoroutine(LoadStockPrefabs());
        }
    }

    public IEnumerator LoadStockPrefabs()
    {
        Debug.Log("Trying to load prefabs from " + Environment.CurrentDirectory + "\\ULTRAKILL_Data\\StreamingAssets\\common");
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Environment.CurrentDirectory + "\\ULTRAKILL_Data\\StreamingAssets\\common");
        yield return request;
        if (request.assetBundle == null)
        {
            Debug.LogError("Couldn't load the prefabs asset bundle");
            yield break;
        }

        // Parellel go brrrrrrrrr
        AssetBundleRequest snakeRequest = request.assetBundle.LoadAssetAsync("ProjectileMinosPrime.prefab");
        AssetBundleRequest swingSnakeRequest = request.assetBundle.LoadAssetAsync("MinosPrimeSwingSnake.prefab");
        AssetBundleRequest gabeSpearRequest = request.assetBundle.LoadAssetAsync("GabrielThrownSpear.prefab");
        AssetBundleRequest zweiRequest = request.assetBundle.LoadAssetAsync("GabrielZweihander.prefab");

        yield return snakeRequest;
        if (snakeRequest.asset == null)
            Debug.LogError("Couldn't load the snake projectile");
        else
            CustomArmController.minosSnakeProjectilePrefab = snakeRequest.asset as GameObject;

        yield return swingSnakeRequest;
        if (swingSnakeRequest.asset == null)
            Debug.LogError("Couldn't load the snake swing");
        else
            CustomArmController.minosSnakeswingPrefab = swingSnakeRequest.asset as GameObject;

        yield return gabeSpearRequest;
        if (gabeSpearRequest.asset == null)
            Debug.LogError("Couldn't load the gabe spear");
        else
            CustomArmController.gabeSpearPrefab = gabeSpearRequest.asset as GameObject;

        yield return zweiRequest;
        if (zweiRequest.asset == null)
            Debug.LogError("Couldn't load the zwei");
        else
            CustomArmController.gabZweihanderPrefab = zweiRequest.asset as GameObject;


        request.assetBundle.Unload(false);

        CustomArmController.LoadStockArms();

        yield break;
    }
}

public static class CustomArmController
{
    public static GameObject currentFistObject; // like if you want v1 holding a zweihander for example
    private static Dictionary<int, CustomArmInfo> allArms = new Dictionary<int, CustomArmInfo>();

    public static GameObject minosSnakeProjectilePrefab;
    public static GameObject minosSnakeswingPrefab;
    public static GameObject gabeSpearPrefab;
    public static GameObject gabZweihanderPrefab;


    public static int armVariations;
    public static int currentVariation = -1;

    public static void LoadStockArms()
    {
        if (minosSnakeProjectilePrefab)
        {
            CustomArmInfo pinosArm = new CustomArmInfo();
            pinosArm.canParry = false;
            pinosArm.armColor = new Color(255, 255, 255, 255);
            pinosArm.onSwing.AddListener(delegate (Punch punch)
            {
                GameObject newSnake = GameObject.Instantiate<GameObject>(minosSnakeProjectilePrefab, punch.transform.position + (2f * punch.transform.forward), CameraController.Instance.transform.rotation);
                if (CameraFrustumTargeter.Instance.CurrentTarget)
                    newSnake.transform.LookAt(CameraFrustumTargeter.Instance.CurrentTarget.bounds.center);
                Projectile projectile = newSnake.GetComponentInChildren<Projectile>();
                projectile.playerBullet = true;
                projectile.friendly = true;
                projectile.damage = 12f;
                projectile.speed *= 2.754f;
                projectile.undeflectable = false;
                projectile.homingType = HomingType.None;
            });
            AddArmInfo(pinosArm);

            CustomArmInfo pinosMultiArm = new CustomArmInfo();
            pinosMultiArm.canParry = false;
            pinosMultiArm.armColor = new Color32(200, 200, 240, 255);
            pinosMultiArm.onSwing.AddListener(delegate (Punch punch)
            {
                foreach (EnemyIdentifier identifier in EnemyTracker.Instance.GetCurrentEnemies())
                {
                    GameObject newSnake = GameObject.Instantiate<GameObject>(minosSnakeProjectilePrefab, punch.transform.position + (2f * punch.transform.forward), CameraController.Instance.transform.rotation);
                    Projectile projectile = newSnake.GetComponentInChildren<Projectile>();
                    projectile.playerBullet = true;
                    projectile.friendly = true;
                    projectile.damage = 12f;
                    projectile.undeflectable = false;
                    projectile.homingType = HomingType.Loose;
                    if (identifier != null)
                    {
                        //projectile.target = identifier.GetComponentsInChildren<Collider>().Last().transform;
                        foreach (Collider collider in identifier.GetComponentsInChildren<Collider>())
                            if (projectile.target == null || collider.transform.position.y > projectile.target.position.y)
                                projectile.target = collider.transform;
                        newSnake.transform.LookAt(projectile.target.transform);
                    }
                }
            });
            AddArmInfo(pinosMultiArm);
        }

        if (gabeSpearPrefab)
        {
            CustomArmInfo gabeSpearArm = new CustomArmInfo();
            gabeSpearArm.canParry = false;
            gabeSpearArm.armColor = new Color32(255, 241, 122, 255);
            gabeSpearArm.onSwing.AddListener(delegate (Punch punch)
            {
                GameObject newProjectile = null;
                if (currentVariation == 2)
                    newProjectile = GameObject.Instantiate<GameObject>(gabeSpearPrefab, punch.transform.position + (2f * punch.transform.forward), CameraController.Instance.transform.rotation);
                if (CameraFrustumTargeter.Instance.CurrentTarget)
                    newProjectile.transform.LookAt(CameraFrustumTargeter.Instance.CurrentTarget.bounds.center);
                foreach (Projectile projectile in newProjectile.GetComponentsInChildren<Projectile>(true))
                {
                    projectile.playerBullet = true;
                    projectile.friendly = true;
                    //projectile.damage = 12f;
                    projectile.undeflectable = false;
                    projectile.homingType = HomingType.None;
                    //projectile.speed *= 2.754f;
                }
            });
            AddArmInfo(gabeSpearArm);
        }

        if (gabZweihanderPrefab)
        {
            CustomArmInfo zweiArm = new CustomArmInfo();
            zweiArm.canParry = true;
            zweiArm.armColor = new Color32(255, 255, 144, 255);
            zweiArm.onEquip.AddListener(delegate (FistControl fist)
            {
                currentFistObject = GameObject.Instantiate(gabZweihanderPrefab, fist.currentArmObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0));
                currentFistObject.transform.localPosition = new Vector3(-0.35f, 1.26f, 0);
                currentFistObject.transform.localEulerAngles = Vector3.zero;
                currentFistObject.transform.localScale = Vector3.one * 6.2409f;
            });
            zweiArm.onHit.AddListener(delegate (Punch punch, Vector3 hit, Transform target)
            {
                EnemyIdentifierIdentifier identifier = target.GetComponent<EnemyIdentifierIdentifier>();
                if (identifier)
                {
                    identifier.eid.DeliverDamage(identifier.gameObject, punch.transform.forward * 4500, hit, 15f, true, 0f);
                    GameObject newSword = GameObject.Instantiate(gabZweihanderPrefab);
                    Transform newBreak = newSword.transform.Find("GabrielWeaponBreak");
                    newBreak.SetParent(null);
                    newBreak.position = hit;
                    newBreak.GetChild(0).localScale *= 2.5f;
                    GameObject.Destroy(newSword);
                }
            });
            AddArmInfo(zweiArm);
        }
    }

    public static void AddArmInfo(CustomArmInfo info)
    {
        allArms.Add(info.variationNumber, info);
        armVariations++;
    }

    public class CustomArmInfo // Sadly ALL arms currently replace the feedbacker, I'd love to add functionality to replace the Knuckleblaster as well, and maybe even the Whiplash, as well as unlocking conditions
    {
        public bool canParry;
        public Color armColor;
        public List<GameObject> persistentObjects = new List<GameObject>();

        public ArmEquipEvent onEquip = new ArmEquipEvent();
        public ArmEvent onDestroy = new ArmEvent();
        public ArmEvent onSwing = new ArmEvent();
        public ArmHitEvent onHit = new ArmHitEvent();

        public int variationNumber { get; private set; } 
        public CustomArmInfo()
        {
            variationNumber = armVariations;
        }

        public class ArmEvent : UnityEvent<Punch>
        {

        }

        public class ArmEquipEvent : UnityEvent<FistControl>
        {

        }

        public class ArmHitEvent : UnityEvent<Punch, Vector3, Transform>
        {

        }
    }

    #region HARMONY_PATCHES

    // The only reason these are classes is because that's what I'm used to, yes I know it's weird but sorry

    [HarmonyPatch(typeof(FistControl), nameof(FistControl.ArmChange))]
    public static class Inject_CustomArms
    {
        public static void Prefix(ref int orderNum)
        {
            if (!CheatsController.Instance.cheatsEnabled)
                return;
            if (orderNum == 1 && currentVariation + 1 < armVariations)
            {
                orderNum = 0;
            }
            else
                currentVariation = armVariations;
        }

        public static void Postfix(int orderNum, FistControl __instance)
        {
            if (!CheatsController.Instance.cheatsEnabled)
                return;
            if (currentFistObject != null)
                GameObject.Destroy(currentFistObject);
            if (orderNum == 0)
            {
                currentVariation++;
                if (currentVariation + 1 > armVariations)
                    currentVariation = -1;
                else
                {
                    __instance.fistIcon.color = allArms[currentVariation].armColor;
                    allArms[currentVariation].onEquip.Invoke(__instance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Punch), "CheckForProjectile")]
    public static class Ensure_CorrectParry
    {
        public static bool Prefix(ref bool __result)
        {
            __result = currentVariation == -1 || allArms[currentVariation].canParry;
            return __result;
        }
    }

    [HarmonyPatch(typeof(Punch), "ParryProjectile")]
    public static class Ensure_CorrectParryProjectile
    {
        public static bool Prefix()
        {
            return currentVariation == -1 || allArms[currentVariation].canParry;
        }
    }

    [HarmonyPatch(typeof(Punch), "Start")]
    public static class Ensure_MinosTimeNotPersistant
    {
        public static void Postfix()
        {
            currentVariation = -1;
            if (currentFistObject)
                GameObject.Destroy(currentFistObject);
            foreach (CustomArmInfo info in allArms.Values)
                    if (info.persistentObjects != null)
                        foreach (GameObject go in info.persistentObjects)
                            if (go != null)
                                GameObject.Destroy(go); // end end end end 
        }
    }

    [HarmonyPatch(typeof(Punch), "PunchStart")]
    public static class Inject_CustomArmsPunch
    {
        public static void Postfix(Punch __instance)
        {
            if (currentVariation != -1)
                allArms[currentVariation].onSwing.Invoke(__instance);
        }
    }

    [HarmonyPatch(typeof(Punch), "PunchSuccess")]
    public static class Inject_CusotmArmsHit
    {
        public static void Postfix(Punch __instance, Vector3 point, Transform target)
        {
            if (currentVariation != -1)
                allArms[currentVariation].onHit.Invoke(__instance, point, target);
        }
    }
    #endregion
}