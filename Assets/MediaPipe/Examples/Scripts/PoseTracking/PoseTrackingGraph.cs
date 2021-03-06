using Mediapipe;
using UnityEngine;

public class PoseTrackingGraph : DemoGraph {
  private const string poseLandmarksStream = "pose_landmarks_smoothed";
  private OutputStreamPoller<NormalizedLandmarkList> poseLandmarksStreamPoller;
  private NormalizedLandmarkListPacket poseLandmarksPacket;

  private const string poseDetectionStream = "pose_detection";
  private OutputStreamPoller<Detection> poseDetectionStreamPoller;
  private DetectionPacket poseDetectionPacket;

  private const string poseLandmarksPresenceStream = "pose_landmarks_smoothed_presence";
  private OutputStreamPoller<bool> poseLandmarksPresenceStreamPoller;
  private BoolPacket poseLandmarksPresencePacket;

  private const string poseDetectionPresenceStream = "pose_detection_presence";
  private OutputStreamPoller<bool> poseDetectionPresenceStreamPoller;
  private BoolPacket poseDetectionPresencePacket;

  public override Status StartRun(SidePacket sidePacket) {
    poseLandmarksStreamPoller = graph.AddOutputStreamPoller<NormalizedLandmarkList>(poseLandmarksStream).ConsumeValue();
    poseLandmarksPacket = new NormalizedLandmarkListPacket();

    poseDetectionStreamPoller = graph.AddOutputStreamPoller<Detection>(poseDetectionStream).ConsumeValue();
    poseDetectionPacket = new DetectionPacket();

    poseLandmarksPresenceStreamPoller = graph.AddOutputStreamPoller<bool>(poseLandmarksPresenceStream).ConsumeValue();
    poseLandmarksPresencePacket = new BoolPacket();

    poseDetectionPresenceStreamPoller = graph.AddOutputStreamPoller<bool>(poseDetectionPresenceStream).ConsumeValue();
    poseDetectionPresencePacket = new BoolPacket();

    return graph.StartRun(sidePacket);
  }

  public override void RenderOutput(WebCamScreenController screenController, PixelData pixelData) {
    var poseTrackingValue = FetchNextPoseTrackingValue();
    RenderAnnotation(screenController, poseTrackingValue);

    var texture = screenController.GetScreen();
    texture.SetPixels32(pixelData.Colors);

    texture.Apply();
  }

  private PoseTrackingValue FetchNextPoseTrackingValue() {
    if (!FetchNextPoseLandmarksPresence()) {
      return new PoseTrackingValue();
    }

    var poseLandmarks = FetchNextPoseLandmarks();

    if (!FetchNextPoseDetectionPresence()) {
      return new PoseTrackingValue(poseLandmarks);
    }

    var poseDetection = FetchNextPoseDetection();

    return new PoseTrackingValue(poseLandmarks, poseDetection);
  }

  private NormalizedLandmarkList FetchNextPoseLandmarks() {
    return FetchNext(poseLandmarksStreamPoller, poseLandmarksPacket, poseLandmarksStream);
  }

  private Detection FetchNextPoseDetection() {
    return FetchNext(poseDetectionStreamPoller, poseDetectionPacket, poseDetectionStream);
  }

  private bool FetchNextPoseLandmarksPresence() {
    return FetchNext(poseLandmarksPresenceStreamPoller, poseLandmarksPresencePacket, poseLandmarksPresenceStream);
  }

  private bool FetchNextPoseDetectionPresence() {
    return FetchNext(poseDetectionPresenceStreamPoller, poseDetectionPresencePacket, poseDetectionPresenceStream);
  }

  private void RenderAnnotation(WebCamScreenController screenController, PoseTrackingValue value) {
    // NOTE: input image is flipped
    GetComponent<PoseTrackingAnnotationController>().Draw(screenController.transform, value.PoseLandmarkList, value.PoseDetection, true);
  }
}
