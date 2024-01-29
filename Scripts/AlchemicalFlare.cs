// Project:     First Person Lighting for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace FirstPersonLighting
{
    public class AlchemicalFlare : DaggerfallUnityItem
    {

        public const int AlchemicalFlareTemplateIndex = 1773;

        const int baseValue = 6;    // Base gold value


        public AlchemicalFlare() : this(baseValue)
        {
        }


        public AlchemicalFlare(int baseValue) : base(ItemGroups.MiscellaneousIngredients2, AlchemicalFlareTemplateIndex)
        {
            value = baseValue;
            stackCount = 8;
        }


        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(AlchemicalFlare).ToString();
            return data;
        }

        public override string ItemName
        {
            get { return "Alchemical Flare"; }
        }


        public override string LongName
        {
            get { return ItemName; }
        }


        public override bool UseItem(ItemCollection collection)
        {
            collection.RemoveItem(IsAStack() ? collection.SplitStack(this, 1) : this);

            GameObject flare = new GameObject("Alchemical Flare");
            flare.transform.parent = GameObjectHelper.GetBestParent();
            flare.AddComponent<LitFlare>();

            //Pop the inventory window, assuming it is showing
            DaggerfallUI.Instance.PopToHUD();

            return true;
        }


        public override bool IsStackable()
        {
            return true;
        }


    } //class AlchemicalFlare


    public class LitFlare : MonoBehaviour
    {
        const float runTime = 30f;

        //Assets
        Texture2D litAlchemicalFlareTexture;
        Flare flameFlare;
        AudioClip flareStrike;
        AudioClip flareHiss;

        GameObject flameObject;
        Light flameLight;
        LensFlare lensFlare;
        Rigidbody rigidBody;
        float startTime;
        AudioSource audioSource;


        void Start()
        {
            Mod mod = FirstPersonLightingMod.Mod;
            litAlchemicalFlareTexture = mod.GetAsset<Texture2D>("LitAlchemicalFlare");
            flameFlare = mod.GetAsset<Flare>("AlchemicalFlare");
            flareStrike = mod.GetAsset<AudioClip>("FlareStrike");
            flareHiss = mod.GetAsset<AudioClip>("FlareHiss");

            startTime = Time.time;

            DaggerfallBillboard dfBillboard = gameObject.AddComponent<DaggerfallBillboard>();
            Texture2D texture = litAlchemicalFlareTexture;
            dfBillboard.SetMaterial(texture, new Vector2(0.1f, 0.1f), false);

            flameObject = new GameObject("Flare Flame");
            flameObject.transform.parent = gameObject.transform;
            flameObject.transform.localPosition = new Vector3(0, 0.06f, 0);

            flameLight = flameObject.AddComponent<Light>();
            flameLight.type = LightType.Point;

            float r = Random.Range(0.3f, 1f);
            float g = Random.Range(0.3f, 1f);
            float b = Random.Range(0.3f, 1f);
            Color color = new Color(r, g, b);

            flameLight.color = color;

            flameLight.range = 6f;

            lensFlare = flameObject.AddComponent<LensFlare>();
            lensFlare.flare = flameFlare;
            lensFlare.color = color;

            Vector3 forward = GameManager.Instance.MainCamera.transform.forward;
            gameObject.transform.position = GameManager.Instance.MainCamera.transform.position + forward;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.1f, 0.1f, 0.1f);
            collider.isTrigger = false;
            collider.material = new PhysicMaterial
            {
                bounciness = 0.05f,
                dynamicFriction = 0.9f,
                staticFriction = 0.9f
            };

            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.velocity = forward * 12;
            rigidBody.mass = 0.1f;

            //Setting collision detection mode to continuous to prevent the flare from falling through the floor/walls.
            //It will be set back to Discrete after it stops moving.
            rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1;
            audioSource.panStereo = 0f;
            audioSource.volume = DaggerfallUnity.Settings.SoundVolume;
            audioSource.PlayOneShot(flareStrike);
        }


        void Update()
        {
            if (GameManager.IsGamePaused)
                return;

            float remainingTime = startTime + runTime - Time.time;

            if (remainingTime <= 0)
            {
                GameObject.Destroy(gameObject);
                return;
            }

            //Starts hissing after a second.
            if (audioSource.loop == false && remainingTime > runTime - 1)
            {
                audioSource.clip = flareHiss;
                audioSource.loop = true;
                audioSource.volume = DaggerfallUnity.Settings.SoundVolume;
                audioSource.Play();
            }

            float magnitude = Random.Range(0.8f, 1f);

            if (remainingTime > runTime - 3)
            {
                magnitude *= (runTime - remainingTime) / 3;
            }
            else if (remainingTime < 3)
            {
                magnitude *= remainingTime / 3;
                audioSource.volume *= 0.98f;
            }

            lensFlare.brightness = magnitude;
            flameLight.intensity = magnitude;

            //Setting collision detection back to discrete after flare comes to a rest, for efficiency.
            if (rigidBody.velocity == Vector3.zero)
                rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;

        }


    } //class LitFlare


} //namespace
