﻿using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KitchenAutomationPlus.Customs
{
    public class GrabberMixer : CustomAppliance
    {
        public override int BaseGameDataObjectID => ApplianceReferences.MixerHeated;
        public override string UniqueNameID => "grabbermixer";
        public override GameObject Prefab => Main.Bundle.LoadAsset<GameObject>("Mixer - Grabber");
        public override List<IApplianceProperty> Properties { get => ((Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.Mixer)).Properties; protected set => base.Properties = value; }
        public override List<Appliance.ApplianceProcesses> Processes { get => ((Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.Mixer)).Processes; protected set => base.Processes = value; }
        public override bool IsNonInteractive => false;
        public override OccupancyLayer Layer => OccupancyLayer.Default;
        public override bool IsPurchasable => false;
        public override bool IsPurchasableAsUpgrade => true;
        public override DecorationType ThemeRequired => DecorationType.Null;
        public override ShoppingTags ShoppingTags => ShoppingTags.Automation;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override PriceTier PriceTier => PriceTier.Expensive;
        public override bool StapleWhenMissing => false;
        public override bool SellOnlyAsDuplicate => false;
        public override bool PreventSale => false;
        public override bool IsNonCrated => false;
        public override List<(Locale, ApplianceInfo)> InfoList => InfoList = new List<(Locale, ApplianceInfo)>()
        {
            (Locale.English, new ApplianceInfo()
            {
                Name = "Grabber Mixer",
                Description = "Like having an extra pair of hands",
                Tags = new List<string>()
                {
                    "Automatic",
                    "Slow"
                },
                Sections = new List<Appliance.Section>()
                {
                    new Appliance.Section()
                    {
                        Title = "Multipurpose",
                        Description = "Can perform both $chop$ and $knead$"
                    },
                    new Appliance.Section()
                    {
                        Title = "$upgradable$ Grabber",
                        Description = "Pulls items to be processed"
                    }
                }
            })
        };

        public override List<Appliance> Upgrades => new List<Appliance>()
        {
            GDOUtils.GetExistingGDO(ApplianceReferences.MixerRapid) as Appliance
        };

        bool isRegistered = false;

        static FieldInfo playOnActive = ReflectionUtils.GetField<ApplianceProcessView>("PlayOnActive", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo animator = ReflectionUtils.GetField<ApplianceProcessView>("Animator", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo clip = ReflectionUtils.GetField<ApplianceProcessView>("Clip", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo sound = ReflectionUtils.GetField<ApplianceProcessView>("Sound", BindingFlags.NonPublic | BindingFlags.Instance);

        static FieldInfo pushObject = ReflectionUtils.GetField<ConveyItemsView>("PushObject", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void OnRegister(GameDataObject gameDataObject)
        {
            base.OnRegister(gameDataObject);

            if (!isRegistered)
            {
                ApplyMaterials();
                ApplyComponents();
                ApplyProperties();

                isRegistered = true;
            }

        }

        private void ApplyMaterials()
        {
            var materials = new Material[1];
            var materials2 = new Material[2];

            materials[0] = MaterialUtils.GetExistingMaterial("Wood - Default");
            MaterialUtils.ApplyMaterial(Prefab, "Block", materials);

            materials2[0] = MaterialUtils.GetExistingMaterial("Metal");
            materials2[1] = MaterialUtils.GetExistingMaterial("Wood 3");
            MaterialUtils.ApplyMaterial(Prefab, "cookingKnifeChopping/cookingKnifeChopping", materials2);

            materials2[0] = MaterialUtils.GetExistingMaterial("Plastic");
            materials2[1] = MaterialUtils.GetExistingMaterial("Wood 1");
            MaterialUtils.ApplyMaterial(Prefab, "cuttingBoard/cuttingBoard", materials2);

            materials[0] = MaterialUtils.GetExistingMaterial("Wood 4 - Painted");
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Counter2/Counter", materials);
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Counter2/Counter Doors", materials);
            materials[0] = MaterialUtils.GetExistingMaterial("Wood - Default");
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Counter2/Counter Surface", materials);
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Counter2/Counter Top", materials);
            materials[0] = MaterialUtils.GetExistingMaterial("Knob");
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Counter2/Handles", materials);

            materials[0] = MaterialUtils.GetExistingMaterial("Metal Black");
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Blade", materials);

            materials[0] = MaterialUtils.GetExistingMaterial("Metal - Brass");
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Heated", materials);
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Rapid/BezierCurve", materials);

            materials2[0] = MaterialUtils.GetExistingMaterial("Metal - Brass");
            materials2[1] = MaterialUtils.GetExistingMaterial("Metal - Brass");
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Rapid", materials2);

            materials[0] = MaterialUtils.GetExistingMaterial("Plastic - Yellow");
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Pusher", materials);

            materials2[0] = MaterialUtils.GetExistingMaterial("Plastic - Red");
            materials2[1] = MaterialUtils.GetExistingMaterial("Tape");
            MaterialUtils.ApplyMaterial(Prefab, "Blender/Stand", materials2);
        }

        private void ApplyProperties()
        {
            Properties.Add(new CApplianceGrabPoint());

            Properties.Add(
                new CConveyCooldown()
                {
                    Total = 0f
                });
            Properties.Add(
                new CConveyPushItems()
                {
                    Delay = 1f,
                    Push = false,
                    Grab = true,
                    GrabSpecificType = false
                });
        }

        private void ApplyComponents()
        {
            Prefab.AddComponent<HoldPointContainer>().HoldPoint = GameObjectUtils.GetChildObject(Prefab, "GameObject/HoldPoint").transform;

            GameObject basePrefab = ((Appliance)GDOUtils.GetExistingGDO(BaseGameDataObjectID)).Prefab;
            ApplianceProcessView baseApplianceProcessView = basePrefab.GetComponent<ApplianceProcessView>();

            ApplianceProcessView applianceProcessView = Prefab.AddComponent<ApplianceProcessView>();
            playOnActive.SetValue(applianceProcessView, playOnActive.GetValue(baseApplianceProcessView));
            animator.SetValue(applianceProcessView, Prefab.GetComponent<Animator>());
            clip.SetValue(applianceProcessView, clip.GetValue(baseApplianceProcessView));
            sound.SetValue(applianceProcessView, sound.GetValue(baseApplianceProcessView));

            ConveyItemsView conveyItemsView = Prefab.AddComponent<ConveyItemsView>();
            pushObject.SetValue(conveyItemsView, GameObjectUtils.GetChildObject(Prefab, "GameObject/HoldPoint"));
        }
    }
}
