﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Spreads
{
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException()
        {
            throw GetArgumentException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException(string message)
        {
            throw GetArgumentException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentOutOfRangeException()
        {
            throw GetArgumentOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentOutOfRangeException(string argument)
        {
            throw GetArgumentOutOfRangeException(argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException()
        {
            throw GetInvalidOperationException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidCastException()
        {
            throw GetInvalidCastException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException(string message)
        {
            throw GetInvalidOperationException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_ForVariantTypeMissmatch()
        {
            throw GetInvalidOperationException_ForVariantTypeMissmatch();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotImplementedException()
        {
            throw GetNotImplementedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotImplementedException(string message)
        {
            throw GetNotImplementedException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotSupportedException()
        {
            throw GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotSupportedException(string message)
        {
            throw GetNotSupportedException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowOutOfOrderKeyException<TKey>(TKey key)
        {
            throw GetOutOfOrderKeyException(key);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowOutOfOrderKeyException<TKey>(TKey key, string message)
        {
            throw GetOutOfOrderKeyException(key, message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowKeyNotFoundException(string message)
        {
            throw GetKeyNotFoundException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentNullException(string argument)
        {
            throw new ArgumentNullException(argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowObjectDisposedException(string objectName)
        {
            throw GetObjectDisposedException(objectName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowIOException(string message)
        {
            throw GetIOException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AssertFailFast(bool expectedTrueCondition, string message)
        {
            if (!expectedTrueCondition)
            {
                DoFailFast(message);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailFast(string message)
        {
            DoFailFast(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void DoFailFast(string message)
        {
            Environment.FailFast(message);
        }

        /////////////////////////////////////////////////////////////////////////////

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArgumentException GetArgumentException()
        {
            return new ArgumentException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArgumentException GetArgumentException(string message)
        {
            return new ArgumentException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException()
        {
            return new ArgumentOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(string argument)
        {
            return new ArgumentOutOfRangeException(argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException GetInvalidOperationException()
        {
            return new InvalidOperationException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidCastException GetInvalidCastException()
        {
            return new InvalidCastException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException GetInvalidOperationException(string message)
        {
            return new InvalidOperationException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException GetInvalidOperationException_ForVariantTypeMissmatch()
        {
            return new InvalidOperationException("Variant type doesn't match typeof(T)");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NotImplementedException GetNotImplementedException()
        {
            return new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NotImplementedException GetNotImplementedException(string message)
        {
            return new NotImplementedException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NotSupportedException GetNotSupportedException()
        {
            return new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NotSupportedException GetNotSupportedException(string message)
        {
            return new NotSupportedException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static OutOfOrderKeyException<TKey> GetOutOfOrderKeyException<TKey>(TKey key)
        {
            return new OutOfOrderKeyException<TKey>(key);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static OutOfOrderKeyException<TKey> GetOutOfOrderKeyException<TKey>(TKey key, string message)
        {
            return new OutOfOrderKeyException<TKey>(key, message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static KeyNotFoundException GetKeyNotFoundException(string message)
        {
            return new KeyNotFoundException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ObjectDisposedException GetObjectDisposedException(string objectName)
        {
            return new ObjectDisposedException(objectName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IOException GetIOException(string message)
        {
            return new IOException(message);
        }
    }
}
