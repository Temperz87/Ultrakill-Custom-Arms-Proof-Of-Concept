using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UMM;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;


[UKPlugin("tempy.customArms", "Custom Arms", "1.6.0", "Custom arms or something", false, true)]
public class CustomArmMod : UKMod
{
    private static Harmony harmony;
    public override void OnModLoaded()
    {
        harmony = new Harmony("tempy.customArms");
        harmony.PatchAll();
        UKAPI.DisableCyberGrindSubmission("Mister fister is active!");
        StartCoroutine(LoadStockPrefabs());
    }

    public override void OnModUnload()
    {
        CustomArmController.UnloadArms();
        harmony.UnpatchSelf();
        UKAPI.RemoveDisableCyberGrindReason("Mister fister is active!");
        base.OnModUnload();
    }

    public IEnumerator LoadStockPrefabs()
    {
        // Parallel go brrrrrrrrr
        Debug.Log("Starting to load assets");
        AsyncOperationHandle snakeRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Minos Prime Snake.prefab");
        AsyncOperationHandle minosChargeRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Particles/Enemies/MinosProjectileCharge.prefab");
        AsyncOperationHandle gabeThrownSpearRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Gabriel/GabrielThrownSpear.prefab");
        AsyncOperationHandle zweiRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Gabriel/GabrielZweihander.prefab");
        AsyncOperationHandle fireRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Particles/Fire.prefab");
        AsyncOperationHandle chargeRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Decorative 2.prefab");
        AsyncOperationHandle virtueRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Virtue Insignia.prefab");

        //AsyncOperationHandle turretAimBeamRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Virtue Insignia.prefab");
        //AsyncOperationHandle turretBeamRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Turret Beam.prefab");
        //AsyncOperationHandle turretBeepRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Virtue Insignia.prefab");
        //AsyncOperationHandle turretBeepSoundRequest = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Virtue Insignia.prefab");
        //AsyncOperationHandle ferrymanRequest = Addressables.LoadAssetAsync<GameObject>("Ferryman.prefab");

        yield return snakeRequest;
        if (snakeRequest.Result == null)
            Debug.LogError("Couldn't load the snake projectile");
        else
            CustomArmController.minosSnakeProjectilePrefab = snakeRequest.Result as GameObject;

        yield return minosChargeRequest;
        if (minosChargeRequest.Result == null)
            Debug.LogError("Couldn't load minos's charge");
        else
            CustomArmController.minosChargePrefab = minosChargeRequest.Result as GameObject;

        yield return gabeThrownSpearRequest;
        if (gabeThrownSpearRequest.Result == null)
            Debug.LogError("Couldn't load the thrown gabe spear");
        else
            CustomArmController.gabeSpearThrownPrefab = gabeThrownSpearRequest.Result as GameObject;

        yield return zweiRequest;
        if (zweiRequest.Result == null)
            Debug.LogError("Couldn't load the zwei");
        else
            CustomArmController.gabeZweihanderPrefab = zweiRequest.Result as GameObject;

        yield return fireRequest;
        if (fireRequest.Result == null)
            Debug.LogError("Couldn't load the fire prefab");
        else
            CustomArmController.firePrefab = fireRequest.Result as GameObject;

        yield return chargeRequest;
        if (chargeRequest.Result == null)
            Debug.LogError("Couldn't load the charge projectile prefab");
        else
            CustomArmController.chargeProjectilePrefab = chargeRequest.Result as GameObject;

        yield return virtueRequest;
        if (virtueRequest.Result == null)
            Debug.LogError("Couldn't load the virtue charge prefab");
        else
            CustomArmController.virtueChargePrefab = virtueRequest.Result as GameObject;

        //yield return ferrymanRequest;
        //if (ferrymanRequest.Result == null)
        //Debug.LogError("Couldn't load the ferryman prefab");
        //else
        //CustomArmController.oarPrefab = ferrymanRequest.Result as GameObject;

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
    public static GameObject minosChargePrefab;
    public static GameObject gabeSpearThrownPrefab;
    public static GameObject gabeZweihanderPrefab;
    public static GameObject firePrefab;
    public static GameObject chargeProjectilePrefab;
    public static GameObject virtueChargePrefab;
    public static GameObject oarPrefab;

    public static CustomArmInfo currentArm;
    public static int blueArmVariations;
    public static int redArmVariations;
    public static int currentVariation = -1;

    public static void LoadStockArms()
    {
        if (minosSnakeProjectilePrefab && minosChargePrefab)
        {
            CustomArmInfo minosMultiArm = new CustomArmInfo();
            minosMultiArm.canUseDefaultAlt = false;
            minosMultiArm.armColor = new Color32(200, 200, 255, 255);


            const float initSnakesToFire = 3;
            float snakesToFire = initSnakesToFire;
            IEnumerator SwingRoutine(Punch punch)
            {
                Animator anim = punch.GetComponent<Animator>();
                if (anim.speed == 0)
                    anim.speed = 1;
                float speed = anim.speed;
                anim.speed = 0f;

                currentFistObject = GameObject.Instantiate(minosChargePrefab, punch.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).Find("Holder (1)"));
                currentFistObject.transform.localPosition = new Vector3(-0.21f, 0.064f, -0.017f);
                currentFistObject.transform.localEulerAngles = Vector3.zero;
                currentFistObject.transform.localScale = new Vector3(5f, 5f, 5f);
                float dt = 0f;
                while (dt < 0.5f)
                {
                    if (InputManager.Instance.InputSource.ChangeFist.IsPressed || !InputManager.Instance.InputSource.Punch.IsPressed || currentArm != minosMultiArm)
                    {
                        anim.speed = speed;
                        yield return new WaitForSeconds(0.1f);
                        yield break;
                    }
                    dt += Time.deltaTime;
                    yield return null;
                }

                List<EnemyIdentifier> identifiers = EnemyTracker.Instance.GetCurrentEnemies();
                for (int i = 1; i <= snakesToFire; i++)
                {
                    GameObject newSnake = GameObject.Instantiate<GameObject>(minosSnakeProjectilePrefab, punch.transform.position + (2f * punch.transform.forward), Quaternion.identity);
                    Projectile projectile = newSnake.GetComponentInChildren<Projectile>();
                    projectile.playerBullet = true;
                    projectile.friendly = true;
                    projectile.damage = 4f;
                    projectile.undeflectable = false;
                    projectile.homingType = HomingType.None;


                    if (CameraFrustumTargeter.Instance.CurrentTarget && CameraFrustumTargeter.Instance.CurrentTarget.TryGetComponent<EnemyIdentifier>(out _))
                        projectile.target = CameraFrustumTargeter.Instance.CurrentTarget.transform;
                    else if (identifiers.Count > 0)
                    {
                        int toUse = i;
                        while (toUse >= identifiers.Count)
                            toUse = toUse - identifiers.Count;
                        EnemyIdentifier identifier = identifiers[toUse];
                        if (identifier != null)
                        {
                            foreach (Collider collider in identifier.GetComponentsInChildren<Collider>())
                            {
                                if (projectile.target == null || collider.transform.position.y > projectile.target.position.y)
                                {
                                    projectile.target = collider.transform;
                                    projectile.homingType = HomingType.Gradual;
                                }
                            }
                        }
                    }
                    newSnake.transform.SetParent(punch.transform);
                    newSnake.transform.localEulerAngles = Vector3.zero;
                    float angle = UnityEngine.Random.Range(2f, 5f);
                    int numpadIdx = i;
                    if (numpadIdx > 9)
                        numpadIdx = numpadIdx - 9;
                    if (CameraFrustumTargeter.Instance.CurrentTarget)
                        newSnake.transform.LookAt(CameraFrustumTargeter.Instance.CurrentTarget.bounds.center);
                    else
                        newSnake.transform.rotation = CameraController.Instance.transform.rotation;
                    if (numpadIdx == 1 || numpadIdx == 4 || numpadIdx == 7)
                        newSnake.transform.localEulerAngles += new Vector3(1 - angle, 0, 0);
                    angle = UnityEngine.Random.Range(2f, 5f);
                    if (numpadIdx == 1 || numpadIdx == 2 || numpadIdx == 3)
                        newSnake.transform.localEulerAngles += new Vector3(0, 1 - angle, 0);
                    angle = UnityEngine.Random.Range(2f, 5f);
                    if (numpadIdx == 3 || numpadIdx == 6 || numpadIdx == 9)
                        newSnake.transform.localEulerAngles += new Vector3(angle, 0, 0);
                    angle = UnityEngine.Random.Range(2f, 5f);
                    if (numpadIdx == 7 || numpadIdx == 8 || numpadIdx == 9)
                        newSnake.transform.localEulerAngles += new Vector3(0, angle, 0);
                    newSnake.transform.SetParent(null);
                }

                /*
                if (snakesToFire >= 9)
                    StyleHUD.Instance.AddPoints((int)(100 * (snakesToFire % 10)), "<color=#C8C8FF>Asclepieion</color>"); // for example, if i fire 20-29 snakes then it'll add 200 points, cuz remainder moment
                */
                snakesToFire = initSnakesToFire;

                anim.speed = speed;
                yield break;
            }

            minosMultiArm.onSwing.AddListener(delegate (Punch punch, bool hitSomething)
            {
                FistControl.Instance.StartCoroutine(SwingRoutine(punch));
            });


            minosMultiArm.onHit.AddListener(delegate (Punch punch, Vector3 hit, Transform target)
            {
                EnemyIdentifierIdentifier identifier = target.GetComponent<EnemyIdentifierIdentifier>();
                if (identifier)
                {
                    identifier.eid.DeliverDamage(identifier.gameObject, punch.transform.forward * 4500, hit, .75f, true, 0f); // normal blue punches do 1 damage so we're just adding onto that
                    if (identifier.eid.dead)
                        snakesToFire = 9;
                    else
                    {
                        if (snakesToFire < 9)
                            snakesToFire += 1f;
                    }
                }
            });

            AddArmInfo(minosMultiArm);

            UKAPI.GetKeyBind("Serpent Arm", KeyCode.V).onPerformInScene.AddListener(delegate
            {
                while (currentArm != minosMultiArm)
                    FistControl.Instance.ScrollArm();
            });
        }

        if (gabeZweihanderPrefab && gabeSpearThrownPrefab && firePrefab)
        {
            CustomArmInfo gabeArm = new CustomArmInfo();
            float parriedDamage = 0;
            const float damageThreshold = 35f;

            gabeArm.canUseDefaultAlt = true;
            gabeArm.armColor = new Color32(255, 255, 144, 255);
            gabeArm.onEquip.AddListener(delegate (FistControl fist)
            {
                currentFistObject = GameObject.Instantiate(gabeZweihanderPrefab, fist.currentArmObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).Find("Holder (1)"));
                currentFistObject.transform.localPosition = new Vector3(-0.163f, -0.011f, -0.071f);
                currentFistObject.transform.localEulerAngles = new Vector3(-58.065f, -29.183f, 3.885f);
                currentFistObject.transform.localScale = Vector3.one * 0.54699f;
                if (parriedDamage >= damageThreshold && !currentFistObject.transform.Find("Fire boi"))
                {
                    GameObject fire = GameObject.Instantiate(firePrefab, currentFistObject.transform);
                    fire.transform.localPosition = new Vector3(0f, 0f, 5.87f);
                    fire.transform.localEulerAngles = new Vector3(180f, 90f, 90f);
                    fire.name = "Fire Boi";
                }
            });
            gabeArm.onSwing.AddListener(delegate (Punch punch, bool hitSomething)
            {
                if (parriedDamage < damageThreshold)
                    return;
                GameObject newProjectile = null;
                newProjectile = GameObject.Instantiate<GameObject>(gabeSpearThrownPrefab, punch.transform.position + (2f * punch.transform.forward), CameraController.Instance.transform.rotation);
                if (CameraFrustumTargeter.Instance.CurrentTarget)
                    newProjectile.transform.LookAt(CameraFrustumTargeter.Instance.CurrentTarget.bounds.center);
                foreach (Projectile projectile in newProjectile.GetComponentsInChildren<Projectile>(true))
                {
                    projectile.friendly = true;
                    projectile.playerBullet = true;
                    projectile.undeflectable = false;
                    projectile.damage = damageThreshold / 3f;
                    foreach (Explosion explosion in projectile.GetComponentsInChildren<Explosion>(true))
                        explosion.damage = (int)(damageThreshold / 3f);
                    projectile.homingType = HomingType.None;
                }

                Traverse.Create(punch).Field("alreadyBoostedProjectile").SetValue(true);
                parriedDamage = 0;
                GameObject.Destroy(currentFistObject.transform.Find("Fire Boi").gameObject);

                Transform newBreak = GameObject.Instantiate(currentFistObject.GetComponent<BreakParticle>().particle).transform;
                newBreak.transform.position = new Vector3(-0.163f, -0.011f, -0.071f);
                newBreak.transform.GetChild(0).localScale *= 2.5f;
            });
            gabeArm.onHit.AddListener(delegate (Punch punch, Vector3 hit, Transform target)
            {
                Transform newBreak = GameObject.Instantiate(currentFistObject.GetComponent<BreakParticle>().particle).transform;
                //newBreak.GetChild(0).localScale *= 2.5f;
                newBreak.transform.position = hit;
                EnemyIdentifierIdentifier identifier = target.GetComponent<EnemyIdentifierIdentifier>();
                if (identifier)
                {
                    identifier.eid.DeliverDamage(identifier.gameObject, punch.transform.forward * 4500, hit, parriedDamage / 3f, true, 0f);
                }
                newBreak.transform.GetChild(0).localScale *= 2.5f;
            });
            gabeArm.onParryProjectile.AddListener(delegate (Punch punch, Projectile proj)
            {
                proj.playerBullet = true;
                parriedDamage += proj.damage;
                if (proj.damage >= damageThreshold)
                    StyleHUD.Instance.AddPoints(250, "<color=yellow>INSTA-CHARGE</color>");
                proj.gameObject.SetActive(false);
                GameObject newSword = GameObject.Instantiate(gabeZweihanderPrefab);
                Transform newBreak = newSword.transform.Find("GabrielWeaponBreak");
                newBreak.SetParent(null);
                newBreak.position = proj.transform.position;
                newBreak.GetChild(0).localScale *= 2.5f;
                GameObject.Destroy(newSword);
                if (parriedDamage >= damageThreshold && !currentFistObject.transform.Find("Fire boi"))
                {
                    GameObject fire = GameObject.Instantiate(firePrefab, currentFistObject.transform);
                    fire.transform.localPosition = new Vector3(0f, 0f, 5.87f);
                    fire.transform.localEulerAngles = new Vector3(180f, 90f, 90f);
                    fire.name = "Fire Boi";
                }
            });

            AddArmInfo(gabeArm);
            UKAPI.GetKeyBind("ZweiHander Arm", KeyCode.C).onPerformInScene.AddListener(delegate
            {
                while (currentArm != gabeArm)
                    FistControl.Instance.ScrollArm();
            });
        }

