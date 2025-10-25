// Tools/ToolHighlightSystem.cs
namespace ARTZone.Tools
{
    using System.Collections.Generic;
    using Game;
    using Game.Common;
    using Unity.Entities;

    /// <summary>
    /// Minimal, local highlighter that satisfies our tool’s needs.
    /// It intentionally no-ops visual effects for now; we just track a set
    /// and poke Updated so other systems can react.
    /// </summary>
    public sealed partial class ToolHighlightSystem : GameSystemBase
    {
        private HashSet<Entity> m_Highlighted = null!;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Highlighted = new HashSet<Entity>();
        }

        protected override void OnUpdate()
        {
            // No per-frame work needed for now.
        }

        /// <summary>
        /// Toggle highlight bookkeeping for an entity.
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
        /// Clear all tracked highlights (used on cancel/tool swap).
        /// </summary>
        public void ClearAll()
        {
            if (m_Highlighted.Count == 0)
                return;

            foreach (Entity e in m_Highlighted)
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
