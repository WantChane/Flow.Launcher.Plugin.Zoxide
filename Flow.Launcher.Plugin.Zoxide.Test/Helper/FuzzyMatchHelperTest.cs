using Flow.Launcher.Plugin.SharedModels;
using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;
using Moq;

namespace Flow.Launcher.Plugin.Zoxide.Test.Helper;

[TestFixture]
public class FuzzyMatchHelperTest
{
    private PluginInitContext? _previousContext;
    private Mock<IPublicAPI> _api = null!;

    private static ZoxideEntry E(string path, int score) => new() { Path = path, Score = score };

    [SetUp]
    public void SetUp()
    {
        _previousContext = Main.Context;
        _api = new Mock<IPublicAPI>(MockBehavior.Strict);
        Main.Context = new PluginInitContext(new PluginMetadata(), _api.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Main.Context = _previousContext!;
    }

    [Test]
    public void Filter_SingleTerm_ConsecutiveMatch_ReturnsEntry()
    {
        var entry = E(@"D:\ipsum\lorem", 100);

        var result = FuzzyMatchHelper.Filter([entry], "lor");

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Path, Is.EqualTo(entry.Path));
    }

    [Test]
    public void Filter_SingleTerm_ConsecutiveMatch_RespectsLastFolder()
    {
        var entry = E(@"D:\ipsum\lorem", 100);

        var result = FuzzyMatchHelper.Filter([entry], "D:");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Filter_SingleTerm_NonSubstring_Rejected()
    {
        var entry = E(@"D:\ipsum\lorem", 100);

        var result = FuzzyMatchHelper.Filter([entry], "Lm");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Filter_SingleTerm_NotMatched_Rejected()
    {
        var entry = E(@"D:\ipsum\lorem", 100);

        var result = FuzzyMatchHelper.Filter([entry], "xyz");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Filter_TwoTerms_CorrectOrderAndConsecutive_ReturnsEntry()
    {
        var entry = E(@"D:\lorem\ipsum\dolor", 100);

        var result = FuzzyMatchHelper.Filter([entry], "lor dol");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Filter_TwoTerms_WrongOrder_Rejected()
    {
        var entry = E(@"D:\lorem\ipsum\dolor", 100);

        var result = FuzzyMatchHelper.Filter([entry], "dol lor");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Filter_LastTerm_MatchesLastFolder_ReturnsEntry()
    {
        var entry = E(@"D:\x\y\abc_def", 60);

        var result = FuzzyMatchHelper.Filter([entry], "abc");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Filter_LastTerm_MatchesBeforeLastFolder_Rejected()
    {
        var entry = E(@"D:\x\y\abc_def", 60);

        var result = FuzzyMatchHelper.Filter([entry], "x");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Filter_LastTermPrefersTail_MatchesLastFolder()
    {
        var entry = E(@"D:\ab\cd\ab", 100);

        var result = FuzzyMatchHelper.Filter([entry], "ab");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Filter_MultiTerm_LastTermPrefersTail()
    {
        var entry = E(@"D:\lorem\ipsum\lipsum", 100);

        var result = FuzzyMatchHelper.Filter([entry], "lor ips");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Zoxide_CaseInsensitive_Match()
    {
        var entry = E("/foo/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "fOo bAr");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Zoxide_CaseInsensitive_Upper()
    {
        var entry = E("/foo/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "FOO BAR");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Zoxide_LastKeywordMatchesTail()
    {
        var entry = E("/foo/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "ba");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Zoxide_KeywordNotInLastComponent_Rejected()
    {
        var entry = E("/foo/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "fo");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Zoxide_KeywordWithSeparator_NotAtTail_Rejected()
    {
        var entry = E("/foo", 100);

        var result = FuzzyMatchHelper.Filter([entry], "foo/");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Zoxide_KeywordWithSeparator_Matches()
    {
        var entry = E("/foo/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "foo/");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Zoxide_CrossComponent_Match()
    {
        var entry = E("/foo/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "/ fo / ar");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Zoxide_CrossComponentLiteral_Match()
    {
        var entry = E("/foo/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "oo/ba");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Zoxide_Overlapping_Rejected()
    {
        var entry = E("/foo/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "/foo/ /bar");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Zoxide_NonOverlapping_Matches()
    {
        var entry = E("/foo/baz/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "/foo/ /bar");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Zoxide_OverlapPrevention_Rejected()
    {
        var entry = E("/foo/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "foo o bar");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Zoxide_NonOverlappingDistinct_Matches()
    {
        var entry = E("/foo/bar", 100);

        var result = FuzzyMatchHelper.Filter([entry], "foo bar");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Pinyin_ChineseQuery_FuzzySearchCalled_Matches()
    {
        var entry = E(@"D:\文档\项目", 100);
        _api.Setup(a => a.FuzzySearch("wendang", entry.Path))
            .Returns(new MatchResult(true, SearchPrecisionScore.Regular, [3, 4, 5, 6, 7, 8, 9], 100));

        var result = FuzzyMatchHelper.Filter([entry], "wendang");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Pinyin_ChineseQuery_FuzzySearchFails_Rejected()
    {
        var entry = E(@"D:\文档\项目", 100);
        _api.Setup(a => a.FuzzySearch("xyz", entry.Path))
            .Returns(new MatchResult(false, SearchPrecisionScore.None, null, 0));

        var result = FuzzyMatchHelper.Filter([entry], "xyz");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Pinyin_ChinesePath_ZoxideSubstringMatch_SkipsFuzzySearch()
    {
        var entry = E(@"D:\文档\readme", 100);

        var result = FuzzyMatchHelper.Filter([entry], "read");

        Assert.That(result, Has.Count.EqualTo(1));
    }
}
