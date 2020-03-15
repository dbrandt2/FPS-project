using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ThreadManager : MonoBehaviour
{

        private static readonly List<Action> executeOnMainThread = new List<Action>();
        private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
        private static bool actionToExecuteOnMainThread = false;

        private void FixedUpdate()
        {
            UpdateMain();
        }

        ///<summary>Sets an action to the executed on the main thread</summary>
        ///<param name="__action">The action to be executed on the main thread</param>param>
        public static void ExecuteOnMainThread(Action __action)
        {
            if (__action == null)
            {
                Console.WriteLine("No action to execute on main thread");
            }

            lock (executeOnMainThread)
            {
                executeOnMainThread.Add(__action);
                actionToExecuteOnMainThread = true;
            }
        }

        ///<summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread</summary>summary>
        public static void UpdateMain()
        {
            if (actionToExecuteOnMainThread)
            {
                executeCopiedOnMainThread.Clear();
                lock (executeOnMainThread)
                {
                    executeCopiedOnMainThread.AddRange(executeOnMainThread);
                    executeOnMainThread.Clear();
                    actionToExecuteOnMainThread = false;
                }

                for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
                {
                    executeCopiedOnMainThread[i]();
                }
            }
        }
    }
