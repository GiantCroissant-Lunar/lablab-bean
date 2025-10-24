# Tasks — Noesis Wrapper Vendoring

- Create vendor folder `dotnet/windows-ui/NoesisGUI.MonoGameWrapper` and copy wrapper sources
- Create `dotnet/windows-ui/LablabBean.UI.Noesis` project
- Implement `NoesisLayer` service (init, update, render, dispose)
- Add provider setup using `NoesisProviderManager` (folder-based)
- Add sample XAML in `Assets/` (Root.xaml, Theme.xaml)
- Add WindowsDX build flavor to `LablabBean.Windows` and references to both projects
- Wire `Program.cs` to initialize and drive `NoesisLayer` under WindowsDX
- Add configuration (license via env/appsettings) and guard if missing
- Validate render, input, and resize behavior
- Document build/run instructions in project README
