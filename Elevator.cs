using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elevator_Ellevation
{
	class Elevator
	{


		const int STD_INPUT_HANDLE = -10;

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool CancelIoEx(IntPtr handle, IntPtr lpOverlapped);

		//the status will tell which direction the elevator is going, or if it's idle.
		public ElevatorStatusEnum status { get; set; }

		public int CurrentFloor { get; set; }

		public int LowestFloor = 1;
		
		public int HighestFloor = 10;

		public Publisher pub = new Publisher();

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

			(var nextStop, var isDestination) = queue.FindNextStop(this.CurrentFloor, this.status);

			//current issue is that it will stop at all requests along the way IF they are all input before it starts moving. if not, it will move to the destination first.
			while (queue.RequestQueue.Count > 0 || queue.DestinationQueue.Count > 0)
			{			
				if (nextStop > this.CurrentFloor)
				{
					this.status = ElevatorStatusEnum.Up;
				}
				else if (nextStop < this.CurrentFloor)
				{
					this.status = ElevatorStatusEnum.Down;
				}
				
				////check for any new updates to 
				//var floorsToTravel = Math.Abs(this.CurrentFloor - nextStop - 1);
				//for (var i = 1; i < floorsToTravel + 1; i++)
				//{
					
					//while the elevator is traveling to the destination, we want to check if any new request has come in along the way
					//if there are no changes, the elevator continues
				(var newStop, var newIsDestination) = queue.FindNextStop(this.CurrentFloor, this.status);					

				if (!queue.DestinationQueue.Any(x => x == nextStop) && !queue.RequestQueue.Any(x => x.OriginFloor == nextStop && x.Direction == this.status))
				{
					nextStop = newStop;
					isDestination = newIsDestination;
				}

				if (newStop > 0 && this.status == ElevatorStatusEnum.Up && newStop < nextStop)
				{
					nextStop = newStop;
					isDestination = newIsDestination;
					Thread.Sleep(1000);
				}
				else if (newStop > 0 && this.status == ElevatorStatusEnum.Down && newStop > nextStop)
				{
					nextStop = newStop;
					isDestination = newIsDestination;
					Thread.Sleep(1000);
				}

				if (nextStop == 0)
				{
					continue;
				}

				if (this.CurrentFloor != nextStop)
				{
					Console.WriteLine("Elevator moving past floor " + this.CurrentFloor.ToString());
				}
				else if (this.CurrentFloor == nextStop)
				{
					//check what the current direction of the elevator is. the elevator should stop at the next one in line for that direction
					Console.WriteLine();
					Console.WriteLine("***Elevator moved to: " + nextStop.ToString() + "***");
					this.CurrentFloor = nextStop;

					//if this was to service a new request, we have to ask for the user to input a detination floor
					//queue logic is now too entangled with the elevator - would probably want to split it
					if (!isDestination)
					{
						var previousDirection = this.status;
						this.status = ElevatorStatusEnum.Waiting;

						//have to send a key to break the console readline from Program.cs. Solution found from https://stackoverflow.com/questions/9479573/how-to-interrupt-console-readline
						var handle = GetStdHandle(STD_INPUT_HANDLE);
						CancelIoEx(handle, IntPtr.Zero);

						Console.WriteLine("Please enter the destination floor to continue: ");
						var input = Console.ReadLine();
						var destinationFloor = int.Parse(input);

						queue.DestinationQueue.Add(destinationFloor);

						this.status = previousDirection;
					}

					//in case we have multiple requests to go to one floor, remove it here. this should be improved upon to ensure multiple requests cannot be added to the queue
					queue.RequestQueue.RemoveAll(x => x.OriginFloor == nextStop && x.Direction == this.status);
					queue.DestinationQueue.RemoveAll(x => x == nextStop);
				}

				Thread.Sleep(1000);

				//find which in the queue to use
				(nextStop, isDestination) = queue.FindNextStop(this.CurrentFloor, this.status);

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

					(nextStop, isDestination) = queue.FindNextStop(this.CurrentFloor, this.status);

					if (nextStop == this.CurrentFloor)
					{
						//have to send a key to break the console readline from Program.cs. Solution found from https://stackoverflow.com/questions/9479573/how-to-interrupt-console-readline
						var handle = GetStdHandle(STD_INPUT_HANDLE);
						CancelIoEx(handle, IntPtr.Zero);

						Console.WriteLine("Please enter the destination floor to continue: ");
						var input = Console.ReadLine();
						var destinationFloor = int.Parse(input);

						queue.DestinationQueue.Add(destinationFloor);
					}
				}

				if (this.status == ElevatorStatusEnum.Down)
				{
					this.CurrentFloor--;
				}					
				else if (this.status == ElevatorStatusEnum.Up)
				{
					this.CurrentFloor++;
				}
					
				//}															
			}

			//delay before resetting the elevator			
			Thread.Sleep(5000);
			ResetElevator();
		}

		//set it to an idle status and the default floor
		public void ResetElevator()
		{
			this.status = ElevatorStatusEnum.Idle;
			
			
			Console.Write("Origin Floor: ");
		}
	}
}
