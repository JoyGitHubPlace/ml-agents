#if MLA_INPUT_SYSTEM && UNITY_2019_4_OR_NEWER

using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.MLAgents.Extensions.Input
{
    /// <summary>
    /// This implementation of <see cref="IActuator"/> will send events from the ML-Agents training process, or from
    /// neural networks to the <see cref="InputSystem"/> via the <see cref="IRLActionInputAdaptor"/> interface.  If an
    /// <see cref="Agent"/>'s <see cref="BehaviorParameters"/> indicate that the Agent is running in Heuristic Mode,
    /// this Actuator will write actions from the <see cref="InputSystem"/> to the <see cref="ActionBuffers"/> object.
    /// </summary>
    public class InputActionActuator : IActuator, IHeuristicProvider
    {
        readonly BehaviorParameters m_BehaviorParameters;
        readonly InputAction m_Action;
        readonly IRLActionInputAdaptor m_InputAdaptor;
        InputDevice m_Device;
        InputControl m_Control;


        /// <summary>
        /// Construct an <see cref="InputActionActuator"/> with the <see cref="BehaviorParameters"/> of the
        /// <see cref="Agent"/> component, the relevant <see cref="InputAction"/>, and the relevant
        /// <see cref="IRLActionInputAdaptor"/> to convert between ml-agents &lt;--&gt; <see cref="InputSystem"/>.
        /// </summary>
        /// <param name="behaviorParameters">Used to determine if the <see cref="Agent"/> is running in
        /// heuristic mode.</param>
        /// <param name="action">The <see cref="InputAction"/> this <see cref="IActuator"/> we read/write data to/from
        /// via the <see cref="IRLActionInputAdaptor"/>.</param>
        /// <param name="adaptor">The <see cref="IRLActionInputAdaptor"/> that will convert data between ML-Agents
        /// and the <see cref="InputSystem"/>.</param>
        public InputActionActuator(
            BehaviorParameters behaviorParameters,
            InputAction action,
            IRLActionInputAdaptor adaptor)
        {
            m_BehaviorParameters = behaviorParameters;
            Name = $"InputActionActuator-{action.name}";
            m_Action = action;
            m_InputAdaptor = adaptor;
            ActionSpec = adaptor.GetActionSpecForInputAction(m_Action);
        }

        /// <summary>
        /// Sets the virtual device for this actuator and looks for it's corresponding control.
        /// </summary>
        /// <param name="device">The virtual device with the control associated with this actuator.</param>
        internal void SetDevice(InputDevice device)
        {
            m_Device = device;
            m_Control = m_Device.GetChildControl(m_Action.name);
        }

        /// <inheritdoc cref="IActionReceiver.OnActionReceived"/>
        public void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (!m_BehaviorParameters.IsInHeuristicMode())
            {
                m_InputAdaptor.QueueInputEventForAction(m_Action, m_Control, ActionSpec, actionBuffers);
            }
        }

        /// <inheritdoc cref="IActionReceiver.WriteDiscreteActionMask"/>
        public void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            // TODO configure mask from editor UI?
        }

        /// <inheritdoc cref="IActuator.ActionSpec"/>
        public ActionSpec ActionSpec { get; }

        /// <inheritdoc cref="IActuator.Name"/>
        public string Name { get; }

        /// <inheritdoc cref="IActuator.ResetData"/>
        public void ResetData()
        {
            // do nothing for now
        }

        /// <inheritdoc cref="IHeuristicProvider.Heuristic"/>
        public void Heuristic(in ActionBuffers actionBuffersOut)
        {
            m_InputAdaptor.WriteToHeuristic(m_Action, actionBuffersOut);
        }
    }
}

#endif // MLA_INPUT_SYSTEM && UNITY_2019_4_OR_NEWER
