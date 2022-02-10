﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    /// <summary>
    /// Dynamic call-site delegate, for call-sites that do not
    /// match System.Action or System.Func signatures.
    /// </summary>
    internal sealed class SynthesizedDelegateSymbol : SynthesizedContainer
    {
        private readonly NamespaceOrTypeSymbol _containingSymbol;
        private readonly MethodSymbol _constructor;
        private readonly MethodSymbol _invoke;

        public SynthesizedDelegateSymbol(
            NamespaceOrTypeSymbol containingSymbol,
            string name,
            TypeSymbol objectType,
            TypeSymbol intPtrType,
            TypeSymbol? voidReturnTypeOpt,
            int parameterCount,
            RefKindVector refKinds)
            : base(name, parameterCount, returnsVoid: voidReturnTypeOpt is not null)
        {
            Debug.Assert(refKinds.IsNull || parameterCount == refKinds.Capacity - (voidReturnTypeOpt is { } ? 0 : 1));

            _containingSymbol = containingSymbol;
            _constructor = new SynthesizedDelegateConstructor(this, objectType, intPtrType);
            _invoke = createInvokeMethod(this, refKinds, voidReturnTypeOpt);

            static SynthesizedDelegateInvokeMethod createInvokeMethod(SynthesizedDelegateSymbol containingType, RefKindVector refKinds, TypeSymbol? voidReturnTypeOpt)
            {
                var typeParams = containingType.TypeParameters;

                int parameterCount = typeParams.Length - (voidReturnTypeOpt is null ? 1 : 0);
                var parameterTypes = ArrayBuilder<TypeWithAnnotations>.GetInstance(parameterCount);
                var parameterRefKinds = ArrayBuilder<RefKind>.GetInstance(parameterCount);
                for (int i = 0; i < parameterCount; i++)
                {
                    parameterTypes.Add(TypeWithAnnotations.Create(typeParams[i]));
                    parameterRefKinds.Add(refKinds.IsNull ? RefKind.None : refKinds[i]);
                }

                // if we are given Void type the method returns Void, otherwise its return type is the last type parameter of the delegate:
                var returnType = TypeWithAnnotations.Create(voidReturnTypeOpt ?? typeParams[parameterCount]);
                var returnRefKind = (refKinds.IsNull || voidReturnTypeOpt is { }) ? RefKind.None : refKinds[parameterCount];

                var method = new SynthesizedDelegateInvokeMethod(containingType, parameterTypes, parameterRefKinds, returnType, returnRefKind);
                parameterRefKinds.Free();
                parameterTypes.Free();
                return method;
            }
        }

        public override Symbol ContainingSymbol
        {
            get { return _containingSymbol; }
        }

        public override TypeKind TypeKind
        {
            get { return TypeKind.Delegate; }
        }

        internal override MethodSymbol Constructor
        {
            get { return _constructor; }
        }

        public override IEnumerable<string> MemberNames
        {
            get { return new[] { _constructor.Name, _invoke.Name }; }
        }

        public override ImmutableArray<Symbol> GetMembers()
        {
            return ImmutableArray.Create<Symbol>(_constructor, _invoke);
        }

        public override ImmutableArray<Symbol> GetMembers(string name)
        {
            return
                (name == _constructor.Name) ? ImmutableArray.Create<Symbol>(_constructor) :
                (name == _invoke.Name) ? ImmutableArray.Create<Symbol>(_invoke) :
                ImmutableArray<Symbol>.Empty;
        }

        public override Accessibility DeclaredAccessibility
        {
            get { return Accessibility.Internal; }
        }

        public override bool IsSealed
        {
            get { return true; }
        }

        internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics
            => ContainingAssembly.GetSpecialType(SpecialType.System_MulticastDelegate);

        public sealed override bool AreLocalsZeroed
        {
            get { throw ExceptionUtilities.Unreachable; }
        }

        internal override bool IsRecord => false;
        internal override bool IsRecordStruct => false;
        internal override bool HasPossibleWellKnownCloneMethod() => false;
    }

    internal sealed class SynthesizedDelegateConstructor : SynthesizedInstanceConstructor
    {
        private readonly ImmutableArray<ParameterSymbol> _parameters;

        public SynthesizedDelegateConstructor(NamedTypeSymbol containingType, TypeSymbol objectType, TypeSymbol intPtrType)
            : base(containingType)
        {
            _parameters = ImmutableArray.Create<ParameterSymbol>(
               SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(objectType), 0, RefKind.None, "object"),
               SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(intPtrType), 1, RefKind.None, "method"));
        }

        public override ImmutableArray<ParameterSymbol> Parameters
        {
            get { return _parameters; }
        }
    }

    internal sealed class SynthesizedDelegateInvokeMethod : SynthesizedInstanceMethodSymbol
    {
        private readonly NamedTypeSymbol _containingType;

        internal SynthesizedDelegateInvokeMethod(NamedTypeSymbol containingType, ArrayBuilder<TypeWithAnnotations> parameterTypes, ArrayBuilder<RefKind> parameterRefKinds, TypeWithAnnotations returnType, RefKind refKind)
        {
            _containingType = containingType;

            var parameters = ArrayBuilder<ParameterSymbol>.GetInstance(parameterTypes.Count);
            for (int i = 0; i < parameterTypes.Count; i++)
            {
                parameters.Add(SynthesizedParameterSymbol.Create(this, parameterTypes[i], i, parameterRefKinds[i]));
            }
            Parameters = parameters.ToImmutableAndFree();
            ReturnTypeWithAnnotations = returnType;
            RefKind = refKind;
        }

        public override string Name
        {
            get { return WellKnownMemberNames.DelegateInvokeName; }
        }

        internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
        {
            return true;
        }

        internal override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
        {
            return true;
        }

        internal override bool IsMetadataFinal
        {
            get
            {
                return false;
            }
        }

        public override MethodKind MethodKind
        {
            get { return MethodKind.DelegateInvoke; }
        }

        public override int Arity
        {
            get { return 0; }
        }

        public override bool IsExtensionMethod
        {
            get { return false; }
        }

        internal override bool HasSpecialName
        {
            get { return false; }
        }

        internal override System.Reflection.MethodImplAttributes ImplementationAttributes
        {
            get { return System.Reflection.MethodImplAttributes.Runtime; }
        }

        internal override bool HasDeclarativeSecurity
        {
            get { return false; }
        }

        public override DllImportData? GetDllImportData()
        {
            return null;
        }

        internal override IEnumerable<Microsoft.Cci.SecurityAttribute> GetSecurityInformation()
        {
            throw ExceptionUtilities.Unreachable;
        }

        internal override MarshalPseudoCustomAttributeData? ReturnValueMarshallingInformation
        {
            get { return null; }
        }

        internal override bool RequiresSecurityObject
        {
            get { return false; }
        }

        public override bool HidesBaseMethodsByName
        {
            get { return false; }
        }

        public override bool IsVararg
        {
            get { return false; }
        }

        public override bool ReturnsVoid
        {
            get { return ReturnType.IsVoidType(); }
        }

        public override bool IsAsync
        {
            get { return false; }
        }

        public override RefKind RefKind { get; }

        public override TypeWithAnnotations ReturnTypeWithAnnotations { get; }

        public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

        public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

        public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations
        {
            get { return ImmutableArray<TypeWithAnnotations>.Empty; }
        }

        public override ImmutableArray<TypeParameterSymbol> TypeParameters
        {
            get { return ImmutableArray<TypeParameterSymbol>.Empty; }
        }

        public override ImmutableArray<ParameterSymbol> Parameters { get; }

        public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations
        {
            get { return ImmutableArray<MethodSymbol>.Empty; }
        }

        public override ImmutableArray<CustomModifier> RefCustomModifiers
        {
            get { return ImmutableArray<CustomModifier>.Empty; }
        }

        public override Symbol? AssociatedSymbol
        {
            get { return null; }
        }

        internal override ImmutableArray<string> GetAppliedConditionalSymbols()
        {
            return ImmutableArray<string>.Empty;
        }

        internal override Microsoft.Cci.CallingConvention CallingConvention
        {
            get { return Microsoft.Cci.CallingConvention.HasThis; }
        }

        internal override bool GenerateDebugInfo
        {
            get { return false; }
        }

        public override Symbol ContainingSymbol
        {
            get { return _containingType; }
        }

        public override ImmutableArray<Location> Locations
        {
            get { return ImmutableArray<Location>.Empty; }
        }

        public override Accessibility DeclaredAccessibility
        {
            get
            {
                // Invoke method of a delegate used in a dynamic call-site must be public 
                // since the DLR looks only for public Invoke methods:
                return Accessibility.Public;
            }
        }

        public override bool IsStatic
        {
            get { return false; }
        }

        public override bool IsVirtual
        {
            get { return true; }
        }

        public override bool IsOverride
        {
            get { return false; }
        }

        public override bool IsAbstract
        {
            get { return false; }
        }

        public override bool IsSealed
        {
            get { return false; }
        }

        public override bool IsExtern
        {
            get { return false; }
        }
    }
}
