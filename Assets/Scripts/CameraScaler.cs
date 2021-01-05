/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Main Camera
// The implementation of the UpdateOrthographicCameraSize() method is inspired by this source: https://pressstart.vip/tutorials/2018/06/14/37/understanding-orthographic-size.html
using UnityEngine;

/*
 * The purpose of this class is to scale the size of the Main Camera
 * By default, Unity will scale the height of the camera to the match the device's screen height when the height changes, but it will
 * not scale the width of the camera to match the device's screen width when the width changes.
 * So, this script makes the width and height of the camera scale to match the device's screen width and height whenever the aspect ratio is lower than the indended ratio of 16:10.
 */
public class CameraScaler : MonoBehaviour
{
    private Camera cam; // The Main Camera

    private const float BASE_ORTHOGRAPHIC_CAMERA_SIZE = 240f; // The orthographic size of the camera when the device aspect ratio is greater than or equal to the game aspect ratio
    private const float GAME_ASPECT_RATIO = 16f / 10f; // The aspect ratio intended for the game (16:10)
    private static float currentDeviceAspectRatio = 0f; // The aspect ratio of the device that the game is currently being played on, which may change if the user resizes the game window
    private static float currentOrthographicCameraSize = BASE_ORTHOGRAPHIC_CAMERA_SIZE; // The current orthographic camera size, which depends on the device aspect ratio

    // Awake is called before any other script's Start method
    void Awake()
    {
        cam = Camera.main;
        UpdateOrthographicCameraSize();
    }

    // Update is called once per frame
    void Update()
    {
        // Update the orthographic camera size if the device's aspect ratio has changed, meaning the user has resized the game window
        if ((float)Screen.width / (float)Screen.height != currentDeviceAspectRatio)
        {
            UpdateOrthographicCameraSize();
        }
    }

    // Updates the orthographic camera size according the the device's current aspect ratio
    // This allows the game to scale up or down with the width of the screen
    private void UpdateOrthographicCameraSize()
    {
        currentDeviceAspectRatio = (float)Screen.width / (float)Screen.height; // Compute the device's current aspect ratio

        if (currentDeviceAspectRatio >= GAME_ASPECT_RATIO)
        {
            currentOrthographicCameraSize = BASE_ORTHOGRAPHIC_CAMERA_SIZE; // Keep the size at the base size
        }
        else
        {
            currentOrthographicCameraSize = BASE_ORTHOGRAPHIC_CAMERA_SIZE * (GAME_ASPECT_RATIO / currentDeviceAspectRatio); // Scale up/down the size to fit the screen
        }

        cam.orthographicSize = currentOrthographicCameraSize; // Set camera size
    }
}
