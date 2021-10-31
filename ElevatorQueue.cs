using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elevator_Ellevation
{
	class ElevatorQueue
	{
		//request queue needs to know the origination of the request and which direction they want to go
		public List<QueueObject> RequestQueue { get; set; }

		//destination just needs to be a list of floors that the elevator has to stop at on its way.
		public List<int> DestinationQueue { get; set; }

		public ElevatorQueue()
		{
			RequestQueue = new List<QueueObject>();
			DestinationQueue = new List<int>();
		}

		public void AddToRequestQueue(int originFloor, ElevatorStatusEnum direction)
		{			
			this.RequestQueue.Add(new QueueObject
			{
				OriginFloor = originFloor,
				Direction = direction
			});
		}

		public void AddToDestinationQueue(int destinationFloor)
		{
			this.DestinationQueue.Add(destinationFloor);
		}

		public bool IsEmpty()
		{
			if (this.DestinationQueue.Count == 0 && this.RequestQueue.Count == 0)
			{
				return true;
			}

			return false;
		}

		public (int, bool) FindNextStop(int currentFloor, ElevatorStatusEnum direction)
		{
			//default next stop to floor 0
			int nextRequest = 0;
			int nextDestination = 0;
			int nextStop = 0;
			bool isDestination = false;

			//improve the logic of choosing which to go to next.
			if (direction == ElevatorStatusEnum.Up)
			{
				nextRequest = RequestQueue.Where(x => x.OriginFloor >= currentFloor && x.Direction == ElevatorStatusEnum.Up).OrderBy(x => x.OriginFloor - currentFloor).Select(x => x.OriginFloor).FirstOrDefault();
				nextDestination = DestinationQueue.Where(x => x >= currentFloor).OrderBy(x => x - currentFloor).Select(x => x).FirstOrDefault();
				
				if (nextRequest < nextDestination && nextRequest != 0)
				{
					nextStop = nextRequest;
					isDestination = false;
				}
				else
				{
					nextStop = nextDestination;
					isDestination = true;
				}
			}
			else
			{
				nextRequest = RequestQueue.Where(x => x.OriginFloor <= currentFloor && x.Direction == ElevatorStatusEnum.Down).OrderBy(x => currentFloor - x.OriginFloor).Select(x => x.OriginFloor).FirstOrDefault();
				nextDestination = DestinationQueue.Where(x => x <= currentFloor).OrderBy(x => currentFloor - x).Select(x => x).FirstOrDefault();

				if (nextRequest > nextDestination && nextRequest != 0)
				{
					nextStop = nextRequest;
					isDestination = false;
				}
				else 
				{
					nextStop = nextDestination;
					isDestination = true;
				}
			}

			

			if (nextDestination > 0 && nextRequest == 0 && nextStop == 0)
			{
				isDestination = true;
				nextStop = nextDestination;
			}
			else if (nextRequest > 0 && nextDestination == 0 && nextStop == 0)
			{
				isDestination = false;
				nextStop = nextRequest;
			}
			
			return (nextStop, isDestination);
		}
	}
}
