﻿// dnlib: See LICENSE.txt for more info

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace dnlib.Utils {
	sealed class CollectionDebugView<TValue> {
		readonly ICollection<TValue> list;
		public CollectionDebugView(ICollection<TValue> list) { if (list != null) this.list = list; else throw new ArgumentNullException("list"); }

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public TValue[] Items {
			get {
				var array = new TValue[list.Count];
				list.CopyTo(array, 0);
				return array;
			}
		}
	}
}
