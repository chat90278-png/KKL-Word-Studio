namespace KKL.WordStudio.Application.Tests;

using KKL.WordStudio.Application.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class PluginCatalogTests
{
    [Fact]
    public void Register_AddsModule_AndApplyToInvokesConfigureServices()
    {
        var catalog = new PluginCatalog();
        var module = new FakePluginModule();
        catalog.Register(module);

        var services = new ServiceCollection();
        catalog.ApplyTo(services);

        Assert.True(module.WasConfigured);
        Assert.Single(catalog.Modules);
    }

    private sealed class FakePluginModule : IPluginModule
    {
        public string Name => "Fake";
        public bool WasConfigured { get; private set; }
        public void ConfigureServices(IServiceCollection services) => WasConfigured = true;
    }
}
