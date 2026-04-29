using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Feldspar.IDS.Format;
using Feldspar.IDS.Format.OBJ;
using JetBrains.Annotations;
using Pluto.IO.Binary;

namespace Feldspar.IDS;

public sealed class ResourceObject : IDisposable {
	public ResourceObject() => Data = RentedArray<byte>.Empty;

	public ResourceObject(Span<OBJProperty> properties, RentedArray<byte> data) {
		Data = data;

		var offset = 0;
		foreach (var property in properties) {
			var stride = property.Type switch {
				OBJPropertyType.Bool => 1,
				OBJPropertyType.Byte => 1,
				OBJPropertyType.Int16 => 2,
				OBJPropertyType.UInt16 => 2,
				OBJPropertyType.Int32 => 4,
				OBJPropertyType.UInt32 => 4,
				OBJPropertyType.Int64 => 8,
				OBJPropertyType.UInt64 => 8,
				OBJPropertyType.Float32 => 4,
				OBJPropertyType.Float64 => 8,
				OBJPropertyType.Vector4F => 16,
				OBJPropertyType.Quaternion => 16,
				OBJPropertyType.Vector2F => 8,
				OBJPropertyType.Vector3F => 12,
				_ => 0,
			};

			if (stride == 0) {
				continue;
			}

			Properties[property.Name] = (property, offset, stride);
			offset += stride * property.Count;
		}
	}

	public static ResourceObject Empty { get; } = new();

	public RentedArray<byte> Data { get; }
	public Dictionary<KTID, (OBJProperty Property, int Offset, int Stride)> Properties { get; } = [];

	private static MethodInfo ReadVariantNumberMethod { get; } = typeof(ResourceObject).GetMethod("ReadVariantNumber", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new UnreachableException();

	public void Dispose() => Data.Dispose();

	[UsedImplicitly]
	private TOutput ReadVariantNumber<TInput, TOutput>(int offset) where TInput : struct, INumberBase<TInput> where TOutput : struct, INumberBase<TOutput> => TOutput.CreateTruncating(MemoryMarshal.Read<TInput>(Data.Span[offset..]));

	public T ReadProperty<T>(KTID name, int index) where T : struct {
		using var enumerator = ReadProperties<T>(name, index).GetEnumerator();
		return !enumerator.MoveNext() ? default : enumerator.Current;
	}

	public IEnumerable<T> ReadProperties<T>(KTID name, int skip = 0) where T : struct {
		if (!Properties.TryGetValue(name, out var tuple)) {
			yield break;
		}

		var (property, offset, stride) = tuple;

		if (skip > property.Count || property.Type == OBJPropertyType.None) {
			yield break;
		}

		offset += stride * skip;

		MethodInfo? call = null;
		object[]? args = null;
		if (stride != Unsafe.SizeOf<T>()) {
			// this will blow up if T is not a INumberBase<T>
			call = property.Type switch {
				OBJPropertyType.Bool => ReadVariantNumberMethod.MakeGenericMethod(typeof(bool), typeof(T)),
				OBJPropertyType.Byte => ReadVariantNumberMethod.MakeGenericMethod(typeof(byte), typeof(T)),
				OBJPropertyType.Int16 => ReadVariantNumberMethod.MakeGenericMethod(typeof(short), typeof(T)),
				OBJPropertyType.UInt16 => ReadVariantNumberMethod.MakeGenericMethod(typeof(ushort), typeof(T)),
				OBJPropertyType.Int32 => ReadVariantNumberMethod.MakeGenericMethod(typeof(int), typeof(T)),
				OBJPropertyType.UInt32 => ReadVariantNumberMethod.MakeGenericMethod(typeof(uint), typeof(T)),
				OBJPropertyType.Int64 => ReadVariantNumberMethod.MakeGenericMethod(typeof(long), typeof(T)),
				OBJPropertyType.UInt64 => ReadVariantNumberMethod.MakeGenericMethod(typeof(ulong), typeof(T)),
				OBJPropertyType.Float32 => ReadVariantNumberMethod.MakeGenericMethod(typeof(float), typeof(T)),
				OBJPropertyType.Float64 => ReadVariantNumberMethod.MakeGenericMethod(typeof(double), typeof(T)),
				_ => null,
			};

			if (call != null) {
				args = new object[1];
			} else {
				yield break;
			}
		}

		for (var i = skip; i < property.Count; ++i) {
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
