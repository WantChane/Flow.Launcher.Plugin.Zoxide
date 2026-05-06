using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;

namespace Flow.Launcher.Plugin.Zoxide.Test.Helper;

[TestFixture]
public class PathFormatterTest
{
    private static readonly Settings FishSettings = new()
    {
        FishStyleEnabled = true,
        PathTruncationLength = 1,
    };

    private static readonly Settings HeadSettings = new()
    {
        FishStyleEnabled = false,
        PathTruncationLength = 2,
        TruncationSymbol = "…",
    };

    [Test]
    public void TruncatePath_LengthZero_ReturnsFullPath()
    {
        var settings = new Settings { PathTruncationLength = 0 };
        var path = @"D:\Documents\Projects\repo";

        var result = PathFormatter.TruncatePath(path, settings);

        Assert.That(result, Is.EqualTo(path));
    }

    [Test]
    public void TruncatePath_NullSettings_ReturnsPathAsIs()
    {
        var path = @"D:\Documents\Projects\repo";

        var result = PathFormatter.TruncatePath(path, null);

        Assert.That(result, Is.EqualTo(path));
    }

    [Test]
    public void TruncatePath_EmptyPath_ReturnsEmpty()
    {
        var result = PathFormatter.TruncatePath("", FishSettings);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TruncatePath_FishStyle_AbbreviatesParentDirs()
    {
        var path = @"D:\Documents\Folder A\SubFolder B";

        var result = PathFormatter.TruncatePath(path, FishSettings);

        Assert.That(result, Is.EqualTo(@"D:\D\F\SubFolder B"));
    }

    [Test]
    public void TruncatePath_FishStyle_ShortParentStaysIntact()
    {
        var path = @"D:\a\b\c";
        var settings = new Settings
        {
            FishStyleEnabled = true,
            PathTruncationLength = 3,
        };

        var result = PathFormatter.TruncatePath(path, settings);

        Assert.That(result, Is.EqualTo(@"D:\a\b\c"));
    }

    [Test]
    public void TruncatePath_FishStyle_LeafDirAlwaysFull()
    {
        var path = @"D:\parent\VeryLongLeafDirName";
        var settings = new Settings
        {
            FishStyleEnabled = true,
            PathTruncationLength = 1,
        };

        var result = PathFormatter.TruncatePath(path, settings);

        Assert.That(result, Is.EqualTo(@"D:\p\VeryLongLeafDirName"));
    }

    [Test]
    public void TruncatePath_FishStyle_RootOnly_ReturnsPathAsIs()
    {
        var path = @"C:\";

        var result = PathFormatter.TruncatePath(path, FishSettings);

        Assert.That(result, Is.EqualTo(path));
    }

    [Test]
    public void TruncatePath_HeadMode_KeepsLastN()
    {
        var path = @"D:\Documents\Folder A\SubFolder B";

        var result = PathFormatter.TruncatePath(path, HeadSettings);

        Assert.That(result, Is.EqualTo(@"…\Folder A\SubFolder B"));
    }

    [Test]
    public void TruncatePath_HeadMode_LengthExceedsComponents_ReturnsFullPath()
    {
        var path = @"D:\Foo\Bar";
        var settings = new Settings
        {
            FishStyleEnabled = false,
            PathTruncationLength = 5,
            TruncationSymbol = "…",
        };

        var result = PathFormatter.TruncatePath(path, settings);

        Assert.That(result, Is.EqualTo(path));
    }

    [Test]
    public void TruncatePath_HeadMode_SingleDir_TruncatesWithSymbol()
    {
        var path = @"D:\Foo\Bar";
        var settings = new Settings
        {
            FishStyleEnabled = false,
            PathTruncationLength = 1,
            TruncationSymbol = "…",
        };

        var result = PathFormatter.TruncatePath(path, settings);

        Assert.That(result, Is.EqualTo(@"…\Bar"));
    }

}
