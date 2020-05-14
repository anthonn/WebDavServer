﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System;

namespace Fubar.System
{
    internal static class Error
    {
        public static Exception ArgumentNull(string paramName) => new ArgumentNullException(paramName);
        public static Exception ArgumentOutOfRange(string paramName) => new ArgumentOutOfRangeException(paramName);
        public static Exception NoElements() => new InvalidOperationException(Strings.NO_ELEMENTS);
        public static Exception MoreThanOneElement() => new InvalidOperationException(Strings.MORE_THAN_ONE_ELEMENT);
        public static Exception NotSupported() => new NotSupportedException(Strings.NOT_SUPPORTED);
    }

     internal static class Strings
    {
        public static string NO_ELEMENTS = "Source sequence doesn't contain any elements.";
        public static string MORE_THAN_ONE_ELEMENT = "Source sequence contains more than one element.";
        public static string NOT_SUPPORTED = "Specified method is not supported.";
    }
}
