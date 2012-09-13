﻿using System;
using System.Collections.Generic;
using dot10.IO;
using dot10.PE;

namespace dot10.DotNet.MD {
	/// <summary>
	/// Used when a #- stream is present in the metadata
	/// </summary>
	class ENCMetaData : MetaData {
		static readonly UTF8String DeletedName = new UTF8String("_Deleted");
		bool hasMethodPtr, hasFieldPtr, hasParamPtr, hasEventPtr, hasPropertyPtr;
		bool hasDeletedRows;
		Dictionary<Table, SortedTable> sortedTables = new Dictionary<Table, SortedTable>();

		/// <summary>
		/// Sorts a table by key column
		/// </summary>
		class SortedTable {
			RowInfo[] rows;

			/// <summary>
			/// Remembers <c>rid</c> and key
			/// </summary>
			struct RowInfo : IComparable<RowInfo> {
				public uint rid;
				public uint key;

				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="rid">Row ID</param>
				/// <param name="key">Key</param>
				public RowInfo(uint rid, uint key) {
					this.rid = rid;
					this.key = key;
				}

				/// <inheritdoc/>
				public int CompareTo(RowInfo other) {
					if (key < other.key)
						return -1;
					if (key > other.key)
						return 1;
					return rid.CompareTo(other.rid);
				}
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="mdTable">The MD table</param>
			/// <param name="keyColIndex">Index of key column</param>
			public SortedTable(MDTable mdTable, int keyColIndex) {
				InitializeKeys(mdTable, keyColIndex);
				Array.Sort(rows);
			}

			void InitializeKeys(MDTable mdTable, int keyColIndex) {
				var keyColumn = mdTable.TableInfo.Columns[keyColIndex];
				rows = new RowInfo[mdTable.Rows + 1];
				var reader = mdTable.ImageStream;
				reader.Position = keyColumn.Offset;
				int increment = mdTable.TableInfo.RowSize - keyColumn.Size;
				for (uint i = 1; i <= mdTable.Rows; i++) {
					rows[i] = new RowInfo(i, keyColumn.Read(reader));
					reader.Position += increment;
				}
			}

			/// <summary>
			/// Binary searches for a row with a certain key
			/// </summary>
			/// <param name="key">The key</param>
			/// <returns>The row or 0 if not found</returns>
			int BinarySearch(uint key) {
				int lo = 1, hi = rows.Length - 1;
				while (lo <= hi) {
					int curr = (lo + hi) / 2;
					uint key2 = rows[curr].key;
					if (key == key2)
						return curr;
					if (key2 > key)
						hi = curr - 1;
					else
						lo = curr + 1;
				}

				return 0;
			}

			/// <summary>
			/// Find all rids that contain <paramref name="key"/>
			/// </summary>
			/// <param name="key">The key</param>
			/// <returns>A new <see cref="RidList"/> instance</returns>
			public RidList FindAllRows(uint key) {
				int startIndex = BinarySearch(key);
				if (startIndex == 0)
					return ContiguousRidList.Empty;
				int endIndex = startIndex + 1;
				for (; startIndex > 1; startIndex--) {
					if (key != rows[startIndex - 1].key)
						break;
				}
				for (; endIndex < rows.Length; endIndex++) {
					if (key != rows[endIndex].key)
						break;
				}
				var list = new RandomRidList(endIndex - startIndex);
				for (int i = startIndex; i < endIndex; i++)
					list.Add(rows[i].rid);
				return list;
			}
		}

		/// <inheritdoc/>
		public ENCMetaData(IPEImage peImage, ImageCor20Header cor20Header, MetaDataHeader mdHeader)
			: base(peImage, cor20Header, mdHeader) {
		}

