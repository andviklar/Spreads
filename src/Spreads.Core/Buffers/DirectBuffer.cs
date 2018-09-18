﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static System.Runtime.CompilerServices.Unsafe;

namespace Spreads.Buffers
{
    /// <summary>
    /// Provides unsafe read/write opertaions on a memory pointer.
    /// </summary>
    [DebuggerTypeProxy(typeof(SpreadsBuffers_DirectBufferDebugView))]
    [DebuggerDisplay("Length={" + nameof(Length) + ("}"))]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct DirectBuffer
    {
        public static DirectBuffer Invalid = new DirectBuffer(-1, (byte*)IntPtr.Zero);

        // NB this is used for Spreads.LMDB as MDB_val, where length is IntPtr. However, LMDB works normally only on x64
        // if we even support x86 we will have to create a DTO with IntPtr length. But for x64 casting IntPtr to/from long
        // is surprisingly expensive, e.g. Slice and ctor show up in profiler.

        internal readonly long _length;
        internal readonly byte* _data;

        /// <summary>
        /// Attach a view to an unmanaged buffer owned by external code
        /// </summary>
        /// <param name="data">Unmanaged byte buffer</param>
        /// <param name="length">Length of the buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DirectBuffer(long length, IntPtr data)
        {
            if (data == IntPtr.Zero)
            {
                ThrowHelper.ThrowArgumentNullException("data");
            }
            if (length <= 0)
            {
                ThrowHelper.ThrowArgumentException("Memory size must be > 0");
            }
            _length = length;
            _data = (byte*)data;
        }

        /// <summary>
        /// Unsafe constructors performs no input checks.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DirectBuffer(long length, byte* data)
        {
            _length = length;
            _data = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DirectBuffer(RetainedMemory<byte> retainedMemory)
        {
            _length = retainedMemory.Length;
            _data = (byte*)retainedMemory.Pointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DirectBuffer(Span<byte> span)
        {
            _length = span.Length;
            _data = (byte*)AsPointer(ref MemoryMarshal.GetReference(span));
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length > 0 && _data != null;
        }

        public Span<byte> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Span<byte>(_data, (int)_length);
        }

        /// <summary>
        /// Capacity of the underlying buffer
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => checked((int)_length);
        }

        public long LongLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        public byte* Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DirectBuffer Slice(long start)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(0, start); }

