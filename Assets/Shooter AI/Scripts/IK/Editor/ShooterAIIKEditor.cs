using UnityEngine;
using UnityEditor;
using System.Collections;

namespace GatewayGames.ShooterAI {

	[CustomEditor(typeof(ShooterAIIK))]
	public class ShooterAIIKEditor : Editor {

		private MonoScript monoScript;

		void OnEnable() {
			if (serializedObject == null) return;

			// Changing the script execution order
			if (!Application.isPlaying) {
				monoScript = MonoScript.FromMonoBehaviour(target as ShooterAIIK);

				int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
				if (currentExecutionOrder != 9999) MonoImporter.SetExecutionOrder(monoScript, 9999);
			}
		}
	}
}
