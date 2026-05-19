namespace HarmonyLib
{
	public enum HarmonyPatchType
	{
		All = 0,
		Prefix = 1,
		Postfix = 2,
		Transpiler = 3,
		Finalizer = 4,
		ReversePatch = 5,
		ILManipulator = 6
	}
}
