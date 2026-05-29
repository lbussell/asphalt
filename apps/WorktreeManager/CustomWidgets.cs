// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Asphalt;
using Asphalt.Widgets;

internal static class CustomWidgets
{
    extension(AsphaltContext context)
    {
        public void Await<T>(Task<Result<T>> loadingTask, Action<AsphaltContext, T> onSuccess)
        {
            if (loadingTask.IsCompleted && loadingTask.Result is T result)
            {
                onSuccess(context, result);
            }
            else if (loadingTask.IsCompleted && loadingTask.Result is Error error)
            {
                context.Text(error.Message);
            }
            else if (loadingTask.IsFaulted)
            {
                context.Text("An error occurred while loading.");
            }
            else
            {
                context.Spinner();
            }
        }
    }
}
