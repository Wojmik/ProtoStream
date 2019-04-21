using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.InternalModel
{
	public class NestDataWriteStack : NestDataStack<NestDataWrite>
	{
		public NestDataWriteStack(int capacity = 0)
			:base(capacity: capacity)
		{ }

		public void Push(int fieldNo, int headerLength, int levelDataStartIndex, int bufferAvailableLength)
		{
			this.CurrentNestLevel++;
			if(this.NestDatas.Count<=this.CurrentNestLevel)
				this.NestDatas.Add(this.CurrentNestData=new NestDataWrite(nestLevel: this.CurrentNestLevel));
			else
				this.CurrentNestData=this.NestDatas[this.CurrentNestLevel];

			this.CurrentNestData.Reset(fieldNo: fieldNo, headerLength: headerLength, levelDataStartIndex: levelDataStartIndex, bufferAvailableLength: bufferAvailableLength);
		}
	}
}