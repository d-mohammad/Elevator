using Elevator_Ellevation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elevator_Ellevation
{
    class Program
    {
        static async Task Main(string[] args)
        {           
            var elevator = new Elevator();
            var queue = new ElevatorQueue();

            var pub = new Publisher();
            var sub1 = new Subscriber(pub);

            //add to queue and make sure it handles and logs the stops of the elevator as it moves
            sub1.Publisher.Handler += delegate (object sender, MoveCommand command)
            {
                queue.AddToQueue(command.OriginFloor, command.DestinationFloor);

                if (elevator.status == ElevatorStatusEnum.Idle)
				{
                    Task.Run(() => elevator.MoveElevator(queue));
				}
            };
            
            while (true)
			{
                Console.Write("OriginFloor DestinationFloor (enter with a space): ");
                var input = Console.ReadLine();
                var splitData = input.Split(' ');
                
                var currentFloor = int.Parse(splitData[0]);
                var destinationFloor = int.Parse(splitData[1]);

                if (currentFloor >= elevator.LowestFloor && currentFloor <= elevator.HighestFloor && destinationFloor >= elevator.LowestFloor && destinationFloor <= elevator.HighestFloor)
				{
                    pub.Publish(currentFloor, destinationFloor);
                }
                else
				{
                    Console.WriteLine("Only floor 1-10 can be entered");
                }                
            }            
        }
    }


    class MoveCommand
    {
        public int OriginFloor;
        public int DestinationFloor;

        public MoveCommand(int originFloor, int destinationFloor)
        {
            this.OriginFloor = originFloor;
            this.DestinationFloor = destinationFloor;
        }
    }

    interface IPublisher
    {
        event EventHandler<MoveCommand> Handler;
        void Publish(int originFloor, int destinationFloor);
    }

    class Publisher : IPublisher
    {
        public event EventHandler<MoveCommand> Handler;

        public void OnPublish(MoveCommand msg)
        {
            Handler?.Invoke(this, msg);
        }

        public void Publish(int originFloor, int destinationFloor)
        {
            MoveCommand msg = (MoveCommand)Activator.CreateInstance(typeof(MoveCommand), originFloor, destinationFloor);
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