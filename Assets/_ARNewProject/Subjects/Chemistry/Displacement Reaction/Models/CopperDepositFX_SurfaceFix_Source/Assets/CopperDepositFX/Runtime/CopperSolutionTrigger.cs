using UnityEngine;

namespace CopperDepositFX
{
    /// <summary>
    /// Optional trigger for the copper-sulfate liquid volume.
    /// Add this to a GameObject with a trigger collider. When a nail carrying
    /// CopperDepositEffect enters, the copper growth begins automatically.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CopperSolutionTrigger : MonoBehaviour
    {
        public bool startOnlyOnce = true;
        public bool resetOnExit = false;
        private bool used;

        private void Reset()
        {
            Collider c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (used && startOnlyOnce) return;

            CopperDepositEffect fx = other.GetComponentInParent<CopperDepositEffect>();
            if (fx == null) fx = other.GetComponentInChildren<CopperDepositEffect>();
            if (fx == null) return;

            fx.StartDeposition();
            used = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!resetOnExit) return;
            CopperDepositEffect fx = other.GetComponentInParent<CopperDepositEffect>();
            if (fx != null) fx.ResetDeposit();
        }
    }
}
