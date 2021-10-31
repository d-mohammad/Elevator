using Elevator_Ellevation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Elevator_Ellevation
{
    class Program
    {
        static async Task Main(string[] args)
        {           
            var elevator = new Elevator();
            var elevatorQueue = new ElevatorQueue();
            var destinationQueue = new List<int>();

            var pub = new Publisher();
            var sub = new Subscriber(pub);

            //add to queue and make sure it handles and logs the stops of the elevator as it moves
            sub.Publisher.Handler += delegate (object sender, MoveCommand command)
            {
                //if on the same floor, the user needs to input a new floor now
                if (command.OriginFloor == elevator.CurrentFloor)
				{
                    Console.WriteLine("Please enter the destination floor to continue: ");
                    var input = Console.ReadLine();
                    var destinationFloor = int.Parse(input);

                    elevatorQueue.AddToDestinationQueue(destinationFloor);
                }
                //only add it to the list of requests if it's on a different floor
                else
                {
                    elevatorQueue.AddToRequestQueue(command.OriginFloor, command.Direction);
                }
                

                if (elevator.Status == ElevatorStatus.Idle)
				{
                    Task.Run(() => elevator.MoveElevator(elevatorQueue));
				}
            };

            Console.Write("OriginFloor Direction (enter with a space): ");
            
            //make sure to always be checking for inputs
            while (true)
            {                
                //if the elevator is waiting for a requests' input, we avoid this running again to avoid multiple input issues in console
                if (elevator.Status != ElevatorStatus.Waiting)
                {
                    try
                    {
                        //get int and string indicating the floor of origin and up or down request
                        var input = Console.ReadLine();
                        if (!string.IsNullOrEmpty(input))
                        {
                            var splitData = input.Split(' ');

                            var originFloor = int.Parse(splitData[0]);
                            var destionationString = splitData[1];

                            //setting to idle as default. this logic would need some updating
                            ElevatorStatus direction = ElevatorStatus.Idle;

                            if (destionationString == "u")
                            {
                                direction = ElevatorStatus.Up;
                            }
                            else if (destionationString == "d")
                            {
                                direction = ElevatorStatus.Down;
                            }

                            //publish the event to trigger adding it to the queue and to move the elevator if it's idle
                            if (originFloor >= elevator.LowestFloor && originFloor <= elevator.HighestFloor)
                            {
                                pub.Publish(originFloor, direction);
                            }
                            else
                            {
                                Console.WriteLine("Only floor 1-10 can be entered");
                            }
                        }
                    }
                    catch (InvalidOperationException)
					{
                        //just catch and allow to continue (raised from canceling the console read in Elevator.cs
					}
                    catch (OperationCanceledException)
					{
                        //just catch and allow to continue (raised from canceling the console read in Elevator.cs
                    }
                } 
                
                Thread.Sleep(1000);
            }
        }
    }


    class MoveCommand
    {
        public int OriginFloor;
        public ElevatorStatus Direction;

        public MoveCommand(int originFloor, ElevatorStatus direction)
        {
            this.OriginFloor = originFloor;
            this.Direction = direction;
        }
    }

    interface IPublisher
    {
        event EventHandler<MoveCommand> Handler;
        void Publish(int originFloor, ElevatorStatus direction);
    }

    class Publisher : IPublisher
    {
        public event EventHandler<MoveCommand> Handler;

        public void OnPublish(MoveCommand msg)
        {
            Handler?.Invoke(this, msg);
        }

        public void Publish(int originFloor, ElevatorStatus direction)
        {
            MoveCommand msg = (MoveCommand)Activator.CreateInstance(typeof(MoveCommand), originFloor, direction);
            OnPublish(msg);
        }
    }

    class Subscriber
    {
        public IPublisher Publisher { get; set; }
        public Subscriber(IPublisher publisher)
        {
            Publisher = publisher;
        }
    }
}