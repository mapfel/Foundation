// The MIT License (MIT)
//
// Copyright (c) 2020 Markus Raufer
//
// All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
﻿namespace Foundation
{
    public static class ResultExtensions
    {
        /// <summary>
        /// If IsOk is true <paramref name="ok"/> is called otherwise <paramref name="error"/> is called.
        /// </summary>
        /// <typeparam name="TOk"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="ok"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static TResult Either<TOk, TError, TResult>(
            this Result<TOk, TError> result,
            Func<TOk, TResult> ok,
            Func<TError, TResult> error)
        {
            if (result.TryGetOk(out TOk? okValue)) return ok(okValue!);

            return error(result.ToError());
        }

        /// <summary>
        /// If IsOk is true <paramref name="ok"/> is called otherwise <paramref name="error"/> is called.
        /// </summary>
        /// <typeparam name="TOk"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <param name="ok"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static Unit Invoke<TOk, TError>(
            this Result<TOk, TError> result,
            Action<TOk> ok,
            Action<TError> error)
        {
            return result.Either(_ => result.OnOk(ok), _ => result.OnError(error));
        }

        /// <summary>
        /// <paramref name="error"/> is only called when IsOk is false.
        /// </summary>
        /// <typeparam name="TOk"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static Unit OnError<TOk, TError>(this Result<TOk, TError> result, Action<TError> error)
        {
            error.ThrowIfNull();

            if (!result.IsOk && result.TryGetError(out TError? errorValue)) error(errorValue!);
            
            return new Unit();
        }

        /// <summary>
        /// <paramref name="ok"/> is only called when IsOk is true.
        /// </summary>
        /// <typeparam name="TOk"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <param name="ok"></param>
        /// <returns></returns>
        public static Unit OnOk<TOk, TError>(this Result<TOk, TError> result, Action<TOk> ok)
        {
            ok.ThrowIfNull();

            if (!result.IsOk && result.TryGetOk(out TOk? okValue)) ok(okValue!);

            return new Unit();
        }

        /// <summary>
        /// If IsOk is true it returns the Ok value otherwise it throws an exception.
        /// </summary>
        /// <typeparam name="TOk">The value if IsOk is true.</typeparam>
        /// <typeparam name="TError">The exception which is thrown when IsOk is false.</typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TOk OkOrThrow<TOk>(this Result<TOk, Exception> result)
        {
            if (result.TryGetOk(out TOk? ok)) return ok;

            if (result.TryGetError(out Exception? error)) throw error;

            throw new ArgumentException($"invalid {result}");
        }

        /// <summary>
        /// If IsOk is true it returns the Ok value otherwise it throws an exception.
        /// </summary>
        /// <typeparam name="TOk">The value if IsOk is true.</typeparam>
        /// <typeparam name="TError">The exception which is thrown when IsOk is false.</typeparam>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static TOk OkOrThrow<TOk>(this Result<TOk, Exception> result, Func<Exception> error)
        {
            error.ThrowIfNull();

            if (result.TryGetOk(out TOk? ok)) return ok;

            throw error();
        }

        /// <summary>
        /// If result contains an error an error is returned otherwise an exception is thrown.
        /// </summary>
        /// <typeparam name="TOk"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Is thrown if result does not contain an error.</exception>
        public static TError ToError<TError>(this Result<TError> result)
        {
            if (result.TryGetError(out TError? error)) return error!;

            throw new ArgumentException($"{nameof(result)} does not contain an error");
        }

        /// <summary>
        /// If result contains an error an error is returned otherwise an exception is thrown.
        /// </summary>
        /// <typeparam name="TOk"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Is thrown if result does not contain an error.</exception>
        public static TError ToError<TOk, TError>(this Result<TOk, TError> result)
        {
            if (result.TryGetError(out TError? error)) return error!;

            throw new ArgumentException($"{nameof(result)} does not contain an error");
        }

        /// <summary>
        /// If result contains a value a value is returned otherwise an exception is thrown.
        /// </summary>
        /// <typeparam name="TOk"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Is thrown if result does contain an error.</exception>
        public static TOk ToOk<TOk, TError>(this Result<TOk, TError> result)
        {
            if (result.TryGetOk(out TOk? ok)) return ok!;

            throw new ArgumentException($"{nameof(result)} does not contains a value");
        }

        /// <summary>
        /// Converts a result to an optional.
        /// </summary>
        /// <typeparam name="TOk"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Option<TOk> ToOption<TOk, TError>(this Result<TOk, TError> result)
            => result.IsOk && result.TryGetOk(out TOk? ok) ? Option.Some(ok!) : Option.None<TOk>();
    }
}
