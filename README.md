# rclnet
rclnet is fast and easy-to-use .NET wrapper over ROS 2 client library, allowing .NET applications to interact with other ROS applications.

Currently supports .NET 7 and ROS2 Foxy Fitzroy.
## Showcase
### Subscribing
```csharp
using var ctx = new RclContext(args);
using var node = ctx.CreateNode("/hellow_world");
using var sub = node.CreateSubscription<Twist>("/cmd_vel");
await foreach (Twist msg in sub.ReadAllAsync())
{
    ...
}
```
### Publishing
```csharp
using var pub = node.CreatePublisher<Vector3>("/vec");
pub.Publish(new Vector3(x: 1, y: 2, z: 3));
```
### Handling Service Calls
```csharp
using var server = node.CreateService<
    EmptyService,
    EmptyServiceRequest,
    EmptyServiceResponse>("/vec",
        (request, cancellationToken, state) =>
        {
            return new EmptyServiceResponse();
        });
await Task.Delay(-1);
```
### Calling Services
```csharp
using var client = node.CreateClient<
    EmptyService,
    EmptyServiceRequest,
    EmptyServiceResponse>("/vec");
await client.InvokeAsync(new EmptyServiceRequest());
``` 
### Monitoring ROS Graph Changes
```csharp
node.Graph
    .OfType<NodeEstablishedEvent>()
    .Subscribe(x =>
    {
        Console.WriteLine($"Node {x.Node.Name} is online.");
    });
await Task.Delay(-1);
```
### Calling Action Servers
```csharp
using var client = node.CreateActionClient<
    SpinAction,
    SpinActionGoal,
    SpinActionResult,
    SpinActionFeedback>("/spin");

using var goal = await client.SendGoalAsync(
        new SpinActionGoal(targetYaw: Math.PI));

await foreach (var feedback in goal.ReadFeedbacksAsync())
{
    Console.WriteLine(feedback.AngularDistanceTraveled);
}

var result = await goal.GetResultAsync();
```
### Zero (Managed Heap) Allocation APIs
```csharp
using var sub = node.CreateNativeSubscription<Twist>("/cmd_vel");
await foreach (RosMessageBuffer msg in sub.ReadAllAsync())
{
    using (msg) ProcessMessage(msg);

    static void ProcessMessage(RosMessageBuffer buffer)
    {
        ref var twist = ref buffer.AsRef<Twist.Priv>();
        ...
    }
}
```

## How to Generate Messages
1. Create a new .NET library project targeting .NET 7.
2. Add add the following lines to the `.csproj` file:
   ```xml
   <PropertyGroup>
      <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
      <RootNamespace>Rosidl.Messages</RootNamespace>
   </PropertyGroup>

   <ItemGroup>
      <PackageReference Include="Rosidl.Runtime" Version="1.0.0" />
   </ItemGroup>
   ```
3. Add a `ros2cs.spec` file to the root of the project.
    See [here](https://github.com/noelex/rclnet/blob/main/src/ros2cs/ros2cs.spec) for detailed explanations on how to write a `ros2cs.spec` file.

    You may want to exclude packages that are already included in [Rosidl.CommonMessages](https://github.com/noelex/rclnet/tree/main/src/Rosidl.CommonMessages/ros2cs.spec).
4. Install `ros2cs` with the following command:
   ```
   dotnet tool install -g ros2cs
   ```
5. Run `ros2cs /path/to/ros2cs.spec` to generate message classes / structs.