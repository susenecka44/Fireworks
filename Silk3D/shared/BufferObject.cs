using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace Util;

public class BufferObject<TDataType> : IDisposable
	where TDataType : unmanaged
{
	private uint _handle;
	private BufferTargetARB _bufferType;
	private GL _gl;

  public unsafe BufferObject (GL gl, uint size, BufferTargetARB bufferType)
  {
    _gl = gl;
    _bufferType = bufferType;

    _handle = _gl.GenBuffer();
    Bind();
    _gl.BufferData(bufferType, (nuint)(size * sizeof(TDataType)), (void*)0, BufferUsageARB.DynamicDraw);
  }

	public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
	{
		_gl = gl;
		_bufferType = bufferType;

		_handle = _gl.GenBuffer();
		Bind();
		fixed (void* d = data)
		{
			_gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.DynamicDraw);
		}
	}

	public unsafe void UpdateData(Span<TDataType> data, int from, int size)
	{
		Bind();
		fixed (void* d = data)
		{
			_gl.BufferSubData(_bufferType, (nint)(from * sizeof(TDataType)), (nuint)(size * sizeof(TDataType)), ((byte*)d) + from * sizeof(TDataType));
		}
	}

  public unsafe void UpdateData (List<TDataType> array, int from, int size)
  {
    var span = CollectionsMarshal.AsSpan(array);
    Bind();
    fixed (void* d = span)
    {
      _gl.BufferSubData(_bufferType, (nint)(from * sizeof(TDataType)), (nuint)(size * sizeof(TDataType)), ((byte*)d) + from * sizeof(TDataType));
    }
  }

	public void Bind()
	{
		_gl.BindBuffer(_bufferType, _handle);
	}

	public void Dispose()
	{
		_gl.DeleteBuffer(_handle);
	}
}
