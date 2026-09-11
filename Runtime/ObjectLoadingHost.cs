using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Deucarian.ObjectLoading
{
    /// <summary>Owns loaded handles and in-flight requests by ID. All methods run on Unity's main thread.</summary>
    [DefaultExecutionOrder(-1000), DisallowMultipleComponent]
    public sealed class ObjectLoadingHost : MonoBehaviour
    {
        [SerializeField] private bool registerAsDefault = true;
        private readonly Dictionary<string, Operation> operations = new Dictionary<string, Operation>(StringComparer.Ordinal);
        private Func<IObjectLoadingPipeline> createPipeline = () => new ObjectLoadingPipeline();
        private IDisposable registration;

        public void Configure(Func<IObjectLoadingPipeline> pipelineFactory)
        {
            if (pipelineFactory == null) throw new ArgumentNullException(nameof(pipelineFactory));
            if (operations.Count != 0) throw new InvalidOperationException("Unload existing objects before changing the pipeline factory.");
            createPipeline = pipelineFactory;
        }

        public Task<ObjectLoadResult> LoadAsync(ObjectKey key, string url, Transform parent = null,
            CancellationToken cancellationToken = default)
        {
            if (!isActiveAndEnabled) throw new InvalidOperationException("The object loading host must be enabled.");
            string id = RequireKey(key);
            var request = ObjectLoadRequest.FromUrl(url);
            request.Parent = parent;
            UnloadById(id);
            var operation = new Operation(cancellationToken);
            operations.Add(id, operation);
            request.CancellationToken = operation.Cancellation.Token;
            try
            {
                var pipeline = createPipeline() ?? throw new InvalidOperationException("The pipeline factory returned null.");
                operation.Stack.Push(pipeline.LoadAsync(request, result =>
                {
                    if (operation.Released) result?.Handle?.Dispose();
                    else operation.Result = result;
                }));
                operation.Coroutine = StartCoroutine(Run(id, operation));
            }
            catch (Exception error) { operation.Completion.TrySetException(Combine(error, Release(id, operation))); }
            return operation.Completion.Task;
        }

        /// <summary>Releases the handle or cancels an in-flight load. Unknown IDs are harmless.</summary>
        public void Unload(ObjectKey key) => UnloadById(RequireKey(key));

        private static string RequireKey(ObjectKey key) => key != null ? key.Id :
            throw new ArgumentNullException(nameof(key), "Select an ObjectKey in the Inspector or reuse a named object definition.");

        private void UnloadById(string id)
        {
            if (!operations.TryGetValue(id, out var operation)) return;
            Exception failure = null;
            try { operation.Cancellation?.Cancel(); }
            catch (Exception error) { failure = error; }
            try { if (operation.Coroutine != null) StopCoroutine(operation.Coroutine); }
            catch (Exception error) { failure = Combine(failure, error); }
            failure = Combine(failure, Release(id, operation));
            if (failure == null) operation.Completion.TrySetResult(Cancelled());
            else { operation.Completion.TrySetException(failure); throw failure; }
        }

        private IEnumerator Run(string id, Operation operation)
        {
            while (operation.Stack.Count > 0)
            {
                object yielded = null;
                Exception failure = null;
                try
                {
                    operation.Cancellation.Token.ThrowIfCancellationRequested();
                    var iterator = operation.Stack.Peek();
                    if (!iterator.MoveNext()) { operation.Stack.Pop(); (iterator as IDisposable)?.Dispose(); continue; }
                    yielded = iterator.Current;
                    if (yielded is IEnumerator nested) { operation.Stack.Push(nested); continue; }
                }
                catch (Exception error) { failure = error; }
                if (failure != null)
                {
                    var cleanup = Release(id, operation);
                    if (failure is OperationCanceledException && cleanup == null) operation.Completion.TrySetResult(Cancelled());
                    else operation.Completion.TrySetException(Combine(failure, cleanup));
                    yield break;
                }
                yield return yielded;
            }
            var result = operation.Result;
            if (operation.Cancellation.IsCancellationRequested)
            {
                var cleanup = Release(id, operation);
                if (cleanup == null) operation.Completion.TrySetResult(Cancelled());
                else operation.Completion.TrySetException(cleanup);
                yield break;
            }
            operation.Cancellation.Dispose();
            operation.Cancellation = null;
            operation.Coroutine = null;
            var releaseFailure = result == null || !result.Succeeded ? Release(id, operation) : null;
            if (releaseFailure != null) operation.Completion.TrySetException(releaseFailure);
            else if (result == null) operation.Completion.TrySetException(new InvalidOperationException("The object pipeline returned no result."));
            else operation.Completion.TrySetResult(result);
        }

        private Exception Release(string id, Operation operation)
        {
            operation.Released = true;
            Exception failure = null;
            if (operations.TryGetValue(id, out var current) && ReferenceEquals(current, operation)) operations.Remove(id);
            while (operation.Stack.Count > 0)
            {
                try { (operation.Stack.Pop() as IDisposable)?.Dispose(); }
                catch (Exception error) { failure = Combine(failure, error); }
            }
            try { operation.Result?.Handle?.Dispose(); }
            catch (Exception error) { failure = Combine(failure, error); }
            operation.Result = null;
            operation.Cancellation?.Dispose();
            operation.Cancellation = null;
            return failure;
        }

        private static Exception Combine(Exception first, Exception second) =>
            first == null ? second : second == null ? first : new AggregateException(first, second);

        private static ObjectLoadResult Cancelled() => ObjectLoadResult.Failure(
            ObjectLoadError.Create(ObjectLoadErrorCode.Canceled, "Object load was canceled."));
        private void OnEnable() { if (registerAsDefault) registration = Objects.Bind(this); }
        private void Update()
        {
            List<string> cancelled = null;
            foreach (var pair in operations)
                if (pair.Value.Cancellation != null && pair.Value.Cancellation.IsCancellationRequested)
                    (cancelled ?? (cancelled = new List<string>())).Add(pair.Key);
            if (cancelled == null) return;
            Exception failure = null;
            foreach (var id in cancelled)
            {
                try { UnloadById(id); }
                catch (Exception error) { failure = Combine(failure, error); }
            }
            if (failure != null) throw failure;
        }
        private void OnDisable()
        {
            registration?.Dispose();
            registration = null;
            Exception failure = null;
            foreach (var id in new List<string>(operations.Keys))
            {
                try { UnloadById(id); }
                catch (Exception error) { failure = Combine(failure, error); }
            }
            if (failure != null) throw failure;
        }

        private sealed class Operation
        {
            public Operation(CancellationToken token) { Cancellation = CancellationTokenSource.CreateLinkedTokenSource(token); }
            public CancellationTokenSource Cancellation;
            public readonly Stack<IEnumerator> Stack = new Stack<IEnumerator>();
            public readonly TaskCompletionSource<ObjectLoadResult> Completion = new TaskCompletionSource<ObjectLoadResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            public Coroutine Coroutine;
            public ObjectLoadResult Result;
            public bool Released;
        }
    }
}
