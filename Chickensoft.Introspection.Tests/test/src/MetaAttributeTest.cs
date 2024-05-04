namespace Chickensoft.Introspection.Tests;

using Shouldly;
using Xunit;

public class MetaAttributeTest {
  [Mixin]
  public interface ITestMixin : IMixin<ITestMixin>;

  [Fact]
  public void InitializesWithId() {
    const string id = "id";
    var meta = new MetaAttribute(id);
    meta.Id.ShouldBe(id);
  }

  [Fact]
  public void InitializesWithMixins() {
    var meta = new MetaAttribute(typeof(ITestMixin));
    meta.Mixins.ShouldContain(typeof(ITestMixin));
  }
}
