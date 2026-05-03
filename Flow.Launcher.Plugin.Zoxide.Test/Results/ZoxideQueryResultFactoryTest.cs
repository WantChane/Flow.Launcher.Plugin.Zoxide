using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;
using Flow.Launcher.Plugin.Zoxide.Results;
using Moq;

namespace Flow.Launcher.Plugin.Zoxide.Test.Results;

[TestFixture]
public class ZoxideQueryResultFactoryTest
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
    public void FromEntries_Empty_ReturnsEmptyList()
    {
        var results = ZoxideQueryResultFactory.FromEntries([]);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void FromEntries_SingleEntry_MapsFieldsAndTitleFromFolderName()
    {
        var path = @"D:\Documents\Projects\repo";
        var entries = new List<ZoxideEntry>
        {
            new() { Path = path, Score = 150 }
        };

        var results = ZoxideQueryResultFactory.FromEntries(entries);

        Assert.That(results, Has.Count.EqualTo(1));
        var r = results[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(r.Title, Is.EqualTo("repo"));
            Assert.That(r.SubTitle, Is.EqualTo(path));
            Assert.That(r.IcoPath, Is.EqualTo(IconHelper.PluginIcon));
            Assert.That(r.AddSelectedCount, Is.False);
            Assert.That(r.Score, Is.EqualTo(150));
            Assert.That(r.ContextData, Is.EqualTo(path));
            Assert.That(r.Action, Is.Not.Null);
        }
    }

    [Test]
    public void FromEntries_TrailingSeparators_UsesDirectoryNameAsTitle()
    {
        var path = @"D:\Data\backup\";
        var entries = new List<ZoxideEntry> { new() { Path = path, Score = 10 } };

        var results = ZoxideQueryResultFactory.FromEntries(entries);

        Assert.That(results[0].Title, Is.EqualTo("backup"));
    }

    [Test]
    public void FromEntries_RootOnlyPath_FallsBackToFullPathAsTitle()
    {
        var path = @"C:\";
        var entries = new List<ZoxideEntry> { new() { Path = path, Score = 1 } };

        var results = ZoxideQueryResultFactory.FromEntries(entries);

        Assert.That(results[0].Title, Is.EqualTo(path));
    }

    [Test]
    public void FromEntries_EmptyPath_LeavesTitleEmpty()
    {
        var entries = new List<ZoxideEntry> { new() { Path = "", Score = 0 } };

        var results = ZoxideQueryResultFactory.FromEntries(entries);

        Assert.That(results[0].Title, Is.EqualTo(""));
    }

    [Test]
    public void FromEntries_MultipleEntries_PreservesOrder()
    {
        var entries = new List<ZoxideEntry>
        {
            new() { Path = @"E:\a", Score = 100 },
            new() { Path = @"E:\b", Score = 50 }
        };

        var results = ZoxideQueryResultFactory.FromEntries(entries);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[0].Title, Is.EqualTo("a"));
            Assert.That(results[1].Title, Is.EqualTo("b"));
            Assert.That(results[0].Score, Is.EqualTo(100));
            Assert.That(results[1].Score, Is.EqualTo(50));
        }
    }

    [Test]
    public void FromQueryExecution_Success_EmptyOutput_ShowsNoMatches()
    {
        var api = new Mock<IPublicAPI>();
        api.Setup(a => a.GetTranslation(It.IsAny<string>())).Returns<string>(k => k);
        var metadata = new PluginMetadata { IcoPath = @"C:\plugins\zoxide\icon.png" };
        Main.Context = new PluginInitContext(metadata, api.Object);

        var results = ZoxideQueryResultFactory.FromQueryExecution(ZoxideCommandExecutionResult.Ok(string.Empty));

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Title, Is.EqualTo("flowlauncher_plugin_zoxide_query_nomatches_title"));
    }

    [Test]
    public void FromQueryExecution_Success_UnparseableLines_ShowsParseFailed()
    {
        var api = new Mock<IPublicAPI>();
        api.Setup(a => a.GetTranslation(It.IsAny<string>())).Returns<string>(k => k);
        var metadata = new PluginMetadata { IcoPath = @"C:\plugins\zoxide\icon.png" };
        Main.Context = new PluginInitContext(metadata, api.Object);

        var results = ZoxideQueryResultFactory.FromQueryExecution(
            ZoxideCommandExecutionResult.Ok("totally invalid line"));

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Title, Is.EqualTo("flowlauncher_plugin_zoxide_query_parse_failed_title"));
    }

    [Test]
    public void FromQueryExecution_Success_WhitespaceOnlyMultiline_ShowsNoMatches()
    {
        var api = new Mock<IPublicAPI>();
        api.Setup(a => a.GetTranslation(It.IsAny<string>())).Returns<string>(k => k);
        var metadata = new PluginMetadata { IcoPath = @"C:\plugins\zoxide\icon.png" };
        Main.Context = new PluginInitContext(metadata, api.Object);

        var results = ZoxideQueryResultFactory.FromQueryExecution(
            ZoxideCommandExecutionResult.Ok("   \n  \t  \n   "));

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Title, Is.EqualTo("flowlauncher_plugin_zoxide_query_nomatches_title"));
    }

    [Test]
    public void FromQueryExecution_TimedOut_ShowsTimeout()
    {
        var api = new Mock<IPublicAPI>();
        api.Setup(a => a.GetTranslation(It.IsAny<string>())).Returns<string>(k => k);
        var metadata = new PluginMetadata { IcoPath = @"C:\plugins\zoxide\icon.png" };
        Main.Context = new PluginInitContext(metadata, api.Object);

        var results = ZoxideQueryResultFactory.FromQueryExecution(ZoxideCommandExecutionResult.TimedOut());

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Title, Is.EqualTo("flowlauncher_plugin_zoxide_query_timeout_title"));
    }

    [Test]
    public void FromQueryExecution_Success_ParsedEntries_DelegatesToFromEntries()
    {
        var api = new Mock<IPublicAPI>();
        api.Setup(a => a.GetTranslation(It.IsAny<string>())).Returns<string>(k => k);
        var metadata = new PluginMetadata { IcoPath = @"C:\plugins\zoxide\icon.png" };
        Main.Context = new PluginInitContext(metadata, api.Object);

        var results = ZoxideQueryResultFactory.FromQueryExecution(
            ZoxideCommandExecutionResult.Ok("  20.0 D:\\Projects\\repo"));

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Title, Is.EqualTo("repo"));
    }
}