            return new DirectBuffer(_length - start, _data + start);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DirectBuffer Slice(long start, long length)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(start, length); }

            return new DirectBuffer(length, _data + start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Assert(long index, long length)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            {
                // NB Not FailFast because we do not modify data on failed checks
                if (!IsValid)
                {
                    ThrowHelper.ThrowInvalidOperationException("DirectBuffer is invalid");
                }
                if ((ulong)index + (ulong)length > (ulong)_length)
                {
                    ThrowHelper.ThrowArgumentException("Not enough space in DirectBuffer");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="byte"/> value at a given index.
        /// </summary>
        /// <param name="index">index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadChar(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 2); }
            return ReadUnaligned<char>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="byte"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChar(long index, char value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 2); }
            WriteUnaligned(_data + index, value);
        }

        /// <summary>
        /// Gets the <see cref="sbyte"/> value at a given index.
        /// </summary>
        /// <param name="index"> index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 1); }
            return ReadUnaligned<sbyte>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="sbyte"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSByte(long index, sbyte value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 1); }
            WriteUnaligned(_data + index, value);
        }

        /// <summary>
        /// Gets the <see cref="byte"/> value at a given index.
        /// </summary>
        /// <param name="index"> index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 1); }
            return ReadUnaligned<byte>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="byte"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(long index, byte value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 1); }
            WriteUnaligned(_data + index, value);
        }

        [Pure]
        public byte this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (Settings.AdditionalCorrectnessChecks.Enabled)
                {
                    Assert(index, 1);
                }

                return *(_data + index);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (Settings.AdditionalCorrectnessChecks.Enabled)
                { Assert(index, 1); }

                *(_data + index) = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="short"/> value at a given index.
        /// </summary>
        /// <param name="index"> index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public short ReadInt16(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 2); }
            return ReadUnaligned<short>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="short"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(long index, short value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 2); }
            WriteUnaligned(_data + index, value);
        }

        /// <summary>
        /// Gets the <see cref="int"/> value at a given index.
        /// </summary>
        /// <param name="index"> index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public int ReadInt32(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            return ReadUnaligned<int>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="int"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(long index, int value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            WriteUnaligned(_data + index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public int VolatileReadInt32(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            return Volatile.Read(ref *(int*)(_data + index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VolatileWriteInt32(long index, int value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            Volatile.Write(ref *(int*)(_data + index), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public uint VolatileReadUInt32(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            return Volatile.Read(ref *(uint*)(_data + index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VolatileWriteUInt32(long index, uint value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            Volatile.Write(ref *(uint*)(_data + index), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public long VolatileReadInt64(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            return Volatile.Read(ref *(long*)(_data + index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VolatileWriteUInt64(long index, ulong value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            Volatile.Write(ref *(ulong*)(_data + index), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public ulong VolatileReadUInt64(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            return Volatile.Read(ref *(ulong*)(_data + index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VolatileWriteInt64(long index, long value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            Volatile.Write(ref *(long*)(_data + index), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public int InterlockedIncrementInt32(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            return Interlocked.Increment(ref *(int*)(_data + index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public int InterlockedDecrementInt32(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            return Interlocked.Decrement(ref *(int*)(_data + index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public int InterlockedAddInt32(long index, int value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            return Interlocked.Add(ref *(int*)(_data + index), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public int InterlockedReadInt32(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            return Interlocked.Add(ref *(int*)(_data + index), 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public int InterlockedCompareExchangeInt32(long index, int value, int comparand)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            return Interlocked.CompareExchange(ref *(int*)(_data + index), value, comparand);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public long InterlockedIncrementInt64(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            return Interlocked.Increment(ref *(long*)(_data + index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public long InterlockedDecrementInt64(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            return Interlocked.Decrement(ref *(long*)(_data + index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public long InterlockedAddInt64(long index, long value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            return Interlocked.Add(ref *(long*)(_data + index), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public long InterlockedReadInt64(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            return Interlocked.Add(ref *(long*)(_data + index), 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public long InterlockedCompareExchangeInt64(long index, long value, long comparand)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            return Interlocked.CompareExchange(ref *(long*)(_data + index), value, comparand);
        }

        /// <summary>
        /// Gets the <see cref="long"/> value at a given index.
        /// </summary>
        /// <param name="index"> index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public long ReadInt64(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            return ReadUnaligned<long>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="long"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long index, long value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            WriteUnaligned(_data + index, value);
        }

        /// <summary>
        /// Gets the <see cref="ushort"/> value at a given index.
        /// </summary>
        /// <param name="index"> index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public ushort ReadUInt16(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 2); }
            return ReadUnaligned<ushort>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="ushort"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(long index, ushort value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 2); }
            WriteUnaligned(_data + index, value);
        }

        /// <summary>
        /// Gets the <see cref="uint"/> value at a given index.
        /// </summary>
        /// <param name="index"> index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public uint ReadUInt32(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            return ReadUnaligned<uint>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="uint"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(long index, uint value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            WriteUnaligned(_data + index, value);
        }

        /// <summary>
        /// Gets the <see cref="ulong"/> value at a given index.
        /// </summary>
        /// <param name="index"> index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public ulong ReadUInt64(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            return ReadUnaligned<ulong>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="ulong"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        public void WriteUInt64(long index, ulong value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            {
                Assert(index, 8);
            }
            WriteUnaligned(_data + index, value);
        }

        /// <summary>
        /// Gets the <see cref="float"/> value at a given index.
        /// </summary>
        /// <param name="index"> index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public float ReadFloat(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            return ReadUnaligned<float>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="float"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFloat(long index, float value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 4); }
            WriteUnaligned(_data + index, value);
        }

        /// <summary>
        /// Gets the <see cref="double"/> value at a given index.
        /// </summary>
        /// <param name="index"> index in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public double ReadDouble(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            return ReadUnaligned<double>(_data + index);
        }

        /// <summary>
        /// Writes a <see cref="double"/> value to a given index.
        /// </summary>
        /// <param name="index">index in bytes for where to put.</param>
        /// <param name="value">value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDouble(long index, double value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 8); }
            WriteUnaligned(_data + index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public T Read<T>(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            {
                var size = SizeOf<T>();
                Assert(index, size);
            }
            return ReadUnaligned<T>(_data + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public int Read<T>(long index, out T value)
        {
            var size = SizeOf<T>();
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, size); }
            value = ReadUnaligned<T>(_data + index);
            return size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(long index, T value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, SizeOf<T>()); }
            WriteUnaligned(_data + index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(long index, int length)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, length); }
            var destination = _data + index;
            InitBlockUnaligned(destination, 0, (uint)length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        // ReSharper disable once InconsistentNaming
        public UUID ReadUUID(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 16); }
            return ReadUnaligned<UUID>(_data + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once InconsistentNaming
        public void WriteUUID(long index, UUID value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 16); }
            WriteUnaligned(_data + index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public int ReadAsciiDigit(long index)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 1); }
            return ReadUnaligned<byte>(_data + index) - '0';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteAsciiDigit(long index, byte value)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, 1); }
            WriteUnaligned(_data + index, (byte)(value + '0'));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerifyAlignment(int alignment)
        {
            if (0 != ((long)_data & (alignment - 1)))
            {
                ThrowHelper.ThrowInvalidOperationException(
                    $"DirectBuffer is not correctly aligned: addressOffset={(long)_data:D} in not divisible by {alignment:D}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(long index, Memory<byte> destination, int length)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            {
                if ((ulong)destination.Length < (ulong)length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }
            }

            fixed (byte* destPtr = &MemoryMarshal.GetReference(destination.Span))
            {
                CopyTo(index, destPtr, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(long index, in DirectBuffer destination, int destinationOffset, int length)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            {
                destination.Assert(destinationOffset, length);
            }
            CopyTo(index, destination.Data + destinationOffset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(long index, byte* destination, int length)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { Assert(index, length); }
            CopyBlockUnaligned(destination, _data + index, checked((uint)length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(long index, ReadOnlyMemory<byte> source, int length)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            {
                if ((ulong)source.Length < (ulong)length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }
            }

            fixed (byte* srcPtr = &MemoryMarshal.GetReference(source.Span))
            {
                CopyFrom(index, srcPtr, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(long index, in DirectBuffer source, int sourceOffset, int length)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            { source.Assert(sourceOffset, length); }
            CopyFrom(index, source.Data + sourceOffset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(long index, byte* source, int length)
        {
            if (Settings.AdditionalCorrectnessChecks.Enabled)
            {
                Assert(index, length);
            }

            CopyBlockUnaligned(_data + index, source, checked((uint)length));
        }

        #region Debugger proxy class

        // ReSharper disable once InconsistentNaming
        internal class SpreadsBuffers_DirectBufferDebugView
        {
            private readonly DirectBuffer _db;

            public SpreadsBuffers_DirectBufferDebugView(DirectBuffer db)
            {
                _db = db;
            }

            public byte* Data => _db.Data;

            public long Length => _db.Length;

            public bool IsValid => _db.IsValid;

            public Span<byte> Span => _db.IsValid ? _db.Span : default;
        }

        #endregion Debugger proxy class
    }

    public static unsafe class DirectBufferExtensions
    {
        public static string GetString(this Encoding encoding, in DirectBuffer buffer)
        {
            return encoding.GetString(buffer._data, buffer.Length);
        }
    }
}
