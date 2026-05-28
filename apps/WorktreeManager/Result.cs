// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

internal union Result<T>(T, Error);

internal sealed record Error(string Message);

internal static class ResultExtensions
{
    extension<T>(Result<T> result)
    {
        public Result<U> Bind<U>(Func<T, Result<U>> next)
        {
            return result switch
            {
                T value => next(value),
                Error error => error,
                null => default,
            };
        }

        public Result<U> Map<U>(Func<T, U> selector)
        {
            return result switch
            {
                T value => selector(value),
                Error error => error,
                null => default,
            };
        }
    }

    extension<T>(Task<Result<T>> task)
    {
        public async Task<Result<U>> Map<U>(Func<T, U> selector) =>
            (await task).Map(selector);
    }
}
