using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elevator_Ellevation
{
	class ElevatorQueue
	{
		public List<QueueObject> Queue { get; set; }

		public ElevatorQueue()
		{
			Queue = new List<QueueObject>();	
		}

		public void AddToQueue(int originFloor, int destinationFloor)
		{
			if (originFloor == destinationFloor)
			{
				return;
			}

			this.Queue.Add(new QueueObject
			{
				OriginFloor = originFloor,
				DestinationFloor = destinationFloor,
				Direction = originFloor > destinationFloor ? ElevatorStatusEnum.Down : ElevatorStatusEnum.Up
			});
		}

		public int FindNextStop(int currentFloor, ElevatorStatusEnum direction)
		{
			//default next stop to floor 0
			var nextStop = 0;
			if (direction == ElevatorStatusEnum.Up)
			{
				nextStop = Queue.Where(x => x.OriginFloor >= currentFloor && x.Direction == ElevatorStatusEnum.Up).OrderBy(x => x.OriginFloor - currentFloor).Select(x => x.DestinationFloor).FirstOrDefault();
			}
			else
			{
				nextStop = Queue.Where(x => x.OriginFloor <= currentFloor && x.Direction == ElevatorStatusEnum.Down).OrderBy(x => currentFloor - x.OriginFloor).Select(x => x.DestinationFloor).FirstOrDefault();
			}

			return nextStop;
		}
	}
}
