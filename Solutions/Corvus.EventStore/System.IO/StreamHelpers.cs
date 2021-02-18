// <copyright file="StreamHelpers.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable SA1611 // Element parameters should be documented

namespace System.IO
{
    /// <summary>Provides methods to help in the implementation of Stream-derived types.</summary>
    internal static partial class StreamHelpers
    {
        /// <summary>Validate the arguments to CopyTo, as would Stream.CopyTo.</summary>
        public static void ValidateCopyToArgs(Stream source, Stream destination, int bufferSize)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, "Must be greater than zero");
            }

            bool sourceCanRead = source.CanRead;
            if (!sourceCanRead && !source.CanWrite)
            {
                throw new ObjectDisposedException(null, "Stream closed");
            }

            bool destinationCanWrite = destination.CanWrite;
            if (!destinationCanWrite && !destination.CanRead)
            {
                throw new ObjectDisposedException(nameof(destination), "Stream closed");
            }

            if (!sourceCanRead)
            {
                throw new NotSupportedException("Unreadable stream");
            }

            if (!destinationCanWrite)
            {
                throw new NotSupportedException("Unwritable stream");
            }
        }
    }
}