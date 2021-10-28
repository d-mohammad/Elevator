using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elevator_Ellevation
{
	class Elevator
	{
		//the status will tell which direction the elevator is going, or if it's idle.
		public ElevatorStatusEnum status { get; set; }

		public int CurrentFloor { get; set; }

		public int LowestFloor = 1;
		
		public int HighestFloor = 10;

		public Elevator()
		{
			//default elevator to status of Idle and starting on the lobby
			this.status = ElevatorStatusEnum.Idle;
			this.CurrentFloor = 1;
		}

		//move elevator function
		public async Task MoveElevator(ElevatorQueue queue)
		{
			//it can only go up from here
			if (this.CurrentFloor == LowestFloor)
			{
				this.status = ElevatorStatusEnum.Up;
			}
			else if (this.CurrentFloor == HighestFloor)
			{
				this.status = ElevatorStatusEnum.Down;
			}

			while (queue.Queue.Count > 0)
			{
				//find which in the queue to use
				var nextStop = queue.FindNextStop(this.CurrentFloor, this.status);

				//if we can't find a floor going in the current direction, but we still have something in the queue, we need to reverse direction
				if (nextStop == 0)
				{
					if (this.status == ElevatorStatusEnum.Up)
					{
						this.status = ElevatorStatusEnum.Down;
					}
					else if (this.status == ElevatorStatusEnum.Down)
					{
						this.status = ElevatorStatusEnum.Up;
					}

					nextStop = queue.FindNextStop(this.CurrentFloor, this.status);
				}

				if (nextStop > this.CurrentFloor)
				{
					this.status = ElevatorStatusEnum.Up;
				}
				else if (nextStop < this.CurrentFloor)
				{
					this.status = ElevatorStatusEnum.Down;
				}
				else
				{
					this.status = ElevatorStatusEnum.Idle;
					return;
				}

				Thread.Sleep(5000);

				//check what the current direction of the elevator is. the elevator should stop at the next one in line for that direction
				Console.WriteLine();
				Console.WriteLine("***Elevator moved to: " + nextStop.ToString() + "***");
				this.CurrentFloor = nextStop;				

				//in case we have multiple requests to go to one floor, remove it here. this should be improved upon to ensure multiple requests cannot be added to the queue
				queue.Queue.RemoveAll(x => x.DestinationFloor == nextStop);
			}

			//delay before resetting the elevator			
			Thread.Sleep(5000);
			ResetElevator();
		}

		//set it to an idle status and the default floor
		public void ResetElevator()
		{
			this.status = ElevatorStatusEnum.Idle;
			this.CurrentFloor = 1;
			Console.WriteLine("***Elevator reset to floor 1***");
			Console.Write("Origin Floor: ");
		}
	}
}
