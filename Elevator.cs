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
		public int CurrentFloor { get; set; }
		public int LowestFloor = 1;		
		public int HighestFloor = 10;
		
		public Elevator()
		{
			//default elevator to status of Idle and starting on the lobby
			this.Status = ElevatorStatus.Idle;
			this.CurrentFloor = LowestFloor;
		}

		//starts to move the elevator and continues until nothing is left in the request or destination queue
		public async Task MoveElevator(ElevatorQueue queue)
		{
			if (this.CurrentFloor == LowestFloor)
			{
				this.Status = ElevatorStatus.Up;
			}
			else if (this.CurrentFloor == HighestFloor)
			{
				this.Status = ElevatorStatus.Down;
			}
			
			//keep moving the elevator until there is nothing left in the queue
			while (!queue.IsEmpty())
			{							
				//before the elevator starts moving, make sure to check if any there are any stops on the way
				(var nextStop, var isDestination) = queue.FindNextStop(this.CurrentFloor, this.Status);

				if (nextStop > this.CurrentFloor)
				{
					this.Status = ElevatorStatus.Up;
				}
				else if (nextStop < this.CurrentFloor)
				{
					this.Status = ElevatorStatus.Down;
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
					this.AnnounceArrival();

					//if this was to service a new request, we have to ask for the user to input a detination floor
					//queue logic is now too entangled with the elevator - would probably want to split it
					if (!isDestination)
					{
						//set status to waiting so that we don't continue the console requests and potentially have issues with 2 console reads
						var previousDirection = this.Status;
						this.Status = ElevatorStatus.Waiting;

						//have to send a key to break the console readline from Program.cs. Solution found from https://stackoverflow.com/questions/9479573/how-to-interrupt-console-readline
						var handle = GetStdHandle(STD_INPUT_HANDLE);
						CancelIoEx(handle, IntPtr.Zero);
						CancelIoEx(handle, IntPtr.Zero);

						//get new input from user and add it to the queue
						Console.WriteLine("Please enter the destination floor to continue: ");
						var input = Console.ReadLine();
						var destinationFloor = int.Parse(input);

						queue.DestinationQueue.Add(destinationFloor);

						this.Status = previousDirection;
					}

					//in case we have multiple requests to go to one floor, remove it here. this should be improved upon to ensure multiple requests cannot be added to the queue
					queue.RemoveFloorFromQueues(nextStop, this.Status);					
				}

				Thread.Sleep(1000);

				//find next stop
				(nextStop, isDestination) = queue.FindNextStop(this.CurrentFloor, this.Status);

				//if nextStop == 0 and the queue isn't empty, it means the elevator needs to reverse directions
				if (nextStop == 0 && !queue.IsEmpty())
				{
					if (this.Status == ElevatorStatus.Up)
					{
						this.Status = ElevatorStatus.Down;
					}
					else if (this.Status == ElevatorStatus.Down)
					{
						this.Status = ElevatorStatus.Up;
					}

					//attempt to get the next stop after reversing directions of the elevator
					(nextStop, isDestination) = queue.FindNextStop(this.CurrentFloor, this.Status);

					//if it happens to match up, we need to gather the destination from the requester
					if (nextStop == this.CurrentFloor)
					{
						//have to send a key to break the console readline from Program.cs. Solution found from https://stackoverflow.com/questions/9479573/how-to-interrupt-console-readline
						var handle = GetStdHandle(STD_INPUT_HANDLE);
						CancelIoEx(handle, IntPtr.Zero);
						CancelIoEx(handle, IntPtr.Zero);

						var destinationFloor = GetDestinationFromRequest();						
						queue.DestinationQueue.Add(destinationFloor);
					}
				}

				//update floor number based on 
				if (this.Status == ElevatorStatus.Down && this.CurrentFloor != this.HighestFloor)
				{
					this.CurrentFloor--;
				}					
				else if (this.Status == ElevatorStatus.Up)
				{
					this.CurrentFloor++;
				}				
			}

			//if the queue is empty, reset the elevator back to its default position
			Thread.Sleep(300);
			ResetElevator(queue);
		}

		//set it to an idle status and the default floor
		public void ResetElevator(ElevatorQueue queue)
		{
			Console.WriteLine("Elevator resetting");
			this.Status = ElevatorStatus.Idle;
			queue.AddToDestinationQueue(this.LowestFloor);
			this.MoveElevator(queue);
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
	}
}