        if (false)
        {
            CustomArmInfo tempestArm = new CustomArmInfo();
            tempestArm.canUseDefaultAlt = false;
            tempestArm.armColor = new Color32(245, 226, 15, 255);
            tempestArm.type = FistType.Standard;



            tempestArm.onSwing.AddListener((punch, hitSomething) =>
            {
                int mask = (1 << 14) | (1 << 10) | (1 << 11); // 14 is the layer for projectiles, 10 for libs, 11 for big limbs, basically masking for projectiles and enemies

                Collider[] colliders = Physics.OverlapSphere(CameraController.Instance.GetDefaultPos(), 8.5f, mask);
                List<EnemyIdentifierIdentifier> hitEids = new List<EnemyIdentifierIdentifier>();
                List<Projectile> hitProjectiles = new List<Projectile>();

                foreach (Collider col in colliders)
                {
                    EnemyIdentifierIdentifier eidid = col.GetComponentInParent<EnemyIdentifierIdentifier>();
                    if (eidid != null && !hitEids.Contains(eidid))
                    {
                        hitEids.Add(eidid);
                        if (eidid.eid.bigEnemy)
                            continue;
                        Vector3 forceDirection = (col.transform.position - CameraController.Instance.GetDefaultPos()).normalized;

                        eidid.eid.DeliverDamage(eidid.gameObject, forceDirection * 3000f, col.transform.position, 0, false);
                    }
                    else
                    {
                        Projectile proj = col.GetComponentInParent<Projectile>();
                        if (proj != null && !hitProjectiles.Contains(proj))
                        {
                            hitProjectiles.Add(proj);
                            if (proj.playerBullet || proj.friendly)
                                continue;

                            proj.homingType = HomingType.None;
                            proj.speed *= 1.5f;
                            proj.friendly = true;
                            proj.hittingPlayer = false;

                            proj.transform.Rotate(0f, 180f, 0f, Space.Self);
                        }
                    }
                }
            });

            AddArmInfo(tempestArm);
            UKAPI.GetKeyBind("Tempest Arm", KeyCode.H).onPerformInScene.AddListener(delegate
            {
                while (currentArm != tempestArm)
                    FistControl.Instance.ScrollArm();
            });
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
                    yield return null;
                    if (!InputManager.Instance.InputSource.Punch.IsPressed)
                        yield break;
                    currentFistObject.SetActive(true);
                    float speed = anim.speed;
                    GunControl.Instance.NoWeapon();
                    anim.speed = 0f;
                    ProjectileParryZone ppz = punch.transform.parent.GetComponentInChildren<ProjectileParryZone>();
                    int suckedObjects = 0;

                    while (InputManager.Instance.InputSource.Punch.IsPressed && currentArm == vortexArm)
                    {
                        if (InputManager.Instance.InputSource.ChangeFist.IsPressed)
                        {
                            anim.speed = speed;
                            break;
                        }
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
                                suckedObjects++;
                                if (suckedObjects >= 25)
                                {
                                    suckedObjects = 0;
                                    StyleHUD.Instance.AddPoints(50, "<color=cyan>HUGE SUCK</color>");
                                }
                            }
                        }
                        yield return null;
                    }


                    if (vortexArm.persistentObjects.Count > 0)
                    {
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
                        vortexArm.persistentObjects.Clear();
                        TimeController.Instance.ParryFlash();
                    }

                    GunControl.Instance.YesWeapon();
                    anim.speed = speed;
                    if (currentFistObject != null)
                        currentFistObject.SetActive(false);
                }
                FistControl.Instance.StartCoroutine(heldRoutine());
            });
            vortexArm.onSwing.AddListener(delegate (Punch punch, bool hitSomething)
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
                vortexArm.persistentObjects.Clear();
                TimeController.Instance.ParryFlash();
            });
            AddArmInfo(vortexArm);
            UKAPI.GetKeyBind("Vortex Arm", KeyCode.T).onPerformInScene.AddListener(delegate
            {
                while (currentArm != vortexArm)
                    FistControl.Instance.ScrollArm();
            });
        }

        if (virtueChargePrefab)
        {
            CustomArmInfo virtueArm = new CustomArmInfo();
            virtueArm.canUseDefaultAlt = false;
            virtueArm.armColor = new Color32(181, 246, 255, 255);
            virtueArm.type = FistType.Heavy;
            List<EnemyIdentifier> allVirtueMarkedIdentifiers = new List<EnemyIdentifier>();
            virtueArm.onHit.AddListener(delegate (Punch punch, Vector3 hitPoint, Transform target)
            {
                EnemyIdentifier identifier = target.gameObject.GetComponentInParent<EnemyIdentifier>();
                if (identifier && !allVirtueMarkedIdentifiers.Contains(identifier))
                    allVirtueMarkedIdentifiers.Add(identifier);
            });

            IEnumerator heldRoutine(Punch punch)
            {
                Animator anim = punch.GetComponent<Animator>();
                if (anim.speed == 0)
                    anim.speed = 1;
                yield return null;
                if (!InputManager.Instance.InputSource.Punch.IsPressed)
                    yield break;
                float speed = anim.speed;
                anim.speed = 0f;
                List<VirtueInsignia> insignias = new List<VirtueInsignia>();
                foreach (EnemyIdentifier identifier in allVirtueMarkedIdentifiers)
                {
                    if (identifier != null && !identifier.dead)
                    {
                        GameObject newVirtueCharge = GameObject.Instantiate(virtueChargePrefab, identifier.transform.position, Quaternion.identity);
                        VirtueInsignia newInsignia = newVirtueCharge.GetComponent<VirtueInsignia>();
                        newInsignia.target = identifier.transform;
                        newInsignia.noTracking = false;
                        newInsignia.predictiveVersion = null;
                        newInsignia.tag = "Moving"; // it has to be a built-in tag, so it's moving i guess
                        insignias.Add(newInsignia);
                    }
                }

                foreach (VirtueInsignia insignia in insignias)
                    insignia.damage = Math.Min(5 * insignias.Count, 25);

                float dt = 0f;
                if (insignias.Count <= 0)
                {
                    anim.speed = speed;
                    yield break;
                }

                GameObject reticlePivotInsignia = GameObject.Instantiate(new GameObject(), punch.transform.Find("Armature").Find("clavicle").Find("wrist").Find("hand").Find("Holder (1)"));
                reticlePivotInsignia.transform.localPosition = new Vector3(-0.144f, -0.035f, 0.079f);
                reticlePivotInsignia.transform.localEulerAngles = new Vector3(0, -90f, 45f);
                reticlePivotInsignia.transform.localScale = new Vector3(0.35024f, 0.17512f, 0.35024f);
                GameObject reticleInsignia = GameObject.Instantiate(virtueChargePrefab, reticlePivotInsignia.transform); // a pivot is required so the reticle doesn't do "funky stuff" when spinning
                reticleInsignia.transform.localScale = Vector3.one;
                reticleInsignia.GetComponent<VirtueInsignia>().target = reticlePivotInsignia.transform;


                while (dt < 0.95f && InputManager.Instance.InputSource.Punch.IsPressed && currentArm == virtueArm)
                {
                    if (InputManager.Instance.InputSource.ChangeFist.IsPressed)
                    {
                        anim.speed = speed;
                        yield break;
                    }
                    dt += Time.deltaTime;
                    yield return null;
                }

                GameObject.Destroy(reticlePivotInsignia);
                GameObject.Destroy(reticleInsignia);
                anim.speed = speed;
                if (dt < 0.95f)
                {
                    for (int i = 0; i < insignias.Count; i++)
                    {
                        if (insignias[i] != null)
                            GameObject.Destroy(insignias[i].gameObject);
                    }
                }
                yield break;
            }

            virtueArm.onStartRedAlt.AddListener(delegate (Punch punch)
            {
                FistControl.Instance.StartCoroutine(heldRoutine(punch));
            });
            AddArmInfo(virtueArm);
            UKAPI.GetKeyBind("Virtue Arm", KeyCode.V).onPerformInScene.AddListener(delegate
            {
                while (currentArm != virtueArm)
                    FistControl.Instance.ScrollArm();
            });
        }


        UKAPI.GetKeyBind("Feedbacker", KeyCode.Z).onPerformInScene.AddListener(delegate
        {
            while (currentArm != null && FistControl.Instance.currentPunch.type != FistType.Standard)
                FistControl.Instance.ScrollArm();
        });

        UKAPI.GetKeyBind("Knuckle Blaster", KeyCode.B).onPerformInScene.AddListener(delegate
        {
            while (currentArm != null && FistControl.Instance.currentPunch.type != FistType.Heavy)
                FistControl.Instance.ScrollArm();
        });

        //CustomArmInfo pushyArm = new CustomArmInfo();
        //pushyArm.canUseDefaultAlt = true;
        //pushyArm.armColor = new Color32(176, 11, 105, 255);
        //pushyArm.type = FistType.Heavy;
        //pushyArm.onStartRedAlt.AddListener(delegate (Punch punch)
        //{
        //    NewMovement.Instance.rb.velocity -= 100f * punch.transform.forward;
        //});
        //AddArmInfo(pushyArm);   
    }

    public static void UnloadArms()
    {
        GameObject.Destroy(currentFistObject);
        allArms = new Dictionary<int, CustomArmInfo>();
        allBlueArms = new Dictionary<int, CustomArmInfo>();
        allRedArms = new Dictionary<int, CustomArmInfo>();
        currentVariation = -1;
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
            info.variationNumber = redArmVariations;
            allRedArms.Add(info.variationNumber, info);
            redArmVariations++;
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
        public ArmSwingEvent onSwing = new ArmSwingEvent();
        public ArmHitEvent onHit = new ArmHitEvent();
        public ArmEvent onStartRedAlt = new ArmEvent();
        public ArmParryProjectileEvent onParryProjectile = new ArmParryProjectileEvent();
        public ArmParryEvent onParry = new ArmParryEvent();

        public CustomArmInfo() { }
        public class ArmEvent : UnityEvent<Punch> { }
        public class ArmSwingEvent : UnityEvent<Punch, bool> { }
        public class ArmEquipEvent : UnityEvent<FistControl> { }
        public class ArmHitEvent : UnityEvent<Punch, Vector3, Transform> { }
        public class ArmParryProjectileEvent : UnityEvent<Punch, Projectile> { }
        public class ArmParryEvent : UnityEvent<Punch, EnemyIdentifier> { }
    }

    #region HARMONY_PATCHES

    // The only reason these are classes is because that's what I'm used to, yes I know it's weird but sorry

    [HarmonyPatch(typeof(FistControl), nameof(FistControl.ArmChange))]
    public static class Inject_CustomArms
    {
        public static void Prefix(ref int orderNum)
        {
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
                        currentArm.onEquip.Invoke(__instance);
                        __instance.fistIcon.color = currentArm.armColor;
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
                        currentArm.onEquip.Invoke(__instance);
                        __instance.fistIcon.color = currentArm.armColor;
                    }
                }
            }
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
                currentArm.onParryProjectile.Invoke(__instance, proj);
            }
        }
    }

    [HarmonyPatch(typeof(Punch), "Parry")]
    public static class Ensure_CorrectParry
    {
        public static bool Prefix()
        {
            return currentArm == null || currentVariation == -1 || currentArm.canUseDefaultAlt;
        }

        public static void Postfix(EnemyIdentifier eid, Punch __instance)
        {
            if (currentArm != null && currentArm.canUseDefaultAlt)
            {
                currentArm.onParry.Invoke(__instance, eid);
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
                currentArm.onSwing.Invoke(__instance, (bool)Traverse.Create(__instance).Field("hitSomething").GetValue());
        }
    }

    [HarmonyPatch(typeof(Punch), "PunchSuccess")]
    public static class Inject_CustomArmsHit
    {
        public static void Postfix(Punch __instance, Vector3 point, Transform target)
        {
            if (currentArm != null)
                currentArm.onHit.Invoke(__instance, point, target);
        }
    }

    [HarmonyPatch(typeof(VirtueInsignia), "OnTriggerEnter")]
    public static class Ensure_InsigniaEnemiesDamaged
    {
        public static bool Prefix(VirtueInsignia __instance, Collider other)
        {
            if (__instance.CompareTag("Moving")) // tfw you can only use built in tags
            {
                other.GetComponentInParent<EnemyIdentifier>()?.DeliverDamage(other.GetComponentInParent<EnemyIdentifier>().gameObject, __instance.transform.up * 4500, other.transform.position, __instance.damage, true, 0f);
                __instance.gameObject.tag = "Untagged"; // this is to make it not deal continuos damage
                return false;
            }
            return true;
        }
    }
    #endregion
}