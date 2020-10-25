using System;
using System.Runtime.InteropServices;

using MpGlTextureBuffer = System.IntPtr;
using GlSyncTokenPtr = System.IntPtr;
using UnityEngine;

namespace Mediapipe {
  public class GlTextureBuffer : ResourceHandle {
    private static UInt32 GL_TEXTURE_2D = 0x0DE1;

    private bool _disposed = false;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void DeletionCallback(GlSyncTokenPtr ptr);

    private GlTextureBuffer(MpGlTextureBuffer ptr) : base(ptr) {}

    public GlTextureBuffer(UInt32 target, UInt32 name, int width, int height,
        GpuBufferFormat format, DeletionCallback callback, GlContext glContext)
    {
      ptr = UnsafeNativeMethods.MpGlTextureBufferCreate(
        target, name, width, height, (UInt32)format,
        Marshal.GetFunctionPointerForDelegate(callback),
        glContext == null ? IntPtr.Zero : glContext.GetPtr());

      base.TakeOwnership(ptr);
    }

    public GlTextureBuffer(UInt32 name, int width, int height, GpuBufferFormat format, DeletionCallback callback, GlContext glContext) :
        this(GL_TEXTURE_2D, name, width, height, format, callback, glContext) {}

    public GlTextureBuffer(UInt32 name, int width, int height, GpuBufferFormat format, DeletionCallback callback) :
        this(name, width, height, format, callback, null) {}

    protected override void Dispose(bool disposing) {
      if (_disposed) return;

      if (OwnsResource()) {
        UnsafeNativeMethods.MpGlTextureBufferDestroy(ptr);
      }

      ptr = IntPtr.Zero;

      _disposed = true;
    }
  }
}
