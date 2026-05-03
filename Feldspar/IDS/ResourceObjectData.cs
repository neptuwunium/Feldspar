// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Feldspar.IDS.Format;
using Feldspar.IDS.Format.OBJ;
using Feldspar.Json;
using JetBrains.Annotations;
using Pluto.IO.Binary;

namespace Feldspar.IDS;

[JsonConverter(typeof(ResourceObjectDataConverter))]
public sealed class ResourceObjectData : IDisposable {
	public ResourceObjectData() => Data = RentedArray<byte>.Empty;

	public ResourceObjectData(ReadOnlySpan<OBJParam> @params, RentedArray<byte> data) {
		Data = data;

		var offset = 0;
		foreach (var param in @params) {
			var stride = param.Type switch {
				OBJParamType.Bool => 1,
				OBJParamType.Byte => 1,
				OBJParamType.Int16 => 2,
				OBJParamType.UInt16 => 2,
				OBJParamType.Int32 => 4,
				OBJParamType.UInt32 => 4,
				OBJParamType.Int64 => 8,
				OBJParamType.UInt64 => 8,
				OBJParamType.Float32 => 4,
				OBJParamType.Float64 => 8,
				OBJParamType.Vector4F => 16,
				OBJParamType.Matrix4F => 64,
				OBJParamType.Vector2F => 8,
				OBJParamType.Vector3F => 12,
				_ => 0,
			};

			if (stride == 0) {
				continue;
			}

			Parameters[param.Name] = (param, offset, stride);
			offset += stride * param.Count;
		}
	}

	public static ResourceObjectData Empty { get; } = new();

	public RentedArray<byte> Data { get; }
	public bool IsEmpty => Data.Length == 0;
	public Dictionary<KTID, (OBJParam Param, int Offset, int Stride)> Parameters { get; } = [];

	private static MethodInfo ReadVariantNumberMethod { get; } = typeof(ResourceObjectData).GetMethod("ReadVariantNumber", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new UnreachableException();

	public void Dispose() => Data.Dispose();

	[UsedImplicitly]
	private TOutput ReadVariantNumber<TInput, TOutput>(int offset) where TInput : struct, INumberBase<TInput> where TOutput : struct, INumberBase<TOutput> => TOutput.CreateTruncating(MemoryMarshal.Read<TInput>(Data.Span[offset..]));

	public T ReadParam<T>(KTID name, int index) where T : struct {
		using var enumerator = ReadParams<T>(name, index).GetEnumerator();
		return !enumerator.MoveNext() ? default : enumerator.Current;
	}

	public IEnumerable<T> ReadParams<T>(KTID name, int skip = 0) where T : struct {
		if (!Parameters.TryGetValue(name, out var tuple)) {
			yield break;
		}

		var (param, offset, stride) = tuple;

		if (skip > param.Count || param.Type == OBJParamType.None) {
			yield break;
		}

		offset += stride * skip;

		MethodInfo? call = null;
		object[]? args = null;
		if (stride != Unsafe.SizeOf<T>()) {
			// this will blow up if T is not a INumberBase<T>
			call = param.Type switch {
				OBJParamType.Bool => ReadVariantNumberMethod.MakeGenericMethod(typeof(bool), typeof(T)),
				OBJParamType.Byte => ReadVariantNumberMethod.MakeGenericMethod(typeof(byte), typeof(T)),
				OBJParamType.Int16 => ReadVariantNumberMethod.MakeGenericMethod(typeof(short), typeof(T)),
				OBJParamType.UInt16 => ReadVariantNumberMethod.MakeGenericMethod(typeof(ushort), typeof(T)),
				OBJParamType.Int32 => ReadVariantNumberMethod.MakeGenericMethod(typeof(int), typeof(T)),
				OBJParamType.UInt32 => ReadVariantNumberMethod.MakeGenericMethod(typeof(uint), typeof(T)),
				OBJParamType.Int64 => ReadVariantNumberMethod.MakeGenericMethod(typeof(long), typeof(T)),
				OBJParamType.UInt64 => ReadVariantNumberMethod.MakeGenericMethod(typeof(ulong), typeof(T)),
				OBJParamType.Float32 => ReadVariantNumberMethod.MakeGenericMethod(typeof(float), typeof(T)),
				OBJParamType.Float64 => ReadVariantNumberMethod.MakeGenericMethod(typeof(double), typeof(T)),
				_ => null,
			};

			if (call != null) {
				args = new object[1];
			} else {
				yield break;
			}
		}

		for (var i = skip; i < param.Count; ++i) {
			if (offset + stride > Data.Length) {
				yield return default;
				continue;
			}

			if (call != null) {
				args![0] = offset;
				yield return (T) call.Invoke(this, args)!;
			} else {
				yield return MemoryMarshal.Read<T>(Data.Span[offset..]);
			}

			offset += stride;
		}
	}
}
