using Flow.Launcher.Plugin.Zoxide.Results;
using Moq;

namespace Flow.Launcher.Plugin.Zoxide.Test.Results;

[TestFixture]
public class ZoxideSetupRequiredResultFactoryTest
{
    private PluginInitContext? _previousContext;

    [SetUp]
    public void SetUp()
    {
        _previousContext = Main.Context;
        var metadata = new PluginMetadata
        {
            IcoPath = @"C:\plugins\zoxide\icon.png",
            ID = "EC779402-4FF2-496D-862E-C2A14E1767C8"
        };
        var api = new Mock<IPublicAPI>();
        api.Setup(a => a.GetTranslation(It.IsAny<string>()))
            .Returns((string key) => key);
        Main.Context = new PluginInitContext(metadata, api.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Main.Context = _previousContext!;
    }

    [Test]
    public void Create_ReturnsTwoResults_WithPluginIcon()
    {
        var results = ZoxideSetupRequiredResultFactory.Create();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results, Has.Count.EqualTo(2));
            Assert.That(results[0].IcoPath, Is.EqualTo(Main.Context.CurrentPluginMetadata.IcoPath));
            Assert.That(results[1].IcoPath, Is.EqualTo(Main.Context.CurrentPluginMetadata.IcoPath));
            Assert.That(results[0].Score, Is.GreaterThan(results[1].Score));
        }
    }

    [Test]
    public void Create_FirstResult_OpensFlowSettings()
    {
        var api = Mock.Get(Main.Context.API);
        var results = ZoxideSetupRequiredResultFactory.Create();

        Assert.That(results[0].Action!(null), Is.True);
        api.Verify(a => a.OpenSettingDialog(), Times.Once);
        api.Verify(a => a.OpenUrl(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Create_SecondResult_OpensZoxideProjectUrl()
    {
        var api = Mock.Get(Main.Context.API);
        var results = ZoxideSetupRequiredResultFactory.Create();

        Assert.That(results[1].Action!(null), Is.True);
        api.Verify(a => a.OpenUrl(ZoxideSetupRequiredResultFactory.ZoxideProjectUrl), Times.Once);
    }
}
