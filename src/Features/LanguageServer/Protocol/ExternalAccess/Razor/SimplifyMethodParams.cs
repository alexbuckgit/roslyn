﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Serialization;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace Microsoft.CodeAnalysis.LanguageServer.ExternalAccess.Razor;

//
// Summary:
//     Class representing the parameters sent from the client to the server for the
//     textDocument/simplifyTypeNames request.
[DataContract]
internal record SimplifyMethodParams : ITextDocumentParams
{
    //
    // Summary:
    //     Gets or sets the value which identifies the document the request came from.
    [DataMember(Name = "textDocument")]
    public required TextDocumentIdentifier TextDocument { get; set; }

    [DataMember(Name = "textEdit")]
    public required TextEdit TextEdit { get; set; }
}
