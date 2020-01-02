// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Newtonsoft.Json.Linq;

namespace Leacme.Lib.LiteDbTraveler {

	public class Library {

		/// <summary>
		/// Returns recursively retireved document values with their matching collections.
		/// /// </summary>
		/// <param name="db">LiteDB to parse</param>
		/// <returns></returns>
		public Dictionary<string, List<Dictionary<string, string>>> GetDbValues(LiteDatabase db) {
			var colls = new Dictionary<string, List<Dictionary<string, string>>>();
			foreach (var coll in db.GetCollectionNames().Select(z => db.GetCollection(z))) {
				var docs = new List<Dictionary<string, string>>();
				foreach (var doc in coll.FindAll()) {
					Dictionary<string, string> docEntries = new Dictionary<string, string>();
					RecursJ(JObject.Parse(doc.ToString()), docEntries);
					docs.Add(docEntries);
				}
				colls.Add(coll.Name, docs);
			}
			return colls;
		}

		private bool RecursJ(JToken t, Dictionary<string, string> outp, string p = "") {
			if (t.HasValues) {
				foreach (var c in t.Children()) {
					if (t.Type.Equals(JTokenType.Property)) {
						if (p.Equals("")) {
							p = ((JProperty)t).Name;
						} else {
							p += "." + ((JProperty)t).Name;
						}
					}
					RecursJ(c, outp, p);
				}
				return true;
			} else {
				if (outp.ContainsKey(p)) {
					outp[p] += ", " + t.ToString();
				} else {
					outp.Add(p, t.ToString());
				}
				return false;
			}
		}
	}
}