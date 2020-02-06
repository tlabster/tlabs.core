﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tlabs.Diagnostic {

  /// <summary>Diagnostic logging.</summary>
  public static class Diag {
    /// <summary>Diagnostic log listener name.</summary>
    public const string LOG_LISTENER= "Tlabs.Diagnostic.Log";
    /// <summary>Diagnostic log event name.</summary>
    public const string LOG_EVENT= "Log";
    static readonly DiagnosticListener logSrc= new DiagnosticListener(LOG_LISTENER);

    /// <summary>Log detail.</summary>
    public class Detail {
      /// <summary>Error.</summary>
      public static readonly Detail ERROR= new Detail(10);
      /// <summary>Warning.</summary>
      public static readonly Detail WARN= new Detail(20);
      /// <summary>Informational.</summary>
      public static readonly Detail INFO= new Detail(30);
      /// <summary>Debugging.</summary>
      public static readonly Detail DEBUG= new Detail(40);
      int level;
      private Detail(int lev) { this.level= lev; }
      /// <summary>Detail level.</summary>
      public int Level => level;
    }

    /// <summary>Log <paramref name="payload"/> from <paramref name="src"/> with <paramref name="detail"/>.</summary>
    public static void Log(object src, Detail detail, object payload) {
      if (logSrc.IsEnabled(LOG_EVENT, src, detail))
        logSrc.Write(LOG_EVENT, payload);
    }

    /// <summary>Subscripe log <paramref name="handler"/> .</summary>
    public static IDisposable SubscribeLogHandler(IReadOnlyDictionary<string, Detail> detailMap, Action<object, Detail, object> handler) {
      Func<object, Detail, bool> match= (src, dtl) => {
        Detail detail;
        var tname= src?.GetType().ToString();
        if (null == tname) return   detailMap.TryGetValue(tname, out detail)
                                  ? dtl.Level <= detail.Level
                                  : false; //no default detail
        foreach (var pair in detailMap)
          if (tname.StartsWith(pair.Key) && dtl.Level <= pair.Value.Level) return true;
        return false;
      };
      return SubscribeLogHandler(match, handler);
    }

    /// <summary>Subscripe log <paramref name="handler"/> with predicate <paramref name="match"/>.</summary>
    public static IDisposable SubscribeLogHandler(Func<object, Detail, bool> match, Action<object, Detail, object> handler) {
      object src= null;
      Detail dtl= null;

      Func<object, object, bool> predicate= (a1, a2) => {
        src= a1;
        dtl= a2 as Detail;
        return match(a1, dtl);
      };
      return DiagnosticExt.SubscribeToListener(LOG_LISTENER, LOG_LISTENER, predicate, payload => handler(src, dtl, payload));
    }
  }
}