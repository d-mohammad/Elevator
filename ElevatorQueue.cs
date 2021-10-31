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
		public List<Requests> RequestQueue { get; set; }

		//destination just needs to be a list of floors that the elevator has to stop at on its way.
		public List<int> DestinationQueue { get; set; }

		public ElevatorQueue()
		{
			RequestQueue = new List<Requests>();
			DestinationQueue = new List<int>();
		}

		public void AddToRequestQueue(int originFloor, ElevatorStatus direction)
		{			
			this.RequestQueue.Add(new Requests
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

		public void RemoveFloorFromQueues(int floorNumber, ElevatorStatus direction)
		{
			//in case we have multiple requests to go to one floor, remove it here. this should be improved upon to ensure multiple requests cannot be added to the queue
			this.RequestQueue.RemoveAll(x => x.OriginFloor == floorNumber && x.Direction == direction);
			this.DestinationQueue.RemoveAll(x => x == floorNumber);
		}

		//need to improve the logic of choosing which to go to next.
		public (int, bool) FindNextStop(int currentFloor, ElevatorStatus direction)
		{
			//default next stop to floor 0
			int nextRequest = 0;
			int nextDestination = 0;
			int nextStop = 0;
			bool isDestination = false;
		
			//compare the next request vs next destination to know which to go to next, depending on direction
			if (direction == ElevatorStatus.Up)
			{
				nextRequest = RequestQueue.Where(x => x.OriginFloor >= currentFloor && x.Direction == ElevatorStatus.Up).OrderBy(x => x.OriginFloor - currentFloor).Select(x => x.OriginFloor).FirstOrDefault();
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
				nextRequest = RequestQueue.Where(x => x.OriginFloor <= currentFloor && x.Direction == ElevatorStatus.Down).OrderBy(x => currentFloor - x.OriginFloor).Select(x => x.OriginFloor).FirstOrDefault();
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
