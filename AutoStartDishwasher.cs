﻿using Kitchen;
using KitchenLib.References;
using Unity.Collections;
using Unity.Entities;

namespace KitchenAutomationPlus
{
    internal class AutoStartDishwasher : RestaurantSystem
    {
        public struct CHasActivated : IComponentData { };

        EntityQuery ApplianceQuery;

        protected override void Initialise()
        {
            base.Initialise();
            ApplianceQuery = GetEntityQuery(new QueryHelper()
                .All(typeof(CAppliance), typeof(CItemProvider), typeof(CIsInactive)));
        }

        protected override void OnUpdate()
        {
            if (Main.PrefManager.Get<int>(Main.DISHWASHER_AUTO_START_ID) == 1)
            {
                NativeArray<Entity> entities = ApplianceQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in entities)
                {
                    if (Require(entity, out CAppliance appliance) && appliance.ID == ApplianceReferences.DishWasher)
                    {
                        if (Require(entity, out CItemProvider provider))
                        {
                            if (Has<CHasActivated>() && provider.Available < 1)
                            {
                                EntityManager.RemoveComponent<CHasActivated>(entity);
                            }

                            if (!Has<CHasActivated>(entity) && provider.Available >= provider.Maximum)
                            {
                                Main.LogInfo("Auto-starting Dishwasher");
                                EntityManager.AddComponent<CHasActivated>(entity);
                                EntityManager.RemoveComponent<CIsInactive>(entity);
                            }
                        }
                    }
                }
                entities.Dispose();
            }
        }
    }
}
