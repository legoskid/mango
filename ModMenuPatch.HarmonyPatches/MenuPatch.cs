using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.XR;

namespace ModMenuPatch.HarmonyPatches;

[HarmonyPatch(typeof(GorillaLocomotion.Player))]
[HarmonyPatch("LateUpdate", MethodType.Normal)]
internal class MenuPatch
{
	public enum PhotonEventCodes
	{
		left_jump_photoncode = 69,
		right_jump_photoncode,
		left_jump_deletion,
		right_jump_deletion
	}

	public class TimedBehaviour : MonoBehaviour
	{
		public bool complete = false;

		public bool loop = true;

		public float progress = 0f;

		protected bool paused = false;

		protected float startTime;

		protected float duration = 2f;

		public virtual void Start()
		{
			startTime = Time.time;
		}

		public virtual void Update()
		{
			if (complete)
			{
				return;
			}
			progress = Mathf.Clamp((Time.time - startTime) / duration, 0f, 1f);
			if (Time.time - startTime > duration)
			{
				if (loop)
				{
					OnLoop();
				}
				else
				{
					complete = true;
				}
			}
		}

		public virtual void OnLoop()
		{
			startTime = Time.time;
		}
	}

	public class ColorChanger : TimedBehaviour
	{
		public Renderer gameObjectRenderer;

		public Gradient colors = null;

		public Color color;

		public bool timeBased = true;

		public override void Start()
		{
			base.Start();
			gameObjectRenderer = GetComponent<Renderer>();
		}
	}

	public static bool ResetSpeed = false;

	private static string[] buttons = new string[2]
	{
		"disconnect",
		"open caves"
	};

	private static bool?[] buttonsActive = new bool?[2]
	{
		false,
		false
	};

	private static bool gripDown;

	private static GameObject menu = null;

	private static GameObject canvasObj = null;

	private static GameObject reference = null;

	public static int framePressCooldown = 0;

	private static GameObject pointer = null;

	private static bool gravityToggled = false;

	private static bool flying = false;

	private static int btnCooldown = 0;

	private static int soundCooldown = 0;

	private static float? maxJumpSpeed = null;

	private static float? jumpMultiplier = null;

	private static object index;

	public static int BlueMaterial = 5;

	public static int TransparentMaterial = 6;

	public static int LavaMaterial = 2;

	public static int RockMaterial = 1;

	public static int DefaultMaterial = 5;

	public static int NeonRed = 3;

	public static int RedTransparent = 4;

	public static int self = 0;

	private static Vector3? leftHandOffsetInitial = null;

	private static Vector3? rightHandOffsetInitial = null;

	private static float? maxArmLengthInitial = null;

	private static bool noClipDisabledOneshot = false;

	private static bool noClipEnabledAtLeastOnce = false;

	private static bool ghostToggle = false;

	private static bool bigMonkeyEnabled = false;

	private static bool bigMonkeAntiRepeat = false;

	private static int bigMonkeCooldown = 0;

	private static bool ghostMonkeEnabled = false;

	private static bool ghostMonkeAntiRepeat = false;

	private static int ghostMonkeCooldown = 0;

	private static bool checkedProps = false;

	private static bool teleportGunAntiRepeat = false;

	private static Color colorRgbMonke = new Color(0f, 0f, 0f);

	private static float hueRgbMonke = 0f;

	private static float timerRgbMonke = 0f;

	private static float updateRateRgbMonke = 0f;

	private static float updateTimerRgbMonke = 0f;

	private static bool flag2 = false;

	private static bool flag1 = true;

