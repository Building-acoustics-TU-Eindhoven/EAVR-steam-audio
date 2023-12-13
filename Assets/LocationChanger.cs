using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationChanger : MonoBehaviour
{
    public GameObject xrOrigin;
    public GameObject mainXRCamera;

    public RoomSizeManager roomSizeManager;
    private Vector3 origCameraPos = new Vector3();

    public GameObject root;

    private float playerColliderDiameter = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        root = GameObject.Find("root");
        origCameraPos = mainXRCamera.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        roomSizeManager.SetPlayerTransform();
    }

    public void TeleportToLocation (Vector3 newLocation, Vector3 roomDimensions)
    {
        Vector3 test = new Vector3((newLocation.x - 0.5f) * (roomDimensions.x - playerColliderDiameter) + root.transform.position.x,
                                    newLocation.y * roomDimensions.y + root.transform.position.y,
                                   (newLocation.z - 0.5f) * (roomDimensions.z - playerColliderDiameter) + root.transform.position.z);


        var distanceDiff = mainXRCamera.transform.position - test;
        xrOrigin.transform.position -= distanceDiff;
    }

    public void SetPlayerX(float x, float roomWidth)
    {
        float cameraOffset = xrOrigin.transform.GetChild(0).transform.position.y;

        Vector3 test = new Vector3(x * (roomWidth - playerColliderDiameter) + root.transform.position.x,
                                         transform.position.y + cameraOffset,
                                         transform.position.z);
        var distanceDiff = mainXRCamera.transform.position - test;
        xrOrigin.transform.position -= distanceDiff;

    }

    public void SetPlayerY(float y, float roomHeight)
    {
        float cameraOffset = xrOrigin.transform.GetChild(0).transform.position.y;

        Vector3 test = new Vector3(transform.position.x,
                                         y * roomHeight + root.transform.position.y + cameraOffset,
                                         transform.position.z);
        var distanceDiff = mainXRCamera.transform.position - test;
        xrOrigin.transform.position -= distanceDiff;

    }

    public void SetPlayerZ(float z, float roomDepth)
    {
        float cameraOffset = xrOrigin.transform.GetChild(0).transform.position.y;

        Vector3 test = new Vector3(transform.position.x,
                                         transform.position.y + cameraOffset,
                                         z * (roomDepth - playerColliderDiameter) + root.transform.position.z);
        var distanceDiff = mainXRCamera.transform.position - test;
        xrOrigin.transform.position -= distanceDiff;
    }

}