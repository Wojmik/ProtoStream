using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.InternalModel
{
	public class NestDataReadStack : NestDataStack<NestDataRead>
	{
		public NestDataReadStack(int capacity = 0)
			: base(capacity: capacity)
		{ }

		public void Push(int fieldNo, int nestLevelLength)
		{
			this.CurrentNestLevel++;
			if(this.NestDatas.Count<=this.CurrentNestLevel)
				this.NestDatas.Add(this.CurrentNestData=new NestDataRead(nestLevel: this.CurrentNestLevel));
			else
				this.CurrentNestData=this.NestDatas[this.CurrentNestLevel];

			this.CurrentNestData.Reset(fieldNo: fieldNo, nestLevelLength: nestLevelLength);
		}
	}
}