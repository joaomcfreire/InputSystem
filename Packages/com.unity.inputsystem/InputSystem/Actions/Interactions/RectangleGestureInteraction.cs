using System.ComponentModel;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEngine.InputSystem.Editor;
#endif

namespace UnityEngine.InputSystem.Interactions
{
    /// <summary>
    /// Performs the action if the control movement is recognized as a rectangle
    /// Currently, it only recognizes "right-down-left-up" rectangles.
    /// </summary>
    [DisplayName("RectangleGesture")]
    public class RectangleGestureInteraction : IInputInteraction
    {
        /// <summary>
        /// Threshold value, in number of Pointer pixels, for which a position difference on
        /// an Vector2 axis is allowed.
        /// </summary>
        ///
        /// <remarks>
        /// This value allows for imperfections when making the movement of a line with a Pointer.
        /// For instance with a Mouse, it is very hard do draw a straight line without moving some
        /// pixels above/below on (x,y). This value accounts for those imperfections.
        /// The smaller it is, the more "perfect straight line movements" need to be made in order for
        /// for an action to be performed.
        /// </remarks>
        public float sloppinessThreshold;
        private Vector2 m_PreviousPosition = Vector2.zero;
        private Vector2 m_LineStartPosition = Vector2.zero;
        private Vector2 m_StartPosition = Vector2.zero;

        private float sloppinessOrDefault => sloppinessThreshold > 0.0f ? sloppinessThreshold : InputSystem.settings.defaultSloppinessThreshold;
        private float sloppinessNegativeOrDefault => sloppinessOrDefault * (-1.0f);

        private enum RectanglePhase
        {
            None,
            FirstLineMovement,
            SecondLineMovement,
            ThirdLineMovement,
            LastLineMovement
        }

        private RectanglePhase m_CurrentPhase = RectanglePhase.None;

        private float AbsDiffX(Vector2 a, Vector2 b)
        {
            float value = Mathf.Abs(a.x - b.x);

            return value;
        }

        private float AbsDiffY(Vector2 a, Vector2 b)
        {
            float value = Mathf.Abs(a.y - b.y);

            return value;
        }

        private float DiffX(Vector2 a, Vector2 b)
        {
            float value = (a.x - b.x);

            return value;
        }

        private float DiffY(Vector2 a, Vector2 b)
        {
            float value = (a.y - b.y);

            return value;
        }

        private void StartRecognition()
        {
            m_CurrentPhase = RectanglePhase.FirstLineMovement;
            m_LineStartPosition = m_PreviousPosition;
            m_StartPosition = m_PreviousPosition;
        }

        public void Process(ref InputInteractionContext context)
        {
            if (context.ControlIsActuated())
            {
                Vector2 position = context.ReadValue<Vector2>();

                float absDiffX = AbsDiffX(position, m_PreviousPosition);
                float absDiffY = AbsDiffY(position, m_PreviousPosition);

                float startPositionDiffX = DiffX(position, m_LineStartPosition);
                float startPositionDiffY = DiffY(position, m_LineStartPosition);

                //TODO: Make it generic, currently detects right-down-left-up rectangles
                //TODO: Remove debug logs
                switch (m_CurrentPhase)
                {
                    case RectanglePhase.None:

                        float diffX = DiffX(position, m_PreviousPosition);
                        float diffY = DiffY(position, m_PreviousPosition);

                        // Checking for right line movement
                        if (diffX > 0.0f && absDiffY <= sloppinessOrDefault)
                        {
                            StartRecognition();
                            context.Started();
                            Debug.Log("First line ");
                        }
                        else
                        {
                            context.Canceled();
                        }
                        break;

                    case RectanglePhase.FirstLineMovement:

                        // Control movement is done to the right while waiting to go down
                        if (startPositionDiffY < sloppinessNegativeOrDefault && absDiffX < sloppinessOrDefault)
                        {
                            m_CurrentPhase = RectanglePhase.SecondLineMovement;
                            m_LineStartPosition = m_PreviousPosition;
                            Debug.Log("Second line");
                        }

                        // When control movement starts going up
                        if (startPositionDiffY > sloppinessOrDefault)
                        {
                            context.Canceled();
                        }

                        break;
                    case RectanglePhase.SecondLineMovement:
                        // Control movement is done down while waiting to go left
                        if (startPositionDiffX < sloppinessNegativeOrDefault && absDiffY < sloppinessOrDefault)
                        {
                            m_CurrentPhase = RectanglePhase.ThirdLineMovement;
                            m_LineStartPosition = m_PreviousPosition;
                            Debug.Log("Third line");
                        }

                        // When control starts going right
                        if (startPositionDiffX > sloppinessOrDefault)
                        {
                            context.Canceled();
                        }

                        break;
                    case RectanglePhase.ThirdLineMovement:

                        // Control movement is done left while waiting to go up
                        if (startPositionDiffY > sloppinessOrDefault && absDiffX < sloppinessOrDefault)
                        {
                            // If third line movement ends around start position x,
                            // meaning it's on the same vertical "plane" as start start position x
                            if (AbsDiffX(position, m_StartPosition) < sloppinessOrDefault)
                            {
                                m_CurrentPhase = RectanglePhase.LastLineMovement;
                                m_LineStartPosition = m_PreviousPosition;
                                Debug.Log("Last line");
                            }
                            else
                            {
                                context.Canceled();
                            }
                        }

                        // When control movement starts going down
                        if (startPositionDiffY < sloppinessNegativeOrDefault)
                        {
                            context.Canceled();
                        }
                        break;

                    case RectanglePhase.LastLineMovement:
                        // Last line needs to go up and be near start position, withing a threshold
                        if (AbsDiffY(position, m_StartPosition) < sloppinessOrDefault &&
                            AbsDiffX(position, m_StartPosition) < sloppinessOrDefault &&
                            absDiffX < sloppinessOrDefault)
                        {
                            context.Performed();
                        }

                        // When control movement starts going down
                        if (startPositionDiffY < sloppinessNegativeOrDefault)
                        {
                            context.Canceled();
                        }
                        break;

                }

                m_PreviousPosition = position;

            }
        }

        public void Reset()
        {
            m_CurrentPhase = RectanglePhase.None;
            m_LineStartPosition = Vector2.zero;
            m_PreviousPosition = Vector2.zero;
            m_StartPosition = Vector2.zero;
        }
    }
}

