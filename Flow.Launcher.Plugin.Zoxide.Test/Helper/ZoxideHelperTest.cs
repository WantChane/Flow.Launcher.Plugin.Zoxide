using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;
using Moq;

namespace Flow.Launcher.Plugin.Zoxide.Test.Helper;

[TestFixture]
public class ZoxideHelperTest
{
    private PluginInitContext? _previousContext;
    private Mock<IPublicAPI> _api = null!;

    [SetUp]
    public void SetUp()
    {
        _previousContext = Main.Context;
        _api = new Mock<IPublicAPI>(MockBehavior.Loose);
        Main.Context = new PluginInitContext(new PluginMetadata(), _api.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Main.Context = _previousContext!;
    }

    [Test]
    public async Task ValidateAsync_RealZoxide_SetsStateAndReturnsTrue()
    {
        var ok = await ZoxideHelper.ValidateAsync("zoxide.exe");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ok, Is.True);
            Assert.That(ZoxideHelper.IsPathValid, Is.True);
            Assert.That(ZoxideHelper.CurrentVersion, Is.Not.Null.And.Not.Empty);
        }
    }

    [Test]
    public async Task QueryListAsync_RealZoxide_ReturnsListOutput()
    {
        var result = await ZoxideHelper.ZoxideQueryAsync("zoxide.exe", "doc");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(ZoxideCommandExecutionType.Success));
            Assert.That(result.StandardOutput, Is.Not.Null.And.Not.Empty);
        }

        _api.Verify(
            a => a.LogWarn(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public void ParseQueryOutputToEntries_ValidLines_ReturnsEntries()
    {
        var raw = "  20.0 D:\\Documents\r\n   3.2 D:\\Docker\r\nnot valid\r\n";

        var entries = ZoxideHelper.ParseQueries(raw);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entries, Has.Count.EqualTo(2));
            Assert.That(entries[0].Score, Is.EqualTo(200));
            Assert.That(entries[0].Path, Is.EqualTo(@"D:\Documents"));
            Assert.That(entries[1].Score, Is.EqualTo(32));
            Assert.That(entries[1].Path, Is.EqualTo(@"D:\Docker"));
        }
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("\r\n")]
    public void ParseQueryOutputToEntries_NullOrWhitespace_ReturnsEmpty(string? raw)
    {
        Assert.That(ZoxideHelper.ParseQueries(raw), Is.Empty);
    }
}