		/// <inheritdoc/>
		public override void Initialize() {
			IImageStream imageStream = null;
			DotNetStream dns = null;
			try {
				var mdRva = cor20Header.MetaData.VirtualAddress;
				foreach (var sh in mdHeader.StreamHeaders) {
					var rva = mdRva + sh.Offset;
					imageStream = peImage.CreateStream(rva, sh.StreamSize);
					switch (sh.Name) {
					case "#Strings":
						if (stringsStream == null) {
							stringsStream = new StringsStream(imageStream, sh);
							imageStream = null;
							allStreams.Add(stringsStream);
							continue;
						}
						break;

					case "#US":
						if (usStream == null) {
							usStream = new USStream(imageStream, sh);
							imageStream = null;
							allStreams.Add(usStream);
							continue;
						}
						break;

					case "#Blob":
						if (blobStream == null) {
							blobStream = new BlobStream(imageStream, sh);
							imageStream = null;
							allStreams.Add(blobStream);
							continue;
						}
						break;

					case "#GUID":
						if (guidStream == null) {
							guidStream = new GuidStream(imageStream, sh);
							imageStream = null;
							allStreams.Add(guidStream);
							continue;
						}
						break;

					case "#-":
						if (tablesStream == null) {
							tablesStream = new ENCTablesStream(imageStream, sh);
							imageStream = null;
							allStreams.Add(tablesStream);
							continue;
						}
						break;
					}
					dns = new DotNetStream(imageStream, sh);
					imageStream = null;
					allStreams.Add(dns);
					dns = null;
				}

				if (tablesStream == null)
					throw new BadImageFormatException("Missing MD stream");
				tablesStream.Initialize(peImage);

				// The pointer tables are used iff row count != 0
				hasFieldPtr = !tablesStream.Get(Table.FieldPtr).IsEmpty;
				hasMethodPtr = !tablesStream.Get(Table.MethodPtr).IsEmpty;
				hasParamPtr = !tablesStream.Get(Table.ParamPtr).IsEmpty;
				hasEventPtr = !tablesStream.Get(Table.EventPtr).IsEmpty;
				hasPropertyPtr = !tablesStream.Get(Table.PropertyPtr).IsEmpty;
				hasDeletedRows = tablesStream.HasDelete;
			}
			finally {
				if (imageStream != null)
					imageStream.Dispose();
				if (dns != null)
					dns.Dispose();
			}
		}

		/// <summary>
		/// Converts a logical <c>Field</c> rid to a physical <c>Field</c> rid
		/// </summary>
		/// <param name="listRid">A valid rid</param>
		/// <returns>Converted rid or any invalid rid value if <paramref name="listRid"/> is invalid</returns>
		uint ToFieldRid(uint listRid) {
			if (!hasFieldPtr)
				return listRid;
			uint listValue;
			return tablesStream.ReadColumn(Table.FieldPtr, listRid, 0, out listValue) ? listValue : 0;
		}

		/// <summary>
		/// Converts a logical <c>Method</c> rid to a physical <c>Method</c> rid
		/// </summary>
		/// <param name="listRid">A valid rid</param>
		/// <returns>Converted rid or any invalid rid value if <paramref name="listRid"/> is invalid</returns>
		uint ToMethodRid(uint listRid) {
			if (!hasMethodPtr)
				return listRid;
			uint listValue;
			return tablesStream.ReadColumn(Table.MethodPtr, listRid, 0, out listValue) ? listValue : 0;
		}

		/// <summary>
		/// Converts a logical <c>Param</c> rid to a physical <c>Param</c> rid
		/// </summary>
		/// <param name="listRid">A valid rid</param>
		/// <returns>Converted rid or any invalid rid value if <paramref name="listRid"/> is invalid</returns>
		uint ToParamRid(uint listRid) {
			if (!hasParamPtr)
				return listRid;
			uint listValue;
			return tablesStream.ReadColumn(Table.ParamPtr, listRid, 0, out listValue) ? listValue : 0;
		}

		/// <summary>
		/// Converts a logical <c>Event</c> rid to a physical <c>Event</c> rid
		/// </summary>
		/// <param name="listRid">A valid rid</param>
		/// <returns>Converted rid or any invalid rid value if <paramref name="listRid"/> is invalid</returns>
		uint ToEventRid(uint listRid) {
			if (!hasEventPtr)
				return listRid;
			uint listValue;
			return tablesStream.ReadColumn(Table.EventPtr, listRid, 0, out listValue) ? listValue : 0;
		}

		/// <summary>
		/// Converts a logical <c>Property</c> rid to a physical <c>Property</c> rid
		/// </summary>
		/// <param name="listRid">A valid rid</param>
		/// <returns>Converted rid or any invalid rid value if <paramref name="listRid"/> is invalid</returns>
		uint ToPropertyRid(uint listRid) {
			if (!hasPropertyPtr)
				return listRid;
			uint listValue;
			return tablesStream.ReadColumn(Table.PropertyPtr, listRid, 0, out listValue) ? listValue : 0;
		}

