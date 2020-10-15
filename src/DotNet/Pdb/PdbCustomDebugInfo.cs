// dnlib: See LICENSE.txt for more info

using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb {
	/// <summary>
	/// Custom debug info kind
	/// </summary>
	/// <remarks>See <c>CustomDebugInfoKind</c> in Roslyn source code</remarks>
	public enum PdbCustomDebugInfoKind {
		/// <summary>
		/// <see cref="PdbUsingGroupsCustomDebugInfo"/>
		/// </summary>
		UsingGroups,

		/// <summary>
		/// <see cref="PdbForwardMethodInfoCustomDebugInfo"/>
		/// </summary>
		ForwardMethodInfo,

		/// <summary>
		/// <see cref="PdbForwardModuleInfoCustomDebugInfo"/>
		/// </summary>
		ForwardModuleInfo,

		/// <summary>
		/// <see cref="PdbStateMachineHoistedLocalScopesCustomDebugInfo"/>
		/// </summary>
		StateMachineHoistedLocalScopes,

		/// <summary>
		/// <see cref="PdbStateMachineTypeNameCustomDebugInfo"/>
		/// </summary>
		StateMachineTypeName,

		/// <summary>
		/// <see cref="PdbDynamicLocalsCustomDebugInfo"/>
		/// </summary>
		DynamicLocals,

		/// <summary>
		/// <see cref="PdbEditAndContinueLocalSlotMapCustomDebugInfo"/>
		/// </summary>
		EditAndContinueLocalSlotMap,

		/// <summary>
		/// <see cref="PdbEditAndContinueLambdaMapCustomDebugInfo"/>
		/// </summary>
		EditAndContinueLambdaMap,

		/// <summary>
		/// <see cref="PdbTupleElementNamesCustomDebugInfo"/>
		/// </summary>
		TupleElementNames,

		// Values 0x00-0xFF are reserved for Windows PDB CDIs.

		/// <summary>
		/// Unknown
		/// </summary>
		Unknown = int.MinValue,

		/// <summary>
		/// <see cref="PortablePdbTupleElementNamesCustomDebugInfo"/>
		/// </summary>
		TupleElementNames_PortablePdb,

		/// <summary>
		/// <see cref="PdbDefaultNamespaceCustomDebugInfo"/>
		/// </summary>
		DefaultNamespace,

		/// <summary>
		/// <see cref="PdbDynamicLocalVariablesCustomDebugInfo"/>
		/// </summary>
		DynamicLocalVariables,

		/// <summary>
		/// <see cref="PdbEmbeddedSourceCustomDebugInfo"/>
		/// </summary>
		EmbeddedSource,

		/// <summary>
		/// <see cref="PdbSourceLinkCustomDebugInfo"/>
		/// </summary>
		SourceLink,

		/// <summary>
		/// <see cref="PdbSourceServerCustomDebugInfo"/>
		/// </summary>
		SourceServer,

		/// <summary>
		/// <see cref="PdbAsyncMethodCustomDebugInfo"/>
		/// </summary>
		AsyncMethod,

		/// <summary>
		/// <see cref="PdbIteratorMethodCustomDebugInfo"/>
		/// </summary>
		IteratorMethod,

		/// <summary>
		/// <see cref="PdbCompilationMetadataReferencesCustomDebugInfo"/>
		/// </summary>
		CompilationMetadataReferences,

		/// <summary>
		/// <see cref="PdbCompilationOptionsCustomDebugInfo"/>
		/// </summary>
		CompilationOptions,
	}

	/// <summary>
	/// Base class of custom debug info added to the PDB file by the compiler
	/// </summary>
	public abstract class PdbCustomDebugInfo {
		/// <summary>
		/// Gets the custom debug info kind
		/// </summary>
		public abstract PdbCustomDebugInfoKind Kind { get; }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public abstract Guid Guid { get; }
	}

	/// <summary>
	/// Unknown custom debug info. If you see an instance of this class, you're using an old dnlib version or
	/// dnlib hasn't been updated to support this new custom debug info kind.
	/// </summary>
	public sealed class PdbUnknownCustomDebugInfo : PdbCustomDebugInfo {
		readonly PdbCustomDebugInfoKind kind;
		readonly Guid guid;
		readonly byte[] data;

		/// <summary>
		/// Gets the custom debug info kind
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return kind; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return guid; } }

		/// <summary>
		/// Gets the data
		/// </summary>
		public byte[] Data { get { return data; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="kind">Custom debug info kind</param>
		/// <param name="data">Raw custom debug info data</param>
		public PdbUnknownCustomDebugInfo(PdbCustomDebugInfoKind kind, byte[] data) {
			this.kind = kind;
            if (data != null) this.data = data; else throw new ArgumentNullException("data");
			guid = Guid.Empty;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="guid">Custom debug info guid</param>
		/// <param name="data">Raw custom debug info data</param>
		public PdbUnknownCustomDebugInfo(Guid guid, byte[] data) {
			kind = PdbCustomDebugInfoKind.Unknown;
            if (data != null) this.data = data; else throw new ArgumentNullException("data");
			this.guid = guid;
		}
	}

	/// <summary>
	/// Contains sizes of using groups
	/// </summary>
	public sealed class PdbUsingGroupsCustomDebugInfo : PdbCustomDebugInfo {
		readonly IList<ushort> usingCounts;

		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.UsingGroups"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.UsingGroups; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return Guid.Empty; } }

		/// <summary>
		/// Gets the using counts
		/// </summary>
		public IList<ushort> UsingCounts { get { return usingCounts; } }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbUsingGroupsCustomDebugInfo() { usingCounts = new List<ushort>(); }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="capacity">Initial capacity of <see cref="UsingCounts"/></param>
		public PdbUsingGroupsCustomDebugInfo(int capacity) { usingCounts = new List<ushort>(capacity); }
	}

	/// <summary>
	/// Contains a reference to another method that contains the import strings
	/// </summary>
	public sealed class PdbForwardMethodInfoCustomDebugInfo : PdbCustomDebugInfo {
		IMethodDefOrRef method;

		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.ForwardMethodInfo"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.ForwardMethodInfo; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return Guid.Empty; } }

		/// <summary>
		/// Gets/sets the referenced method
		/// </summary>
		public IMethodDefOrRef Method {
			get { return method; }
			set { method = value; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbForwardMethodInfoCustomDebugInfo() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="method">The referenced method</param>
		public PdbForwardMethodInfoCustomDebugInfo(IMethodDefOrRef method) { this.method = method; }
	}

	/// <summary>
	/// Contains a reference to another method that contains the per-module debug info (assembly reference aliases)
	/// </summary>
	public sealed class PdbForwardModuleInfoCustomDebugInfo : PdbCustomDebugInfo {
		IMethodDefOrRef method;

		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.ForwardModuleInfo"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.ForwardModuleInfo; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return Guid.Empty; } }

		/// <summary>
		/// Gets/sets the referenced method
		/// </summary>
		public IMethodDefOrRef Method {
			get { return method; }
			set { method = value; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbForwardModuleInfoCustomDebugInfo() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="method">The referenced method</param>
		public PdbForwardModuleInfoCustomDebugInfo(IMethodDefOrRef method) { this.method = method; }
	}

	/// <summary>
	/// State machine hosted local scope info
	/// </summary>
	public struct StateMachineHoistedLocalScope {
		/// <summary>
		/// true if it's a syntesized local (<see cref="Start"/> and <see cref="End"/> are both null)
		/// </summary>
		public bool IsSynthesizedLocal { get { return Start == null && End == null; } }

		/// <summary>
		/// The instruction of the first operation in the scope. Can be null if it's a synthesized local
		/// </summary>
		public Instruction Start;

		/// <summary>
		/// The instruction of the first operation outside of the scope or null if it ends at the last instruction in the body.
		/// Can also be null if it's a synthesized local (in which case <see cref="Start"/> is also null, see <see cref="IsSynthesizedLocal"/>)
		/// </summary>
		public Instruction End;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="start">Start of the scope</param>
		/// <param name="end">First instruction after the end of the scope</param>
		public StateMachineHoistedLocalScope(Instruction start, Instruction end) {
			Start = start;
			End = end;
		}
	}

	/// <summary>
	/// Contains local scopes for state machine hoisted local variables.
	/// </summary>
	public sealed class PdbStateMachineHoistedLocalScopesCustomDebugInfo : PdbCustomDebugInfo {
		readonly IList<StateMachineHoistedLocalScope> scopes;

		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.StateMachineHoistedLocalScopes"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.StateMachineHoistedLocalScopes; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.StateMachineHoistedLocalScopes; } }

		/// <summary>
		/// Gets the scopes
		/// </summary>
		public IList<StateMachineHoistedLocalScope> Scopes { get { return scopes; } }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbStateMachineHoistedLocalScopesCustomDebugInfo() { scopes = new List<StateMachineHoistedLocalScope>(); }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="capacity">Initial capacity of <see cref="Scopes"/></param>
		public PdbStateMachineHoistedLocalScopesCustomDebugInfo(int capacity) { scopes = new List<StateMachineHoistedLocalScope>(capacity); }
	}

	/// <summary>
	/// Contains the state machine type
	/// </summary>
	public sealed class PdbStateMachineTypeNameCustomDebugInfo : PdbCustomDebugInfo {
		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.StateMachineTypeName"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.StateMachineTypeName; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return Guid.Empty; } }

		/// <summary>
		/// Gets/sets the state machine type
		/// </summary>
		public TypeDef Type { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbStateMachineTypeNameCustomDebugInfo() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type">State machine type</param>
		public PdbStateMachineTypeNameCustomDebugInfo(TypeDef type) { Type = type; }
	}

	/// <summary>
	/// Contains dynamic flags for local variables and constants
	/// </summary>
	public sealed class PdbDynamicLocalsCustomDebugInfo : PdbCustomDebugInfo {
		readonly IList<PdbDynamicLocal> locals;

		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.DynamicLocals"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.DynamicLocals; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return Guid.Empty; } }

		/// <summary>
		/// Gets the dynamic locals
		/// </summary>
		public IList<PdbDynamicLocal> Locals { get { return locals; } }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbDynamicLocalsCustomDebugInfo() { locals = new List<PdbDynamicLocal>(); }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="capacity">Initial capacity of <see cref="Locals"/></param>
		public PdbDynamicLocalsCustomDebugInfo(int capacity) { locals = new List<PdbDynamicLocal>(capacity); }
	}

	/// <summary>
	/// Dynamic local info
	/// </summary>
	public sealed class PdbDynamicLocal {
		readonly IList<byte> flags;
		string name;
		Local local;

		/// <summary>
		/// Gets the dynamic flags
		/// </summary>
		public IList<byte> Flags { get { return flags; } }

		/// <summary>
		/// Gets/sets the name of the local. The name must have at most 64 characters and no char can be NUL (0x0000).
		/// If null is written, <see cref="dnlib.DotNet.Emit.Local.Name"/> is returned instead.
		/// </summary>
		public string Name {
			get {
				var n = name;
				if (!(n == null))
					return n;
				return local != null?local.Name:null;
			}
			set { name = value; }
		}

		/// <summary>
		/// true if it's a constant and not a variable (<see cref="Local"/> == null)
		/// </summary>
		public bool IsConstant { get { return Local == null; } }

		/// <summary>
		/// true if it's a variable (<see cref="Local"/> is not null)
		/// </summary>
		public bool IsVariable { get { return Local != null; } }

		/// <summary>
		/// Gets/sets the local. Could be null if there's no local (it's a 'const' local).
		/// </summary>
		public Local Local {
			get { return local; }
			set { local = value; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbDynamicLocal() { flags = new List<byte>(); }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="capacity">Initial capacity of <see cref="Flags"/></param>
		public PdbDynamicLocal(int capacity) { flags = new List<byte>(capacity); }
	}

	/// <summary>
	/// Contains the EnC local variable slot map
	/// </summary>
	public sealed class PdbEditAndContinueLocalSlotMapCustomDebugInfo : PdbCustomDebugInfo {
		readonly byte[] data;

		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.EditAndContinueLocalSlotMap"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.EditAndContinueLocalSlotMap; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.EncLocalSlotMap; } }

		/// <summary>
		/// Gets the data. Spec: https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/specs/PortablePdb-Metadata.md#EditAndContinueLocalSlotMap
		/// </summary>
		public byte[] Data { get { return data; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="data">Raw custom debug info data</param>
		public PdbEditAndContinueLocalSlotMapCustomDebugInfo(byte[] data) { if (data != null) this.data = data; else throw new ArgumentNullException("data"); }
	}

	/// <summary>
	/// Contains the EnC lambda map
	/// </summary>
	public sealed class PdbEditAndContinueLambdaMapCustomDebugInfo : PdbCustomDebugInfo {
		readonly byte[] data;

		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.EditAndContinueLambdaMap"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.EditAndContinueLambdaMap; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.EncLambdaAndClosureMap; } }

		/// <summary>
		/// Gets the data. Spec: https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/specs/PortablePdb-Metadata.md#EditAndContinueLambdaAndClosureMap
		/// </summary>
		public byte[] Data { get { return data; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="data">Raw custom debug info data</param>
		public PdbEditAndContinueLambdaMapCustomDebugInfo(byte[] data) { if (data != null) this.data = data; else throw new ArgumentNullException("data"); }
	}

	/// <summary>
	/// Contains tuple element names for local variables and constants
	/// </summary>
	public sealed class PdbTupleElementNamesCustomDebugInfo : PdbCustomDebugInfo {
		readonly IList<PdbTupleElementNames> names;

		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.TupleElementNames"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.TupleElementNames; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return Guid.Empty; } }

		/// <summary>
		/// Gets the tuple element names
		/// </summary>
		public IList<PdbTupleElementNames> Names { get { return names; } }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbTupleElementNamesCustomDebugInfo() { names = new List<PdbTupleElementNames>(); }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="capacity">Initial capacity of <see cref="Names"/></param>
		public PdbTupleElementNamesCustomDebugInfo(int capacity) { names = new List<PdbTupleElementNames>(capacity); }
	}

	/// <summary>
	/// Tuple element name info
	/// </summary>
	public sealed class PdbTupleElementNames {
		readonly IList<string> tupleElementNames;
		string name;
		Local local;
		Instruction scopeStart, scopeEnd;

		/// <summary>
		/// Gets/sets the name of the local. If null is written, <see cref="dnlib.DotNet.Emit.Local.Name"/> is returned instead.
		/// </summary>
		public string Name {
			get {
				var n = name;
				if (!(n == null))
					return n;
				return local != null?local.Name:null;
			}
			set { name = value; }
		}

		/// <summary>
		/// Gets/sets the local. It's null if it's a constant, and non-null if it's a variable
		/// </summary>
		public Local Local {
			get { return local; }
			set { local = value; }
		}

		/// <summary>
		/// true if it's a constant. Constants have a scope (<see cref="ScopeStart"/> and <see cref="ScopeEnd"/>)
		/// </summary>
		public bool IsConstant { get { return local == null; } }

		/// <summary>
		/// true if it's a variable. Variables don't have a scope (<see cref="ScopeStart"/> and <see cref="ScopeEnd"/>)
		/// </summary>
		public bool IsVariable { get { return local != null; } }

		/// <summary>
		/// Gets/sets the start of the scope or null. Only constants have a scope.
		/// </summary>
		public Instruction ScopeStart {
			get { return scopeStart; }
			set { scopeStart = value; }
		}

		/// <summary>
		/// Gets/sets the end of the scope or null if it has no scope or if the scope ends at the end of the body. Only constants have a scope.
		/// </summary>
		public Instruction ScopeEnd {
			get { return scopeEnd; }
			set { scopeEnd = value; }
		}

		/// <summary>
		/// Gets the tuple element names
		/// </summary>
		public IList<string> TupleElementNames { get { return tupleElementNames; } }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbTupleElementNames() { tupleElementNames = new List<string>(); }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="capacity">Initial capacity of <see cref="TupleElementNames"/></param>
		public PdbTupleElementNames(int capacity) { tupleElementNames = new List<string>(capacity); }
	}

	/// <summary>
	/// Contains tuple element names for local variables and constants
	/// </summary>
	public sealed class PortablePdbTupleElementNamesCustomDebugInfo : PdbCustomDebugInfo {
		readonly IList<string> names;

		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.TupleElementNames_PortablePdb"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.TupleElementNames_PortablePdb; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.TupleElementNames; } }

		/// <summary>
		/// Gets the tuple element names
		/// </summary>
		public IList<string> Names { get { return names; } }

		/// <summary>
		/// Constructor
		/// </summary>
		public PortablePdbTupleElementNamesCustomDebugInfo() { names = new List<string>(); }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="capacity">Initial capacity of <see cref="Names"/></param>
		public PortablePdbTupleElementNamesCustomDebugInfo(int capacity) { names = new List<string>(capacity); }
	}

	/// <summary>
	/// Async method stepping info
	/// 
	/// It's internal and translated to a <see cref="PdbAsyncMethodCustomDebugInfo"/>
	/// </summary>
	sealed class PdbAsyncMethodSteppingInformationCustomDebugInfo : PdbCustomDebugInfo {
		readonly IList<PdbAsyncStepInfo> asyncStepInfos;

		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.Unknown"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.Unknown; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.AsyncMethodSteppingInformationBlob; } }

		/// <summary>
		/// Gets the catch handler instruction or null
		/// </summary>
		public Instruction CatchHandler { get; set; }

		/// <summary>
		/// Gets all async step infos
		/// </summary>
		public IList<PdbAsyncStepInfo> AsyncStepInfos { get { return asyncStepInfos; } }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbAsyncMethodSteppingInformationCustomDebugInfo() { asyncStepInfos = new List<PdbAsyncStepInfo>(); }
	}

	/// <summary>
	/// Default namespace
	/// </summary>
	public sealed class PdbDefaultNamespaceCustomDebugInfo : PdbCustomDebugInfo {
		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.DefaultNamespace"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.DefaultNamespace; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.DefaultNamespace; } }

		/// <summary>
		/// Gets the default namespace
		/// </summary>
		public string Namespace { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbDefaultNamespaceCustomDebugInfo() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="defaultNamespace">Default namespace</param>
		public PdbDefaultNamespaceCustomDebugInfo(string defaultNamespace) { Namespace = defaultNamespace; }
	}

	/// <summary>
	/// Dynamic flags
	/// </summary>
	public sealed class PdbDynamicLocalVariablesCustomDebugInfo : PdbCustomDebugInfo {
		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.DynamicLocalVariables"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.DynamicLocalVariables; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.DynamicLocalVariables; } }

		/// <summary>
		/// Gets/sets the dynamic flags
		/// </summary>
		public bool[] Flags { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbDynamicLocalVariablesCustomDebugInfo() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="flags">Dynamic flags</param>
		public PdbDynamicLocalVariablesCustomDebugInfo(bool[] flags) { Flags = flags; }
	}

	/// <summary>
	/// Contains the source code
	/// </summary>
	public sealed class PdbEmbeddedSourceCustomDebugInfo : PdbCustomDebugInfo {
		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.EmbeddedSource"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.EmbeddedSource; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.EmbeddedSource; } }

		/// <summary>
		/// Gets the source code blob.
		/// 
		/// It's not decompressed and converted to a string because the encoding isn't specified.
		/// 
		/// https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/specs/PortablePdb-Metadata.md#embedded-source-c-and-vb-compilers
		/// </summary>
		public byte[] SourceCodeBlob { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbEmbeddedSourceCustomDebugInfo() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sourceCodeBlob">Source code blob</param>
		public PdbEmbeddedSourceCustomDebugInfo(byte[] sourceCodeBlob) { SourceCodeBlob = sourceCodeBlob; }
	}

	/// <summary>
	/// Contains the source link file
	/// </summary>
	public sealed class PdbSourceLinkCustomDebugInfo : PdbCustomDebugInfo {
		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.SourceLink"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.SourceLink; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.SourceLink; } }

		/// <summary>
		/// Gets the source link file contents
		/// </summary>
		public byte[] FileBlob { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbSourceLinkCustomDebugInfo() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fileBlob">Source link file contents</param>
		public PdbSourceLinkCustomDebugInfo(byte[] fileBlob) { FileBlob = fileBlob; }
	}

	/// <summary>
	/// Contains the source server file
	/// </summary>
	public sealed class PdbSourceServerCustomDebugInfo : PdbCustomDebugInfo {
		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.SourceServer"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.SourceServer; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return Guid.Empty; } }

		/// <summary>
		/// Gets the source server file contents
		/// </summary>
		public byte[] FileBlob { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbSourceServerCustomDebugInfo() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fileBlob">Source server file contents</param>
		public PdbSourceServerCustomDebugInfo(byte[] fileBlob) { FileBlob = fileBlob; }
	}

	/// <summary>
	/// Async method info
	/// </summary>
	public sealed class PdbAsyncMethodCustomDebugInfo : PdbCustomDebugInfo {
		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.AsyncMethod"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.AsyncMethod; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return Guid.Empty; } }

		readonly IList<PdbAsyncStepInfo> asyncStepInfos;

		/// <summary>
		/// Gets/sets the starting method that initiates the async operation
		/// </summary>
		public MethodDef KickoffMethod { get; set; }

		/// <summary>
		/// Gets/sets the instruction for the compiler generated catch handler that wraps an async method.
		/// This can be null.
		/// </summary>
		public Instruction CatchHandlerInstruction { get; set; }

		/// <summary>
		/// Gets all step infos used by the debugger
		/// </summary>
		public IList<PdbAsyncStepInfo> StepInfos { get { return asyncStepInfos; } }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbAsyncMethodCustomDebugInfo() { asyncStepInfos = new List<PdbAsyncStepInfo>(); }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="stepInfosCapacity">Default capacity for <see cref="StepInfos"/></param>
		public PdbAsyncMethodCustomDebugInfo(int stepInfosCapacity) { asyncStepInfos = new List<PdbAsyncStepInfo>(stepInfosCapacity); }
	}

	/// <summary>
	/// Async step info used by debuggers
	/// </summary>
	public struct PdbAsyncStepInfo {
		/// <summary>
		/// The yield instruction
		/// </summary>
		public Instruction YieldInstruction;

		/// <summary>
		/// Resume method
		/// </summary>
		public MethodDef BreakpointMethod;

		/// <summary>
		/// Resume instruction (where the debugger puts a breakpoint)
		/// </summary>
		public Instruction BreakpointInstruction;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="yieldInstruction">The yield instruction</param>
		/// <param name="breakpointMethod">Resume method</param>
		/// <param name="breakpointInstruction">Resume instruction (where the debugger puts a breakpoint)</param>
		public PdbAsyncStepInfo(Instruction yieldInstruction, MethodDef breakpointMethod, Instruction breakpointInstruction) {
			YieldInstruction = yieldInstruction;
			BreakpointMethod = breakpointMethod;
			BreakpointInstruction = breakpointInstruction;
		}
	}

	/// <summary>
	/// Iterator method
	/// </summary>
	public sealed class PdbIteratorMethodCustomDebugInfo : PdbCustomDebugInfo {
		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.IteratorMethod"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.IteratorMethod; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return Guid.Empty; } }

		/// <summary>
		/// Gets the kickoff method
		/// </summary>
		public MethodDef KickoffMethod { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbIteratorMethodCustomDebugInfo() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="kickoffMethod">Kickoff method</param>
        public PdbIteratorMethodCustomDebugInfo(MethodDef kickoffMethod) { KickoffMethod = kickoffMethod; }
	}

	/// <summary>
	/// Compilation metadata references
	/// </summary>
	public sealed class PdbCompilationMetadataReferencesCustomDebugInfo : PdbCustomDebugInfo {
		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.CompilationMetadataReferences"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.CompilationMetadataReferences; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.CompilationMetadataReferences; } }

		/// <summary>
		/// Gets all references
		/// </summary>
        public List<PdbCompilationMetadataReference> References;

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbCompilationMetadataReferencesCustomDebugInfo() { References = new List<PdbCompilationMetadataReference>(); }
	}

	/// <summary>
	/// Compilation metadata reference flags, see https://github.com/dotnet/roslyn/blob/master/docs/features/pdb-compilation-options.md
	/// </summary>
	[Flags]
	public enum PdbCompilationMetadataReferenceFlags : byte {
		/// <summary>
		/// No bit is set
		/// </summary>
		None					= 0,

		/// <summary>
		/// Set if it's an assembly reference, clear if it's a module reference
		/// </summary>
		Assembly				= 0x01,

		/// <summary>
		/// EmbedInteropTypes was enabled
		/// </summary>
		EmbedInteropTypes		= 0x02,
	}

	/// <summary>
	/// A compilation metadata reference
	/// </summary>
	public sealed class PdbCompilationMetadataReference {
		/// <summary>
		/// Name of the reference (eg. filename)
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Aliases (or an empty string), separated with commas
		/// </summary>
		public string Aliases { get; set; }

		/// <summary>
		/// Gets the flags
		/// </summary>
		public PdbCompilationMetadataReferenceFlags Flags { get; set; }

		/// <summary>
		/// Gets the timestamp stored in the PE header
		/// </summary>
		public uint Timestamp { get; set; }

		/// <summary>
		/// Gets SizeOfImage stored in the PE header
		/// </summary>
		public uint SizeOfImage { get; set; }

		/// <summary>
		/// Gets the MVID stored in the .NET metadata
		/// </summary>
		public Guid Mvid { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbCompilationMetadataReference() {
			Name = string.Empty;
			Aliases = string.Empty;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of reference</param>
		/// <param name="aliases">Aliases (or an empty string), separated with commas</param>
		/// <param name="flags">Reference flags</param>
		/// <param name="timestamp">Timestamp in PE header</param>
		/// <param name="sizeOfImage">SizeOfImage in PE header</param>
		/// <param name="mvid">MVID stored in the .NET metadata</param>
		public PdbCompilationMetadataReference(string name, string aliases, PdbCompilationMetadataReferenceFlags flags, uint timestamp, uint sizeOfImage, Guid mvid) {
			Name = name;
			Aliases = aliases;
			Flags = flags;
			Timestamp = timestamp;
			SizeOfImage = sizeOfImage;
			Mvid = mvid;
		}
	}

	/// <summary>
	/// Compilation options
	/// </summary>
	public sealed class PdbCompilationOptionsCustomDebugInfo : PdbCustomDebugInfo {
		/// <summary>
		/// Returns <see cref="PdbCustomDebugInfoKind.CompilationOptions"/>
		/// </summary>
		public override PdbCustomDebugInfoKind Kind { get { return PdbCustomDebugInfoKind.CompilationOptions; } }

		/// <summary>
		/// Gets the custom debug info guid, see <see cref="CustomDebugInfoGuids"/>
		/// </summary>
		public override Guid Guid { get { return CustomDebugInfoGuids.CompilationOptions; } }

		/// <summary>
		/// Gets all compilation options, see https://github.com/dotnet/roslyn/blob/master/docs/features/pdb-compilation-options.md .
		/// Option names (key): see roslyn/src/Compilers/Core/Portable/PEWriter/CompilationOptionNames.cs
		/// </summary>
        public List<KeyValuePair<string, string>> Options;

		/// <summary>
		/// Constructor
		/// </summary>
        public PdbCompilationOptionsCustomDebugInfo() { Options = new List<KeyValuePair<string, string>>(); }
	}
}
