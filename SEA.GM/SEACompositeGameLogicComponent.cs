using System.Collections.Generic;
using VRage.Game.Components;
using VRage.ObjectBuilders;

namespace SEA.GM.GameLogic
{
    class SEACompositeGameLogicComponent : MyGameLogicComponent
    {
        private HashSet<MyGameLogicComponent> m_logicComponents;

        public SEACompositeGameLogicComponent(VRage.ModAPI.IMyEntity entity)
        {
            m_logicComponents = new HashSet<MyGameLogicComponent>();
        }

        public void Add(MyGameLogicComponent logicComponent)
        {
            if (!m_logicComponents.Contains(logicComponent))
            {
                logicComponent.SetContainer(Entity.Components);
                m_logicComponents.Add(logicComponent);
            }
        }

        public void Remove(MyGameLogicComponent logicComponent)
        {
            if (m_logicComponents.Contains(logicComponent))
            {
                logicComponent.Close();
                m_logicComponents.Remove(logicComponent);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            foreach (var component in m_logicComponents)
                component.UpdateOnceBeforeFrame();
        }

        public override void UpdateBeforeSimulation()
        {
            foreach (var component in m_logicComponents)
                component.UpdateBeforeSimulation();
        }

        public override void UpdateBeforeSimulation10()
        {
            foreach (var component in m_logicComponents)
                component.UpdateBeforeSimulation10();
        }

        public override void UpdateBeforeSimulation100()
        {
            foreach (var component in m_logicComponents)
                component.UpdateBeforeSimulation100();
        }

        public override void UpdateAfterSimulation()
        {
            foreach (var component in m_logicComponents)
                component.UpdateAfterSimulation();
        }

        public override void UpdateAfterSimulation10()
        {
            foreach (var component in m_logicComponents)
                component.UpdateAfterSimulation10();
        }

        public override void UpdateAfterSimulation100()
        {
            foreach (var component in m_logicComponents)
                component.UpdateAfterSimulation100();
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            foreach (var component in m_logicComponents)
                component.Init(objectBuilder);
        }

        public override void MarkForClose()
        {
            foreach (var component in m_logicComponents)
                component.MarkForClose();
        }

        public override void Close()
        {
            foreach (var component in m_logicComponents)
                component.Close();
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            foreach (var component in m_logicComponents)
            {
                MyObjectBuilder_EntityBase builder = component.GetObjectBuilder(copy);
                if (builder != null)
                    return builder;
            }
            return null;
        }

        public override T GetAs<T>()
        {
            foreach (var component in m_logicComponents)
                if (component is T)
                    return component as T;

            return null;
        }

        public static SEACompositeGameLogicComponent Get(VRage.ModAPI.IMyEntity entity)
        {
            MyGameLogicComponent component;

            if (entity.GameLogic.Container.TryGet<MyGameLogicComponent>(out component) && !(component is MyNullGameLogicComponent))
                if (component is SEACompositeGameLogicComponent)
                    return (SEACompositeGameLogicComponent)component;
                else
                {
                    SEACompositeGameLogicComponent new_component = new SEACompositeGameLogicComponent(entity);
                    entity.GameLogic.Container.Add<MyGameLogicComponent>(new_component);
                    new_component.Add(component);
                    return (SEACompositeGameLogicComponent)new_component;
                }

            component = new SEACompositeGameLogicComponent(entity);
            entity.GameLogic.Container.Add<MyGameLogicComponent>(component);

            return (SEACompositeGameLogicComponent)component;
        }

        public static SEACompositeGameLogicComponent Get(VRage.Game.ModAPI.Ingame.IMyEntity entity)
        {
            return Get(Sandbox.ModAPI.MyAPIGateway.Entities.GetEntityById(entity.EntityId));
        }
    }
}
