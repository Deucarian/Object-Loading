using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Deucarian.ObjectLoading.Tests
{
    public sealed class ObjectLoadingHostTests
    {
        [UnityTest]
        public IEnumerator CallerCancellationSettlesEvenWhilePipelineWaitsOnUnity()
        {
            var go = new GameObject("objects");
            using (var cancellation = new CancellationTokenSource())
            {
                try
                {
                    var pipeline = new WaitingPipeline { NativeWait = true };
                    var host = go.AddComponent<ObjectLoadingHost>();
                    host.Configure(() => pipeline);
                    var pending = Objects.LoadAsync(new ObjectLoadingHostTestsKey("preview"), "https://example.test/model", cancellationToken: cancellation.Token);
                    yield return null;
                    cancellation.Cancel();
                    yield return null;
                    yield return null;
                    Assert.That(pending.IsCompleted, Is.True);
                    Assert.That(pipeline.Disposed, Is.EqualTo(1));
                    Assert.That(pending.Result.Succeeded, Is.False);
                }
                finally { UnityEngine.Object.DestroyImmediate(go); }
            }
        }

        [UnityTest]
        public IEnumerator CleanupFailureFaultsTheTaskAndReleasesTheId()
        {
            var go = new GameObject("objects");
            try
            {
                var pipeline = new WaitingPipeline { ThrowOnDispose = true };
                var host = go.AddComponent<ObjectLoadingHost>();
                host.Configure(() => pipeline);
                var pending = host.LoadAsync(new ObjectLoadingHostTestsKey("preview"), "https://example.test/model");
                yield return null;
                Assert.Throws<InvalidOperationException>(() => host.Unload(new ObjectLoadingHostTestsKey("preview")));
                Assert.That(pending.IsFaulted, Is.True);
                Assert.That(pending.Exception, Is.Not.Null);
                Assert.DoesNotThrow(() => host.Unload(new ObjectLoadingHostTestsKey("preview")));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [UnityTest]
        public IEnumerator UnloadCancelsTheTaskAndDisposesNestedEnumerators()
        {
            var go = new GameObject("objects");
            try
            {
                var pipeline = new WaitingPipeline();
                var host = go.AddComponent<ObjectLoadingHost>();
                host.Configure(() => pipeline);
                var pending = Objects.LoadAsync(new ObjectLoadingHostTestsKey("preview"), "https://example.test/model");
                yield return null;
                Objects.Unload(new ObjectLoadingHostTestsKey("preview"));
                Assert.That(pending.IsCompleted, Is.True);
                Assert.That(pending.Result.Succeeded, Is.False);
                Assert.That(pipeline.Disposed, Is.EqualTo(1));
                Assert.That(Objects.IsConfigured, Is.True);
                go.SetActive(false);
                Assert.That(Objects.IsConfigured, Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        private sealed class WaitingPipeline : IObjectLoadingPipeline
        {
            public int Disposed;
            public bool NativeWait;
            public bool ThrowOnDispose;
            public IEnumerator LoadAsync(ObjectLoadRequest request, Action<ObjectLoadResult> onCompleted)
            {
                try { while (true) yield return NativeWait ? new WaitForSeconds(60) : null; }
                finally { Disposed++; if (ThrowOnDispose) throw new InvalidOperationException("Cleanup failed."); }
            }
            public void UnloadLast() { }
        }
    }
}
