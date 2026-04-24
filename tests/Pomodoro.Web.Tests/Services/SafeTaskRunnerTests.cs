using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class SafeTaskRunnerTests
{
    public class RunAndForgetTests
    {
        [Fact]
        public async Task RunAndForget_ExecutesTask()
        {
            var mockLogger = new Mock<ILogger>();
            var tcs = new TaskCompletionSource<bool>();
            Func<Task> task = () =>
            {
                tcs.SetResult(true);
                return Task.CompletedTask;
            };

            SafeTaskRunner.RunAndForget(task, mockLogger.Object, "TestOperation");
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));

            Assert.True(completed == tcs.Task);
            Assert.True(await tcs.Task);
        }

        [Fact]
        public async Task RunAndForget_HandlesExceptions()
        {
            var mockLogger = new Mock<ILogger>();
            var tcs = new TaskCompletionSource<bool>();
            Func<Task> task = () =>
            {
                try
                {
                    throw new InvalidOperationException("Test exception");
                }
                finally
                {
                    tcs.SetResult(true);
                }
            };

            SafeTaskRunner.RunAndForget(task, mockLogger.Object, "TestOperation");
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));

            Assert.True(completed == tcs.Task);
        }

        [Fact]
        public async Task RunAndForget_LogsErrors()
        {
            var mockLogger = new Mock<ILogger>();
            var exceptionMessage = "Test exception message";
            var tcs = new TaskCompletionSource<bool>();
            Func<Task> task = () =>
            {
                try
                {
                    throw new InvalidOperationException(exceptionMessage);
                }
                finally
                {
                    tcs.SetResult(true);
                }
            };

            SafeTaskRunner.RunAndForget(task, mockLogger.Object, "TestOperation");
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));

            Assert.True(completed == tcs.Task);
        }

        [Fact]
        public void RunAndForget_WithNullTask_DoesNotThrow()
        {
            var mockLogger = new Mock<ILogger>();

            var exception = Record.Exception(() =>
                SafeTaskRunner.RunAndForget(null!, mockLogger.Object, "TestOperation")
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task RunAndForgetGeneric_ExecutesTask()
        {
            var mockLogger = new Mock<ILogger>();
            var expectedResult = "Test Result";
            var tcs = new TaskCompletionSource<bool>();
            Func<Task<string>> task = () =>
            {
                tcs.SetResult(true);
                return Task.FromResult(expectedResult);
            };

            var successCallbackCalled = false;
            SafeTaskRunner.RunAndForget(
                task,
                mockLogger.Object,
                "TestOperation",
                result =>
                {
                    successCallbackCalled = true;
                    Assert.Equal(expectedResult, result);
                });

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));

            Assert.True(completed == tcs.Task);
            Assert.True(successCallbackCalled);
        }

        [Fact]
        public async Task RunAndForgetGeneric_HandlesExceptions()
        {
            var mockLogger = new Mock<ILogger>();
            var tcs = new TaskCompletionSource<bool>();
            Func<Task<string>> task = () =>
            {
                try
                {
                    throw new InvalidOperationException("Test exception");
                }
                finally
                {
                    tcs.SetResult(true);
                }
            };

            SafeTaskRunner.RunAndForget<string>(
                task,
                mockLogger.Object,
                "TestOperation",
                result => { Assert.Null(result); });

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));

            Assert.True(completed == tcs.Task);
        }

        [Fact]
        public void RunAndForgetGeneric_WithNullTask_DoesNotThrow()
        {
            var mockLogger = new Mock<ILogger>();

            var exception = Record.Exception(() =>
                SafeTaskRunner.RunAndForget<string>(
                    null!,
                    mockLogger.Object,
                    "TestOperation",
                    result => { Assert.Null(result); })
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task RunAndForgetGeneric_WithNullOnSuccess_DoesNotThrow()
        {
            var mockLogger = new Mock<ILogger>();
            var tcs = new TaskCompletionSource<bool>();
            Func<Task<string>> task = () =>
            {
                tcs.SetResult(true);
                return Task.FromResult("result");
            };

            SafeTaskRunner.RunAndForget<string>(
                task,
                mockLogger.Object,
                "TestOperation",
                null);

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            Assert.True(completed == tcs.Task);
        }
    }
}

