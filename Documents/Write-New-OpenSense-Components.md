# Write new OpenSense components (draft)

Add OpenSense components only when you want to use that component with OpenSense APIs and applications.

Since OpenSense components are wrapping /psi components.
You need to have a /psi component first. Please refer to [/psi document](https://github.com/microsoft/psi/wiki/Writing-Components) for writing a /psi component.

There are only 2 simple interfaces you need to implement to wrap a /psi component to become a OpenSense component.
After you implemented these interfaces, compile them (/psi component together with interfaces) into a DLL assembly file and paste the DLL into the same directory of OpenSense assembly is located, then you can use it.

If you want to add a user interface for setting component's options, 1 more interface need to be implemented.
If you want to add a user interface for interaction with the component at runtime, there is 1 more interface.

## Write component

Add this project as a reference project.

```xml
<ProjectReference Include="$(SolutionDir)\ComponentContract\ComponentContract.csproj" />
```

Use the following template to implement the interfaces:

```C#
[Serializable]
public class MyComponentConfiguration : ConventionalComponentConfiguration {

    //You can put options here, they will be serialized and deserialized. A method SetProperty() is provided for triggering IPropertyChanged event if necessary.

    public override IComponentMetadata GetMetadata() => new MyComponentMetadata();

    protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new MyComponent(pipeline) { 
        //Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name), //if you want an ILogger, then define a field Logger for /psi component.
    };
}

[Export(typeof(IComponentMetadata))] //This ExportAttribute is from System.Composition namespace, not System.ComponentModel.Composition
public class MyComponentMetadata : ConventionalComponentMetadata {

    public override string Description => "My component description";

    protected override Type ComponentType => typeof(MyComponent); //Unspecified generic types are not supported, set all generic parameters here.

    public override ComponentConfiguration CreateConfiguration() => new MyComponentConfiguration();
}
```

The following types of public properties of the /psi component can be detected automatically, `T` is the transmitted data type:

+ `IConsumer<T>`
+ `IProducer<T>`
+ `IReadOnlyList<IConsumer<T>>`
+ `IReadOnlyList<IProducer<T>>`
+ `IReadOnlyDictionary<string, IConsumer<T>>`
+ `IReadOnlyDictionary<string, IProducer<T>>`

Finally, copy and paste the DLL file.

If you want to add a component that knows its transmitting data types only after the pipeline is assembled (and before the pipeline is run), this will be a little bit harder.
Check the implementation of `Join Operator` and/or `PsiStoreExporter` for a reference.

## Write UI

User interfaces are not mandatory.

Add this project as a reference project.

```xml
<ProjectReference Include="$(SolutionDir)\WpfComponentContract\WpfComponentContract.csproj" />
```

Use the following template to implement the interfaces:

```C#
[Export(typeof(IConfigurationControlCreator))]
public class MyComponentConfigurationControlCreator : IConfigurationControlCreator { //This is for modifying options of the component

    public bool CanCreate(ComponentConfiguration configuration) => configuration is MyComponentConfiguration;

    public UIElement Create(ComponentConfiguration configuration) => new MyComponentConfigurationControl() { DataContext = configuration };
}

[Export(typeof(IInstanceControlCreator))]
public class MyComponentInstanceControlCreator : IInstanceControlCreator { //This is for interacting with component instance when the pipeline is running.

    public bool CanCreate(object instance) => instance is MyComponent;

    public UIElement Create(object instance) => new MyComponentInstanceControl() { DataContext = instance }; //Implement IPropertyChanged in your /psi component if you want to reflect changes to the UI control.
}
```

Then, implement the user controls.

Finally, copy and paste the DLL file.
