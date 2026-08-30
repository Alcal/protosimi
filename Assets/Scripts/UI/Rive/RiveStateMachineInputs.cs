using Rive;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Resolves state-machine inputs and view-model properties by the exact names in simi_prototype.riv.
    /// </summary>
    public static class RiveStateMachineInputs
    {
        public static SMINumber GetNumber(StateMachine stateMachine, string name)
        {
            return stateMachine == null || string.IsNullOrEmpty(name) ? null : stateMachine.GetNumber(name);
        }

        public static SMIBool GetBool(StateMachine stateMachine, string name)
        {
            return stateMachine == null || string.IsNullOrEmpty(name) ? null : stateMachine.GetBool(name);
        }

        public static SMITrigger GetTrigger(StateMachine stateMachine, string name)
        {
            return stateMachine == null || string.IsNullOrEmpty(name) ? null : stateMachine.GetTrigger(name);
        }

        public static T GetViewModelProperty<T>(ViewModelInstance instance, string path)
            where T : ViewModelInstanceProperty
        {
            return instance == null || string.IsNullOrEmpty(path) ? null : instance.GetProperty<T>(path);
        }
    }
}
