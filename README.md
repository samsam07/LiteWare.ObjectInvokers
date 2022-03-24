# LiteWare.ObjectInvokers

[![Nuget](https://img.shields.io/nuget/v/LiteWare.ObjectInvokers)](https://www.nuget.org/packages/LiteWare.ObjectInvokers)
[![License](https://img.shields.io/github/license/samsam07/LiteWare.ObjectInvokers)](https://github.com/samsam07/LiteWare.ObjectInvokers/blob/master/LICENSE)

LiteWare.ObjectInvokers allows you to dynamically invoke methods and modify properties or fields at runtime by specifying the member's name, for example:

``` csharp
ObjectInvoker objectInvoker = ObjectInvoker.Bind<IIotHub>(iotHub);
objectInvoker.Invoke("InitializeIotHub", "HUB01", MaxDeviceCout);
```

Events can also be monitored, for example:

``` csharp
using EventListener eventListener = EventListener.Bind<IIotHub>(iotHub, eventNotifier);
eventListener.StartListening();
```

This library is useful in scenarios where an external system wants to dynamically invoke or listen for events of an internal object.

## Usage

Start by defining a contract type containing the members to invoke and/or the events to listen to:

``` csharp
public interface IMyService
{
    [Listenable]
    event EventHandler MyEvent;

    [Invokable]
    int MyProperty { get; set; }

    [Invokable]
    void MyProcedure();

    [Invokable("MyFunction")]
    int SomeFunction(int arg);
}
```

The contract type is not limited to only interfaces and can be of any type.
However, it must either have members qualified by the `InvokableAttribute` or `ListenableAttribute` or a predicate must be provided during binding
to select wanted members.

`InvokableAttribute` and `ListenableAttribute` can also be applied on `private` members.

### ObjectInvoker

Dynamic invoke of methods and modification of properties or fields is done by the `ObjectInvoker.Invoke` method,
which accepts a member name and any generic types and/or parameters and redirects the call to a concrete object.

An instance of `ObjectInvoker` is created by binding a contract type containing the members to invoke to an instance of the contract:

``` csharp
ObjectInvoker objectInvoker = ObjectInvoker.Bind<IMyService>(myService);
```

Depending on the underlying concrete object member, the invoke process can:

- Invoke a method and return the result if the member is a method:

    ``` csharp
    bool? success = objectInvoker.Invoke("SetLightSwitchState", "SW012", LightSwitchState.On) as bool?;
    ```

- Get or set a property if the member is a property:

    ``` csharp
    string? id = objectInvoker.Invoke("DeviceId") as string?; // Property getter
    objectInvoker.Invoke("DeviceDescription", "Livingroom light switch"); // Property setter
    ```

- Get or set a field value if the member is a field:

    ``` csharp
    int? timeout = objectInvoker.Invoke("Timeout") as int?; // Get field value
    objectInvoker.Invoke("Timeout", 1000); // Set field value
    ```

### EventListener

The monitoring of raised events is done by the `EventListener` class which hooks listenable events on a concrete object to runtime-created
subscriber delegates that notifies using instances of `IEventNotifier`.

This is done as follows:

``` csharp
using EventListener eventListener = EventListener.Bind<IMyService>(myService, eventNotifier1, eventNotifier2);
eventListener.StartListening();
```

`eventNotifier1` and `eventNotifier2` are cutom implementations of `IEventNotifier`.
They will notify the event name and any associated event arguments when an event is raised on the concrete class `myService`.

``` csharp
public class EventNotifier : IEventNotifier
{
    public void NotifyEvent(string eventName, object?[] arguments)
    {
        // Triggered when an event is raised
    }
}

...

EventNotifier eventNotifier = new();
```

By default, new instance of `EventListener` does not listen for raised events. The `EventListener.StartListening` method must be called to do so.
