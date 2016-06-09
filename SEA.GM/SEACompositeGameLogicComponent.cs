using System.Collections.Generic;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SEA.GM.GameLogic
{
    class SEACompositeGameLogicComponent : MyGameLogicComponent
    {
        private HashSet<MyGameLogicComponent> m_logicComponents;

        public SEACompositeGameLogicComponent(IMyEntity entity)
        {
            m_logicComponents = new HashSet<MyGameLogicComponent>();
        }

        public void Add(MyGameLogicComponent logicComponent)
        {
            logicComponent.SetContainer(Entity.Components);
            m_logicComponents.Add(logicComponent);
        }

        public void Remove(MyGameLogicComponent logicComponent)
        {
            logicComponent.Close();
            m_logicComponents.Remove(logicComponent);
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

        public static SEACompositeGameLogicComponent Get(IMyEntity entity)
        {
            MyGameLogicComponent gameLogic;

            if (entity.GameLogic.Container.TryGet<MyGameLogicComponent>(out gameLogic) && !(gameLogic is MyNullGameLogicComponent))
                if (gameLogic is SEACompositeGameLogicComponent)
                    return (SEACompositeGameLogicComponent)gameLogic;
                else
                {
                    var new_gameLogic = new SEACompositeGameLogicComponent(entity);
                    new_gameLogic.Add(gameLogic);
                    entity.GameLogic.Container.Add<MyGameLogicComponent>(new_gameLogic);
                    return (SEACompositeGameLogicComponent)new_gameLogic;
                }

            gameLogic = new SEACompositeGameLogicComponent(entity);
            entity.GameLogic.Container.Add<MyGameLogicComponent>(gameLogic);

            return (SEACompositeGameLogicComponent)gameLogic;

            /*            
            SEAUtilities.Logging.Static.WriteLine(" Types:");
            foreach (var e in entity.GameLogic.Container.GetComponentTypes())
                SEAUtilities.Logging.Static.WriteLine("     " + e.ToString());

            SEAUtilities.Logging.Static.WriteLine(" Full Types:");
            foreach (var e in entity.GameLogic.Container)
                SEAUtilities.Logging.Static.WriteLine("     " + e.GetType().ToString());
            */
        }
    }
}
