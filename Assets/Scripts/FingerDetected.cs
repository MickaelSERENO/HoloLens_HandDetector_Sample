using UnityEngine;

namespace Sereno
{
    /// <summary>
    /// Represents a finger detected
    /// </summary>
    class FingerDetected
    {
        /// <summary>
        /// The finger position
        /// </summary>
        private Vector3 m_position;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos">The initial finger 3D position</param>
        public FingerDetected(Vector3 pos)
        {
            m_position = pos;
        }

        /// <summary>
        /// The finger 3D position
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return m_position;
            }
            set
            {
                m_position = value;
            }
        }
    }
}
