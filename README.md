# LiteWare.ObjectInvokers

[![Version](https://img.shields.io/nuget/v/LiteWare.ObjectInvokers)](https://www.nuget.org/packages/LiteWare.ObjectInvokers)

Dynamically invoke methods and modify properties or fields of an object by referencing the member's name.
This is done via the `ObjectInvoker.Invoke` method which accepts a member name and any generic types and/or parameters and redirects the call to a concrete object.

Depending on the underlying concrete object member, the invoke process can:

- Invoke a method and return the result if the member is a method:

    ``` cs
    bool success = (bool)objectInvoker.Invoke("SetLightSwitchState", "SW012", LightSwitchState.On)!;
    ```

- Get or set a property if the member is a property:

    ``` cs
    string id = (string)objectInvoker.Invoke("DeviceId")!; // Property getter
    objectInvoker.Invoke("DeviceDescription", "Livingroom light switch"); // Property setter
    ```

- Get or set a field value if the member is a field:

    ``` cs
    int timeout = (int)objectInvoker.Invoke("Timeout")!; // Get field value
    objectInvoker.Invoke("Timeout", 1000); // Set field value
    ```

This library is useful in scenarios like RPC/RMI, where a remote endpoint wants to invoke a local method by specifying its name.

## Installation

The library is available as a [Nuget Package](https://www.nuget.org/packages/LiteWare.ObjectInvokers/).

## Usage

To dynamically invoke members by member name, an `ObjectInvoker` needs to be created, usually by binding a contract type containing the members to invoke to an instance of the contract:

``` cs
ObjectInvoker objectInvoker = ObjectInvoker.Bind<IMyService>(myService);
```

The contract type is not limited to only interfaces and can be of any type. However, it must have members qualified by the `InvokableMemberAttribute` so that they can be invoked:

``` cs
public interface IMyService
{
    [InvokableMember]
    int MyProperty { get; set; }

    [InvokableMember]
    void MyProcedure();

    [InvokableMember("MyFunction")]
    int SomeFunction(int arg);
}
```

> `InvokableMemberAttribute` can also be applied on `private` members.

Finally, the members can be dynamically invoked by member name:

``` cs
int result = (int)objectInvoker.Invoke("MyFunction", 123)!;
```
