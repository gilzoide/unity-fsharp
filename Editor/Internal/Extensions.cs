using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Gilzoide.FSharp.Editor.Internal
{
    public static class Extensions
    {
        public static async void Forget(this Task task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                // no-op
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
