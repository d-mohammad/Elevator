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

		public ElevatorStatus Status { get; set; }
		public ElevatorDirection Direction { get; set; }
		public int CurrentFloor { get; set; }
		public int LowestFloor = 1;
		public int HighestFloor = 10;

		public Elevator()
		{
			//default elevator to status of Idle and starting on the lobby
			this.Status = ElevatorStatus.Idle;
			this.Direction = ElevatorDirection.Idle;
			this.CurrentFloor = this.LowestFloor;

		}

		//starts to move the elevator and continues until nothing is left in the request or destination queue
		public async Task MoveElevator(ElevatorQueue queue)
		{
			//keep moving the elevator until there is nothing left in the queue
			while (!queue.IsEmpty())
			{				
				//before the elevator starts moving, make sure to check if any there are any stops on the way
				(var nextStop, var isDestination) = queue.FindNextStop(this.CurrentFloor, this.Direction, this.Status);

				if (nextStop > this.CurrentFloor)
				{
					this.Direction = ElevatorDirection.Up;
					this.Status = ElevatorStatus.MovingToDestination;
				}
				else if (nextStop < this.CurrentFloor)
				{
					this.Direction = ElevatorDirection.Down;
					this.Status = ElevatorStatus.MovingToDestination;
				}

				if (isDestination == true)
				{
					this.Status = ElevatorStatus.MovingToDestination;
				}
				else
				{
					this.Status = ElevatorStatus.ServicingRequest;
				}

				if (nextStop == 0)
				{
					continue;
				}

				if (this.CurrentFloor != nextStop)
				{
					Console.WriteLine("Elevator moving past floor " + this.CurrentFloor.ToString());

					//update floor number based on direction
					if (this.Direction == ElevatorDirection.Down && this.CurrentFloor != this.LowestFloor)
					{
						this.CurrentFloor--;
					}
					else if (this.Direction == ElevatorDirection.Up && this.CurrentFloor != this.HighestFloor)
					{
						this.CurrentFloor++;
					}
				}
				else if (this.CurrentFloor == nextStop)
				{
					this.AnnounceArrival();

					//if this was to service a new request, we have to ask for the user to input a detination floor
					//queue logic is now too entangled with the elevator - would probably want to split it
					if (!isDestination)
					{
						//set status to waiting so that we don't continue the console requests and potentially have issues with 2 console reads
						var previousDirection = this.Direction;
						this.Status = ElevatorStatus.Waiting;

						//have to send a key to break the console readline from Program.cs. Solution found from https://stackoverflow.com/questions/9479573/how-to-interrupt-console-readline
						CancelConsoleRead();

						//get new input from user and add it to the queue
						Console.WriteLine("Please enter the destination floor to continue: ");
						var input = Console.ReadLine();
						var destinationFloor = int.Parse(input);

						queue.DestinationQueue.Add(destinationFloor);
						queue.RemoveFloorFromRequestQueue(this.CurrentFloor);

						this.Direction = previousDirection;
					}
					else
					{
						queue.RemoveFloorFromDestinationQueue(nextStop);
					}					
				}

				Thread.Sleep(1000);

				//find next stop
				(nextStop, isDestination) = queue.FindNextStop(this.CurrentFloor, this.Direction, this.Status);

				//if nextStop == 0 and the queue isn't empty, it means the elevator needs to reverse directions
				if (nextStop == 0 && !queue.IsEmpty())
				{
					this.ReverseDirection();

					//attempt to get the next stop after reversing directions of the elevator
					(nextStop, isDestination) = queue.FindNextStop(this.CurrentFloor, this.Direction, this.Status);

					//if it happens to match up, we need to gather the destination from the requester
					if (nextStop == this.CurrentFloor)
					{
						CancelConsoleRead();
						var destinationFloor = GetDestinationFromRequest();
						queue.DestinationQueue.Add(destinationFloor);
					}
				}
			}			
		}		

		public void AnnounceArrival()
		{
			Console.WriteLine();
			Console.WriteLine("***Elevator arrived at: " + this.CurrentFloor.ToString() + "***");
		}

		//we are going to assume this is always a number and input is valid, since it should come from a button in reality
		public int GetDestinationFromRequest()
		{
			Console.WriteLine("Please enter the destination floor to continue: ");
			var input = Console.ReadLine();

			return int.Parse(input);
		}

		public void ReverseDirection()
		{
			if (this.Direction == ElevatorDirection.Up)
			{
				this.Direction = ElevatorDirection.Down;
			}
			else if (this.Direction == ElevatorDirection.Down)
			{
				this.Direction = ElevatorDirection.Up;
			}
		}

		public void CancelConsoleRead()
		{
			//have to send a key to break the console readline from Program.cs. Solution found from https://stackoverflow.com/questions/9479573/how-to-interrupt-console-readline
			var handle = GetStdHandle(STD_INPUT_HANDLE);
			CancelIoEx(handle, IntPtr.Zero);
			CancelIoEx(handle, IntPtr.Zero);
		}
	}
}
