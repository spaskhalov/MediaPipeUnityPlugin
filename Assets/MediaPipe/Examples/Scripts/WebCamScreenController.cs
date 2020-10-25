using System;
using UnityEngine;

public class WebCamScreenController : MonoBehaviour {
  [SerializeField] int defaultWidth = 640;
  [SerializeField] int defaultHeight = 480;
  [SerializeField] int fps = 30;
  [SerializeField] float defaultFocalLengthPx = 2.0f;

  private WebCamDevice webCamDevice;
  private WebCamTexture webCamTexture;
  private Texture2D outputTexture;
  private Color32[] pixelData;

  public int width { get; private set; }
  public int height { get; private set; }
  public bool isPlaying {
    get { return webCamTexture == null ? false : webCamTexture.isPlaying; }
  }

  void Start() {
    this.width = defaultWidth;
    this.height = defaultHeight;
  }

  public void ResetScreen(WebCamDevice? device) {
    if (isPlaying) {
      webCamTexture.Stop();
      webCamTexture = null;
    }

    if (device is WebCamDevice deviceValue) {
      webCamDevice = deviceValue;
    } else {
      return;
    }

    GetComponent<TextureFramePool>().SetDimension(width, height);
    webCamTexture = new WebCamTexture(device?.name, width, height, fps);

    try {
      webCamTexture.Play();
    } catch (Exception e) {
      Debug.LogWarning(e.ToString());
      return;
    }

    Renderer renderer = GetComponent<Renderer>();
    outputTexture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.BGRA32, false);
    renderer.material.mainTexture = outputTexture;

    pixelData = new Color32[webCamTexture.width * webCamTexture.height];
  }

  public void ChangeDimension(int width, int height) {
    this.width = width;
    this.height = height;

    ResetScreen(webCamDevice);
  }

  public float GetFocalLengthPx() {
    return isPlaying ? defaultFocalLengthPx : 0;
  }

  public Color32[] GetPixels32() {
    return isPlaying ? webCamTexture.GetPixels32(pixelData) : null;
  }

  public Color[] GetPixels() {
    return isPlaying ? webCamTexture.GetPixels() : null;
  }

  public IntPtr GetNativeTexturePtr() {
    return webCamTexture.GetNativeTexturePtr();
  }

  public Texture2D GetScreen() {
    return outputTexture;
  }

  public void DrawScreen(Color32[] colors) {
    // TODO: size assertion
    outputTexture.SetPixels32(colors);
    outputTexture.Apply();
  }

  public void DrawScreen(TextureFrame src) {
    // TODO: size assertion
    src.CopyTexture(outputTexture);
  }

  public TextureFramePool.TextureFrameRequest RequestNextFrame() {
    return GetComponent<TextureFramePool>().RequestNextTextureFrame((TextureFrame textureFrame) => {
      textureFrame.CopyTextureFrom(webCamTexture);
    });
  }

  public void OnReleaseFrame(TextureFrame textureFrame) {
    GetComponent<TextureFramePool>().OnTextureFrameReleased(textureFrame);
  }

  private void CopyWebCamTexture(Texture dst) {
    Graphics.CopyTexture(webCamTexture, dst);
  }
}