	private static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);

	private static bool gripDown_left;

	private static bool gripDown_right;

	private static bool once_left;

	private static bool once_right;

	private static bool once_left_false;

	private static bool once_right_false;

	private static bool once_networking;

	private static GameObject[] jump_left_network = new GameObject[9999];

	private static GameObject[] jump_right_network = new GameObject[9999];

	private static GameObject jump_left_local = null;

	private static GameObject jump_right_local = null;

	private static GradientColorKey[] colorKeysPlatformMonke = new GradientColorKey[4];

	private static Vector3? checkpointPos;

	private static bool checkpointTeleportAntiRepeat = false;

	private static bool foundPlayer = false;

	private static int btnTagSoundCooldown = 0;

	private static float timeSinceLastChange = 0f;

	private static float myVarY1 = 0f;

	private static float myVarY2 = 0f;

	private static bool gain = false;

	private static bool less = false;

	private static bool reset = false;

	private static bool fastr = false;

	private static bool speed1 = true;

	private static float gainSpeed = 1f;

	private static int pageSize = 4;

	private static int pageNumber = 0;

	private static void Prefix()
	{
		try
		{
			if (!maxJumpSpeed.HasValue)
			{
				maxJumpSpeed = GorillaLocomotion.Player.Instance.maxJumpSpeed;
			}
			if (!jumpMultiplier.HasValue)
			{
				jumpMultiplier = GorillaLocomotion.Player.Instance.jumpMultiplier;
			}
			if (!maxArmLengthInitial.HasValue)
			{
				maxArmLengthInitial = GorillaLocomotion.Player.Instance.maxArmLength;
				leftHandOffsetInitial = GorillaLocomotion.Player.Instance.leftHandOffset;
				rightHandOffsetInitial = GorillaLocomotion.Player.Instance.rightHandOffset;
			}
			List<InputDevice> list = new List<InputDevice>();
			InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left, list);
			list[0].TryGetFeatureValue(CommonUsages.secondaryButton, out gripDown);
			if (gripDown && menu == null)
			{
				Draw();
				if (reference == null)
				{
					reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					UnityEngine.Object.Destroy(reference.GetComponent<MeshRenderer>());
					reference.transform.parent = GorillaLocomotion.Player.Instance.rightControllerTransform;
					reference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
					reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
				}
			}
			else if (!gripDown && menu != null)
			{
				UnityEngine.Object.Destroy(menu);
				menu = null;
				UnityEngine.Object.Destroy(reference);
				reference = null;
			}
			if (gripDown && menu != null)
			{
				menu.transform.position = GorillaLocomotion.Player.Instance.leftControllerTransform.position;
				menu.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
			}
			if (buttonsActive[0] == true)
			{
				PhotonNetwork.Disconnect();
			}
            if (buttonsActive[1] == true)
            {
                GameObject.Find("Level/cave/CaveBlockerRocks_Prefab").SetActive(false);
                GameObject.Find("Level/cave/CaveBlockerMeshBaker").SetActive(false);
            }
			else
			{
                GameObject.Find("Level/cave/CaveBlockerRocks_Prefab").SetActive(true);
                GameObject.Find("Level/cave/CaveBlockerMeshBaker").SetActive(true);
            }
            if (btnCooldown > 0 && Time.frameCount > btnCooldown)
			{
				btnCooldown = 0;
				buttonsActive[7] = false;
				UnityEngine.Object.Destroy(menu);
				menu = null;
				Draw();
			}
			if (soundCooldown > 0 && Time.frameCount > soundCooldown)
			{
				soundCooldown = 0;
				buttonsActive[14] = false;
				UnityEngine.Object.Destroy(menu);
				menu = null;
				Draw();
			}
			if (btnTagSoundCooldown > 0 && Time.frameCount > btnTagSoundCooldown)
			{
				btnTagSoundCooldown = 0;
				buttonsActive[14] = false;
				UnityEngine.Object.Destroy(menu);
				menu = null;
				Draw();
			}
			if (bigMonkeCooldown > 0 && Time.frameCount > bigMonkeCooldown)
			{
				bigMonkeCooldown = 0;
			}
			if (ghostMonkeCooldown > 0 && Time.frameCount > ghostMonkeCooldown)
			{
				ghostMonkeCooldown = 0;
			}
		}
		catch (Exception ex)
		{
			File.WriteAllText("YOUR_MOD_error.log", ex.ToString());
		}
	}
	private static void ProcessTeleportGun()
	{
		bool value = false;
		bool value2 = false;
		List<InputDevice> list = new List<InputDevice>();
		InputDevices.GetDevices(list);
		InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, list);
		list[0].TryGetFeatureValue(CommonUsages.triggerButton, out value);
		list[0].TryGetFeatureValue(CommonUsages.gripButton, out value2);
		if (value2)
		{
			Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out var hitInfo);
			if (pointer == null)
			{
				pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
				pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
			}
			pointer.transform.position = hitInfo.point;
			if (value)
			{
				if (!teleportGunAntiRepeat)
				{
					GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().isKinematic = true;
					GorillaLocomotion.Player.Instance.transform.position = hitInfo.point;
					GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
					GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().isKinematic = false;
					teleportGunAntiRepeat = true;
				}
			}
			else
			{
				teleportGunAntiRepeat = false;
			}
		}
		else
		{
			UnityEngine.Object.Destroy(pointer);
			pointer = null;
			teleportGunAntiRepeat = false;
		}
	}

	private static void ProcessPlatformMonke()
	{
		colorKeysPlatformMonke[0].color = Color.red;
		colorKeysPlatformMonke[0].time = 0f;
		colorKeysPlatformMonke[1].color = Color.green;
		colorKeysPlatformMonke[1].time = 0.3f;
		colorKeysPlatformMonke[2].color = Color.blue;
		colorKeysPlatformMonke[2].time = 0.6f;
		colorKeysPlatformMonke[3].color = Color.red;
		colorKeysPlatformMonke[3].time = 1f;
		if (!once_networking)
		{
			PhotonNetwork.NetworkingClient.EventReceived += PlatformNetwork;
			once_networking = true;
		}
		List<InputDevice> list = new List<InputDevice>();
		InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left, list);
		list[0].TryGetFeatureValue(CommonUsages.gripButton, out gripDown_left);
		InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, list);
		list[0].TryGetFeatureValue(CommonUsages.gripButton, out gripDown_right);
		if (gripDown_right)
		{
			if (!once_right && jump_right_local == null)
			{
				jump_right_local = GameObject.CreatePrimitive(PrimitiveType.Cube);
				jump_right_local.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
				jump_right_local.transform.localScale = scale;
				jump_right_local.transform.position = new Vector3(0f, -0.0075f, 0f) + GorillaLocomotion.Player.Instance.rightControllerTransform.position;
				jump_right_local.transform.rotation = GorillaLocomotion.Player.Instance.rightControllerTransform.rotation;
				object[] eventContent = new object[2]
				{
					new Vector3(0f, -0.0075f, 0f) + GorillaLocomotion.Player.Instance.rightControllerTransform.position,
					GorillaLocomotion.Player.Instance.rightControllerTransform.rotation
				};
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions
				{
					Receivers = ReceiverGroup.Others
				};
				PhotonNetwork.RaiseEvent(70, eventContent, raiseEventOptions, SendOptions.SendReliable);
				once_right = true;
				once_right_false = false;
				ColorChanger colorChanger = jump_right_local.AddComponent<ColorChanger>();
				Gradient gradient = new Gradient();
				gradient.colorKeys = colorKeysPlatformMonke;
				colorChanger.colors = gradient;
				colorChanger.Start();
			}
		}
		else if (!once_right_false && jump_right_local != null)
		{
			UnityEngine.Object.Destroy(jump_right_local);
			jump_right_local = null;
			once_right = false;
			once_right_false = true;
			RaiseEventOptions raiseEventOptions2 = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others
			};
			PhotonNetwork.RaiseEvent(72, null, raiseEventOptions2, SendOptions.SendReliable);
		}
		if (gripDown_left)
		{
			if (!once_left && jump_left_local == null)
			{
				jump_left_local = GameObject.CreatePrimitive(PrimitiveType.Cube);
				jump_left_local.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
				jump_left_local.transform.localScale = scale;
				jump_left_local.transform.position = GorillaLocomotion.Player.Instance.leftControllerTransform.position;
				jump_left_local.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
				object[] eventContent2 = new object[2]
				{
					GorillaLocomotion.Player.Instance.leftControllerTransform.position,
					GorillaLocomotion.Player.Instance.leftControllerTransform.rotation
				};
				RaiseEventOptions raiseEventOptions3 = new RaiseEventOptions
				{
					Receivers = ReceiverGroup.Others
				};
				PhotonNetwork.RaiseEvent(69, eventContent2, raiseEventOptions3, SendOptions.SendReliable);
				once_left = true;
				once_left_false = false;
				ColorChanger colorChanger2 = jump_left_local.AddComponent<ColorChanger>();
				Gradient gradient2 = new Gradient();
				gradient2.colorKeys = colorKeysPlatformMonke;
				colorChanger2.colors = gradient2;
				colorChanger2.Start();
			}
		}
		else if (!once_left_false && jump_left_local != null)
		{
			UnityEngine.Object.Destroy(jump_left_local);
			jump_left_local = null;
			once_left = false;
			once_left_false = true;
			RaiseEventOptions raiseEventOptions4 = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others
			};
			PhotonNetwork.RaiseEvent(71, null, raiseEventOptions4, SendOptions.SendReliable);
		}
		if (!PhotonNetwork.InRoom)
		{
			for (int i = 0; i < jump_right_network.Length; i++)
			{
				UnityEngine.Object.Destroy(jump_right_network[i]);
			}
			for (int j = 0; j < jump_left_network.Length; j++)
			{
				UnityEngine.Object.Destroy(jump_left_network[j]);
			}
		}
	}

    private static void PlatformNetwork(EventData eventData)
	{
		switch (eventData.Code)
		{
		case 69:
		{
			object[] array2 = (object[])eventData.CustomData;
			jump_left_network[eventData.Sender] = GameObject.CreatePrimitive(PrimitiveType.Cube);
			jump_left_network[eventData.Sender].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
			jump_left_network[eventData.Sender].transform.localScale = scale;
			jump_left_network[eventData.Sender].transform.position = (Vector3)array2[0];
			jump_left_network[eventData.Sender].transform.rotation = (Quaternion)array2[1];
			ColorChanger colorChanger2 = jump_left_network[eventData.Sender].AddComponent<ColorChanger>();
			Gradient gradient2 = new Gradient();
			gradient2.colorKeys = colorKeysPlatformMonke;
			colorChanger2.colors = gradient2;
			colorChanger2.Start();
			break;
		}
		case 70:
		{
			object[] array = (object[])eventData.CustomData;
			jump_right_network[eventData.Sender] = GameObject.CreatePrimitive(PrimitiveType.Cube);
			jump_right_network[eventData.Sender].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
			jump_right_network[eventData.Sender].transform.localScale = scale;
			jump_right_network[eventData.Sender].transform.position = (Vector3)array[0];
			jump_right_network[eventData.Sender].transform.rotation = (Quaternion)array[1];
			ColorChanger colorChanger = jump_right_network[eventData.Sender].AddComponent<ColorChanger>();
			Gradient gradient = new Gradient();
			gradient.colorKeys = colorKeysPlatformMonke;
			colorChanger.colors = gradient;
			colorChanger.Start();
			break;
		}
		case 71:
			UnityEngine.Object.Destroy(jump_left_network[eventData.Sender]);
			jump_left_network[eventData.Sender] = null;
			break;
		case 72:
			UnityEngine.Object.Destroy(jump_right_network[eventData.Sender]);
			jump_right_network[eventData.Sender] = null;
			break;
		}
	}


   

    private static void AddButton(float offset, string text)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
		gameObject.GetComponent<BoxCollider>().isTrigger = true;
		gameObject.transform.parent = menu.transform;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
		gameObject.transform.localPosition = new Vector3(0.56f, 1f, 0.28f - offset);
		gameObject.AddComponent<BtnCollider>().relatedText = text;
		int num = -1;
		for (int i = 0; i < buttons.Length; i++)
		{
			if (text == buttons[i])
			{
				num = i;
				break;
			}
		}
		if (buttonsActive[num] == false)
		{
			gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
		}
		else if (buttonsActive[num] == true)
		{
			gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
		}
		else
		{
			gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
		}
		GameObject gameObject2 = new GameObject();
		gameObject2.transform.parent = canvasObj.transform;
		Text text2 = gameObject2.AddComponent<Text>();
		text2.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		text2.text = text;
		text2.fontSize = 1;
		text2.alignment = TextAnchor.MiddleCenter;
		text2.resizeTextForBestFit = true;
		text2.resizeTextMinSize = 0;
		RectTransform component = text2.GetComponent<RectTransform>();
		component.localPosition = Vector3.zero;
		component.sizeDelta = new Vector2(0.2f, 0.03f);
		component.localPosition = new Vector3(0.064f, 0.3f, 0.111f - offset / 2.55f);
		component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
	}

	public static void Draw()
	{
		menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
		UnityEngine.Object.Destroy(menu.GetComponent<Rigidbody>());
		UnityEngine.Object.Destroy(menu.GetComponent<BoxCollider>());
		UnityEngine.Object.Destroy(menu.GetComponent<Renderer>());
		menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f);
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
		UnityEngine.Object.Destroy(gameObject.GetComponent<BoxCollider>());
		gameObject.transform.parent = menu.transform;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = new Vector3(0.1f, 1f, 1f);
		gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
		gameObject.transform.position = new Vector3(0.05f, 0.3f, 0f);
		GradientColorKey[] array = new GradientColorKey[3];
		array[0].color = Color.black;
		array[0].time = 0f;
		array[1].color = Color.blue;
		array[1].time = 0.5f;
		array[2].color = Color.black;
		array[2].time = 1f;
		ColorChanger colorChanger = gameObject.AddComponent<ColorChanger>();
		Gradient gradient = new Gradient();
		gradient.colorKeys = array;
		colorChanger.colors = gradient;
		colorChanger.Start();
		canvasObj = new GameObject();
		canvasObj.transform.parent = menu.transform;
		Canvas canvas = canvasObj.AddComponent<Canvas>();
		CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();
		canvasObj.AddComponent<GraphicRaycaster>();
		canvas.renderMode = RenderMode.WorldSpace;
		canvasScaler.dynamicPixelsPerUnit = 1000f;
		GameObject gameObject2 = new GameObject();
		gameObject2.transform.parent = canvasObj.transform;
		Text text = gameObject2.AddComponent<Text>();
		text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		text.text = "Mango's Mod Menu V5";
		text.fontSize = 1;
		text.alignment = TextAnchor.MiddleCenter;
		text.resizeTextForBestFit = true;
		text.resizeTextMinSize = 0;
		RectTransform component = text.GetComponent<RectTransform>();
		component.localPosition = Vector3.zero;
		component.sizeDelta = new Vector2(0.28f, 0.05f);
		component.position = new Vector3(0.06f, 0.3f, 0.175f);
		component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		AddPageButtons();
		string[] array2 = buttons.Skip(pageNumber * pageSize).Take(pageSize).ToArray();
		for (int i = 0; i < array2.Length; i++)
		{
			AddButton((float)i * 0.13f + 0.26f, array2[i]);
		}
	}

	private static void AddPageButtons()
	{
		int num = (buttons.Length + pageSize - 1) / pageSize;
		int num2 = pageNumber + 1;
		int num3 = pageNumber - 1;
		if (num2 > num - 1)
		{
			num2 = 0;
		}
		if (num3 < 0)
		{
			num3 = num - 1;
		}
		float num4 = 0f;
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
		gameObject.GetComponent<BoxCollider>().isTrigger = true;
		gameObject.transform.parent = menu.transform;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
		gameObject.transform.localPosition = new Vector3(0.56f, 1f, 0.28f - num4);
		gameObject.AddComponent<BtnCollider>().relatedText = "PreviousPage";
		gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
		GameObject gameObject2 = new GameObject();
		gameObject2.transform.parent = canvasObj.transform;
		Text text = gameObject2.AddComponent<Text>();
		text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		text.text = "[" + num3 + "] << Prev";
		text.fontSize = 1;
		text.alignment = TextAnchor.MiddleCenter;
		text.resizeTextForBestFit = true;
		text.resizeTextMinSize = 0;
		RectTransform component = text.GetComponent<RectTransform>();
		component.localPosition = Vector3.zero;
		component.sizeDelta = new Vector2(0.2f, 0.03f);
		component.localPosition = new Vector3(0.064f, 0.3f, 0.111f - num4 / 2.55f);
		component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		num4 = 0.13f;
		GameObject gameObject3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
		UnityEngine.Object.Destroy(gameObject3.GetComponent<Rigidbody>());
		gameObject3.GetComponent<BoxCollider>().isTrigger = true;
		gameObject3.transform.parent = menu.transform;
		gameObject3.transform.rotation = Quaternion.identity;
		gameObject3.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
		gameObject3.transform.localPosition = new Vector3(0.56f, 1f, 0.28f - num4);
		gameObject3.AddComponent<BtnCollider>().relatedText = "NextPage";
		gameObject3.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
		GameObject gameObject4 = new GameObject();
		gameObject4.transform.parent = canvasObj.transform;
		Text text2 = gameObject4.AddComponent<Text>();
		text2.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		text2.text = "Next >> [" + num2 + "]";
		text2.fontSize = 1;
		text2.alignment = TextAnchor.MiddleCenter;
		text2.resizeTextForBestFit = true;
		text2.resizeTextMinSize = 0;
		RectTransform component2 = text2.GetComponent<RectTransform>();
		component2.localPosition = Vector3.zero;
		component2.sizeDelta = new Vector2(0.2f, 0.03f);
		component2.localPosition = new Vector3(0.064f, 0.3f, 0.111f - num4 / 2.55f);
		component2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
	}

	public static void Toggle(string relatedText)
	{
		int num = (buttons.Length + pageSize - 1) / pageSize;
		if (relatedText == "NextPage")
		{
			if (pageNumber < num - 1)
			{
				pageNumber++;
			}
			else
			{
				pageNumber = 0;
			}
			UnityEngine.Object.Destroy(menu);
			menu = null;
			Draw();
			return;
		}
		if (relatedText == "PreviousPage")
		{
			if (pageNumber > 0)
			{
				pageNumber--;
			}
			else
			{
				pageNumber = num - 1;
			}
			UnityEngine.Object.Destroy(menu);
			menu = null;
			Draw();
			return;
		}
		int num2 = -1;
		for (int i = 0; i < buttons.Length; i++)
		{
			if (relatedText == buttons[i])
			{
				num2 = i;
				break;
			}
		}
		if (buttonsActive[num2].HasValue)
		{
			buttonsActive[num2] = !buttonsActive[num2];
			UnityEngine.Object.Destroy(menu);
			menu = null;
			Draw();
		}
	}
}