		/// <inheritdoc/>
		public override RidList GetFieldRidList(uint typeDefRid) {
			var list = GetRidList(Table.TypeDef, typeDefRid, 4, Table.Field);
			if (list.Length == 0 || (!hasFieldPtr && !hasDeletedRows))
				return list;

			var destTable = tablesStream.Get(Table.Field);
			var newList = new RandomRidList((int)list.Length);
			for (uint i = 0; i < list.Length; i++) {
				var rid = ToFieldRid(list[i]);
				if (destTable.IsInvalidRID(rid))
					continue;
				if (hasDeletedRows) {
					// It's a deleted row if RTSpecialName is set and name is "_Deleted"
					var row = tablesStream.ReadFieldRow(rid);
					if (row == null)
						continue;	// Should never happen since rid is valid
					if ((row.Flags & (uint)FieldAttributes.RTSpecialName) != 0) {
						var name = stringsStream.Read(row.Name);
						if ((object)name != null && name == DeletedName)
							continue;	// ignore this deleted row
					}
				}
				// It's a valid non-deleted rid so add it
				newList.Add(rid);
			}
			return newList;
		}

		/// <inheritdoc/>
		public override RidList GetMethodRidList(uint typeDefRid) {
			var list = GetRidList(Table.TypeDef, typeDefRid, 5, Table.Method);
			if (list.Length == 0 || (!hasMethodPtr && !hasDeletedRows))
				return list;

			var destTable = tablesStream.Get(Table.Method);
			var newList = new RandomRidList((int)list.Length);
			for (uint i = 0; i < list.Length; i++) {
				var rid = ToMethodRid(list[i]);
				if (destTable.IsInvalidRID(rid))
					continue;
				if (hasDeletedRows) {
					// It's a deleted row if RTSpecialName is set and name is "_Deleted"
					var row = tablesStream.ReadMethodRow(rid);
					if (row == null)
						continue;	// Should never happen since rid is valid
					if ((row.Flags & (uint)MethodAttributes.RTSpecialName) != 0) {
						var name = stringsStream.Read(row.Name);
						if ((object)name != null && name == DeletedName)
							continue;	// ignore this deleted row
					}
				}
				// It's a valid non-deleted rid so add it
				newList.Add(rid);
			}
			return newList;
		}

		/// <inheritdoc/>
		public override RidList GetParamRidList(uint methodRid) {
			var list = GetRidList(Table.Method, methodRid, 5, Table.Param);
			if (list.Length == 0 || !hasParamPtr)
				return list;

			var destTable = tablesStream.Get(Table.Param);
			var newList = new RandomRidList((int)list.Length);
			for (uint i = 0; i < list.Length; i++) {
				var rid = ToParamRid(list[i]);
				if (destTable.IsInvalidRID(rid))
					continue;
				newList.Add(rid);
			}
			return newList;
		}

		/// <inheritdoc/>
		public override RidList GetEventRidList(uint eventMapRid) {
			var list = GetRidList(Table.EventMap, eventMapRid, 1, Table.Event);
			if (list.Length == 0 || (!hasEventPtr && !hasDeletedRows))
				return list;

			var destTable = tablesStream.Get(Table.Event);
			var newList = new RandomRidList((int)list.Length);
			for (uint i = 0; i < list.Length; i++) {
				var rid = ToEventRid(list[i]);
				if (destTable.IsInvalidRID(rid))
					continue;
				if (hasDeletedRows) {
					// It's a deleted row if RTSpecialName is set and name is "_Deleted"
					var row = tablesStream.ReadEventRow(rid);
					if (row == null)
						continue;	// Should never happen since rid is valid
					if ((row.EventFlags & (uint)EventAttributes.RTSpecialName) != 0) {
						var name = stringsStream.Read(row.Name);
						if ((object)name != null && name == DeletedName)
							continue;	// ignore this deleted row
					}
				}
				// It's a valid non-deleted rid so add it
				newList.Add(rid);
			}
			return newList;
		}

		/// <inheritdoc/>
		public override RidList GetPropertyRidList(uint propertyMapRid) {
			var list = GetRidList(Table.PropertyMap, propertyMapRid, 1, Table.Property);
			if (list.Length == 0 || (!hasPropertyPtr && !hasDeletedRows))
				return list;

			var destTable = tablesStream.Get(Table.Property);
			var newList = new RandomRidList((int)list.Length);
			for (uint i = 0; i < list.Length; i++) {
				var rid = ToPropertyRid(list[i]);
				if (destTable.IsInvalidRID(rid))
					continue;
				if (hasDeletedRows) {
					// It's a deleted row if RTSpecialName is set and name is "_Deleted"
					var row = tablesStream.ReadPropertyRow(rid);
					if (row == null)
						continue;	// Should never happen since rid is valid
					if ((row.PropFlags & (uint)PropertyAttributes.RTSpecialName) != 0) {
						var name = stringsStream.Read(row.Name);
						if ((object)name != null && name == DeletedName)
							continue;	// ignore this deleted row
					}
				}
				// It's a valid non-deleted rid so add it
				newList.Add(rid);
			}
			return newList;
		}

