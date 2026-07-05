using System;
using System.Collections.Generic;
using System.Linq;

namespace Tutan.Functional
{
    /// <summary>A single validation rule: returns <c>Success(t)</c> when the value passes, otherwise an <see cref="Error"/>.</summary>
    public delegate Result<T> Validator<T>(T t);

    public static partial class F
    {
        /// <summary>Composes validators to run in order, stopping at the first failure; the remaining validators are skipped.</summary>
        public static Validator<T> FailFast<T>(IEnumerable<Validator<T>> validators)
            => t => validators.Aggregate(
                Success(t),
                (acc, validator) => acc.Bind(_ => validator(t)));

        /// <summary>Composes validators to run in order, stopping at the first failure; the remaining validators are skipped.</summary>
        public static Validator<T> FailFast<T>(params Validator<T>[] validators)
            => FailFast(validators.AsEnumerable());

        /// <summary>
        /// Composes validators to all run, accumulating every failure into a single composite <see cref="Error"/>.
        /// Succeeds only when every validator passes; use <c>Error.AsEnumerable()</c> to iterate the individual failures.
        /// </summary>
        public static Validator<T> HarvestErrors<T>(IEnumerable<Validator<T>> validators)
           => t =>
           {
               var errors = validators
                .Map(validate => validate(t))
                .Bind(v => v.Match(
                   onError: err => Some(err.AsEnumerable()),
                   onSuccess: _ => None))
                .Flatten()
                .ToList();

               return errors.Count == 0
                ? Success(t)
                : Error(errors);
           };

        /// <summary>
        /// Composes validators to all run, accumulating every failure into a single composite <see cref="Error"/>.
        /// Succeeds only when every validator passes; use <c>Error.AsEnumerable()</c> to iterate the individual failures.
        /// </summary>
        public static Validator<T> HarvestErrors<T>(params Validator<T>[] validators)
            => HarvestErrors(validators.AsEnumerable());
    }
}
