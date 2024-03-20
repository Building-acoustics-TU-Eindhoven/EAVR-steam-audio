// This script allows for a locked cursor to be able to interact with World Space UI elements

// Taken from https://forum.unity.com/threads/world-space-canvas-cursorlockmode-locked-incompatible.566485/
// Also, needed to go to Project Settings -> Script Execution Order -> Put this script at the top

using UnityEngine;

class WorldSpaceUIEnabler : MonoBehaviour
{
    private void Update() {
        Cursor.lockState = CursorLockMode.Confined;
    }
 
    private void LateUpdate() {
        Cursor.lockState = CursorLockMode.Locked;
    }
}