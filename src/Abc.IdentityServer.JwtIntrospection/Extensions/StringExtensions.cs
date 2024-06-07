// ----------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics;

namespace System;
internal static class StringExtensions
{
    [DebuggerStepThrough]
    public static bool IsPresent(this string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}