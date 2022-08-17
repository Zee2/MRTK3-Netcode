using Unity.Netcode;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.MultiUser
{
    public class User : NetworkBehaviour
    {
        public NetworkVariable<Vector3> HeadPosition = new NetworkVariable<Vector3>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public NetworkVariable<Quaternion> HeadRotation = new NetworkVariable<Quaternion>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private Camera userCamera;

        public Transform Avatar; // Root of physical/visible avatar

        void Awake()
        {
            userCamera = Camera.main;
        }

        void Update()
        {
            if (!IsSpawned)
            {
                return;
            }

            if (IsOwner)
            {
                // Don't show our own avatar!
                Avatar.gameObject.SetActive(false);

                // Mirror the camera.
                transform.SetPositionAndRotation(userCamera.transform.position, userCamera.transform.rotation);
                HeadPosition.Value = userCamera.transform.position;
                HeadRotation.Value = userCamera.transform.rotation;
                HeadPosition.SetDirty(true);
                HeadRotation.SetDirty(true);
            }
            else // Better mirror whatever we're told!
            {
                transform.SetPositionAndRotation(HeadPosition.Value, HeadRotation.Value);
            }
        }
    }
}