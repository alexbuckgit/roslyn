﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.SolutionCrawler;
using Roslyn.LanguageServer.Protocol;

namespace Microsoft.CodeAnalysis.LanguageServer.Handler.Diagnostics.Public
{
    internal sealed partial class PublicWorkspacePullDiagnosticsHandler : IOnInitialized
    {
        public async Task OnInitializedAsync(ClientCapabilities clientCapabilities, RequestContext context, CancellationToken cancellationToken)
        {
            if (clientCapabilities?.TextDocument?.Diagnostic?.DynamicRegistration is true && IsFsaEnabled())
            {
                var sources = _diagnosticSourceManager.GetSourceNames(isDocument: false);
                var regParams = new RegistrationParams
                {
                    Registrations = sources.Select(FromSourceName).ToArray()
                };
                await _clientLanguageServerManager.SendRequestAsync(
                    methodName: Methods.ClientRegisterCapabilityName,
                    @params: regParams,
                    cancellationToken).ConfigureAwait(false);

                Registration FromSourceName(string sourceName)
                {
                    return new()
                    {
                        Id = sourceName,
                        Method = Methods.WorkspaceDiagnosticName,
                        RegisterOptions = new DiagnosticRegistrationOptions { Identifier = sourceName, InterFileDependencies = true, WorkDoneProgress = true }
                    };
                }
            }

            bool IsFsaEnabled()
            {
                foreach (var language in context.SupportedLanguages)
                {
                    if (GlobalOptions.GetBackgroundAnalysisScope(language) == BackgroundAnalysisScope.FullSolution)
                        return true;
                }

                return false;
            }
        }
    }
}
