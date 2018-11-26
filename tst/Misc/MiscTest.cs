﻿using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Tlabs.Misc.Tests {

  public class MiscTest {

    private readonly ITestOutputHelper tstout;

    public MiscTest(ITestOutputHelper output) {
      this.tstout= output;
    }

    [Fact]
    public void ExecContextTest() {
      ExecContext<MiscTest>.StartWith(this, () => {
        Assert.Same(this, ExecContext<MiscTest>.CurrentData);
      });
      Assert.Throws<InvalidOperationException>(()=> {
        var fail= ExecContext<MiscTest>.CurrentData;
      });
    }

    [Fact]
    public void PropertyTest() {
      var props= new Dictionary<string, object> {
        ["strProp"]= "test-string",
        ["boolProp"]= true,
        ["one"]= new Dictionary<string, object> {
          ["a"]= "one.a",
          ["b"]= "1.b"
        },
        ["strProp"]= "test-string",
      };

      Assert.Equal(props["strProp"], props.GetString("strProp"));
      Assert.True(props.GetBool("undefined", true));
      Assert.Throws<FormatException>(()=> {
        props.GetBool("strProp", true);
      });
      Assert.Null(props.GetString("boolProp"));

      Assert.Equal(props["strProp"], props.GetOrSet("strProp", "x"));
      Assert.Equal("new", props.GetOrSet("newProp", "new"));
      Assert.Equal("new", props.GetOrSet("newProp", "x"));

      object val= null;
      string key= null;
      Assert.True(props.TryResolveValue("strProp", out val, out key));
      Assert.Equal(props["strProp"], val);
      Assert.Equal("strProp", key);
      Assert.False(props.TryResolveValue("undefined", out val, out key));
      Assert.Null(val);
      Assert.Equal("undefined", key);

      Assert.True(props.TryResolveValue("one.a", out val, out key));
      Assert.Equal("one.a", val);
      Assert.Equal("a", key);

      Assert.False(props.TryResolveValue("one.undefined", out val, out key));
      Assert.Null(val);
      Assert.Equal("undefined", key);

      Assert.Equal("1.b", props.ResolvedProperty("[one.b]"));
      Assert.Equal("one.b", props.ResolvedProperty("one.b"));
      Assert.Equal("[one.b", props.ResolvedProperty("[one.b"));
      Assert.Equal("undefined", props.ResolvedProperty("undefined"));

      Assert.False(props.SetResolvedValue("boolProp.a", "xyz", out key));
      Assert.True(props.SetResolvedValue("some.more.prop", "xyz", out key));
      Assert.Equal("prop", key);
      Assert.True(props.TryResolveValue("some.more.prop", out val, out key));
      Assert.Equal("xyz", val);
      Assert.Equal("prop", key);
      Assert.True(props.SetResolvedValue("some.more", "x", out key));
      Assert.False(props.TryResolveValue("some.more.prop", out val, out key));

    }
  }

}