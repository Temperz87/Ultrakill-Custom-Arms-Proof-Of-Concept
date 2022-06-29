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

        // Parallel go brrrrrrrrr
        AssetBundleRequest snakeRequest = request.assetBundle.LoadAssetAsync("ProjectileMinosPrime.prefab");
        AssetBundleRequest swingSnakeRequest = request.assetBundle.LoadAssetAsync("MinosPrimeSwingSnake.prefab");
        AssetBundleRequest gabeSpearRequest = request.assetBundle.LoadAssetAsync("GabrielThrownSpear.prefab");
        AssetBundleRequest zweiRequest = request.assetBundle.LoadAssetAsync("GabrielZweihander.prefab");
        AssetBundleRequest chargeRequest = request.assetBundle.LoadAssetAsync("ProjectileDecorative 2.prefab");

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
            CustomArmController.gabeZweihanderPrefab = zweiRequest.asset as GameObject;

        yield return chargeRequest;
        if (chargeRequest.asset == null)
            Debug.LogError("Couldn't load the charge projectile prefab");
        else
            CustomArmController.chargeProjectilePrefab = chargeRequest.asset as GameObject;

        request.assetBundle.Unload(false);

        CustomArmController.LoadStockArms();

        yield break;
    }
}

public static class CustomArmController
{
    public static GameObject currentFistObject; // like if you want v1 holding a zweihander for example
    private static Dictionary<int, CustomArmInfo> allArms = new Dictionary<int, CustomArmInfo>();
    private static Dictionary<int, CustomArmInfo> allBlueArms = new Dictionary<int, CustomArmInfo>();
    private static Dictionary<int, CustomArmInfo> allRedArms = new Dictionary<int, CustomArmInfo>();

    public static GameObject minosSnakeProjectilePrefab;
    public static GameObject minosSnakeswingPrefab;
    public static GameObject gabeSpearPrefab;
    public static GameObject gabeZweihanderPrefab;
    public static GameObject chargeProjectilePrefab;

    public static CustomArmInfo currentArm;
    public static int blueArmVariations;
    public static int redArmVariations;
    public static int currentVariation = -1;

    public static void LoadStockArms()
    {
        if (minosSnakeProjectilePrefab)
        {
            CustomArmInfo pinosArm = new CustomArmInfo();
            pinosArm.canUseDefaultAlt = false;
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
            pinosMultiArm.canUseDefaultAlt = false;
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
            gabeSpearArm.canUseDefaultAlt = false;
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

        if (gabeZweihanderPrefab)
        {
            CustomArmInfo zweiArm = new CustomArmInfo();
            zweiArm.canUseDefaultAlt = true;
            zweiArm.armColor = new Color32(255, 255, 144, 255);
            zweiArm.onEquip.AddListener(delegate (FistControl fist)
            {
                currentFistObject = GameObject.Instantiate(gabeZweihanderPrefab, fist.currentArmObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0));
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
                    GameObject newSword = GameObject.Instantiate(gabeZweihanderPrefab);
                    Transform newBreak = newSword.transform.Find("GabrielWeaponBreak");
                    newBreak.SetParent(null);
                    newBreak.position = hit;
                    newBreak.GetChild(0).localScale *= 2.5f;
                    GameObject.Destroy(newSword);
                }
            });
            AddArmInfo(zweiArm);
        }

