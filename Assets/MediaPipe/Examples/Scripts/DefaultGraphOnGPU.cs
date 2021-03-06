// Copyright 2019 The MediaPipe Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Mediapipe;
using UnityEngine;

using GL = Mediapipe.GL;

/// <summary>
///   This class is a translated version of
///   <see href="https://github.com/google/mediapipe/blob/v0.7.10/mediapipe/examples/desktop/demo_run_graph_main_gpu.cc">
///     demo_run_graph_main_gpu.cc
///   </see>
///   in the official repository.
/// </summary>
public class DefaultGraphOnGPU : DemoGraph {
  private const string outputStream = "output_video";

  private OutputStreamPoller<GpuBuffer> outputStreamPoller;
  private GpuBufferPacket outputPacket;

  public override Status StartRun(SidePacket sidePacket) {
    outputStreamPoller = graph.AddOutputStreamPoller<GpuBuffer>(outputStream).ConsumeValue();
    outputPacket = new GpuBufferPacket();

    return graph.StartRun(sidePacket);
  }

  public override void RenderOutput(WebCamScreenController screenController, PixelData pixelData) {
    var texture = screenController.GetScreen();

    if (!outputStreamPoller.Next(outputPacket)) {
      Debug.LogWarning("Failed to fetch an output packet, rendering the input image");
      texture.SetPixels32(pixelData.Colors);
      texture.Apply();
      return;
    }

    ImageFrame outputFrame = null;

    var status = gpuHelper.RunInGlContext(() => {
      var gpuFrame = outputPacket.GetValue();
      var gpuFrameFormat = gpuFrame.Format();
      var sourceTexture = gpuHelper.CreateSourceTexture(gpuFrame);

      outputFrame = new ImageFrame(
        gpuFrameFormat.ImageFormatFor(), gpuFrame.Width(), gpuFrame.Height(), ImageFrame.kGlDefaultAlignmentBoundary);

      gpuHelper.BindFramebuffer(sourceTexture);
      var info = gpuFrameFormat.GlTextureInfoFor(0);

      GL.ReadPixels(0, 0, sourceTexture.Width(), sourceTexture.Height(), info.glFormat, info.glType, outputFrame.PixelDataPtr());
      GL.Flush();

      sourceTexture.Release();

      return Status.Ok(false);
    });

    if (status.IsOk()) {
      texture.SetPixels32(outputFrame.GetColor32s());
    } else {
      Debug.LogError(status.ToString());
      texture.SetPixels32(pixelData.Colors);
    }

    texture.Apply();
  }
}
