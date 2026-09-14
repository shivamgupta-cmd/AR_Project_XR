#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CopperDepositFX.EditorTools
{
    public class CopperDepositSetupWindow : EditorWindow
    {
        [MenuItem("Tools/Copper Deposit FX/Setup Selected Nail")]
        private static void SetupSelectedNail()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null)
            {
                EditorUtility.DisplayDialog("Copper Deposit FX", "Select your iron nail GameObject in the Hierarchy first.", "OK");
                return;
            }

            CopperDepositEffect fx = go.GetComponent<CopperDepositEffect>();
            if (fx == null)
            {
                Undo.AddComponent<CopperDepositEffect>(go);
                fx = go.GetComponent<CopperDepositEffect>();
            }

            fx.targetRenderer = go.GetComponentInChildren<Renderer>();
            fx.autoDetectAxis = true;
            fx.startNormalized = 0f;
            fx.endNormalized = 0.72f;
            fx.blobCount = 140;
            fx.depositionDuration = 5f;
            fx.playOnStart = true;
            fx.createSparkles = false;

            EditorUtility.SetDirty(fx);
            Selection.activeObject = fx;

            EditorUtility.DisplayDialog(
                "Copper Deposit FX",
                "Setup complete. Press Play to watch the copper deposit grow.\n\nIf the wrong end is coated, enable Reverse Direction on CopperDepositEffect.",
                "Done");
        }

        [MenuItem("Tools/Copper Deposit FX/Add Solution Trigger to Selected")]
        private static void SetupTrigger()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null)
            {
                EditorUtility.DisplayDialog("Copper Deposit FX", "Select the liquid/trigger GameObject first.", "OK");
                return;
            }

            Collider c = go.GetComponent<Collider>();
            if (c == null)
                c = Undo.AddComponent<BoxCollider>(go);
            c.isTrigger = true;

            if (go.GetComponent<CopperSolutionTrigger>() == null)
                Undo.AddComponent<CopperSolutionTrigger>(go);

            EditorUtility.DisplayDialog("Copper Deposit FX", "Solution trigger added. Resize the trigger collider to cover the liquid volume.", "Done");
        }
    }
}
#endif
