using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;
using Flow.Launcher.Plugin.Zoxide.Results;
using Moq;

namespace Flow.Launcher.Plugin.Zoxide.Test.Results;

[TestFixture]
public class ZoxideContextMenuFactoryTest
{
    private PluginInitContext? _previousContext;

    [SetUp]
    public void SetUp()
    {
        _previousContext = Main.Context;
        var metadata = new PluginMetadata { IcoPath = @"C:\plugins\zoxide\icon.png" };
        Main.Context = new PluginInitContext(metadata, Mock.Of<IPublicAPI>());
    }

    [TearDown]
    public void TearDown()
    {
        Main.Context = _previousContext!;
    }

    [Test]
    public void Create_NoContextData_ReturnsEmpty()
    {
        var settings = new Settings { ZoxideExePath = @"C:\zoxide.exe" };
        settings.Commands.Add(new Command { Name = "A", Executable = "a.exe", IsEnabled = true });

        var list = ZoxideContextMenuFactory.Create(new Result { ContextData = null }, settings, Main.Context);

        Assert.That(list, Is.Empty);
    }

    [Test]
    public void Create_WhiteSpacePath_ReturnsEmpty()
    {
        var settings = new Settings();
        settings.Commands.Add(new Command { Name = "A", Executable = "a.exe", IsEnabled = true });

        var list = ZoxideContextMenuFactory.Create(new Result { ContextData = "   " }, settings, Main.Context);

        Assert.That(list, Is.Empty);
    }

    [Test]
    public void Create_FiltersDisabled_PreservesOrderOfEnabled()
    {
        var path = @"D:\repo";
        var settings = new Settings { ZoxideExePath = @"C:\z.exe" };
        settings.Commands.Add(new Command { Name = "First", Executable = "1.exe", IsEnabled = true });
        settings.Commands.Add(new Command { Name = "Skip", Executable = "x.exe", IsEnabled = false });
        settings.Commands.Add(new Command { Name = "Second", Executable = "2.exe", IsEnabled = true });

        var list = ZoxideContextMenuFactory.Create(new Result { ContextData = path }, settings, Main.Context);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(list, Has.Count.EqualTo(2));
            Assert.That(list[0].Title, Is.EqualTo("First"));
            Assert.That(list[0].SubTitle, Is.EqualTo("1.exe"));
            Assert.That(list[1].Title, Is.EqualTo("Second"));
            Assert.That(list[1].SubTitle, Is.EqualTo("2.exe"));
            Assert.That(list[0].IcoPath, Is.EqualTo(IconHelper.PluginIcon));
            Assert.That(list[1].IcoPath, Is.EqualTo(IconHelper.PluginIcon));
        }
    }

    [Test]
    public void Create_UsesCustomIcon_WhenCommandHasValidIcon()
    {
        var path = @"D:\repo";
        var settings = new Settings { ZoxideExePath = @"C:\z.exe" };
        settings.Commands.Add(new Command { Name = "Custom", Executable = "c.exe", IsEnabled = true, Icon = "cmd.png" });
        settings.Commands.Add(new Command { Name = "Default", Executable = "d.exe", IsEnabled = true, Icon = "" });

        var list = ZoxideContextMenuFactory.Create(new Result { ContextData = path }, settings, Main.Context);

        Assert.That(list, Has.Count.EqualTo(2));
        Assert.That(list[1].IcoPath, Is.EqualTo(IconHelper.PluginIcon));
    }
}
