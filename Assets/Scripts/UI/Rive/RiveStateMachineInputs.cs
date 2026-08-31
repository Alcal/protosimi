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

        public static bool TrySetNumber(Artboard artboard, string inputName, float value, string path)
        {
            if (artboard == null || string.IsNullOrEmpty(inputName)) return false;
            if (string.IsNullOrEmpty(path))
            {
                var stateMachine = artboard.StateMachine();
                var input = GetNumber(stateMachine, inputName);
                if (input == null) return false;
                input.Value = value;
                return true;
            }

            if (!artboard.GetNumberInputStateAtPath(inputName, path).HasValue) return false;
            artboard.SetNumberInputStateAtPath(inputName, value, path);
            return true;
        }

        public static bool TrySetBool(Artboard artboard, string inputName, bool value, string path)
        {
            if (artboard == null || string.IsNullOrEmpty(inputName)) return false;
            if (string.IsNullOrEmpty(path))
            {
                var stateMachine = artboard.StateMachine();
                var input = GetBool(stateMachine, inputName);
                if (input == null) return false;
                input.Value = value;
                return true;
            }

            if (!artboard.GetBooleanInputStateAtPath(inputName, path).HasValue) return false;
            artboard.SetBooleanInputStateAtPath(inputName, value, path);
            return true;
        }

    }
}
