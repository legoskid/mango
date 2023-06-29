using System.Reflection;
using HarmonyLib;

namespace ModMenuPatch.HarmonyPatches;

public class ModMenuPatches
{
	private static Harmony instance;

	public const string InstanceId = "com.yourname.gorillatag.yourmod";

	public static bool IsPatched { get; private set; }

	internal static void ApplyHarmonyPatches()
	{
		if (!IsPatched)
		{
			if (instance == null)
			{
				instance = new Harmony("com.yourname.gorillatag.yourmod");
			}
			instance.PatchAll(Assembly.GetExecutingAssembly());
			IsPatched = true;
		}
	}

	internal static void RemoveHarmonyPatches()
	{
		if (instance != null && IsPatched)
		{
			instance.UnpatchSelf();
			IsPatched = false;
		}
	}
}
