﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.LanguageServices.Implementation;

internal abstract partial class AbstractOleCommandTarget
{
    public virtual int Exec(ref Guid pguidCmdGroup, uint commandId, uint executeInformation, IntPtr pvaIn, IntPtr pvaOut)
    {
        try
        {
            this.CurrentlyExecutingCommand = commandId;

            // If we don't have a subject buffer, then that means we're outside our code and we should ignore it
            // Also, ignore the command if executeInformation indicates isn't meant to be executed. From env\msenv\core\cmdwin.cpp:
            //      To query the parameter type list of a command, we call Exec with 
            //      the LOWORD of nCmdexecopt set to OLECMDEXECOPT_SHOWHELP (instead of
            //      the more usual OLECMDEXECOPT_DODEFAULT), the HIWORD of nCmdexecopt
            //      set to VSCmdOptQueryParameterList, pvaIn set to NULL, and pvaOut 
            //      pointing to a VARIANT ready to receive the result BSTR.
            var shouldSkipCommand = executeInformation == (((uint)VsMenus.VSCmdOptQueryParameterList << 16) | (uint)OLECMDEXECOPT.OLECMDEXECOPT_SHOWHELP);
            if (shouldSkipCommand || GetSubjectBufferContainingCaret() == null)
            {
                return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
            }

            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                return ExecuteVisualStudio2000(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
            }
            else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                return ExecuteVisualStudio97(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
            }
            else
            {
                return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
            }
        }
        finally
        {
            this.CurrentlyExecutingCommand = default;
        }
    }

    private int ExecuteVisualStudio97(ref Guid pguidCmdGroup, uint commandId, uint executeInformation, IntPtr pvaIn, IntPtr pvaOut)
    {
        switch ((VSConstants.VSStd97CmdID)commandId)
        {
            case VSConstants.VSStd97CmdID.Paste:
            case VSConstants.VSStd97CmdID.Delete:
            case VSConstants.VSStd97CmdID.SelectAll:
            case VSConstants.VSStd97CmdID.Undo:
            case VSConstants.VSStd97CmdID.Redo:
            case VSConstants.VSStd97CmdID.MultiLevelUndo:
            case VSConstants.VSStd97CmdID.MultiLevelRedo:
                GCManager.UseLowLatencyModeForProcessingUserInput();
                return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);

            default:
                return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
        }
    }

    protected virtual int ExecuteVisualStudio2000(ref Guid pguidCmdGroup, uint commandId, uint executeInformation, IntPtr pvaIn, IntPtr pvaOut)
    {
        switch ((VSConstants.VSStd2KCmdID)commandId)
        {
            case VSConstants.VSStd2KCmdID.TYPECHAR:
            case VSConstants.VSStd2KCmdID.RETURN:
            case VSConstants.VSStd2KCmdID.TAB:
            case VSConstants.VSStd2KCmdID.BACKTAB:
            case VSConstants.VSStd2KCmdID.HOME:
            case VSConstants.VSStd2KCmdID.END:
            case VSConstants.VSStd2KCmdID.BOL:
            case VSConstants.VSStd2KCmdID.BOL_EXT:
            case VSConstants.VSStd2KCmdID.EOL:
            case VSConstants.VSStd2KCmdID.EOL_EXT:
            case VSConstants.VSStd2KCmdID.SELECTALL:
            case VSConstants.VSStd2KCmdID.OPENLINEABOVE:
            case VSConstants.VSStd2KCmdID.OPENLINEBELOW:
            case VSConstants.VSStd2KCmdID.UP:
            case VSConstants.VSStd2KCmdID.DOWN:
            case VSConstants.VSStd2KCmdID.BACKSPACE:
            case VSConstants.VSStd2KCmdID.DELETE:
            case VSConstants.VSStd2KCmdID.ECMD_INSERTCOMMENT:
            case VSConstants.VSStd2KCmdID.COMPLETEWORD:
            case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
            case VSConstants.VSStd2KCmdID.PARAMINFO:
            case VSConstants.VSStd2KCmdID.RENAME:
            case VSConstants.VSStd2KCmdID.EXTRACTINTERFACE:
            case VSConstants.VSStd2KCmdID.EXTRACTMETHOD:
            case VSConstants.VSStd2KCmdID.PASTE:
            case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
            case VSConstants.VSStd2KCmdID.SURROUNDWITH:
                GCManager.UseLowLatencyModeForProcessingUserInput();
                return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);

            default:
                return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
        }
    }
}
