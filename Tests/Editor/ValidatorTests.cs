using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Tutan.Functional.F;

namespace Tutan.Functional.Tests
{
    [TestFixture]
    public class ValidatorTests
    {
        static Validator<int> MustBePositive => n => n > 0 ? Success(n) : Error("Must be positive");
        static Validator<int> MustBeEven => n => n % 2 == 0 ? Success(n) : Error("Must be even");
        static Validator<int> MustBeLessThan100 => n => n < 100 ? Success(n) : Error("Must be less than 100");

        // --- FailFast tests ---

        [Test]
        public void FailFast_AllPass_ReturnsSuccess()
        {
            var validator = FailFast(MustBePositive, MustBeEven, MustBeLessThan100);

            var result = validator(42);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(42, result.ValueUnsafe());
        }

        [Test]
        public void FailFast_FirstFails_ReturnsFirstError()
        {
            var validator = FailFast(MustBePositive, MustBeEven, MustBeLessThan100);

            var result = validator(-3);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual("Must be positive", result.ErrorUnsafe().Message);
        }

        [Test]
        public void FailFast_SecondFails_ReturnsSecondError()
        {
            var validator = FailFast(MustBePositive, MustBeEven, MustBeLessThan100);

            var result = validator(7);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual("Must be even", result.ErrorUnsafe().Message);
        }

        [Test]
        public void FailFast_MultipleFail_ReturnsOnlyFirstError()
        {
            // -3 is negative (fails MustBePositive) and odd (fails MustBeEven)
            // FailFast should stop at the first failure
            var validator = FailFast(MustBePositive, MustBeEven, MustBeLessThan100);

            var result = validator(-3);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual("Must be positive", result.ErrorUnsafe().Message);
        }

        [Test]
        public void FailFast_NoValidators_ReturnsSuccess()
        {
            var validator = FailFast(new List<Validator<int>>());

            var result = validator(42);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(42, result.ValueUnsafe());
        }

        [Test]
        public void FailFast_ParamsOverload_Works()
        {
            var validator = FailFast(MustBePositive, MustBeLessThan100);

            var resultPass = validator(50);
            var resultFail = validator(200);

            Assert.IsTrue(resultPass.IsSuccess);
            Assert.AreEqual(50, resultPass.ValueUnsafe());
            Assert.IsTrue(resultFail.IsError);
            Assert.AreEqual("Must be less than 100", resultFail.ErrorUnsafe().Message);
        }

        // --- HarvestErrors tests ---

        [Test]
        public void HarvestErrors_AllPass_ReturnsSuccess()
        {
            var validator = HarvestErrors(MustBePositive, MustBeEven, MustBeLessThan100);

            var result = validator(42);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(42, result.ValueUnsafe());
        }

        [Test]
        public void HarvestErrors_OneFails_ReturnsError()
        {
            var validator = HarvestErrors(MustBePositive, MustBeEven, MustBeLessThan100);

            var result = validator(7); // odd, but positive and < 100

            Assert.IsTrue(result.IsError);
            Assert.AreEqual("Must be even", result.ErrorUnsafe().Message);
        }

        [Test]
        public void HarvestErrors_MultipleFail_CollectsAllErrors()
        {
            var validator = HarvestErrors(MustBePositive, MustBeEven, MustBeLessThan100);

            // -200 fails all three: not positive, not even (well, it is even), let's use -3 for two failures
            // -3: negative (fail), odd (fail), less than 100 (pass)
            var result = validator(-3);

            Assert.IsTrue(result.IsError);

            var innerErrors = result.ErrorUnsafe().InnerErrors;
            Assert.AreEqual(2, innerErrors.Length);
            Assert.AreEqual("Must be positive", innerErrors[0].Message);
            Assert.AreEqual("Must be even", innerErrors[1].Message);
        }

        [Test]
        public void HarvestErrors_NoValidators_ReturnsSuccess()
        {
            var validator = HarvestErrors(new List<Validator<int>>());

            var result = validator(42);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(42, result.ValueUnsafe());
        }

        [Test]
        public void HarvestErrors_ParamsOverload_Works()
        {
            var validator = HarvestErrors(MustBePositive, MustBeLessThan100);

            var resultPass = validator(50);
            var resultFail = validator(200);

            Assert.IsTrue(resultPass.IsSuccess);
            Assert.AreEqual(50, resultPass.ValueUnsafe());
            Assert.IsTrue(resultFail.IsError);
            Assert.AreEqual("Must be less than 100", resultFail.ErrorUnsafe().Message);
        }

        [Test]
        public void HarvestErrors_NestedValidatorError_KeepsOuterMessage()
        {
            Validator<int> nested = n => Error("Range invalid", Error("below zero"));
            var validator = HarvestErrors(nested, MustBeEven);

            var errors = validator(-3).ErrorUnsafe().AsEnumerable().Select(e => e.Message).ToList();

            Assert.That(errors, Is.EqualTo(new[] { "Range invalid", "Must be even" }));
        }

        [Test]
        public void HarvestErrors_MultipleFail_ErrorMessageContainsAll()
        {
            var validator = HarvestErrors(MustBePositive, MustBeEven, MustBeLessThan100);

            // -3: fails MustBePositive and MustBeEven
            var result = validator(-3);

            Assert.IsTrue(result.IsError);

            var message = result.ErrorUnsafe().Message;
            StringAssert.Contains("Must be positive", message);
            StringAssert.Contains("Must be even", message);
        }
    }
}
