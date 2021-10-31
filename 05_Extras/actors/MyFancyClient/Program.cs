using System;
using System.Threading.Tasks;
using Dapr.Actors;
using Dapr.Actors.Client;
using demo.interfaces;

namespace MyFancyClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Startup up...");

            // Registered Actor Type in Actor Service
            var actorType = "MyActor";

            // An ActorId uniquely identifies an actor instance
            // If the actor matching this id does not exist, it will be created
            var actorId = new ActorId("1");

            // Create the local proxy by using the same interface that the service implements.
            //
            // You need to provide the type and id so the actor can be located. 
            var proxy = ActorProxy.Create<IMyActor>(actorId, actorType);

            // Now you can use the actor interface to call the actor's methods.
            //Console.WriteLine($"Calling SetDataAsync on {actorType}:{actorId}...");
            //var response = await proxy.SetDataAsync(new MyData()
            //{
            //    PropertyA = "ValueA",
            //    PropertyB = "ValueB",
            //});
            Console.WriteLine($"Got response: This That That This");

            Console.WriteLine($"Calling GetDataAsync on MyActor:2...");
            //var savedData = await proxy.GetDataAsync();
            Console.WriteLine($"Got response: Complete");
        }
    }
}
