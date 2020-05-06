﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AnalyzerRunner;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis.MSBuild;

namespace IdeCoreBenchmarks
{
    internal class Program
    {
        public const string RoslynRootPathEnvVariableName = "ROSLYN_SOURCE_ROOT_PATH";

        public static string GetRoslynRootLocation([CallerFilePath] string sourceFilePath = "")
        {
            //This file is located at [Roslyn]\src\Tools\IdeCoreBenchmarks\Program.cs
            return Path.Combine(Path.GetDirectoryName(sourceFilePath), @"..\..\..");
        }

        private static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable(RoslynRootPathEnvVariableName, GetRoslynRootLocation());
            new BenchmarkSwitcher(typeof(Program).Assembly).Run(args);
        }
    }
}
