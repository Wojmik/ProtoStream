using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.InternalModel
{
	public class NestDataStack<TNestData>
	{
		protected List<TNestData> NestDatas;

		public int CurrentNestLevel { get; protected set; }

		public TNestData CurrentNestData { get; protected set; }

		public TNestData this[int index] { get => this.NestDatas[index]; }

		public NestDataStack(int capacity = 0)
		{
			this.CurrentNestLevel=-1;
			this.NestDatas=new List<TNestData>(capacity);
		}

		public TNestData Pop()
		{
			TNestData nestData;

			nestData=CurrentNestData;
			CurrentNestData=NestDatas[--CurrentNestLevel];
			return nestData;
		}
	}
}