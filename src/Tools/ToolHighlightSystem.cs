// Tools/ToolHighlightSystem.cs
// Purpose: track which road entities are "highlighted" by our zoning tool (hover / multi-select).
//   Don't draw the highlight here, just:
//     • remember which Entities are "on",
//     • add Updated to entities when they toggle,
//     • clear them when the tool shuts down.
//   Other systems or vanilla can react to Updated the same frame.
// Notes: intentionally lightweight and safe

namespace ARTZone.Tools
{
    using System.Collections.Generic;
    using Game;
    using Game.Common;
    using Unity.Entities;

    public sealed partial class ToolHighlightSystem : GameSystemBase
    {
        private HashSet<Entity> m_Highlighted = null!;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Highlighted = new HashSet<Entity>();
        }

        protected override void OnDestroy()
        {
            // Final signal so anything listening to Updated can reconcile state.
            ClearAll();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            // No per-frame logic needed.
        }

        /// <summary>
        /// Mark or unmark an Entity as highlighted.
        /// We also poke Updated so downstream systems see a change.
        /// </summary>
        public void HighlightEntity(Entity entity, bool enable)
        {
            if (!EntityManager.Exists(entity))
                return;

            if (enable)
            {
                if (m_Highlighted.Add(entity))
                {
                    EntityManager.AddComponent<Updated>(entity);
                }
            }
            else
            {
                if (m_Highlighted.Remove(entity))
                {
                    EntityManager.AddComponent<Updated>(entity);
                }
            }
        }

        /// <summary>
        /// Clear all tracked highlights (e.g. when deselecting or swapping tools).
        /// Every entity we drop from the set gets Updated.
        /// </summary>
        public void ClearAll()
        {
            if (m_Highlighted.Count == 0)
                return;

            foreach (var e in m_Highlighted)
            {
                if (EntityManager.Exists(e))
                {
                    EntityManager.AddComponent<Updated>(e);
                }
            }

            m_Highlighted.Clear();
        }
    }
}
