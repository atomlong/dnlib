﻿// dnlib: See LICENSE.txt for more info

using System;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed {
	sealed class DbiVariable : SymbolVariable {
		public override string Name { get { return name; } }
		string name;

		public override PdbLocalAttributes Attributes { get { return attributes; } }
		PdbLocalAttributes attributes;

		public override int Index { get { return index; } }
		int index;

        public override PdbCustomDebugInfo[] CustomDebugInfos { get { return Array2.Empty<PdbCustomDebugInfo>(); } }

		public void Read(ref DataReader reader) {
			index = reader.ReadInt32();
			reader.Position += 10;
			attributes = GetAttributes(reader.ReadUInt16());
			name = PdbReader.ReadCString(ref reader);
		}

		static PdbLocalAttributes GetAttributes(uint flags) {
			PdbLocalAttributes res = 0;
			const int fCompGenx = 4;
			if ((flags & fCompGenx) != 0)
				res |= PdbLocalAttributes.DebuggerHidden;
			return res;
		}
	}
}
