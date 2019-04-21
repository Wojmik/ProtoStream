using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.InternalModel
{
	class IntArrayEqualityComparer : IEqualityComparer<List<int>>
	{
		public static IntArrayEqualityComparer Default { get; } = new IntArrayEqualityComparer();

		private IntArrayEqualityComparer()
		{ }

		public bool Equals(List<int> x, List<int> y)
		{
			int i;
			bool bEquals;

			bEquals=(x.Count==y.Count);
			for(i=0; bEquals && i<x.Count; i++)
				bEquals=(x[i]==y[i]);
			return bEquals;
		}

		public int GetHashCode(List<int> obj)
		{
			int i, hashCode = 1390139660;

			unchecked
			{
				for(i=0; i<obj.Count; i++)
					hashCode=hashCode*-1521134295+obj[i].GetHashCode();
			}
			return hashCode;
		}
	}
}