using System;
using System.Collections.Generic;
using System.Reflection;
using Rive;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Resolves state-machine inputs and view-model properties by the exact names in simi_prototype.riv.
    /// </summary>
    public static class RiveStateMachineInputs
    {
        static readonly MethodInfo GetInputAtPathMethod = typeof(Artboard).GetMethod(
            "GetInputAtPath",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(string), typeof(string) },
            null);

        static readonly MethodInfo IsTriggerMethod = typeof(SMIInput).GetMethod(
            "isSMITrigger",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(IntPtr) },
            null);

        static readonly MethodInfo IsBooleanMethod = typeof(SMIInput).GetMethod(
            "isSMIBoolean",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(IntPtr) },
            null);

        static readonly MethodInfo GetBoolValueMethod = typeof(SMIBool).GetMethod(
            "getSMIBoolValueStateMachine",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(IntPtr) },
            null);

        public static SMINumber GetNumber(StateMachine stateMachine, string name)
        {
            return FindNumber(stateMachine, name);
        }

        public static SMIBool GetBool(StateMachine stateMachine, string name)
        {
            return FindBool(stateMachine, name);
        }

        public static SMITrigger GetTrigger(StateMachine stateMachine, string name)
        {
            return FindTrigger(stateMachine, name);
        }

        public static SMINumber FindNumber(StateMachine stateMachine, params string[] names)
        {
            return FindInput(stateMachine, names, input => input.IsNumber) as SMINumber;
        }

        public static SMIBool FindBool(StateMachine stateMachine, params string[] names)
        {
            return FindInput(stateMachine, names, input => input.IsBoolean) as SMIBool;
        }

        public static SMITrigger FindTrigger(StateMachine stateMachine, params string[] names)
        {
            return FindInput(stateMachine, names, input => input.IsTrigger) as SMITrigger;
        }

        static SMIInput FindInput(StateMachine stateMachine, string[] names, Func<SMIInput, bool> match)
        {
            if (stateMachine == null || names == null || names.Length == 0)
                return null;

            var inputs = stateMachine.Inputs();
            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                if (input == null || !match(input))
                    continue;
                for (int n = 0; n < names.Length; n++)
                {
                    if (!string.IsNullOrEmpty(names[n]) && input.Name == names[n])
                        return input;
                }
            }

            return null;
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

        public static bool TryFindNestedInputPath(Artboard artboard, string inputName, IReadOnlyList<string> paths, out string path)
        {
            path = null;
            if (artboard == null || string.IsNullOrEmpty(inputName) || paths == null)
                return false;

            for (int i = 0; i < paths.Count; i++)
            {
                var candidate = paths[i];
                if (string.IsNullOrEmpty(candidate))
                    continue;
                if (GetInputPointer(artboard, inputName, candidate) == IntPtr.Zero)
                    continue;
                path = candidate;
                return true;
            }

            return false;
        }

        public static bool TryReadNestedTrigger(Artboard artboard, string inputName, string path)
        {
            if (artboard == null || string.IsNullOrEmpty(inputName) || string.IsNullOrEmpty(path))
                return false;

            var pointer = GetInputPointer(artboard, inputName, path);
            if (pointer == IntPtr.Zero)
                return false;
            if (!IsTriggerOrBoolean(pointer))
                return false;
            return GetNativeBoolValue(pointer);
        }

        static IntPtr GetInputPointer(Artboard artboard, string inputName, string path)
        {
            if (GetInputAtPathMethod == null)
                return IntPtr.Zero;
            var result = GetInputAtPathMethod.Invoke(artboard, new object[] { inputName, path });
            return result is IntPtr pointer ? pointer : IntPtr.Zero;
        }

        static bool IsTriggerOrBoolean(IntPtr pointer)
        {
            if (IsTriggerMethod != null && IsTriggerMethod.Invoke(null, new object[] { pointer }) is bool trigger && trigger)
                return true;
            return IsBooleanMethod != null && IsBooleanMethod.Invoke(null, new object[] { pointer }) is bool boolean && boolean;
        }

        static bool GetNativeBoolValue(IntPtr pointer)
        {
            if (GetBoolValueMethod == null)
                return false;
            return GetBoolValueMethod.Invoke(null, new object[] { pointer }) is bool value && value;
        }
    }
}
