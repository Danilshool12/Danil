using UnityEngine;
using System.Collections.Generic;

namespace BossRoom.Visual
{
    /// <summary>
    /// Abstract base class for playing back the visual feedback of an Action.
    /// </summary>
    public abstract class ActionFX : ActionBase
    {
        protected ClientCharacterVisualization m_Parent;

        /// <summary>
        /// The default hit react animation; several different ActionFXs make use of this.
        /// </summary>
        public const string k_DefaultHitReact = "HitReact1";

        public ActionFX(ref ActionRequestData data, ClientCharacterVisualization parent) : base(ref data)
        {
            m_Parent = parent;
        }

        /// <summary>
        /// Starts the ActionFX. Derived classes may return false if they wish to end immediately without their Update being called.
        /// </summary>
        /// <returns>true to play, false to be immediately cleaned up.</returns>
        public abstract bool Start();

        public abstract bool Update();

        /// <summary>
        /// End is always called when the ActionFX finishes playing. This is a good place for derived classes to put
        /// wrap-up logic (perhaps playing the "puff of smoke" that rises when a persistent fire AOE goes away). Derived
        /// classes should aren't required to call base.End(); by default, the method just calls 'Cancel', to handle the
        /// common case where Cancel and End do the same thing.
        /// </summary>
        public virtual void End()
        {
            Cancel();
        }

        /// <summary>
        /// Cancel is called when an ActionFX is interrupted prematurely. It is kept logically distinct from End to allow
        /// for the possibility that an Action might want to play something different if it is interrupted, rather than
        /// completing. For example, a "ChargeShot" action might want to emit a projectile object in its End method, but
        /// instead play a "Stagger" animation in its Cancel method.
        /// </summary>
        public virtual void Cancel() { }

        public static ActionFX MakeActionFX(ref ActionRequestData data, ClientCharacterVisualization parent)
        {
            ActionLogic logic = GameDataSource.Instance.ActionDataByType[data.ActionTypeEnum].Logic;
            switch (logic)
            {
                case ActionLogic.Melee: return new MeleeActionFX(ref data, parent);
                case ActionLogic.RangedFXTargeted: return new FXProjectileTargetedActionFX(ref data, parent);
                case ActionLogic.Trample: return new TrampleActionFX(ref data, parent);
                case ActionLogic.AoE: return new AoeActionFX(ref data, parent);
                case ActionLogic.Stunned: return new AnimationOnlyActionFX(ref data, parent);
                case ActionLogic.Target: return new TargetActionFX(ref data, parent);
                case ActionLogic.ChargedShield: return new ChargedShieldActionFX(ref data, parent);
                case ActionLogic.ChargedLaunchProjectile: return new ChargedLaunchProjectileActionFX(ref data, parent);
                case ActionLogic.StealthMode: return new StealthModeActionFX(ref data, parent);
                default: throw new System.NotImplementedException();
            }
        }

        public virtual void OnAnimEvent(string id) { }
        public virtual void OnStoppedChargingUp(float finalChargeUpPercentage) { }

        /// <summary>
        /// Utility function that instantiates all the graphics in the Spawns list. If parentToSelf is true,
        /// the new graphics are parented to our owning Transform. If false, they are positioned/oriented the same
        /// as our Transform, but are not parented.
        /// </summary>
        protected List<SpecialFXGraphic> InstantiateSpecialFXGraphics(bool parentToSelf)
        {
            var returnList = new List<SpecialFXGraphic>();
            foreach (var prefab in Description.Spawns)
            {
                if (!prefab) { continue; } // skip blank entries in our prefab list
                returnList.Add(InstantiateSpecialFXGraphic(prefab, parentToSelf));
            }
            return returnList;
        }

        /// <summary>
        /// Utility function that instantiates one of the graphics from the Spawns list. If parentToSelf is true,
        /// the new graphic is parented to our owning Transform. If false, it's positioned/oriented the same
        /// as our Transform, but not parented.
        /// </summary>
        protected SpecialFXGraphic InstantiateSpecialFXGraphic(GameObject prefab, bool parentToSelf)
        {
            if (prefab.GetComponent<SpecialFXGraphic>() == null)
            {
                throw new System.Exception($"One of the Spawns on action {Description.ActionTypeEnum} does not have a SpecialFXGraphic component and can't be instantiated!");
            }
            var graphicsGO = GameObject.Instantiate(prefab, m_Parent.transform.position, m_Parent.transform.rotation, (parentToSelf ? m_Parent.transform : null));
            return graphicsGO.GetComponent<SpecialFXGraphic>();
        }
    }

}