        if (chargeProjectilePrefab)
        {
            CustomArmInfo vortexArm = new CustomArmInfo();
            vortexArm.canUseDefaultAlt = false;
            vortexArm.armColor = new Color32(31, 68, 156, 255);
            vortexArm.type = FistType.Heavy;
            vortexArm.onEquip.AddListener(delegate (FistControl fist)
            {
                currentFistObject = GameObject.Instantiate(chargeProjectilePrefab, fist.currentArmObject.transform.Find("Armature").Find("clavicle").Find("wrist").Find("hand").Find("Holder (1)"));
                GameObject.Destroy(currentFistObject.GetComponent<Projectile>());
                GameObject.Destroy(currentFistObject.transform.Find("Sphere").gameObject);
                currentFistObject.GetComponent<MeshRenderer>().enabled = false;
                currentFistObject.transform.Find("ChargeEffect (1)").GetComponent<MeshRenderer>().enabled = false;
                currentFistObject.transform.localPosition = Vector3.zero;
                currentFistObject.transform.localEulerAngles = Vector3.zero;
                currentFistObject.transform.localScale = Vector3.one;

                Transform particleSystem = currentFistObject.transform.Find("ChargeEffect (1)").GetChild(0);
                particleSystem.localPosition = Vector3.zero;
                particleSystem.localScale = Vector3.one * 1.25f;
                currentFistObject.SetActive(false);
            });
            vortexArm.onStartRedAlt.AddListener(delegate (Punch punch)
            {
                IEnumerator heldRoutine()
                {
                    Animator anim = punch.GetComponent<Animator>();
                    if (anim.speed == 0)
                        anim.speed = 1;
                    yield return null;
                    if (!InputManager.Instance.InputSource.Punch.IsPressed)
                        yield break;
                    currentFistObject.SetActive(true);
                    float speed = anim.speed;
                    GunControl.Instance.NoWeapon();
                    anim.speed = 0f;
                    ProjectileParryZone ppz = punch.transform.parent.GetComponentInChildren<ProjectileParryZone>();
                    while (InputManager.Instance.InputSource.Punch.IsPressed && currentArm == vortexArm)
                    {
                        if (InputManager.Instance.InputSource.ChangeFist.IsPressed)
                            break;
                        Projectile proj = ppz.CheckParryZone();
                        if (proj != null && !proj.undeflectable && !proj.playerBullet)
                        {
                            if (!vortexArm.persistentObjects.Contains(proj.gameObject))
                            {
                                vortexArm.persistentObjects.Add(proj.gameObject);
                                proj.transform.SetParent(null);
                                proj.gameObject.SetActive(false);
                                proj.playerBullet = true;
                                proj.friendly = true;
                                proj.undeflectable = false;
                                proj.homingType = HomingType.None;
                                proj.target = null;
                                proj.hittingPlayer = false;
                            }
                        }
                        yield return null;
                    }
                    GunControl.Instance.YesWeapon();
                    anim.speed = speed;
                    if (currentFistObject != null)
                        currentFistObject.SetActive(false);
                }
                FistControl.Instance.StartCoroutine(heldRoutine());
            });
            vortexArm.onSwing.AddListener(delegate (Punch punch)
            {
                if (vortexArm.persistentObjects.Count <= 0)
                    return;
                foreach (GameObject persistent in vortexArm.persistentObjects)
                {
                    if (persistent == null)
                        continue;
                    persistent.transform.position = punch.transform.position + (2f * punch.transform.forward);
                    persistent.transform.rotation = CameraController.Instance.transform.rotation;
                    if (CameraFrustumTargeter.Instance.CurrentTarget)
                        persistent.transform.LookAt(CameraFrustumTargeter.Instance.CurrentTarget.bounds.center);
                    persistent.SetActive(true);
                    foreach (Projectile projecile in persistent.GetComponentsInChildren<Projectile>(true))
                        Traverse.Create(punch).Method("ParryProjectile", new object[] { projecile }).GetValue(projecile);
                }
                vortexArm.persistentObjects = new List<GameObject>();
                TimeController.Instance.ParryFlash();
            });
            AddArmInfo(vortexArm);
        }
    }

    public static void AddArmInfo(CustomArmInfo info)
    {
        if (info.type == FistType.Standard)
        {
            info.variationNumber = blueArmVariations;
            allBlueArms.Add(info.variationNumber, info);
            blueArmVariations++;
        }
        else
        {
            Debug.Log("adding red arm, variations is " + redArmVariations);
            info.variationNumber = redArmVariations;
            allRedArms.Add(info.variationNumber, info);
            redArmVariations++;
            Debug.Log("added red arm, variations is " + redArmVariations);
        }
        allArms.Add(info.variationNumber + (info.type == FistType.Standard ? 0 : 2048), info); // this solution will work for now, but the day we have 2048 blue arms it won't
    }

    public class CustomArmInfo
    {
        public bool canUseDefaultAlt;
        public Color armColor;
        public List<GameObject> persistentObjects = new List<GameObject>();

        public int variationNumber;
        public FistType type;
        public ArmEquipEvent onEquip = new ArmEquipEvent();
        public ArmEvent onDestroy = new ArmEvent();
        public ArmEvent onSwing = new ArmEvent();
        public ArmHitEvent onHit = new ArmHitEvent();
        public ArmEvent onStartRedAlt = new ArmEvent();
        public ArmParryEvent onParry = new ArmParryEvent();
        public CustomArmInfo()
        {
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

        public class ArmParryEvent : UnityEvent<Punch, Projectile>
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
            if (orderNum == 1)
            {
                if (currentVariation + 1 < blueArmVariations)
                    orderNum = 0;
                else
                    currentVariation = -2;
            }
            else if (orderNum == 0)
            {
                if (currentVariation + 1 < redArmVariations)
                    orderNum = 1;
                else
                    currentVariation = -2;
            }
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
                if (currentVariation + 1 > blueArmVariations)
                {
                    currentVariation = -1;
                    currentArm = null;
                }
                else
                {
                    if (allBlueArms.ContainsKey(currentVariation))
                    {
                        currentArm = allBlueArms[currentVariation];
                        __instance.fistIcon.color = currentArm.armColor;
                        currentArm.onEquip.Invoke(__instance);
                    }
                }
            }
            else
            {
                currentVariation++;
                if (currentVariation + 1 > redArmVariations)
                {
                    currentVariation = -1;
                    currentArm = null;
                }
                else
                {
                    if (allRedArms.ContainsKey(currentVariation))
                    {
                        currentArm = allRedArms[currentVariation];
                        __instance.fistIcon.color = currentArm.armColor;
                        currentArm.onEquip.Invoke(__instance);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Punch), "CheckForProjectile")]
    public static class Ensure_CorrectParry
    {
        public static bool Prefix(ref bool __result)
        {
            if (currentArm == null || currentVariation == -1)
                return true;
            __result = currentArm.canUseDefaultAlt;
            return __result;
        }
    }

    [HarmonyPatch(typeof(Punch), "ParryProjectile")]
    public static class Ensure_CorrectParryProjectile
    {
        public static bool Prefix()
        {
            return currentArm == null || currentVariation == -1 || currentArm.canUseDefaultAlt;
        }

        public static void Postfix(Projectile proj, Punch __instance)
        {
            if (currentArm != null && currentArm.canUseDefaultAlt)
            {
                currentArm.onParry.Invoke(__instance, proj);
            }
        }
    }

    [HarmonyPatch(typeof(Punch), "BlastCheck")]
    public static class Ensure_ShockWaveHeld
    {
        public static bool Prefix(Punch __instance)
        {
            if (currentArm != null)
                currentArm.onStartRedAlt.Invoke(__instance);
            return currentArm == null || currentArm.canUseDefaultAlt;
        }
    }

    [HarmonyPatch(typeof(Punch), "Start")]
    public static class Ensure_MinosCustomArm
    {
        public static void Postfix()
        {
            currentVariation = -1;
            currentArm = null;
            if (currentFistObject)
                GameObject.Destroy(currentFistObject);
            foreach (CustomArmInfo info in allArms.Values)
            {
                if (info.persistentObjects != null)
                    foreach (GameObject go in info.persistentObjects)
                        if (go != null)
                            GameObject.Destroy(go); // end end end end 
                info.persistentObjects = new List<GameObject>();
            }
        }
    }

    [HarmonyPatch(typeof(Punch), "PunchStart")]
    public static class Inject_CustomArmsPunch
    {
        public static void Postfix(Punch __instance)
        {
            if (currentArm != null)
                currentArm.onSwing.Invoke(__instance);
        }
    }

    [HarmonyPatch(typeof(Punch), "PunchSuccess")]
    public static class Inject_CusotmArmsHit
    {
        public static void Postfix(Punch __instance, Vector3 point, Transform target)
        {
            if (currentArm != null)
                currentArm.onHit.Invoke(__instance, point, target);
        }
    }
    #endregion
}