		/// <summary>
		/// Gets a rid list (eg. field list)
		/// </summary>
		/// <param name="tableSource">Source table, eg. <c>TypeDef</c></param>
		/// <param name="tableSourceRid">Row ID in <paramref name="tableSource"/></param>
		/// <param name="colIndex">Column index in <paramref name="tableSource"/>, eg. 4 for <c>TypeDef.FieldList</c></param>
		/// <param name="tableDest">Destination table, eg. <c>Field</c></param>
		/// <returns>A new <see cref="RidList"/> instance</returns>
		ContiguousRidList GetRidList(Table tableSource, uint tableSourceRid, int colIndex, Table tableDest) {
			var column = tablesStream.Get(tableSource).TableInfo.Columns[colIndex];
			uint startRid;
			if (!tablesStream.ReadColumn(tableSource, tableSourceRid, column, out startRid))
				return ContiguousRidList.Empty;
			uint nextListRid;
			bool hasNext = tablesStream.ReadColumn(tableSource, tableSourceRid + 1, column, out nextListRid);

			uint lastRid = tablesStream.Get(tableDest).Rows + 1;
			if (startRid == 0 || startRid >= lastRid)
				return ContiguousRidList.Empty;
			uint endRid = hasNext && nextListRid != 0 ? nextListRid : lastRid;
			if (endRid < startRid)
				endRid = startRid;
			if (endRid > lastRid)
				endRid = lastRid;
			return new ContiguousRidList(startRid, endRid - startRid);
		}

		/// <inheritdoc/>
		protected override uint BinarySearch(Table tableSource, int keyColIndex, uint key) {
			var table = tablesStream.Get(tableSource);
			if (table == null)
				return 0;
			var keyColumn = table.TableInfo.Columns[keyColIndex];
			uint ridLo = 1, ridHi = table.Rows;
			while (ridLo <= ridHi) {
				uint rid = (ridLo + ridHi) / 2;
				uint key2;
				if (!tablesStream.ReadColumn(tableSource, rid, keyColumn, out key2))
					break;	// Never happens since rid is valid
				if (key == key2)
					return rid;
				if (key2 > key && key2 != 0)
					ridHi = rid - 1;
				else
					ridLo = rid + 1;
			}

			if (tableSource == Table.GenericParam && !tablesStream.IsSorted(tableSource))
				return LinearSearch(tableSource, keyColIndex, key);

			return 0;
		}

		/// <summary>
		/// Linear searches the table (O(n)) for a <c>rid</c> whose key column at index
		/// <paramref name="keyColIndex"/> is equal to <paramref name="key"/>.
		/// </summary>
		/// <param name="tableSource">Table to search</param>
		/// <param name="keyColIndex">Key column index</param>
		/// <param name="key">Key</param>
		/// <returns>The <c>rid</c> of the found row, or 0 if none found</returns>
		uint LinearSearch(Table tableSource, int keyColIndex, uint key) {
			var table = tablesStream.Get(tableSource);
			if (table == null)
				return 0;
			var keyColumn = table.TableInfo.Columns[keyColIndex];
			for (uint rid = 1; rid <= table.Rows; rid++) {
				uint key2;
				if (!tablesStream.ReadColumn(tableSource, rid, keyColumn, out key2))
					break;	// Never happens since rid is valid
				if (key == key2)
					return rid;
			}
			return 0;
		}

		/// <inheritdoc/>
		protected override RidList FindAllRowsUnsorted(Table tableSource, int keyColIndex, uint key) {
			if (tablesStream.IsSorted(tableSource))
				return FindAllRows(tableSource, keyColIndex, key);
			SortedTable sortedTable;
			if (!sortedTables.TryGetValue(tableSource, out sortedTable)) {
				var table = tablesStream.Get(tableSource);
				if (table == null)
					return ContiguousRidList.Empty;
				sortedTables[tableSource] = sortedTable = new SortedTable(table, keyColIndex);
			}
			return sortedTable.FindAllRows(key);
		}
	}
}