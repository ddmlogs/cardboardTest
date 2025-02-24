﻿// Copyright 2015 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;

using AOT;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public abstract class VRDevice :
#if UNITY_ANDROID
AndroidBaseVRDevice
#else
BaseVRDevice
#endif
{
  // Event IDs sent up from native layer.
  private const int kTriggered = 1;
  private const int kTilted = 2;
  private const int kProfileChanged = 3;
  private const int kLaunchSettingsDialog = 4;

  private float[] headData = new float[16];
  private float[] viewData = new float[16 * 6 + 8];
  private float[] profileData = new float[13];

  private Matrix4x4 headView = new Matrix4x4();
  private Matrix4x4 leftEyeView = new Matrix4x4();
  private Matrix4x4 rightEyeView = new Matrix4x4();

  private Queue<int> eventQueue = new Queue<int>();

  protected bool debugDisableNativeProjections = false;
  protected bool debugDisableNativeDistortion = false;
  protected bool debugDisableNativeUILayer = false;

  public override bool SupportsNativeDistortionCorrection(List<string> diagnostics) {
    bool supported = base.SupportsNativeDistortionCorrection(diagnostics);
    if (debugDisableNativeDistortion) {
      supported = false;
      diagnostics.Add("Debug override");
    }
    return supported;
  }

  public override bool SupportsNativeUILayer(List<string> diagnostics) {
    bool supported = base.SupportsNativeUILayer(diagnostics);
    if (debugDisableNativeUILayer) {
      supported = false;
      diagnostics.Add("Debug override");
    }
    return supported;
  }

  public override void SetDistortionCorrectionEnabled(bool enabled) {
    EnableDistortionCorrection(enabled);
  }

  public override void SetAlignmentMarkerEnabled(bool enabled) {
    EnableAlignmentMarker(enabled);
  }

  public override void SetSettingsButtonEnabled(bool enabled) {
    EnableSettingsButton(enabled);
  }

  public override void SetNeckModelScale(float scale) {
    SetNeckModelFactor(scale);
  }

  public override void SetAutoDriftCorrectionEnabled(bool enabled) {
    EnableAutoDriftCorrection(enabled);
  }

  public override void Init() {
    // Get landscape dimensions, even if app starts in portrait.
    int width = Mathf.Max(Screen.width, Screen.height);
    int height = Mathf.Min(Screen.height, Screen.width);

    Start(width, height, Screen.dpi, Screen.dpi);
    SetEventCallback(OnVREvent);
  }

  public override void SetStereoScreen(RenderTexture stereoScreen) {
    SetTextureId(stereoScreen != null ? stereoScreen.GetNativeTextureID() : 0);
  }

  public override void UpdateState() {
    GetHeadPose(headData, Time.smoothDeltaTime);
    ExtractMatrix(ref headView, headData);
    headPose.SetRightHanded(headView.inverse);
  }

  public override void UpdateScreenData() {
    UpdateProfile();
    if (debugDisableNativeProjections) {
      ComputeEyesFromProfile();
    } else {
      UpdateView();
    }
  }

  public override void Recenter() {
    ResetHeadTracker();
  }

  public override void PostRender(bool vrMode) {
    if (vrMode) {
      //GL.IssuePluginEvent(0);
      GL.InvalidateState();
    }
    //ProcessEvents();
  }

  public override void OnApplicationQuit() {
    Stop();
    base.OnApplicationQuit();
  }

  private void UpdateView() {
    GetViewParameters(viewData);
    int j = 0;

    j = ExtractMatrix(ref leftEyeView, viewData, j);
    j = ExtractMatrix(ref rightEyeView, viewData, j);
    leftEyePose.SetRightHanded(leftEyeView.inverse);
    rightEyePose.SetRightHanded(rightEyeView.inverse);

    j = ExtractMatrix(ref leftEyeDistortedProjection, viewData, j);
    j = ExtractMatrix(ref rightEyeDistortedProjection, viewData, j);
    j = ExtractMatrix(ref leftEyeUndistortedProjection, viewData, j);
    j = ExtractMatrix(ref rightEyeUndistortedProjection, viewData, j);

    leftEyeUndistortedViewport.Set(viewData[j], viewData[j+1], viewData[j+2], viewData[j+3]);
    leftEyeDistortedViewport = leftEyeUndistortedViewport;
    j += 4;

    rightEyeUndistortedViewport.Set(viewData[j], viewData[j+1], viewData[j+2], viewData[j+3]);
    rightEyeDistortedViewport = rightEyeUndistortedViewport;
    j += 4;
  }

  private void UpdateProfile() {
    GetProfile(profileData);
    CardboardProfile.Device device = new CardboardProfile.Device();
    CardboardProfile.Screen screen = new CardboardProfile.Screen();
    device.maxFOV.outer = profileData[0];
    device.maxFOV.upper = profileData[1];
    device.maxFOV.inner = profileData[2];
    device.maxFOV.lower = profileData[3];
    screen.width = profileData[4];
    screen.height = profileData[5];
    screen.border = profileData[6];
    device.lenses.separation = profileData[7];
    device.lenses.offset = profileData[8];
    device.lenses.screenDistance = profileData[9];
    device.lenses.alignment = (int)profileData[10];
    device.distortion.k1 = profileData[11];
    device.distortion.k2 = profileData[12];
    device.inverse = CardboardProfile.ApproximateInverse(device.distortion);
    Profile.screen = screen;
    Profile.device = device;
  }

  private static int ExtractMatrix(ref Matrix4x4 mat, float[] data, int i = 0) {
    // Matrices returned from our native layer are in row-major order.
    for (int r = 0; r < 4; r++) {
      for (int c = 0; c < 4; c++, i++) {
        mat[r, c] = data[i];
      }
    }
    return i;
  }

  public abstract void LaunchSettingsDialog();

  protected virtual void ProcessEvents() {
    int[] events = null;
    lock (eventQueue) {
      int num = eventQueue.Count;
      if (num > 0) {
        events = new int[num];
        eventQueue.CopyTo(events, 0);
        eventQueue.Clear();
      }
    }
    if (events == null) {
      return;
    }
    foreach (int eventID in events) {
      switch (eventID) {
        case kTriggered:
          triggered = true;
          break;
        case kTilted:
          tilted = true;
          break;
        case kProfileChanged:
          UpdateScreenData();
          break;
        case kLaunchSettingsDialog:
          LaunchSettingsDialog();
          break;
      }
    }
  }

  [MonoPInvokeCallback(typeof(VREventCallback))]
  private static void OnVREvent(int eventID) {
    VRDevice device = GetDevice() as VRDevice;
    // This function is called back from random native code threads.
    lock(device.eventQueue) {
      device.eventQueue.Enqueue(eventID);
    }
  }

#if UNITY_IOS
  private const string dllName = "__Internal";
#else
  private const string dllName = "vrunity";
#endif

  delegate void VREventCallback(int eventID);

  [DllImport(dllName)]
  private static extern void Start(int width, int height, float xdpi, float ydpi);

  [DllImport(dllName)]
  private static extern void SetEventCallback(VREventCallback callback);

  [DllImport(dllName)]
  private static extern void SetTextureId(int id);

  [DllImport(dllName)]
  private static extern void EnableDistortionCorrection(bool enable);

  [DllImport(dllName)]
  private static extern void EnableAlignmentMarker(bool enable);

  [DllImport(dllName)]
  private static extern void EnableSettingsButton(bool enable);

  [DllImport(dllName)]
  private static extern void EnableAutoDriftCorrection(bool enable);

  [DllImport(dllName)]
  private static extern void SetNeckModelFactor(float factor);

  [DllImport(dllName)]
  private static extern void ResetHeadTracker();

  [DllImport(dllName)]
  private static extern void GetProfile(float[] profile);

  [DllImport(dllName)]
  private static extern void GetHeadPose(float[] pose, float fps);

  [DllImport(dllName)]
  private static extern void GetViewParameters(float[] viewParams);

  [DllImport(dllName)]
  private static extern void Stop();
}
