using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Utility;

namespace Tests.EditMode.Utility {
    public class ErrorHandlerTests {
        [Test]
        public async Task DoWithReTryAsync_FirstAttemptSucceeds_InvokesOnce() {
            int callCount = 0;

            bool result = await ErrorHandler.DoWithReTryAsync(() => {
                callCount++;
                return UniTask.FromResult(true);
            }, "test", 3);

            Assert.That(result, Is.True);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public async Task DoWithReTryAsync_ExceptionThenSuccess_Retries() {
            int callCount = 0;
            LogAssert.Expect(LogType.Error, new Regex("InvalidOperationException: transient"));

            bool result = await ErrorHandler.DoWithReTryAsync(() => {
                callCount++;
                if (callCount == 1) throw new InvalidOperationException("transient");
                return UniTask.FromResult(true);
            }, "test", 3);

            Assert.That(result, Is.True);
            Assert.That(callCount, Is.EqualTo(2));
        }

        [Test]
        public async Task DoWithReTryAsync_ZeroRetries_FailsWithoutInvocation() {
            int callCount = 0;
            LogAssert.Expect(LogType.Error, "ErrorHandler.DoWithReTryAsync::all zero-retry retry failed");

            bool result = await ErrorHandler.DoWithReTryAsync(() => {
                callCount++;
                return UniTask.FromResult(true);
            }, "zero-retry", 0);

            Assert.That(result, Is.False);
            Assert.That(callCount, Is.Zero);
        }
    }
}
