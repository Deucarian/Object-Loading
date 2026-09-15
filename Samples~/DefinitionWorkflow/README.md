# Object-Loading: definition workflow

The typed definition supplies a default URL. This offline scene substitutes a local prefab pipeline; the real loading host still owns cancellation, replacement and cleanup.

## Try the package sample

1. Install this package and its declared dependencies. In Package Manager, import
   **Definition Workflow** from Samples.
2. Open the imported `DefinitionWorkflow.unity` scene and enter Play mode.
3. Use its buttons to exercise load with c#, load from component, unload.
4. Inspect the configured hosts and triggers, then open
   [ObjectLoadingWorkflow.cs](Runtime/ObjectLoadingWorkflow.cs). It is the caller
   example; any Sample...Setup component is the one-time application composition.

## Add your own definition

Open **Control Center → Developer → Definitions**, select this package's domain
and choose Create. Complete its fields and synchronize. Its Unity Create menu
uses the same authoring flow. Edit the asset or its generated editable
`Editor/*.definition.cs` declaration; let Unity finish compilation after saving.

Select the resulting typed key on a package component, or use its named property
in `Deucarian.Generated` from C#. Generated accessors and Inspector fields share
the same key type. The asset owns defaults; a runtime host owns current state.
Scene references, such as the host or a target Transform, remain scene setup.

Imported sample definitions include editable declarations. Duplicating through
Definitions assigns a new stable ID; changing a display name preserves identity.
Do not hand-edit `.g.cs` accessors or generated catalogs.

## Code and Inspector calls

The sample demonstrates these actions:

- **Load with C#**: `ObjectLoadingWorkflow.Load()`.
- **Load from component**: `ObjectLoadingWorkflow.LoadComponent()`.
- **Unload**: `ObjectLoadingWorkflow.Unload()`.

For a Unity button or event, assign the relevant package trigger component and
select its public void method. For ordinary C#, call the host/service's typed
method and inspect its returned result. Domain failures such as unavailable
services, an expired offer or an invalid target remain observable outcomes.
Missing setup reports the required host, definition or binding instead of silently
creating another service.

See the [shared authoring guide](https://github.com/Deucarian/Editor/blob/develop/Documentation~/DefinitionAuthoring.md) for code-first creation, generated
assembly references, ownership, conflicts, deletion and troubleshooting.
