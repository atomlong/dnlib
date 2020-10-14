// dnlib: See LICENSE.txt for more info

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet {
	/// <summary>
	/// A high-level representation of a row in the InterfaceImpl table
	/// </summary>
	[DebuggerDisplay("{Interface}")]
	public abstract class InterfaceImpl : IHasCustomAttribute, IContainsGenericParameter, IHasCustomDebugInformation {
		/// <summary>
		/// The row id in its table
		/// </summary>
		protected uint rid;

		/// <inheritdoc/>
		public MDToken MDToken { get { return new MDToken(Table.InterfaceImpl, rid); } }

		/// <inheritdoc/>
		public uint Rid {
			get { return rid; }
			set { rid = value; }
		}

		/// <inheritdoc/>
		public int HasCustomAttributeTag { get { return 5; } }

		/// <summary>
		/// From column InterfaceImpl.Interface
		/// </summary>
		public ITypeDefOrRef Interface {
			get { return @interface; }
			set { @interface = value; }
		}
		/// <summary/>
		protected ITypeDefOrRef @interface;

		/// <summary>
		/// Gets all custom attributes
		/// </summary>
		public CustomAttributeCollection CustomAttributes {
			get {
				if (customAttributes == null)
					InitializeCustomAttributes();
				return customAttributes;
			}
		}
		/// <summary/>
		protected CustomAttributeCollection customAttributes;
		/// <summary>Initializes <see cref="customAttributes"/></summary>
		protected virtual void InitializeCustomAttributes() {
			Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
        }

		/// <inheritdoc/>
		public bool HasCustomAttributes { get { return CustomAttributes.Count > 0; } }

		/// <inheritdoc/>
		public int HasCustomDebugInformationTag { get { return 5; } }

		/// <inheritdoc/>
		public bool HasCustomDebugInfos { get { return CustomDebugInfos.Count > 0; } }

		/// <summary>
		/// Gets all custom debug infos
		/// </summary>
		public IList<PdbCustomDebugInfo> CustomDebugInfos {
			get {
				if (customDebugInfos == null)
					InitializeCustomDebugInfos();
				return customDebugInfos;
			}
		}
		/// <summary/>
		protected IList<PdbCustomDebugInfo> customDebugInfos;
		/// <summary>Initializes <see cref="customDebugInfos"/></summary>
		protected virtual void InitializeCustomDebugInfos() {
			Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
        }

		bool IContainsGenericParameter.ContainsGenericParameter { get { return TypeHelper.ContainsGenericParameter(this); } }
	}

	/// <summary>
	/// An InterfaceImpl row created by the user and not present in the original .NET file
	/// </summary>
	public class InterfaceImplUser : InterfaceImpl {
		/// <summary>
		/// Default constructor
		/// </summary>
		public InterfaceImplUser() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="interface">The interface the type implements</param>
		public InterfaceImplUser(ITypeDefOrRef @interface) { this.@interface = @interface; }
	}

	/// <summary>
	/// Created from a row in the InterfaceImpl table
	/// </summary>
	sealed class InterfaceImplMD : InterfaceImpl, IMDTokenProviderMD, IContainsGenericParameter2 {
		/// <summary>The module where this instance is located</summary>
		readonly ModuleDefMD readerModule;

		readonly uint origRid;
		readonly GenericParamContext gpContext;

		/// <inheritdoc/>
        public uint OrigRid { get { return origRid; } }

		bool IContainsGenericParameter2.ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

		/// <inheritdoc/>
		protected override void InitializeCustomAttributes() {
			var list = readerModule.Metadata.GetCustomAttributeRidList(Table.InterfaceImpl, origRid);
			var tmp = new CustomAttributeCollection(list.Count, list, (list2, index) => readerModule.ReadCustomAttribute(list[index]));
			Interlocked.CompareExchange(ref customAttributes, tmp, null);
		}

		/// <inheritdoc/>
		protected override void InitializeCustomDebugInfos() {
			var list = new List<PdbCustomDebugInfo>();
			readerModule.InitializeCustomDebugInfos(new MDToken(MDToken.Table, origRid), gpContext, list);
			Interlocked.CompareExchange(ref customDebugInfos, list, null);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="readerModule">The module which contains this <c>InterfaceImpl</c> row</param>
		/// <param name="rid">Row ID</param>
		/// <param name="gpContext">Generic parameter context</param>
		/// <exception cref="ArgumentNullException">If <paramref name="readerModule"/> is <c>null</c></exception>
		/// <exception cref="ArgumentException">If <paramref name="rid"/> is invalid</exception>
		public InterfaceImplMD(ModuleDefMD readerModule, uint rid, GenericParamContext gpContext) {
#if DEBUG
			if (readerModule == null)
				throw new ArgumentNullException("readerModule");
			if (readerModule.TablesStream.InterfaceImplTable.IsInvalidRID(rid))
				throw new BadImageFormatException( string.Format( "InterfaceImpl rid {0} does not exist", rid ) );
#endif
			origRid = rid;
			this.rid = rid;
			this.readerModule = readerModule;
			this.gpContext = gpContext;
            RawInterfaceImplRow row;
            bool b = readerModule.TablesStream.TryReadInterfaceImplRow(origRid, out row);
			Debug.Assert(b);
			@interface = readerModule.ResolveTypeDefOrRef(row.Interface, gpContext);
		}
	}
}
