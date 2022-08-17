using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

namespace Microsoft.MixedReality.Toolkit.MultiUser
{
    public class NetworkInteractable : NetworkBehaviour
    {
        public XRBaseInteractable Interactable;

        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public NetworkVariable<Quaternion> Rotation = new NetworkVariable<Quaternion>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (Interactable == null)
            {
                Interactable = GetComponentInChildren<XRBaseInteractable>();
            }
            
            if (Interactable != null)
            {
                Interactable.firstSelectEntered.AddListener(OnFirstSelectEntered);
                Interactable.lastSelectExited.AddListener(OnLastSelectExited);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (Interactable != null)
            {
                Interactable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
                Interactable.lastSelectExited.RemoveListener(OnLastSelectExited);
            }
        }
        public void Update()
        {
            if (!IsSpawned)
            {
                return;
            }

            SyncPose();
        }

        private void SyncPose()
        {
            if (IsOwner)
            {
                Position.Value = transform.localPosition;
                Rotation.Value = transform.localRotation;
                Position.SetDirty(true);
                Rotation.SetDirty(true);
            }
            else // Better mirror whatever we're told!
            {
                transform.localPosition = Position.Value;
                transform.localRotation = Rotation.Value;
            }
        }

        private void OnFirstSelectEntered(SelectEnterEventArgs args)
        {
            if (!IsOwner)
            {
                ChangeOwnershipServerRpc(true);
            }
        }

        private void OnLastSelectExited(SelectExitEventArgs args)
        {
            if (IsOwner)
            {
                ChangeOwnershipServerRpc(false);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeOwnershipServerRpc(bool wantOwnership, ServerRpcParams serverRpcParams = default)
        {
            Debug.Log($"{serverRpcParams.Receive.SenderClientId} {(wantOwnership ? "wants" : "doesn\'t want")} to own {gameObject.name}!");
            if (wantOwnership)
            {
                NetworkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId);
            }
            else
            {
                NetworkObject.RemoveOwnership();
            }
        }
    }
}
