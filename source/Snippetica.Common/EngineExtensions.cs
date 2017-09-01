// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Snippetica
{
    public static class EngineExtensions
    {
        public static string GetIdentifier(this Engine engine)
        {
            switch (engine)
            {
                case Engine.VisualStudio:
                    return "vs";
                case Engine.VisualStudioCode:
                    return "vscode";
                default:
                    throw new ArgumentException("", nameof(engine));
            }
        }
    }
}
